using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlantMimic : CreatureBehaviorScript
{
    [HideInInspector] public NavMeshAgent agent;
    private bool coroutineRunning = false;
    bool hasTarget, attackingPlayer;

    Vector3 despawnPos;

    public Collider attackHitbox;

    public GameObject burrow, fakeCrop;

    int pacesUntilIdle = 5; //How many times does this wander before trying to idle

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

        if(agent.velocity.sqrMagnitude > 0) anim.SetBool("IsMoving", true);
        else anim.SetBool("IsMoving", false);

    }

    public void CheckState(CreatureState currentState)
    {
        switch (currentState)
        {
            case CreatureState.InitialBury:
                InitialBury();
                break;

            case CreatureState.Emerge:
                //EmergeFromTile();
                break;

            case CreatureState.Wander:
                Wander();
                break;

            case CreatureState.Idle:
                Idle();
                break;

            case CreatureState.Rebury:
                //Rebury();
                break;

            case CreatureState.Die:
                // OnDeath();
                break;

            case CreatureState.Stunned:
                //StrafePlayer();
                break;

            case CreatureState.Buried:
                break;

            default:
                Debug.LogError("Unknown state: " + currentState);
                break;
        }
    }

    void InitialBury()
    {
        anim.SetBool("IsBuried", true);
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
               
            if (r > 9 || TimeManager.Instance.isDay) currentState = CreatureState.Rebury;
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
        if(coroutineRunning) return;

        if (hasTarget && !agent.pathPending && agent.remainingDistance < agent.stoppingDistance + 1f)
        {
            hasTarget = false;
            pacesUntilIdle--;
               
            if (pacesUntilIdle <= 0)
            {
                pacesUntilIdle = Random.Range(5, 11);
                if(!playerInSightRange)
                {
                    StartCoroutine(WaitAround());
                    currentState = CreatureState.Idle;
                    return;
                }
            }
        }
        else if (!hasTarget)
        {
            hasTarget = true;
            Vector3 fleeDirection = transform.forward;

               
            float randomAngle = Random.Range(-70f, 70f); //random offset for random movement

            fleeDirection = Quaternion.Euler(0, randomAngle, 0) * fleeDirection;

            Vector3 newDestination = transform.position + fleeDirection * 3;

           
            agent.SetDestination(newDestination);
        }

        targetStructure = CheckForObstacle(transform);
        if(targetStructure)
        {
            //attack the structure and stop moving
            StartCoroutine(SwipeStructure());
        }
        else if(CheckForPlayer(transform))
        {
            StartCoroutine(SwipePlayer());
        }

    }

    IEnumerator SwipeStructure()
    {
        coroutineRunning = true;
        anim.SetTrigger("IsAttackingStructure");
        yield return new WaitForSeconds(1);
        targetStructure.TakeDamage(damageToStructure);
        yield return new WaitForSeconds(1);
        coroutineRunning = false;
    }

    IEnumerator SwipePlayer()
    {
        coroutineRunning = true;
        anim.SetTrigger("IsAttackingPlayer");
        yield return new WaitForSeconds(1);
        attackingPlayer = true;
        yield return new WaitForSeconds(2);
        attackingPlayer = false;
        coroutineRunning = false;
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

    public override void OnStun(float duration)
    {
        if (currentState != CreatureState.Stunned)
        {
            StartCoroutine(Stun(duration));
            agent.destination = transform.position;
            agent.ResetPath();
        }
    }

    private IEnumerator Stun(float duration)
    {
        yield return new WaitForSeconds(duration);
        /*
        currentState = CreatureState.Stun;
        coroutineRunning = false;
        //StopAllCoroutines();
        StopCoroutine(LungeAtPlayer());
        StopCoroutine(SwipePlayer());
        StopTrackingPlayer();
        if(walkRoutine != null)
        {
            StopCoroutine(walkRoutine);
            walkRoutine = null;
        }
        yield return new WaitForSeconds(duration);
        //StartCoroutine(IdleSoundTimer());
        currentState = CreatureState.Wander;
        */
    }

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
    }

}
