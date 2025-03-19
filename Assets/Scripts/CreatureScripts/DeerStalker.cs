using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DeerStalker : CreatureBehaviorScript
{
    
    public Variant variant; // what variant of creature is this?

    bool hasTransformed = false;

    public Animator animTransformed; //anim of the tainted deer

    public GameObject deer,taintedDeer;

    private StructureBehaviorScript targetStructure;

    //public ParticleSystem transformParticles;

    private bool isMoving = false;
    private bool coroutineRunning = false;
    private Transform target;
    float walkSpeed = 4;
    float runSpeed = 13;
    float transformedSightRange = 30;

    [HideInInspector] public NavMeshAgent agent;
    public Collider attackHitbox;
    private bool recoilCooldown = false; //To prevent stunlocking
    private bool Recoiling = false;
    bool hitPlayer = false;

    private Vector3 despawnPos;

    private Coroutine trackPlayerRoutine, walkRoutine; 

    //Its purpose is a player attacker only. Only attacks structures that impede it
    //Still needs Idle anim variance and transform particles

    public enum CreatureState
    {
        SpawnIn,
        Idle,
        Wander,
        Transformation,
        ChaseTarget,
        AttackInFront,
        Stun,
        Die
    }

    public enum Variant
    {
        Normal,
        Pure //Wont Transformation
    }

    public CreatureState currentState;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        base.Start();
        attackHitbox.enabled = false;
        
        agent.enabled = false;
        agent.enabled = true;

        int r = Random.Range(0, NightSpawningManager.Instance.despawnPositions.Length);
        despawnPos = NightSpawningManager.Instance.despawnPositions[r].position;
        targetStructure = null;

        StartCoroutine(IdleSoundTimer());
    }

    public void Spawn()
    {
        if(inWilderness)
        {
            currentState = CreatureState.ChaseTarget;
        }
        else if (!isMoving)
        {
            Vector3 randomPoint = StructureManager.Instance.GetRandomTile();
            walkRoutine = StartCoroutine(MoveToPoint(randomPoint));
        }
    }

    void Update()
    {
        if (health <= 0) isDead = true;

        if (!isDead && currentState != CreatureState.Stun)
        {

            float distance = Vector3.Distance(player.position, transform.position);
            playerInSightRange = distance <= sightRange;
            playerInAttackRange = distance <= attackRange;

            if(currentState == CreatureState.ChaseTarget)
            {
                agent.speed = runSpeed;
            }
            else agent.speed = walkSpeed;

            if(agent.velocity.sqrMagnitude > 0) 
            {
                if(agent.speed == runSpeed)
                {
                    anim.SetBool("IsRunning", true);
                    animTransformed.SetBool("IsRunning", true);
                }
                else
                {
                    anim.SetBool("IsRunning", false);
                    animTransformed.SetBool("IsRunning", false);

                    anim.SetBool("IsMoving", true);
                    animTransformed.SetBool("IsMoving", true);
                }
            }
            else
            {
                anim.SetBool("IsMoving", false);
                animTransformed.SetBool("IsMoving", false);
            }

            CheckState(currentState);
        }
        else
        {
            attackHitbox.enabled = false;
        }
    }

    public void CheckState(CreatureState currentState)
    {
        switch (currentState)
        {
            case CreatureState.SpawnIn:
                Spawn();
                break;

            case CreatureState.Idle:
                Idle();
                break;

            case CreatureState.Wander:
                Wander();
                break;

            case CreatureState.Transformation:
                Transformation();
                break;

            case CreatureState.ChaseTarget:
                if(hasTransformed) ChaseTarget();
                else WalkTowardsPlayer();
                break;

            case CreatureState.AttackInFront:
                Attack();
                break;

            case CreatureState.Stun:
                break;

            case CreatureState.Die:
                // OnDeath();
                break;

            default:
                Debug.LogError("Unknown state: " + currentState);
                break;
        }
    }

    #region WanderingFunctions
    public void Wander()
    {
        if ((playerInSightRange && hasTransformed) || inWilderness)
        {
            currentState = CreatureState.ChaseTarget;
            return;
        }

        if (!isMoving && currentState == CreatureState.Wander)
        {
            Vector3 randomPoint = GetRandomPointAround(transform.position, 5f);
            walkRoutine = StartCoroutine(MoveToPoint(randomPoint));
        }
    }


    private Vector3 GetRandomPointAround(Vector3 origin, float radius)
    {
        Vector2 randomDirection = Random.insideUnitCircle * radius;
        Vector3 randomPoint = new Vector3(randomDirection.x, origin.y, randomDirection.y) + origin;
        return randomPoint;
    }

    private IEnumerator WaitAround()
    {
        coroutineRunning = true;
        float r = Random.Range(1f, 1.7f);
        yield return new WaitForSeconds(r);
        coroutineRunning = false;
    }

    private IEnumerator MoveToPoint(Vector3 destination)
    {
        isMoving = true;
        coroutineRunning = true;

        if (TimeManager.Instance.isDay && !inWilderness) destination = despawnPos;

        agent.destination = destination;

        float timeSpent = 0; //to make sure it doesnt get stuck

        while ((agent.pathPending || agent.remainingDistance > agent.stoppingDistance) && timeSpent < 20)
        {
            timeSpent += Time.deltaTime;
            if (playerInSightRange)
            {
                if(hasTransformed)
                {
                    currentState = CreatureState.ChaseTarget;
                    isMoving = false;
                    coroutineRunning = false;
                    walkRoutine = null;
                }
                else
                {
                    currentState = CreatureState.Transformation;
                    isMoving = false;
                    coroutineRunning = false;
                    walkRoutine = null;
                }
                
                yield break;
            }

            yield return null;
        }

        isMoving = false;
        coroutineRunning = false;

        if(currentState == CreatureState.SpawnIn)
        {
            print("No Longer Spawned In");
            currentState = CreatureState.Wander;
        }

        if (currentState == CreatureState.Wander)
        {
            int randomChoice = Random.Range(0, 3);
            if (randomChoice == 0)
            {
                currentState = CreatureState.Wander;
            }
            else
            {
                currentState = CreatureState.Idle;
            }
        }
        walkRoutine = null;
    }

    void ChaseTarget()
    {
        if (trackPlayerRoutine == null)
        {
            trackPlayerRoutine = StartCoroutine(TrackPlayer());
        }
        if(target == null)
        {
            if(playerInSightRange)
            {
                target = player;
            }
            else
            {
                currentState = CreatureState.Wander;
            }
            return;
        }

        if (target && Vector3.Distance(transform.position, target.position) < 5)
        {
            StopTrackingPlayer();
            currentState = CreatureState.AttackInFront;
        }
        else if (!playerInSightRange && !inWilderness && target == player)
        {
            currentState = CreatureState.Wander;
            target = null;
            StopTrackingPlayer();
            //Code for losing sight
        }
    }

    private void WalkTowardsPlayer() //Movement for passive Deer in the wilderness
    {
        if (trackPlayerRoutine == null)
        {
            trackPlayerRoutine = StartCoroutine(TrackPlayer());
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (playerInSightRange)
        {
            StopTrackingPlayer();
            currentState = CreatureState.Transformation;
        }
        else if (!playerInSightRange && !inWilderness)
        {
            currentState = CreatureState.Wander;
            StopTrackingPlayer();
        }
    }

    private IEnumerator TrackPlayer()
    {
        while ((playerInSightRange || inWilderness))
        {
            if(!target) target = player;
            agent.destination = target.position;
            yield return new WaitForSeconds(0.2f); // update destination every 0.5 seconds to prevent overloading it
        }
        trackPlayerRoutine = null;
    }

    private void StopTrackingPlayer()
    {
        if (trackPlayerRoutine != null)
        {
            StopCoroutine(trackPlayerRoutine);
            trackPlayerRoutine = null;
        }
        agent.ResetPath();
    }
    #endregion
    private void Attack()
    {
        if (coroutineRunning || Recoiling)
            return;

        transform.LookAt(player.position);

        StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        coroutineRunning = true;
        animTransformed.Play("Attack");
        yield return new WaitForSeconds(0.5f);
        attackHitbox.enabled = true;
        agent.SetDestination(transform.position);
        yield return new WaitForSeconds(0.1f);
        attackHitbox.enabled = false;
        if(hitPlayer)
        {
            hitPlayer = false;
            animTransformed.SetBool("AttackSuccessful", true);
            yield return new WaitForSeconds(3.5f);
            animTransformed.SetBool("AttackSuccessful", false);
        }
        else if(targetStructure)
        {
            targetStructure.TakeDamage(damageToStructure);
        }
        coroutineRunning = false;
        currentState = CreatureState.ChaseTarget;
    }

    void Transformation()
    {
        if(coroutineRunning) return;

        StartCoroutine(Transforming());
    }

    IEnumerator Transforming()
    {
        coroutineRunning = true;
        yield return new WaitForSeconds(1.5f);
        deer.SetActive(false);
        taintedDeer.SetActive(true);
        hasTransformed = true;
        currentState = CreatureState.Wander;
        sightRange = transformedSightRange;
        coroutineRunning = false;
    }

    private void Idle()
    {
        if (playerInSightRange)
        {
            if(hasTransformed) currentState = CreatureState.ChaseTarget;
            else currentState = CreatureState.Transformation;
            return;
        }

        if (!coroutineRunning)
        {
            int r = Random.Range(0, 13);
            if (r < 6)
            {
                StartCoroutine(WaitAround());
            }
            else
            {
                currentState = CreatureState.Wander;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isDead)
        {
            PlayerInteraction playerInteraction = other.GetComponent<PlayerInteraction>();
            if (playerInteraction != null)
            {
                playerInteraction.StaminaChange(damageToPlayer);
                hitPlayer = true;
                attackHitbox.enabled = false;
            }
        }
    }

    public override bool OnStun(float duration)
    {
        if (currentState != CreatureState.Stun)
        {
            StartCoroutine(Stun(duration));
            agent.destination = transform.position;
            agent.ResetPath();
            anim.SetTrigger("Recoiling");
            return true;
        }
        return false;
    }

    private IEnumerator Stun(float duration)
    {
        currentState = CreatureState.Stun;
        coroutineRunning = false;
        StopTrackingPlayer();
        StopCoroutine(AttackRoutine());
        attackHitbox.enabled = false;
        if(walkRoutine != null)
        {
            StopCoroutine(walkRoutine);
            walkRoutine = null;
        }
        yield return new WaitForSeconds(duration);
        //StartCoroutine(IdleSoundTimer());
        currentState = CreatureState.Wander;
    }

    public override void OnDeath()
    {
        if (!isDead)
        {
            isDead = true;
            anim.Play("Death");
            animTransformed.Play("Death");
            base.OnDeath();
            agent.enabled = false;
            rb.isKinematic = true;
            rb.freezeRotation = true;
            StopAllCoroutines();
            StartCoroutine(DeathTimer());
        }
    }

    public override void OnDamage()
    {
        if(!recoilCooldown && hasTransformed && !isDead)
        {
            //Giving me too much trouble right now
            /*recoilCooldown = true;
            effectsHandler.OnHit();
            animTransformed.SetTrigger("Recoiling");
            StartCoroutine(RecoilCooldown());*/
        }
        else if(!hasTransformed)
        {
            currentState = CreatureState.Transformation;
        }
    }

    IEnumerator DeathTimer()
    {
        if(hasTransformed) yield return new WaitForSeconds(2);
        else yield return new WaitForSeconds(0.5f);
        health = 0;
        canCorpseBreak = true;
    }

    IEnumerator RecoilCooldown()
    {
        Recoiling = true;
        coroutineRunning = true;
        StopTrackingPlayer();
        StopCoroutine(AttackRoutine());
        attackHitbox.enabled = false;
        yield return new WaitForSeconds(0.7f);
        coroutineRunning = false;
        Recoiling = false;
        yield return new WaitForSeconds(5);
        recoilCooldown = false;
    }

    IEnumerator IdleSoundTimer()
    {
        while(health > 0)
        {
            int i = Random.Range(4,10);
            effectsHandler.RandomIdle();
            yield return new WaitForSeconds(i);
        }
    }

    public override void NewPriorityTarget(StructureBehaviorScript newStruct)
    {
        if(targetStructure) return;
        targetStructure = newStruct;
        if(currentState == CreatureState.Idle || currentState == CreatureState.Wander) currentState = CreatureState.ChaseTarget;
        
    }
}
