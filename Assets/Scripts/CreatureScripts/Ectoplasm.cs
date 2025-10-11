using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;

public class Ectoplasm : CreatureBehaviorScript
{
    public bool isLarge;

    bool isMoving, coroutineRunning;

    [HideInInspector] public NavMeshAgent agent;

    private Vector3 despawnPos;
    public Vector3 mergePoint; //Where the slime is meeting another slime to merge
    public bool mergeParent = false; //Dictates which slime spawns the big one
    Ectoplasm mergePartner;

    public List<StructureObject> targettableStructures;
    private StructureBehaviorScript targetStructure;

    public int debrisLeft = 0;
    public GameObject debrisObject;

    public Transform model; //Punch effect when it attacks player/structure/gets hit
    bool isTweening = false;
    Vector3 originalScale;
    Tween movementTween;

    bool interruptAction = false;

    public GameObject smallSlimePrefab, largeSlimePrefab, harePrefab;

    public GameObject bunnyObject;

    public enum CreatureState
    {
        Idle,
        Wander, //Wanders aimlessly. Randomly it should seek out something to target nearby
        Merge, //Meets with another slime to fuse into the larger version
        AttackStructure, //Absorbs it over time dealing damage. Small targets mini structs and crops, large targets bigger structures
        AttackPlayer, //Shooting projectiles
        Stun
    }

    public CreatureState currentState;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        originalScale = model.localScale;
        base.Start();
        
        agent.enabled = false;
        agent.enabled = true;

        int r = Random.Range(0, NightSpawningManager.Instance.despawnPositions.Length);
        despawnPos = NightSpawningManager.Instance.despawnPositions[r].position;
        StartCoroutine(IdleSoundTimer());

        StartCoroutine(MovingJiggle());
        StartCoroutine(ScanForTargets());

        if(isLarge && Random.Range(0, 10) >= 7) bunnyObject.SetActive(true);
    }

    void Update()
    {
        if (health <= 0) isDead = true;

        if (!isDead && currentState != CreatureState.Stun)
        {

            float distance = Vector3.Distance(player.position, transform.position);
            playerInAttackRange = distance <= attackRange;

            if (playerInAttackRange && currentState != CreatureState.AttackPlayer && debrisLeft > 0)
            {
                currentState = CreatureState.AttackPlayer;
            }

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

            case CreatureState.Merge:
                Merge();
                break;

            case CreatureState.AttackStructure:
                Wander();
                break;

            case CreatureState.AttackPlayer:
                //AttackPlayer();
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
        if (playerInSightRange && debrisLeft > 0)
        {
            currentState = CreatureState.AttackPlayer;
            return;
        }

        if (!coroutineRunning)
        {
            StartCoroutine(WaitAround());
        }
    }

    void Wander()
    {
        /*if (playerInAttackRange && debrisLeft > 0)
        {
            currentState = CreatureState.AttackPlayer;
            return;
        }*/

        if(currentState == CreatureState.Wander && targetStructure)
        {
            currentState = CreatureState.AttackStructure;
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
                StartCoroutine(MoveToPoint(targetStructure.transform.position, 6));
            }
            
        }
    }

    void Merge()
    {
        if(isLarge)
        {
            currentState = CreatureState.Wander;
            return;
        }

        if(mergePoint == Vector3.zero)
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, 60, 1 << 9);
            foreach (Collider collider in hitColliders)
            {
                Ectoplasm slime = collider.gameObject.GetComponentInParent<Ectoplasm>();
                if(slime && slime != this && !slime.isLarge && slime.mergePoint == Vector3.zero)
                {
                    mergePoint = transform.position;
                    slime.mergePoint = transform.position;
                    slime.mergePartner = this;
                    slime.mergeParent = false;
                    mergePartner = slime;
                    mergeParent = true;
                    StartCoroutine(MoveToPoint(mergePoint, 10));
                    return;
                }
            }
            currentState = CreatureState.Wander;
            return;
        }

        if(mergeParent)
        {
            if(mergePartner == null)
            {
                mergeParent = false;
                mergePoint = Vector3.zero;
                currentState = CreatureState.Wander;
            }
            else if(mergePartner.mergeParent) mergePartner.mergeParent = false;
            return;
        }
        else if(!isMoving && !coroutineRunning)
        {
            StartCoroutine(MoveToPoint(mergePoint, 10));
        }
        else if(!mergePartner) interruptAction = true;
    }

    private IEnumerator WaitAround()
    {
        coroutineRunning = true;
        float r = Random.Range(1f, 2f);
        yield return new WaitForSeconds(r);
        if(currentState == CreatureState.Idle)
        {
            if(Random.Range(0, 10) > 5 && !isLarge) currentState = CreatureState.Merge;
            else currentState = CreatureState.Wander;
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

        if(currentState == CreatureState.Merge)
        {
            if(!mergePartner)
            {
                currentState = CreatureState.Wander;
                mergeParent = false;
                mergePoint = Vector3.zero;
            }
            else if(Vector3.Distance(mergePartner.transform.position, transform.position) < 3)
            {
                Destroy(mergePartner.gameObject);
                Instantiate(largeSlimePrefab, transform.position, Quaternion.identity);
                AudioPoolManager.Instance.PlayClipAtPosition(effectsHandler.deathSound, transform.position);
                ParticlePoolManager.Instance.GrabCorpseParticle(corpseType).transform.position = corpseParticleTransform.position;
                Destroy(gameObject);
            }
        }

        if(currentState == CreatureState.AttackStructure && targetStructure)
        {
            if(Vector3.Distance(targetStructure.transform.position, transform.position) < 2f)
            {
                HitStructureParticle(targetStructure.transform.position);
                targetStructure.TakeDamage(damageToStructure);
                effectsHandler.MiscSound();
                if(!isTweening)
                {
                    StartCoroutine(Jiggle());
                }
                StartCoroutine(AttackCoolDown());
                coroutineRunning = true;

                if(!targetStructure || targetStructure.health <= 0) 
                {
                    effectsHandler.PlayExtraSound(0);
                    if(!isLarge) //Grow
                    {
                        Instantiate(largeSlimePrefab, transform.position, Quaternion.identity);
                        AudioPoolManager.Instance.PlayClipAtPosition(effectsHandler.deathSound, transform.position);
                        ParticlePoolManager.Instance.GrabCorpseParticle(corpseType).transform.position = corpseParticleTransform.position;
                        Destroy(gameObject);
                    }
                    else health = maxHealth;
                }
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

    IEnumerator Jiggle()
    {
        if(isTweening) yield break;
        StopCoroutine(MovingJiggle());
        if(movementTween != null) movementTween.Kill();
        model.localScale = originalScale;
        isTweening = true;
        model.DOPunchScale(new Vector3(0.4f, 0.4f, 0.4f), 0.6f, 1, 0.9f);
        yield return new WaitForSeconds(0.8f);
        isTweening = false;
        StartCoroutine(MovingJiggle());
    }

    IEnumerator MovingJiggle()
    {
        while(health > 0)
        {
            if(isTweening) yield break;
            if(agent.velocity.magnitude > 0.2f) movementTween = model.DOPunchScale(new Vector3(0.1f, 0.1f, 0.3f), 0.6f, 1, 0.9f);
            yield return new WaitForSeconds(0.61f);
        }
    }

    IEnumerator AttackCoolDown()
    {
        yield return new WaitForSeconds(2);
        isMoving = false;
        coroutineRunning = false;
        interruptAction = false;
    }

    IEnumerator ScanForTargets()
    {
        while(health > 0)
        {
            if(!targetStructure) yield return new WaitForSeconds(10);
            else yield return new WaitForSeconds(5);
            float closestDistance = 40;

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

    public override void OnDamage()
    {
        effectsHandler.OnHit();
        if(!isTweening)
        {
            StartCoroutine(Jiggle());
        }
    }

    public override void HitWithWater()
    {
        TakeDamage(10);
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        if(type == ToolType.WateringCan && PlayerInteraction.Instance.waterHeld > 0)
        {
            PlayerInteraction.Instance.waterHeld--;
            TakeDamage(30);
            success = true;
        }
        else success = false;
    }

    void OnDestroy()
    {
        base.OnDestroy();
        if (!gameObject.scene.isLoaded) return; 
        //drop seeds
        if(health > 0 && currentState != CreatureState.Merge) return;
        AudioPoolManager.Instance.PlayClipAtPosition(effectsHandler.deathSound, transform.position);
        if(isLarge)
        {
            for(int i = 0; i < 2; i++) Instantiate(smallSlimePrefab, transform.position, Quaternion.identity);
            if(bunnyObject.activeSelf) Instantiate(harePrefab, transform.position, Quaternion.identity);
        }
    }

}
