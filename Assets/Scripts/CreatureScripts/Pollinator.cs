using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Pollinator : CreatureBehaviorScript
{
    public InventoryItemData bugItem;

    private bool isMoving = false;
    private bool coroutineRunning = false;
    private Transform target;
    //public List<StructureObject> targettableStructures;
    private Vector3 despawnPos;
    [HideInInspector] public NavMeshAgent agent;

    float pollenDistance = 2;

    float defaultSpeed = 3;
    float fireSpeed = 9f;

    private StructureBehaviorScript targetStructure; //The thing they will seek out to pollinate like crops. NOT a brazier

    public List<FireFearTrigger> fireSources; //find out which one is the player torch; they will prioritize following this one
    int currentFirePriority = 0;

    public List<ParticleSystem> pollenParticles;

    public GameObject fearObject; //The particle system
    Vector3 fearedObjectPosition; //Where the lavent leaf is
    Vector3 fleeToPos; //Where its fleeing to

    public GameObject bugModel;
    Vector3 startPos;

    int amountPollinated;

    public GameObject dewBall;

    public enum CreatureState
    {
        SpawnIn,
        Wander,
        WanderByFire,
        WalkTowardsTarget,
        Flee,
        Stun
    }
    public CreatureState currentState;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        startPos = bugModel.transform.position;
    }

    void Start()
    {
        int r = Random.Range(0, NightSpawningManager.Instance.despawnPositions.Length);
        despawnPos = NightSpawningManager.Instance.despawnPositions[r].position;

        StartCoroutine(RefreshTarget());
        target = null;

        base.Start();

        int dewChance = 2;
        if(MainMenuScript.currentFileMode == FileMode.Cozy) ++dewChance;
        if(Random.Range(0,10) < dewChance) dewBall.SetActive(true);
    }

    void Update()
    {
        if(currentState == CreatureState.Stun) return;
        Flutter();

        if(currentState != CreatureState.WanderByFire) agent.speed = defaultSpeed;
        else agent.speed = fireSpeed;

        if(coroutineRunning) return;
        if(currentState != CreatureState.Flee)
        {
            if(TimeManager.Instance.isDay) currentState = CreatureState.Wander;
            else if(target)
            {
                if(targetStructure)  currentState = CreatureState.WalkTowardsTarget;
                else  currentState = CreatureState.WanderByFire;
            }
            else if(currentState != CreatureState.SpawnIn) currentState = CreatureState.Wander;
        }
    

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
            case CreatureState.Flee:
                Flee();
                break;
            case CreatureState.Stun:
                //
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
            List<Vector3> flowerPos = new List<Vector3>();
            foreach(StructureBehaviorScript s in StructureManager.Instance.allStructs)
            {
                FarmLand tile = s as FarmLand;
                if(tile && tile.crop && tile.crop.id == 20)
                {
                    flowerPos.Add(tile.transform.position);
                    continue;
                }

                CandleCluster candle = s as CandleCluster;
                if(candle && candle.type == CandleType.Aroma && candle.burning)
                {
                    flowerPos.Add(tile.transform.position);
                    continue;
                }
            }

            if(flowerPos.Count > 0) StartCoroutine(MoveToPoint(flowerPos[Random.Range(0, flowerPos.Count)]));
            else StartCoroutine(MoveToPoint(StructureManager.Instance.GetRandomTile()));
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

        if(agent.enabled) agent.destination = destination;

        float timeSpent = 0; //to make sure it doesnt get stuck
        float maxTime = Random.Range(5, 8);
        if(currentState == CreatureState.SpawnIn) maxTime = 20;
        //else if(currentState == CreatureState.WanderByFire) maxTime = 3;

        while (timeSpent < maxTime)
        {
            if(target && currentState == CreatureState.Wander) timeSpent += 25; //if nearbyfire

            if(currentState == CreatureState.WanderByFire && (target && Vector3.Distance(target.position, transform.position) > 5 || !target))
            {
                timeSpent += 25;
            }

            if(currentState == CreatureState.SpawnIn && target)
            {
                timeSpent += 25;
            }

            if(fearedObjectPosition != Vector3.zero) timeSpent += 25;

            

            timeSpent += Time.deltaTime;
            yield return null;
        }
        print("Done Moving");

        if(fearedObjectPosition != Vector3.zero)
        {
            currentState = CreatureState.Flee;
            isMoving = false;
            coroutineRunning = false;
            yield break;
        }

        if(target)
        {
            if(targetStructure)  currentState = CreatureState.WalkTowardsTarget;
            else  currentState = CreatureState.WanderByFire;
        }
        else if(currentState != CreatureState.Stun) currentState = CreatureState.Wander;

        isMoving = false;
        coroutineRunning = false;
    }

    void WalkTowardsClosestTarget()
    {
        if(coroutineRunning) return;
        if (target == null || !target.gameObject.activeInHierarchy || TimeManager.Instance.isDay)
        {
            currentState = CreatureState.Wander;
        }
        else if (Vector3.Distance(transform.position, target.transform.position) < pollenDistance)//(!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 1f)
        {
            agent.ResetPath();
            coroutineRunning = true;
            StartCoroutine(PollenateStructure());
        }

        if(target && agent.destination != target.position && agent.enabled)
        {
            agent.destination = target.position;
        }
    }

    void Flee()
    {
        if(Vector3.Distance(transform.position, fearedObjectPosition) > 8)
        {
            fearedObjectPosition = Vector3.zero;
            targetStructure = null;
            currentState = CreatureState.Wander;
            fearObject.SetActive(false);
            return;
        }
    }

    IEnumerator PollenateStructure()
    {
        yield return new WaitForSeconds(Random.Range(1,4));
        if(!targetStructure) yield break;
        FarmLand tile = targetStructure as FarmLand;
        if(tile)
        {
            tile.Pollinate();
            ++amountPollinated;
            AchievementManager.Instance.NotifyCropPollinated();
            if(amountPollinated >= 20) AchievementManager.Instance.CompleteProgressWithEnum(ACHKey.Lumen_Pollinate_Many);
            foreach(ParticleSystem p in pollenParticles) p.Play();
            QuestManager.Instance.AddQuestProgress(1, QuestDatabase.Instance.GetTutorialQuest(301)); //Complete the pollination quest

            if(tile.crop && tile.crop == CropDatabase.Instance.GetCrop(49)) dewBall.SetActive(true);
        }
        else
        {
            PollinatorPost post = targetStructure as PollinatorPost;
            if(post && !post.containsMoth)
            {
                post.InsertMoth();
                Destroy(gameObject);
                yield break;
            }
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

            if(fearedObjectPosition != Vector3.zero) continue;

            if(!targetStructure)
            {
                Collider[] nearbyStructures = Physics.OverlapSphere(transform.position, 8f, 1 << 6);
                foreach(Collider collider in nearbyStructures)
                {
                    StructureBehaviorScript structure = collider.gameObject.GetComponentInParent<StructureBehaviorScript>();
                    if(structure)
                    {
                        FarmLand tile = structure as FarmLand;

                        if(tile && tile.NeedsPollination() && Random.Range(0, 10) > 2)
                        {
                            targetStructure = structure;
                            target = structure.transform;
                            break;
                        }

                        PollinatorPost post = structure as PollinatorPost;
                        if(post && !post.containsMoth && post.flowerHealth > 0)
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
                float minDistance = 25;

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
                    if(distFromFire > minDistance) target = null;
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

    public override void NearLaventLeaf(GameObject laventObject)
    {
        if(currentState == CreatureState.Flee) return;
        fearedObjectPosition = laventObject.transform.position;
        fleeToPos = transform.position + ((transform.position - fearedObjectPosition + new Vector3(Random.Range(-3, 3), 0, Random.Range(-3, 3)) * 8));
        agent.destination = fleeToPos;
        fearObject.SetActive(true);
    }

    protected void Flutter()
    {
        float newY = Mathf.Sin(Time.time * 3) * 0.2f; //Last number is the height
        bugModel.transform.position = new Vector3(transform.position.x, startPos.y + newY, transform.position.z);
    }

    public override bool CaughtByBugNet(out InventoryItemData item)
    {
        item = bugItem;

        return true;
    }

    public override bool OnStun(float duration) // For the resin pole trap
    {
        if (currentState != CreatureState.Stun)
        {
            currentState = CreatureState.Stun;
            //agent.Stop();
            agent.enabled = false;
            anim.Play("StuckIdle");
            return true;
        }
        return false;
    }

    public override void NewPriorityTarget(StructureBehaviorScript newStruct)
    {
        if (currentState == CreatureState.Stun || fireSources.Count > 0) return;
        if(targetStructure) return;
        targetStructure = newStruct;
        target = newStruct.transform;
    }
}
