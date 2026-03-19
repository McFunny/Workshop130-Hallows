using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Ferrat : CreatureBehaviorScript
{

    bool isMoving, coroutineRunning; //ISMOVING TRACKS IF THE MOVEMENT COROUTINE IS PLAYING. COROUTINERUNNING CHECKS IF ANY *OTHER* COROUTINE IS RUNNING
    bool isStanding = false;

    [HideInInspector] public NavMeshAgent agent;

    private Vector3 despawnPos;

    float walkSpeed = 3.5f;
    float runSpeed = 16;

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
        agent.speed = runSpeed;

        int r = Random.Range(0, NightSpawningManager.Instance.despawnPositions.Length);
        despawnPos = NightSpawningManager.Instance.despawnPositions[r].position;
        StartCoroutine(IdleSoundTimer());
        StartCoroutine(CheckPlayerNuts());
        timeUntilLeave = Random.Range(100, 500);
        StartCoroutine(LeaveTimer());

        GrabTree();
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
                if(currentState == CreatureState.Wander && !coroutineRunning) currentState = CreatureState.Flee;
            }

            if(currentState == CreatureState.Flee && distanceFromPlayer > sightRange + 10)
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
        agent.velocity = Vector3.zero;
        yield return new WaitForSeconds(0.7f);
        if(isStanding) agent.speed = walkSpeed;
        else agent.speed = runSpeed;
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
                else randomPoint = PointAroundPatrolPoint(10);
                StartCoroutine(MoveToPoint(randomPoint, Random.Range(0.5f, 2.5f)));
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
    
                StartCoroutine(MoveToPoint(player.position, Random.Range(1f, 2.5f)));
            }
            
        }

        if(distanceFromPlayer <= 3 || !IsPlayerStill()) interruptAction = true;
    }

    private Vector3 GetRandomPointAround(Vector3 origin, float radius)
    {
        Vector2 randomDirection = Random.insideUnitCircle * radius;
        Vector3 randomPoint = new Vector3(randomDirection.x, origin.y, randomDirection.y) + origin;
        return randomPoint;
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
        if (NavMesh.SamplePosition(runTo, out var hit, 1.0f, NavMesh.AllAreas)) agent.destination = runTo;
        else if (agent.pathStatus != NavMeshPathStatus.PathComplete) agent.Move(transform.forward * agent.speed * Time.deltaTime);

    }

    void FollowPlayer()
    {
        if(coroutineRunning) return; 

        if (!isMoving)
        {
            if(currentState == CreatureState.FollowPlayer)
            {
                AchievementManager.Instance.TrackWhosFollowingPlayer();
                Vector3 randomPoint = GetRandomPointAround(player.position, 12f);
                StartCoroutine(MoveToPoint(randomPoint, Random.Range(2f, 5f)));
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
        float r = Random.Range(1f, 5.5f);
        float timeElapsed = 0;
        agent.ResetPath();
        //agent.velocity = Vector3.zero;

        bool performedExtraIdle = false;

        while(timeElapsed < r)
        {
            yield return new WaitForSeconds(0.1f);
            timeElapsed += 0.1f;
            if(playerInSightRange && currentState != CreatureState.FollowPlayer) 
            {
                if(currentState != CreatureState.ApproachPlayer || !IsPlayerStill()) timeElapsed += 0.3f;
            }

            if(!isDead && !performedExtraIdle &&  isStanding && Random.Range(0, 100) < 5)
            {
                anim.Play("UniqueIdle");
                performedExtraIdle = true;
                yield return new WaitForSeconds(1.5f);
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
                ParticlePoolManager.Instance.GrabHeartParticle().transform.position = corpseParticleTransform.position;
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
            if(distanceFromPlayer < 20 && PlayerHoldingNuts())
            {
                if(IsPlayerStill())
                {
                    currentState = CreatureState.ApproachPlayer;
                }
            }
            yield return new WaitForSeconds(Random.Range(3, 7));
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
        Vector2 moveInput = PlayerInteraction.Instance.controlManager.movement.action.ReadValue<Vector2>();
        if(moveInput.y >= 0.2f || moveInput.y <= -0.2f || moveInput.x >= 0.2f || moveInput.x <= -0.2f) return false;
        else return true;
    }

    bool PlayerHoldingNuts()
    {
        if(HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData && HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData == timberEar) return true;
        else return false;
    }

    void GrabTree()
    {
        List<StructureBehaviorScript> availableStructures = new List<StructureBehaviorScript>();
        foreach (var structure in StructureManager.Instance.allStructs)
        {
            if (structure && !structure.absentFromFarmGrid)
            {
                FarmTree tree = structure as FarmTree;
                if(!tree) continue;
                
                availableStructures.Add(tree);
            }
                
        }

        if(availableStructures.Count > 0) patrolPoint = availableStructures[Random.Range(0, availableStructures.Count)].transform;
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
