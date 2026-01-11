using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Grub : CreatureBehaviorScript
{
    public Variant variant; // what variant of creature is this?

    bool isMoving, coroutineRunning;

    [HideInInspector] public NavMeshAgent agent;

    public Transform modelPivot;

    private Vector3 despawnPos;

    public GrubSwarm homeSwarm;

    public GameObject fearObject;

    public List<StructureObject> targettableStructures;
    private StructureBehaviorScript targetStructure;

    PlayerWagonScript targetWagon; //set this to the one in wagonmanager
    Transform wagonWeakPoint;

    bool interruptAction = false;
    bool stunnedByFire, stunCooldown;

    public CropData foxgloveData;
    
    List<GameObject> nearbyLavent = new List<GameObject>();
    public ParticleSystem laventParticles;

    FireFearTrigger nearbyFire;

    public EquipEnemyArmor[] equippableArmor;

    ///////////// Miner Variables///////////
    public GameObject model;
    public ParticleSystem burrowingParticles;
    public GameObject burrow;
    Vector3 emergePoint;
    bool burrowDelay = true;

    public enum CreatureState
    {
        Idle,
        Wander,
        AttackStructure,
        Stun,
        AttackWagon, //Wilderness only
        Burrowing //Miner variant behavior
    }

    public CreatureState currentState;

    public enum Variant
    {
        Normal,
        Miner
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        base.Start();
        
        agent.enabled = false;
        agent.enabled = true;

        int r = Random.Range(0, NightSpawningManager.Instance.despawnPositions.Length);
        despawnPos = NightSpawningManager.Instance.despawnPositions[r].position;
        StartCoroutine(IdleSoundTimer());

        if(variant == Variant.Miner)
        {
            StartCoroutine(BurrowDelay());
            model.SetActive(false);
            burrowingParticles.Play();
            currentState = CreatureState.Burrowing;
            agent.enabled = false;
        }
        if(!inWilderness && Random.Range(0,10) > 2)
        {
            Transform burrowPos = StructureManager.Instance.FindBurrow(false, transform.position);
            if(burrowPos != null)
            {
                transform.position = burrowPos.position;
                ParticlePoolManager.Instance.GrabDirtPixelParticle().transform.position = transform.position;
            }
        }

        if(inWilderness)
        {
            targetWagon = WagonManager.Instance.wildernessWagon;
            wagonWeakPoint = targetWagon.GetWeakPoint();
            currentState = CreatureState.AttackWagon;
        }

        StartCoroutine(ScanForTargets());
        StartCoroutine(LaventEffects());

        agent.speed += Random.Range(-0.5f, 0f);

        for(int i = 0; i < equippableArmor.Length; i++)
        {
            r = Random.Range(0,100);
            if(equippableArmor[i].chanceToEquip >= r) equippableArmor[i].armorObject.SetActive(true);
        }
    }

    void Update()
    {
        if (health <= 0) isDead = true;

        if(agent.velocity.magnitude > 1) anim.SetBool("IsWalking", true);
        else anim.SetBool("IsWalking", false);

        if (!isDead && currentState != CreatureState.Stun && !stunnedByFire)
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

            case CreatureState.Idle:
                Idle();
                break;

            case CreatureState.AttackStructure:
                Wander();
                break;

            case CreatureState.Stun:
                break;

            case CreatureState.AttackWagon:
                Wander();
                break;

            case CreatureState.Burrowing:
                Burrowing();
                break;

            default:
                Debug.LogError("Unknown state: " + currentState);
                break;
        }
    }

    private void Idle()
    {
        print("Idled");
        if (!coroutineRunning)
        {
            StartCoroutine(WaitAround());
        }
    }

    void Wander()
    {
        if(currentState == CreatureState.Wander && targetStructure)
        {
            currentState = CreatureState.AttackStructure;
        }

        if(currentState == CreatureState.Wander && targetWagon)
        {
            currentState = CreatureState.AttackWagon;
        }

        if(CheckForObstacle(transform) != null)
        {
            StructureBehaviorScript obstacle = CheckForObstacle(transform);
            if(targettableStructures.Contains(obstacle.structData) && targetStructure != obstacle)
            {
                targetStructure = obstacle;
                if(currentState == CreatureState.Wander)
                {
                    currentState = CreatureState.AttackStructure;
                    interruptAction = true;
                }
            }
        }

        if (!isMoving && !coroutineRunning)
        {
            if(currentState == CreatureState.Wander) //Wandering
            {
                Vector3 randomPoint;
                if(!patrolPoint) randomPoint = StructureManager.Instance.GetRandomTile();
                else randomPoint = PointAroundPatrolPoint(7);
                StartCoroutine(MoveToPoint(randomPoint, Random.Range(4f, 7f)));
            }

            else if(currentState == CreatureState.AttackStructure) ///Attacking Structure
            {
                if(!targetStructure)
                {
                    targetStructure = null;
                    currentState = CreatureState.Wander;
                    return;
                }
                StartCoroutine(MoveToPoint(targetStructure.transform.position, 3));

                if(Vector3.Distance(transform.position, targetStructure.transform.position) < 1.8f) interruptAction = true;
            }

            else if(currentState == CreatureState.AttackWagon) ///Attacking Wagon
            {
                StartCoroutine(MoveToPoint(wagonWeakPoint.position, 5));

                if(Vector3.Distance(transform.position, wagonWeakPoint.position) < 2f) interruptAction = true;
            }
            
        }
    }

    IEnumerator BurrowDelay()
    {
        yield return new WaitForSeconds(5);
        burrowDelay = false;
    }

    void Burrowing()
    {
        if(coroutineRunning || burrowDelay) return;

        if(emergePoint == Vector3.zero)
        {
            emergePoint = StructureManager.Instance.FindFreeTileNearCrop();
            if(emergePoint == Vector3.zero)
            { 
                coroutineRunning = false;
                StartCoroutine(Emerge(false));
            }
        }

        //move to position
        Vector3 direction = emergePoint - transform.position;
        float distance = direction.magnitude;

        // Normalize direction and move
        Vector3 moveStep = direction.normalized * 2 * Time.deltaTime;

        transform.position = Vector3.MoveTowards(transform.position, emergePoint, 2 * Time.deltaTime);

        if (Vector3.Distance(emergePoint, transform.position) < 0.5f)
        {
            if(StructureManager.Instance.CheckTile(emergePoint) == Vector3.zero) //tile got covered
            {
                emergePoint = Vector3.zero;
                return;
            }
            coroutineRunning = true;
            StartCoroutine(Emerge(true));
        }
    }

    IEnumerator Emerge(bool spawnBurrow)
    {
        yield return new WaitForSeconds(Random.Range(0.2f, 1.3f));
        coroutineRunning = true;
        model.SetActive(true);
        burrowingParticles.Stop();
        ParticlePoolManager.Instance.GrabDirtPixelParticle().transform.position = transform.position;
        if(spawnBurrow)
        {
            Vector3 burrowSpawn = StructureManager.Instance.GetTileCenter(emergePoint);
            if(burrowSpawn != Vector3.zero) StructureManager.Instance.SpawnStructure(burrow, burrowSpawn);
        }
        yield return new WaitForSeconds(1);
        agent.enabled = true;
        FindNearbyStructure(5);
        currentState = CreatureState.Wander;
        coroutineRunning = false;
    }

    private IEnumerator WaitAround()
    {
        coroutineRunning = true;
        float r = Random.Range(1f, 2f);
        agent.ResetPath();
        yield return new WaitForSeconds(r);
        if(currentState == CreatureState.Idle)
        {
            if(targetStructure) currentState = CreatureState.AttackStructure;
            currentState = CreatureState.Wander;
        }
        coroutineRunning = false;
        print("I finished Idling");
    }

    private IEnumerator MoveToPoint(Vector3 destination, float maxTime)
    {
        isMoving = true;
        coroutineRunning = true;
        interruptAction = false;

        if (TimeManager.Instance.isDay && !inWilderness) destination = despawnPos;

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
        if (currentState == CreatureState.Wander)
        {
            currentState = CreatureState.Idle;
        }

        if(currentState == CreatureState.AttackStructure && targetStructure)
        {
            if(Vector3.Distance(targetStructure.transform.position, transform.position) < 2.2f)
            {
                agent.Stop();
                StartCoroutine(AttackCoolDown());
                coroutineRunning = true;
                return;
            }
        }

        if(currentState == CreatureState.AttackWagon)
        {
            if(Vector3.Distance(wagonWeakPoint.position, transform.position) < 3f)
            {
                agent.Stop();
                StartCoroutine(AttackCoolDown());
                coroutineRunning = true;
                return;
            }
        }

        isMoving = false;
        coroutineRunning = false;
        interruptAction = false;
    }

    IEnumerator IdleSoundTimer()
    {
        while(health > 0)
        {
            if(currentState == CreatureState.Burrowing)
            {
                yield return new WaitForSeconds(5);
                continue;
            }
            int i = Random.Range(3,6);
            effectsHandler.RandomIdle();
            yield return new WaitForSeconds(i);
        }
    }

    IEnumerator AttackCoolDown()
    {
        anim.Play("GrubAttack");
        yield return new WaitForSeconds(0.2f);
        if(targetStructure)
        {
            FarmLand tile = targetStructure as FarmLand;
            if(tile && tile.crop && tile.crop == foxgloveData && Random.Range(0,5) > 2)
            {
                TakeDamage(99);
                //Destroy(this.gameObject);
                yield break;
            }

            HitStructureParticle(targetStructure.transform.position);
            targetStructure.TakeDamage(damageToStructure);
            effectsHandler.MiscSound();
            ParticlePoolManager.Instance.GrabDirtPixelParticle().transform.position = transform.position;
        }
        else if(targetWagon)
        {
            effectsHandler.MiscSound();
            ParticlePoolManager.Instance.GrabDirtPixelParticle().transform.position = transform.position;
            targetWagon.TakeWagonDamage(damageToStructure);
        }
        yield return new WaitForSeconds(Random.Range(2.5f, 4f));
        if(stunCooldown) yield return new WaitForSeconds(Random.Range(3f, 5f));
        agent.Resume();
        isMoving = false;
        coroutineRunning = false;
        interruptAction = false;
    }

    IEnumerator ScanForTargets()
    {
        while(health > 0)
        {
            yield return new WaitForSeconds(5);
            if(targetStructure) continue;

            if(homeSwarm && homeSwarm.swarmTargets.Count > 0)
            {
                // Specifically for grubs without a swarm
                //Grab a random target from the swarm
                targetStructure = homeSwarm.swarmTargets[Random.Range(0, homeSwarm.swarmTargets.Count)];
                continue;
            }

            FindNearbyStructure(40);
        }
    }

    void FindNearbyStructure(float distance)
    {
        float distanceToStructure;

        List<StructureBehaviorScript> availableStructure = new List<StructureBehaviorScript>();
        foreach (var structure in structManager.allStructs)
        {
            if(!structure) continue;
            FarmLand tile = structure as FarmLand;
            distanceToStructure = Vector3.Distance(transform.position, structure.transform.position);
            if (targettableStructures.Contains(structure.structData) && !structure.absentFromFarmGrid && distanceToStructure < distance && 
            (!tile || (tile.crop && !tile.isWeed && tile.currentUpgrade != FarmLand.FarmTileUpgrade.Corrupt)))
            {
                if(!tile && Random.Range(0,4) == 0) continue;
                availableStructure.Add(structure);
            }
        }

        if (availableStructure.Count > 0)
        {
            int r = Random.Range(0, availableStructure.Count);
            targetStructure = availableStructure[r];
        }
    }

    IEnumerator FireStun()
    {
        stunnedByFire = true;
        stunCooldown = true;
        float oldSpeed = agent.speed;
        agent.speed = 0;
        fearObject.SetActive(true);
        anim.Play("GrubStun");
        effectsHandler.MiscSound2();
        yield return new WaitForSeconds(5f);
        agent.speed = oldSpeed - 1.5f;
        stunnedByFire = false;
        yield return new WaitForSeconds(4f);

        while(nearbyFire && nearbyFire.gameObject.activeSelf && Vector3.Distance(transform.position, nearbyFire.transform.position) < nearbyFire.fleeRange)
        {
            yield return new WaitForSeconds(0.5f);
        }
        fearObject.SetActive(false);
        agent.speed += 1.5f;
        stunCooldown = false;
    }

    public override bool OnStun(float duration) // For the resin pole trap
    {
        if (currentState != CreatureState.Stun)
        {
            currentState = CreatureState.Stun;
            agent.enabled = false;
            agent.speed = 0;
            modelPivot.up = Vector3.up;
            return true;
        }
        return false;
    }

    public override void NewPriorityTarget(StructureBehaviorScript newStruct) //Also for when Swarm assigned a new target structure
    {
        if (currentState == CreatureState.Stun || currentState == CreatureState.Burrowing) return;
        interruptAction = true;
        targetStructure = newStruct;
    }

    public override void EnteredFireRadius(FireFearTrigger _fireSource, out bool successful)
    {
        successful = false;
        if(stunCooldown || currentState == CreatureState.Burrowing) return;
        nearbyFire = _fireSource;
        StartCoroutine(FireStun());
        successful = true;
    }

    public override void NearLaventLeaf(GameObject laventObject)
    {
        if(!nearbyLavent.Contains(laventObject))
        {
            nearbyLavent.Add(laventObject);
        }
        /*
        if(currentState == CreatureState.Flee) return;
        fearedObjectPosition = pos;
        fleeToPos = transform.position + ((transform.position - fearedObjectPosition + new Vector3(Random.Range(-3, 3), 0, Random.Range(-3, 3)) * 8));
        agent.destination = fleeToPos;
        fearObject.SetActive(true);
        */
    }

    IEnumerator LaventEffects()
    {
        while(health > 0)
        {
            yield return new WaitForSeconds(1);
            for(int i = 0; i < nearbyLavent.Count; i++)
            {
                if(!nearbyLavent[i] || nearbyLavent[i].activeSelf == false)
                {
                    nearbyLavent.RemoveAt(i);
                    i--;
                }
                else
                {
                    TakeDamage(2);
                    effectsHandler.MiscSound3();
                    laventParticles.Play();
                }
            }
        }
    }

    void OnDestroy()
    {
        base.OnDestroy();
        if (!gameObject.scene.isLoaded) return; 

        if(homeSwarm) homeSwarm.grubs.Remove(gameObject);
    }

}
