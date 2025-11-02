using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Grub : CreatureBehaviorScript
{
    bool isMoving, coroutineRunning;

    [HideInInspector] public NavMeshAgent agent;

    public Transform modelPivot;

    private Vector3 despawnPos;

    public GrubSwarm homeSwarm;

    public List<StructureObject> targettableStructures;
    private StructureBehaviorScript targetStructure;

    bool interruptAction = false;

    public enum CreatureState
    {
        Idle,
        Wander,
        AttackStructure,
        Stun
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

        if(!inWilderness && Random.Range(0,10) > 2)
        {
            Transform burrowPos = StructureManager.Instance.FindBurrow(false, transform.position);
            if(burrowPos != null)
            {
                transform.position = burrowPos.position;
                ParticlePoolManager.Instance.GrabDirtPixelParticle().transform.position = transform.position;
            }
        }

        StartCoroutine(ScanForTargets());
    }

    void Update()
    {
        if (health <= 0) isDead = true;

        if(agent.velocity.magnitude > 1) anim.SetBool("IsWalking", true);
        else anim.SetBool("IsWalking", false);

        if (!isDead && currentState != CreatureState.Stun)
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

            default:
                Debug.LogError("Unknown state: " + currentState);
                break;
        }
    }

    private void Idle()
    {

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
            if(currentState == CreatureState.Wander)
            {
                Vector3 randomPoint;
                if(!patrolPoint) randomPoint = StructureManager.Instance.GetRandomTile();
                else randomPoint = PointAroundPatrolPoint(7);
                StartCoroutine(MoveToPoint(randomPoint, 6));
            }
            else if(currentState == CreatureState.AttackStructure)
            {
                if(!targetStructure)
                {
                    targetStructure = null;
                    currentState = CreatureState.Wander;
                    return;
                }
                StartCoroutine(MoveToPoint(targetStructure.transform.position, 2));
            }
            
        }
    }

    private IEnumerator WaitAround()
    {
        coroutineRunning = true;
        float r = Random.Range(1f, 2f);
        yield return new WaitForSeconds(r);
        if(currentState == CreatureState.Idle)
        {
            if(targetStructure) currentState = CreatureState.AttackStructure;
            currentState = CreatureState.Wander;
        }
        coroutineRunning = false;
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
                HitStructureParticle(targetStructure.transform.position);
                targetStructure.TakeDamage(damageToStructure);
                effectsHandler.MiscSound();
                anim.Play("GrubAttack");
                ParticlePoolManager.Instance.GrabDirtPixelParticle().transform.position = transform.position;

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
            int i = Random.Range(3,6);
            effectsHandler.RandomIdle();
            yield return new WaitForSeconds(i);
        }
    }

    IEnumerator AttackCoolDown()
    {
        yield return new WaitForSeconds(Random.Range(1.5f, 2.5f));
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

            if(homeSwarm)
            {
                // Specifically for grubs without a swarm
                //Grab a random target from the swarm
                targetStructure = homeSwarm.swarmTargets[Random.Range(0, homeSwarm.swarmTargets.Count)];
                continue;
            }

            float closestDistance = 60;

            float distanceToStructure;

            List<StructureBehaviorScript> availableStructure = new List<StructureBehaviorScript>();
            foreach (var structure in structManager.allStructs)
            {
                FarmLand tile = structure as FarmLand;
                distanceToStructure = Vector3.Distance(transform.position, structure.transform.position);
                if (targettableStructures.Contains(structure.structData) && !structure.absentFromFarmGrid && distanceToStructure < closestDistance && (!tile || (tile.crop && !tile.isWeed)))
                {
                    availableStructure.Add(structure);
                    closestDistance = distanceToStructure;
                }
            }

            if (availableStructure.Count > 0)
            {
                int r = Random.Range(0, availableStructure.Count);
                targetStructure = availableStructure[r];
            }
        }
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
        if (currentState == CreatureState.Stun) return;
        interruptAction = true;
        targetStructure = newStruct;
    }

    void OnDestroy()
    {
        base.OnDestroy();
        if (!gameObject.scene.isLoaded) return; 
    }

}
