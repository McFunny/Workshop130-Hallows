using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Ferrat : CreatureBehaviorScript
{

    bool isMoving, coroutineRunning, isStanding; //ISMOVING TRACKS IF THE MOVEMENT COROUTINE IS PLAYING. COROUTINERUNNING CHECKS IF ANY *OTHER* COROUTINE IS RUNNING

    [HideInInspector] public NavMeshAgent agent;

    private Vector3 despawnPos;

    float walkSpeed = 4;
    float runSpeed = 12;

    public InventoryItemData timberEar;

    public List<ItemWithAmount> gifts = new List<ItemWithAmount>();

    float timeUntilLeave;
    float distanceFromPlayer;
    


    bool interruptAction = false;

    public enum CreatureState
    {
        Wander,
        ApproachPlayer,
        FollowPlayer,
        Flee,
        Leave
    }

    public CreatureState currentState;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        base.Start();
        
        agent.enabled = false;
        agent.enabled = true;

        int r = Random.Range(0, NightSpawningManager.Instance.despawnPositions.Length);
        despawnPos = NightSpawningManager.Instance.despawnPositions[r].position;
        StartCoroutine(IdleSoundTimer());
        StartCoroutine(CheckPlayerNuts());
        timeUntilLeave = Random.Range(300, 500);
        StartCoroutine(LeaveTimer());
    }

    void Update()
    {
        if (health <= 0) isDead = true;

        if(isDead) return;

        if(agent.velocity.magnitude > 1)
        {
            anim.SetBool("IsMoving", true);
        }
        else
        {
            anim.SetBool("IsMoving", false);
        }

        distanceFromPlayer = Vector3.Distance(player.position, transform.position);
        playerInSightRange = distanceFromPlayer <= sightRange;

        if(currentState != CreatureState.Leave)
        {
            if(playerInSightRange)
            {
                if(currentState != CreatureState.Flee && !coroutineRunning) currentState = CreatureState.Flee;
            }

            if(currentState == CreatureState.Flee && distanceFromPlayer > sightRange + 6)
            {
                currentState = CreatureState.Wander;
            }
        }

        if (!isDead)
        {
            CheckState(currentState);
        }
    }

    public void CheckState(CreatureState currentState)
    {
        switch (currentState)
        {
            case CreatureState.Wander:
                Wander();
                break;

            case CreatureState.ApproachPlayer:
                Approach();
                break;

            case CreatureState.Flee:
                Flee();
                break;

            case CreatureState.FollowPlayer:
                FollowPlayer();
                break;

            case CreatureState.Leave:
                Leave();
                break;

            default:
                Debug.LogError("Unknown state: " + currentState);
                break;
        }
    }

    void StanceSwitch(bool _isStanding)
    {
        if(_isStanding == isStanding || coroutineRunning) return;
        isStanding = _isStanding;

        StartCoroutine(StanceSwap());
    }

    IEnumerator StanceSwap()
    {
        coroutineRunning = true;
        anim.SetBool("IsStanding", isStanding);
        agent.speed = 0;
        yield return new WaitForSeconds(0.7f);
        if(isStanding) agent.speed = runSpeed;
        else agent.speed = walkSpeed;
        coroutineRunning = false;
    }

    void Wander()
    {
        if(coroutineRunning) return; 

        if (!isMoving)
        {
            if(currentState == CreatureState.Wander) //Wandering
            {
        
                Vector3 randomPoint;
                if(!patrolPoint) randomPoint = StructureManager.Instance.GetRandomTile();
                else randomPoint = PointAroundPatrolPoint(7);
                StartCoroutine(MoveToPoint(randomPoint, Random.Range(1f, 4f)));
            }
            
        }

        if(playerInSightRange) interruptAction = true;
    }

    void Approach()
    {
        if(coroutineRunning) return; 
        if(!isStanding)
        {
            StanceSwitch(true);
            return;
        }

        if (!isMoving)
        {
            if(currentState == CreatureState.ApproachPlayer)
            {
        
                Vector3 randomPoint;
                if(!patrolPoint) randomPoint = StructureManager.Instance.GetRandomTile();
                else randomPoint = PointAroundPatrolPoint(7);
                StartCoroutine(MoveToPoint(randomPoint, Random.Range(1f, 2.5f)));
            }
            
        }

        if(distanceFromPlayer <= 3) interruptAction = true;
    }

    private void Flee()
    {
        if(coroutineRunning) return;
        if(isStanding)
        {
            StanceSwitch(false);
            return;
        }
        Vector3 runTo = transform.position + ((transform.position - player.position + new Vector3(Random.Range(-3, 3), 0, Random.Range(-3, 3))));
        agent.destination = runTo;

    }

    void FollowPlayer()
    {
        if(coroutineRunning) return; 

        if (!isMoving)
        {
            if(currentState == CreatureState.ApproachPlayer)
            {
        
                Vector3 randomPoint;
                if(!patrolPoint) randomPoint = StructureManager.Instance.GetRandomTile();
                else randomPoint = PointAroundPatrolPoint(7);
                StartCoroutine(MoveToPoint(randomPoint, Random.Range(1f, 2.5f)));
            }
            
        }

        if(distanceFromPlayer <= 3) interruptAction = true;
    }

    void Leave()
    {
        if(coroutineRunning) return; 
        if(isStanding)
        {
            StanceSwitch(false);
            return;
        }

        if (!isMoving)
        {
            StartCoroutine(MoveToPoint(despawnPos, 20f));
            
        }
    }

    private IEnumerator WaitAround()
    {
        coroutineRunning = true;
        float r = Random.Range(1f, 3.5f);
        float timeElapsed = 0;
        agent.ResetPath();

        while(timeElapsed < r)
        {
            yield return new WaitForSeconds(0.1f);
            timeElapsed += 0.1f;
            if(playerInSightRange && currentState != CreatureState.FollowPlayer) 
            {
                if(currentState != CreatureState.ApproachPlayer || !IsPlayerStill()) timeElapsed += 0.3f;
            }

            if(!isDead && isStanding && Random.Range(0, 100) < 3)
            {
                anim.Play("UniqueIdle");
                yield return new WaitForSeconds(0.5f);
            }
            
        }
        coroutineRunning = false;
    }

    private IEnumerator MoveToPoint(Vector3 destination, float maxTime)
    {
        isMoving = true;
        //coroutineRunning = true;
        interruptAction = false;

        agent.destination = destination;

        float timeSpent = 0; //to make sure it doesnt get stuck

        while ((agent.pathPending || agent.remainingDistance > agent.stoppingDistance) && timeSpent < maxTime)
        {
            timeSpent += Time.deltaTime;

            if(interruptAction) timeSpent += 30;
            yield return null;
        }

        FinishedMoving();
    }

    void FinishedMoving()
    {

        if(currentState == CreatureState.Wander)
        {
            if(playerInSightRange)
            {
                currentState = CreatureState.Flee;
                effectsHandler.RandomIdle();
            }
            else if(Random.Range(0,10) > 6) StanceSwitch(!isStanding);
            else StartCoroutine(WaitAround());
        }

        if(currentState == CreatureState.ApproachPlayer)
        {
            if(PlayerHoldingNuts() && distanceFromPlayer <= 3)
            {
                HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
                PlayerInventoryHolder.Instance.UpdateInventory();
                currentState = CreatureState.FollowPlayer;
                timeUntilLeave = Random.Range(30, 160);
                effectsHandler.MiscSound();
            }
        }

        if(currentState == CreatureState.FollowPlayer)
        {
            if(distanceFromPlayer < 15)
            {
                StartCoroutine(WaitAround());
                if(!isStanding && Random.Range(0,10) > 3) StanceSwitch(true);
                else if(isStanding && Random.Range(0,10) > 6) StanceSwitch(false);
            }
            else if(isStanding) StanceSwitch(false);
        }

        if((currentState == CreatureState.Leave && distanceFromPlayer > 60) || (TownGate.Instance.location != PlayerLocation.InTown && TownGate.Instance.location != PlayerLocation.InFarm))
        {
            Destroy(gameObject);
        }
        isMoving = false;
        interruptAction = false;
    }

    IEnumerator IdleSoundTimer()
    {
        while(health > 0)
        {
            int i = Random.Range(4,13);
            effectsHandler.RandomIdle();
            yield return new WaitForSeconds(i);
        }
    }

    IEnumerator CheckPlayerNuts()
    {
        while(health > 0 && currentState != CreatureState.FollowPlayer && currentState != CreatureState.Leave)
        {
            yield return new WaitForSeconds(Random.Range(3, 7));
            if(distanceFromPlayer < 20 && PlayerHoldingNuts())
            {
                if(IsPlayerStill())
                {
                    currentState = CreatureState.ApproachPlayer;
                }
            }
        }
    }

    IEnumerator LeaveTimer()
    {
        while(health > 0 && currentState != CreatureState.Leave)
        {
            yield return new WaitForSeconds(1);
            --timeUntilLeave;
            if(!TimeManager.Instance.isDay) timeUntilLeave -= 10;
            if(timeUntilLeave <= 0)
            {
                if(currentState == CreatureState.FollowPlayer)
                {
                    InventoryItemData chosenItem = null;
                    int x = 0;
                    int r = 0;
                    while(!chosenItem)
                    {
                        r = Random.Range(0, gifts.Count);
                        if(Random.Range(0,100) < gifts[r].amount) chosenItem = gifts[r].item;
                        if(x > 20) chosenItem = gifts[0].item;
                    }
                    GameObject droppedItem = ItemPoolManager.Instance.GrabItem(chosenItem);
                    droppedItem.transform.position = corpseParticleTransform.position;

                    Vector3 dir3 = Random.onUnitSphere;
                    dir3 = new Vector3(dir3.x, droppedItem.transform.position.y, dir3.z);
                    Rigidbody itemRB = droppedItem.GetComponent<Rigidbody>();
                    itemRB.AddForce(dir3 * 20);
                    itemRB.AddForce(Vector3.up * 50);
                }
                currentState = CreatureState.Leave;
            }
        }
    }

    bool IsPlayerStill()
    {
        if(PlayerInteraction.Instance.rb.velocity.magnitude > 5) return false;
        else return true;
    }

    bool PlayerHoldingNuts()
    {
        if(HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData == timberEar) return true;
        else return false;
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

    void OnDestroy()
    {
        base.OnDestroy();
        if (!gameObject.scene.isLoaded) return; 
    }
}
