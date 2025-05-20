using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlantMimic : CreatureBehaviorScript
{
    [HideInInspector] public NavMeshAgent agent;
    private bool coroutineRunning = false;
    bool hasTarget, attackingPlayer, attackCooldown, speedCooldown;

    Vector3 newBurrowPos = new Vector3(0,0,0);

    public Collider attackHitbox;

    public GameObject burrow, fakeCrop;

    int pacesUntilIdle = 5; //How many times does this wander before trying to idle
    int pacesUntilCalm = 0;

    float originalSpeed;
    float fleeSpeed = 15;
    float coolDownSpeed = 7;

    private StructureBehaviorScript targetStructure;

    public enum CreatureState
    {
        InitialBury, //Spawned in
        Emerge, //Was dug up
        Wander, //if it runs into a structure or player, will swipe at it
        Idle, //Will resume wandering after x amount of time or it was struck
        Rebury, //if ignored, will dig a hole and replant itself elsewhere, or despawn at day
        Die,
        Buried,
        Stunned
    }

    public CreatureState currentState;

    //When it buries, it will fully heal. It hides in the scene when it is buried

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        base.Start();

        currentState = CreatureState.InitialBury;
        originalSpeed = agent.speed;
    }

    void Update()
    {
        if (health <= 0) isDead = true;

        if (!isDead && currentState != CreatureState.Stunned)
        {
            CheckState(currentState);
        }

        float distance = Vector3.Distance(player.position, transform.position);
        playerInSightRange = distance <= sightRange;
        playerInAttackRange = distance <= attackRange;

        if(agent.velocity.sqrMagnitude > 0) anim.SetBool("IsMoving", true);
        else anim.SetBool("IsMoving", false);

        if(speedCooldown)
        {
            agent.speed = coolDownSpeed;
        }
        else
        {
            if(pacesUntilCalm > 0 && agent.speed != fleeSpeed) agent.speed = fleeSpeed;
            if(pacesUntilCalm <= 0 && agent.speed != originalSpeed) agent.speed = originalSpeed;
        }

    }

    public void CheckState(CreatureState currentState)
    {
        switch (currentState)
        {
            case CreatureState.InitialBury:
                InitialBury();
                break;

            case CreatureState.Emerge:
                EmergeFromTile();
                break;

            case CreatureState.Wander:
                Wander();
                break;

            case CreatureState.Idle:
                Idle();
                break;

            case CreatureState.Rebury:
                Rebury();
                break;

            case CreatureState.Die:
                // OnDeath();
                break;

            case CreatureState.Stunned:
                //StrafePlayer();
                break;

            case CreatureState.Buried:
                if(agent.enabled && !coroutineRunning) agent.enabled = false;
                break;

            default:
                Debug.LogError("Unknown state: " + currentState);
                break;
        }
    }

    void InitialBury()
    {
        anim.SetBool("IsBuried", true);
        currentState = CreatureState.Buried;

        Vector3 cropSpawn = StructureManager.Instance.FindFreeTileNearCrop();
        if(cropSpawn == new Vector3(0,0,0)) Destroy(this.gameObject);
        else
        {
            Instantiate(fakeCrop, cropSpawn, Quaternion.identity).GetComponent<FakeFarmLand>().mimic = this;
        }
    }

    private void Idle()
    {
        if (!coroutineRunning)
        {
            if(playerInSightRange)
            {
                currentState = CreatureState.Wander;
                return;
            }
            int r = Random.Range(0, 10);
               
            if (r > 5 || TimeManager.Instance.isDay) currentState = CreatureState.Rebury;
            else currentState = CreatureState.Wander;
        }
    }

    private IEnumerator WaitAround()
    {
        coroutineRunning = true;
        float r = Random.Range(2f, 4f);
        yield return new WaitForSeconds(r);
        coroutineRunning = false;
    }

    private void Wander()
    {
        if(coroutineRunning || currentState == CreatureState.Stunned) return;

        if (hasTarget && !agent.pathPending && agent.remainingDistance < agent.stoppingDistance + 1f)
        {
            hasTarget = false;
            pacesUntilIdle--;
            if(pacesUntilCalm > 0) pacesUntilCalm--;
               
            if (pacesUntilIdle <= 0)
            {
                pacesUntilIdle = Random.Range(5, 11);
                StartCoroutine(WaitAround());
                currentState = CreatureState.Idle;
                effectsHandler.Idle1();
                return;
            }
        }
        else if (!hasTarget && !attackCooldown)
        {
            hasTarget = true;
            Vector3 fleeDirection = transform.forward;

               
            float randomAngle = Random.Range(-70f, 70f); //random offset for random movement

            fleeDirection = Quaternion.Euler(0, randomAngle, 0) * fleeDirection;

            Vector3 newDestination = transform.position + fleeDirection * 5;

           
            agent.SetDestination(newDestination);
        }

        targetStructure = CheckForObstacle(corpseParticleTransform);
        if(targetStructure && !attackCooldown)
        {
            //attack the structure and stop moving
            StartCoroutine(SwipeStructure());
            hasTarget = false;
        }
        else if((CheckForPlayer(corpseParticleTransform) || playerInAttackRange) && !attackCooldown)
        {
            StartCoroutine(SwipePlayer());
            agent.SetDestination(player.position);
            hasTarget = false;
        }

    }

    void EmergeFromTile()
    {
        if(coroutineRunning) return;
        anim.SetBool("IsBuried", false);
        StartCoroutine(EmergeCoroutine());
    }

    void Rebury()
    {
        if(newBurrowPos == new Vector3(0,0,0))
        {
            newBurrowPos =  StructureManager.Instance.GetRandomClearTile();
            print("Setting Burrow Pos");
            agent.destination = newBurrowPos;
            return;
        }
        else if((agent.remainingDistance < agent.stoppingDistance + 5 && !coroutineRunning))
        {
            print("I am doing my job");
            StartCoroutine(ReburyCoroutine());
        }
        else if(!coroutineRunning && !attackCooldown)
        {
            targetStructure = CheckForObstacle(transform);
            if(targetStructure)
            {
                //attack the structure and stop moving
                StartCoroutine(SwipeStructure());
                hasTarget = false;
            }
            else if(CheckForPlayer(transform))
            {
                StartCoroutine(SwipePlayer());
                hasTarget = false;
            }
        }
    }

    IEnumerator EmergeCoroutine()
    {
        coroutineRunning = true;
        health = maxHealth;
        StructureManager.Instance.SpawnStructure(burrow, StructureManager.Instance.GetTileCenter(transform.position));
        yield return new WaitForSeconds(1.5f);
        effectsHandler.Idle2();
        yield return new WaitForSeconds(0.5f);
        agent.enabled = true;
        currentState = CreatureState.Wander;
        coroutineRunning = false;
    }

    IEnumerator ReburyCoroutine()
    {
        coroutineRunning = true;
        anim.SetBool("IsBuried", true);
        currentState = CreatureState.Buried;
        StructureManager.Instance.SpawnStructure(burrow, newBurrowPos);
        yield return new WaitForSeconds(2);
        if(isDead) yield break;
        if(TimeManager.Instance.isDay) Destroy(gameObject);
        else
        {
            currentState = CreatureState.Buried;

            Vector3 cropSpawn = StructureManager.Instance.FindFreeTileNearCrop();
            if(cropSpawn == new Vector3(0,0,0)) Destroy(this.gameObject);
            else
            {
                Instantiate(fakeCrop, cropSpawn, Quaternion.identity).GetComponent<FakeFarmLand>().mimic = this;
            }
        }
        newBurrowPos = new Vector3(0,0,0);
        coroutineRunning = false;
    }

    IEnumerator SwipeStructure()
    {
        coroutineRunning = true;
        anim.SetTrigger("IsAttackingStructure");
        StartCoroutine(AttackCooldown());
        yield return new WaitForSeconds(1.1f);
        effectsHandler.MiscSound2();
        targetStructure.TakeDamage(damageToStructure);
        yield return new WaitForSeconds(0.2f);
        targetStructure.TakeDamage(damageToStructure);
        yield return new WaitForSeconds(0.5f);
        
        coroutineRunning = false;
    }

    IEnumerator SwipePlayer()
    {
        coroutineRunning = true;
        anim.SetTrigger("IsAttackingPlayer");
        StartCoroutine(AttackCooldown());
        speedCooldown = true;
        yield return new WaitForSeconds(0.7f);
        effectsHandler.MiscSound();
        attackingPlayer = true;
        attackHitbox.enabled = true;
        yield return new WaitForSeconds(0.3f);
        attackHitbox.enabled = false;
        yield return new WaitForSeconds(1);
        speedCooldown = false;

        attackingPlayer = false;
        coroutineRunning = false;
    }

    IEnumerator AttackCooldown()
    {
        attackCooldown = true;
        float t = Random.Range(2.5f, 3.5f);
        float p = 0;
        while(p < t)
        {
            if(currentState == CreatureState.Wander) agent.SetDestination(player.position);
            yield return new WaitForSeconds(0.2f);
            p += 0.2f;
        }
        attackCooldown = false;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (attackingPlayer && other.CompareTag("Player") && !isDead)
        {
            PlayerInteraction playerInteraction = other.GetComponent<PlayerInteraction>();
            if (playerInteraction != null)
            {
                playerInteraction.StaminaChange(damageToPlayer);
                attackHitbox.enabled = false;
            }
        }
    }

    public override bool OnBearTrapStun(StructureBehaviorScript b)
    {
        if (currentState != CreatureState.Stunned)
        {
            StartCoroutine(BearTrapHold(b));
            agent.destination = transform.position;
            agent.ResetPath();
            newBurrowPos = new Vector3(0,0,0);
            return true;
        }
        else return false;
    }

    private IEnumerator BearTrapHold(StructureBehaviorScript b)
    {
        currentState = CreatureState.Stunned;
        coroutineRunning = false;

        while (b && b.health > 0)
        {
            yield return null;
        }
        //StartCoroutine(IdleSoundTimer());
        currentState = CreatureState.Wander;
        
    }

    /*public override bool OnStun(float duration)
    {
        if (currentState != CreatureState.Stunned)
        {
            StartCoroutine(Stun(duration));
            agent.destination = transform.position;
            agent.ResetPath();
            newBurrowPos = new Vector3(0,0,0);
            return true;
        }
        else return false;
    }

    private IEnumerator Stun(float duration)
    {
        
        currentState = CreatureState.Stunned;
        coroutineRunning = false;

        yield return new WaitForSeconds(duration);
        //StartCoroutine(IdleSoundTimer());
        currentState = CreatureState.Wander;
        
    } */

    public override void OnDeath()
    {
        if (!isDead)
        {
            isDead = true;
            anim.Play("Death");
            base.OnDeath();
            agent.enabled = false;
            rb.isKinematic = true;
            rb.freezeRotation = true;
            StopAllCoroutines();
        }
    }

    public override void OnDamage()
    {
        if(currentState == CreatureState.Idle)
        {
            currentState = CreatureState.Wander;
        }
        pacesUntilCalm = Random.Range(4, 8);
    }

}
