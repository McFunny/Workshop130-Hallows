using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Pollinator : CreatureBehaviorScript
{
    private bool isMoving = false;
    private bool coroutineRunning = false;
    private Transform target;
    public List<StructureObject> targettableStructures;
    private Vector3 despawnPos;
    [HideInInspector] public NavMeshAgent agent;

    float pollenDistance = 2;

    private StructureBehaviorScript targetStructure; //The thing they will seek out to pollinate like crops. NOT a brazier

    public List<FireFearTrigger> fireSources; //find out which one is the player torch; they will prioritize following this one
    int currentFirePriority = 0;

    public enum CreatureState
    {
        SpawnIn,
        Wander,
        WanderByFire,
        WalkTowardsTarget,
    }
    public CreatureState currentState;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        int r = Random.Range(0, NightSpawningManager.Instance.despawnPositions.Length);
        despawnPos = NightSpawningManager.Instance.despawnPositions[r].position;

        StartCoroutine(RefreshTarget());
        target = null;
    }

    void Update()
    {
        if(coroutineRunning) return;
        if(target)
        {
            if(targetStructure)  currentState = CreatureState.WalkTowardsTarget;
            else  currentState = CreatureState.WanderByFire;
        }
        else if(currentState != CreatureState.SpawnIn) currentState = CreatureState.Wander;

        CheckState(currentState);
    }

    public void CheckState(CreatureState currentState)
    {
        switch (currentState)
        {
            case CreatureState.SpawnIn:
                SpawnIn();
                break;
            case CreatureState.Wander:
                Wander();
                break;
            case CreatureState.WanderByFire:
                WanderByFire();
                break;

            case CreatureState.WalkTowardsTarget:
                WalkTowardsClosestTarget();
                break;

            default:
                Debug.LogError("Unknown state: " + currentState);
                break;
        }
    }

    void SpawnIn()
    {
        if (!isMoving)
        {
            StartCoroutine(MoveToPoint(StructureManager.Instance.GetRandomTile()));
        }
    }

    void Wander()
    {
        if (!isMoving && currentState == CreatureState.Wander)
        {
            Vector3 randomPoint = GetRandomPointAround(transform.position, 3f);
            StartCoroutine(MoveToPoint(randomPoint));
        }
    }

    void WanderByFire()
    {
        if (!isMoving && currentState == CreatureState.WanderByFire)
        {
            Vector3 randomPoint = GetRandomPointAround(target.position, 5f);
            StartCoroutine(MoveToPoint(randomPoint));
        }
    }

    private Vector3 GetRandomPointAround(Vector3 origin, float radius)
    {
        Vector2 randomDirection = Random.insideUnitCircle * radius;
        Vector3 randomPoint = new Vector3(randomDirection.x, origin.y, randomDirection.y) + origin;
        return randomPoint;
    }

    private IEnumerator MoveToPoint(Vector3 destination)
    {
        isMoving = true;
        coroutineRunning = true;

        if (TimeManager.Instance.isDay) destination = despawnPos;

        agent.destination = destination;

        float timeSpent = 0; //to make sure it doesnt get stuck
        float maxTime = Random.Range(5, 8);
        if(currentState == CreatureState.SpawnIn) maxTime = 15;
        //else if(currentState == CreatureState.WanderByFire) maxTime = 3;

        while (timeSpent < maxTime)
        {
            if(target && currentState == CreatureState.Wander) timeSpent += 25; //if nearbyfire

            if(currentState == CreatureState.WanderByFire && (target && Vector3.Distance(target.position, transform.position) > 7 || !target))
            {
                timeSpent += 25;
            }

            if(currentState == CreatureState.SpawnIn && target)
            {
                timeSpent += 25;
            }

            timeSpent += Time.deltaTime;
            yield return null;
        }
        print("Done Moving");

        if(target)
        {
            if(targetStructure)  currentState = CreatureState.WalkTowardsTarget;
            else  currentState = CreatureState.WanderByFire;
        }
        else currentState = CreatureState.Wander;

        isMoving = false;
        coroutineRunning = false;
    }

    void WalkTowardsClosestTarget()
    {
        if(coroutineRunning) return;
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            currentState = CreatureState.Wander;
        }
        else if (Vector3.Distance(transform.position, target.transform.position) < pollenDistance)//(!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 1f)
        {
            agent.ResetPath();
            coroutineRunning = true;
            StartCoroutine(PollenateStructure());
        }

        if(target && agent.destination != target.position)
        {
            agent.destination = target.position;
        }
    }

    IEnumerator PollenateStructure()
    {
        yield return new WaitForSeconds(Random.Range(1,4));
        if(!targetStructure) yield break;
        FarmLand tile = targetStructure as FarmLand;
        if(tile)
        {
            tile.isPollinated = true;
        }
        targetStructure = null;
        target = null;
        currentState = CreatureState.Wander;
        coroutineRunning = false;
    }

    IEnumerator RefreshTarget()
    {
        while(true)
        {
            yield return new WaitForSeconds(1.5f);

            if(!targetStructure)
            {
                Collider[] nearbyStructures = Physics.OverlapSphere(transform.position, 8f, 1 << 6);
                foreach(Collider collider in nearbyStructures)
                {
                    StructureBehaviorScript structure = collider.gameObject.GetComponentInParent<StructureBehaviorScript>();
                    if(structure)
                    {
                        FarmLand tile = structure as FarmLand;

                        if(tile && tile.NeedsPollenation() && Random.Range(0, 10) > 2)
                        {
                            targetStructure = structure;
                            target = structure.transform;
                            break;
                        }
                    }
                }
            }

            //Find closest fire, or find the fire held by player
            if(fireSources.Count > 0)
            {
                float distFromFire = 0;
                float minDistance = 15;

                if(!target) currentFirePriority = 0;
                for(int i = 0; i < fireSources.Count; i++)
                {
                    if(!fireSources[i] || fireSources[i].gameObject.activeInHierarchy == false) //remove fire if its deactivated
                    {
                        if(target && target == fireSources[i].transform) target = null;
                        fireSources.RemoveAt(i);
                        i--;
                        continue;
                    }
                    if(targetStructure || fireSources[i].priority < currentFirePriority) continue; //if we already have a structure or if the current fire's priority is higher, ignore entry

                    distFromFire = Vector3.Distance(fireSources[i].transform.position, transform.position);

                    if(distFromFire < minDistance) //if this is the closest fire its the new target
                    {
                        target = fireSources[i].transform;
                        currentFirePriority = fireSources[i].priority;
                        minDistance = distFromFire;
                    }
                }

                if(target && !targetStructure) //if the target is fire and is too far away, remove the target
                {
                    distFromFire = Vector3.Distance(target.position, transform.position);
                    if(distFromFire > 15) target = null;
                } 
            }
        }
    }

    public override void EnteredFireRadius(FireFearTrigger _fireSource, out bool successful)
    {
        if(!fireSources.Contains(_fireSource))
        {
            fireSources.Add(_fireSource);
            successful = true;
        }
        else successful = false;
    }
}
