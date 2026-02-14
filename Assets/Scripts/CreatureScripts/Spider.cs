using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Spider : CreatureBehaviorScript
{
    public InventoryItemData bugItem;
    
    public Variant variant; // what variant of creature is this?

    public Collider attackCollider;
    bool attacking = false;

    public Transform modelPivot; // For stuck in resin pole

    bool isMoving, coroutineRunning; //ISMOVING TRACKS IF THE MOVEMENT COROUTINE IS PLAYING. COROUTINERUNNING CHECKS IF ANY *OTHER* COROUTINE IS RUNNING

    [HideInInspector] public NavMeshAgent agent;

    private Vector3 despawnPos;

    private FireFearTrigger fireSource;
    public GameObject fearObject;

    public SpiderDen homeDen;
    public StructureObject denData, cocoonData;

    public List<StructureObject> targettableStructures;
    private StructureBehaviorScript targetStructure;

    public List<CreatureObject> targettableCreatures;
    private CreatureBehaviorScript targetCreature;

    PlayerWagonScript targetWagon; //set this to the one in wagonmanager
    Transform wagonWeakPoint;

    bool interruptAction = false;
    bool fearCooldown, dodgeCooldown, dodging;
    bool canLunge = true;
    float baseSpeed;
    bool hasFleeTarget;

    public Transform strafePointL, strafePointR;
    
    List<GameObject> nearbyLavent = new List<GameObject>();
    public ParticleSystem laventParticles, biteParticles;

    public enum CreatureState
    {
        Idle,
        Wander,
        AttackPlayer,
        AttackStructure,
        AttackCreature, //Like grubs that get too close or unlit pyreflies
        Stun,
        Flee,
        AttackWagon, //Wilderness only
    }

    public CreatureState currentState;

    public enum Variant
    {
        Normal,
        Cran
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


        if(inWilderness)
        {
            targetWagon = WagonManager.Instance.wildernessWagon;
            wagonWeakPoint = targetWagon.GetWeakPoint();
            if(!homeDen) currentState = CreatureState.AttackWagon;
        }

        StartCoroutine(ScanForTargets());
        StartCoroutine(LaventEffects());

        agent.speed += Random.Range(-0.5f, 0.25f);
        baseSpeed = agent.speed;

        PlayerInteraction.OnToolUse += Dodge;

        if(homeDen) patrolPoint = homeDen.transform;
    }

    void Update()
    {
        if (health <= 0) isDead = true;

        if(isDead) return;

        if(agent.velocity.magnitude > 1) anim.SetBool("IsWalking", true);
        else anim.SetBool("IsWalking", false);

        if(fireSource)
        {
            float distFromFire = Vector3.Distance(fireSource.transform.position, transform.position);

            if(currentState != CreatureState.Flee && !coroutineRunning) currentState = CreatureState.Flee;

            if(fireSource.gameObject.activeInHierarchy == false || distFromFire > fireSource.fleeRange)
            {
                currentState = CreatureState.Wander;
                fireSource = null;
                StartCoroutine(FearCooldown());
                agent.speed = baseSpeed;
                fearObject.SetActive(false);
            }
            if(fireSource)
            {
                CheckState(currentState);
                return;
            } 
        }

        float distance = Vector3.Distance(player.position, transform.position);
        playerInSightRange = distance <= sightRange;
        playerInAttackRange = distance <= attackRange;

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

            case CreatureState.Flee:
                Flee();
                agent.speed = baseSpeed + 3;
                break;

            case CreatureState.Stun:
                break;

            case CreatureState.AttackWagon:
                Wander();
                break;

            case CreatureState.AttackPlayer:
                Wander();
                break;

            case CreatureState.AttackCreature:
                Wander();
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
        if(coroutineRunning || dodging) return; 


        if(currentState == CreatureState.Wander && targetStructure)
        {
            currentState = CreatureState.AttackStructure;
        }

        if(currentState == CreatureState.Wander && targetWagon && !homeDen)
        {
            currentState = CreatureState.AttackWagon;
        }

        if(currentState == CreatureState.Wander && targetCreature)
        {
            currentState = CreatureState.AttackCreature;
        }

        if((currentState == CreatureState.Wander && playerInSightRange) || playerInAttackRange)
        {
            currentState = CreatureState.AttackPlayer;
        }

        if(CheckForObstacle(transform) != null)
        {
            StructureBehaviorScript obstacle = CheckForObstacle(transform);
            if(targettableStructures.Contains(obstacle.structData) && targetStructure != obstacle && !playerInAttackRange)
            {
                targetStructure = obstacle;
                currentState = CreatureState.AttackStructure;
                interruptAction = true; // Attacks the structure instantly
                return;
            }
        }

        if (!isMoving)
        {
            if(currentState == CreatureState.Wander) //Wandering
            {
                agent.updateRotation = true;
                Vector3 randomPoint;
                if(!patrolPoint) randomPoint = StructureManager.Instance.GetRandomTile();
                else randomPoint = PointAroundPatrolPoint(7);
                StartCoroutine(MoveToPoint(randomPoint, Random.Range(1f, 2.5f)));
            }

            else if(currentState == CreatureState.AttackStructure) ///Attacking Structure
            {
                agent.updateRotation = true;
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
                agent.updateRotation = true;
                StartCoroutine(MoveToPoint(wagonWeakPoint.position, 5));

                if(Vector3.Distance(transform.position, wagonWeakPoint.position) < 1.8f) interruptAction = true;
            }

            else if(currentState == CreatureState.AttackPlayer) ///Attacking Player
            {
                agent.updateRotation = false;
                //Reformat this to strafing around the player
                if(playerInAttackRange)
                {
                    float r = Random.Range(0, 10);
                    transform.LookAt(player.position);

                    Vector3 strafePos;

                    if(r > 4) strafePos = strafePointL.position;
                    else strafePos = strafePointR.position;

                    Vector3 retreatDir = (transform.position - player.position).normalized;

                    if(Vector3.Distance(transform.position, player.position) < 6) strafePos += retreatDir * 3;

                    StartCoroutine(MoveToPoint(strafePos, Random.Range(0.8f,1.5f)));
                }
                else 
                {
                    StartCoroutine(MoveToPoint(player.position, 0.3f));
                }

            }

            else if(currentState == CreatureState.AttackCreature) ///Attacking Creature
            {
                if(!targetCreature)
                {
                    currentState = CreatureState.Wander;
                    return;
                }
                agent.updateRotation = true;
                StartCoroutine(MoveToPoint(targetCreature.transform.position, 10));

                if(Vector3.Distance(transform.position, targetCreature.transform.position) <= 5) interruptAction = true;
            }
            
        }
    }

    private void Flee()
    {
        if(!fireSource)
        {
            fireSource = null;
            StartCoroutine(FearCooldown());
            currentState = CreatureState.Wander;
            agent.speed = baseSpeed;
            fearObject.SetActive(false);
            return;
        }
        //Vector3 runTo = transform.position + ((transform.position - fireSource.transform.position + new Vector3(Random.Range(-3, 3), 0, Random.Range(-3, 3))));
        //agent.destination = runTo;
        agent.updateRotation = true;

        if (hasFleeTarget && !agent.pathPending && agent.remainingDistance < agent.stoppingDistance + 1f)
        {
            hasFleeTarget = false;
        }
        else if (!hasFleeTarget)
        {
            hasFleeTarget = true;
            Vector3 fleeDirection = (transform.position - fireSource.transform.position).normalized;

            
            float randomAngle = Random.Range(-20f, 20); //random offset for random movement

            fleeDirection = Quaternion.Euler(0, randomAngle, 0) * fleeDirection;

            Vector3 newDestination = transform.position + fleeDirection * Random.Range(4f, 7f);

        
            agent.SetDestination(newDestination);
        }
    }

    void Dodge()
    {
        if(isDead || coroutineRunning || dodgeCooldown || Random.Range(0,10) > 7 || currentState != CreatureState.AttackPlayer) return;

        if(MainMenuScript.currentFileMode == FileMode.Cozy) return;

        StartCoroutine(DodgeJump());
    }

    IEnumerator DodgeJump()
    {
        dodgeCooldown = true;
        dodging = true;
        interruptAction = true;
        Vector3 jumpDirection;
        agent.updateRotation = false;
        if(Random.Range(0,2) == 1) jumpDirection = (strafePointL.position - transform.position).normalized;
        else jumpDirection = jumpDirection = (strafePointR.position - transform.position).normalized;
        agent.velocity = 15 * jumpDirection;
        anim.Play("Dodge");
        yield return new WaitForSeconds(1);
        agent.velocity = Vector3.zero;
        dodging = false;
        agent.speed = baseSpeed;
        agent.updateRotation = true;
        yield return new WaitForSeconds(2);
        dodgeCooldown = false;
    }

    bool CanPlaceDen()
    {
        float chance = 0;
        switch(StructureManager.Instance.TallyStructure(denData))
        {
            case 0:
                chance = 18;
                break;
            case 1:
                chance = 8;
                break;
            case 2:
                chance = 5;
                break;
            case 3:
                chance = 1;
                break;
            default :
                chance = -1;
                break;
        }
        if(Random.Range(0,100) < chance && StructureManager.Instance.ValidateGridType(transform.position, GridType.Farm)) return true;
        else return false;
    }

    bool CanPlaceCocoon()
    {
        float chance = 0;
        switch(StructureManager.Instance.TallyStructure(denData))
        {
            case 0:
                chance = 0.1f;
                break;
            case 1:
                chance = 1f;
                break;
            case 2:
                chance = 2f;
                break;
            default :
                chance = 4f;
                break;
        }
        if(Random.Range(0f,100f) < chance && StructureManager.Instance.ValidateGridType(transform.position, GridType.Farm)) return true;
        else return false;
    }

    private IEnumerator WaitAround()
    {
        coroutineRunning = true;
        float r = Random.Range(2f, 4.5f);
        float timeElapsed = 0;
        agent.ResetPath();

        //Try to make a den
        bool canPlaceDen = false;
        bool canPlaceCocoon = false;
        Vector3 denSpawn = StructureManager.Instance.CheckLargeTile(transform.position);
        Vector3 cocoonSpawn = StructureManager.Instance.CheckTile(transform.position);
        if(denSpawn != Vector3.zero || cocoonSpawn != Vector3.zero) 
        {
            if(CanPlaceDen()) canPlaceDen = true;
            else if(CanPlaceCocoon()) canPlaceCocoon = true;

            if(canPlaceDen || canPlaceCocoon)
            {
                yield return new WaitForSeconds(0.5f);
                anim.Play("Dig");
            }
        }

        while(timeElapsed < r)
        {
            yield return new WaitForSeconds(0.1f);
            timeElapsed += 0.1f;
            if(playerInSightRange) timeElapsed += 0.3f;
        }

        denSpawn = StructureManager.Instance.CheckLargeTile(transform.position); //Have to check again in case of obstruction
        cocoonSpawn = StructureManager.Instance.CheckTile(transform.position);

        if(!playerInSightRange) 
        {
            if(canPlaceDen && denSpawn != Vector3.zero) StructureManager.Instance.SpawnStructure(denData.objectPrefab, denSpawn);
            if(canPlaceCocoon && cocoonSpawn != Vector3.zero) StructureManager.Instance.SpawnStructure(cocoonData.objectPrefab, cocoonSpawn);
        }

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
        //coroutineRunning = true;
        interruptAction = false;

        if (TimeManager.Instance.isDay && !inWilderness && currentState == CreatureState.Wander)
        {
            if(homeDen)
            {
                if(homeDen.heldSpiders < homeDen.maxSpiders && homeDen.isLarge) destination = homeDen.transform.position;
            }
            else destination = despawnPos;
        } 

        agent.destination = destination;

        float timeSpent = 0; //to make sure it doesnt get stuck

        while ((agent.pathPending || agent.remainingDistance > agent.stoppingDistance) && timeSpent < maxTime)
        {
            timeSpent += Time.deltaTime;

            if(agent.updateRotation == false)
            {
                Vector3 directionToTarget = player.position - transform.position;
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 2 * Time.deltaTime);
            }

            if(interruptAction) timeSpent += 30;
            yield return null;
        }

        FinishedMoving();
    }

    void FinishedMoving()
    {
        if (currentState == CreatureState.Wander)
        {
            if(TimeManager.Instance.isDay && homeDen && homeDen.heldSpiders < homeDen.maxSpiders && Vector3.Distance(transform.position, homeDen.transform.position) < 4)
            {
                homeDen.heldSpiders++;
                Destroy(this.gameObject);
                return;
            }
            currentState = CreatureState.Idle;
        }

        if(!playerInAttackRange) agent.updateRotation = true;

        if(currentState == CreatureState.AttackStructure && targetStructure)
        {
            if(Vector3.Distance(targetStructure.transform.position, transform.position) < 2.2f)
            {
                agent.Stop();
                StartCoroutine(StructureAttack());
                return;
            }
        }

        if(currentState == CreatureState.AttackWagon)
        {
            if(Vector3.Distance(wagonWeakPoint.position, transform.position) < 3f)
            {
                agent.Stop();
                StartCoroutine(StructureAttack());
                return;
            }
        }

        if(currentState == CreatureState.AttackPlayer)
        {
            int attackChance = 6;
            if(MainMenuScript.currentFileMode == FileMode.Cozy) attackChance = 8;
            if(playerInAttackRange && Random.Range(0,10) > attackChance && canLunge && !fearCooldown && !dodging)
            {
                //Do the lunge attack
                //agent.Stop();
                StartCoroutine(LungeAttack(player.position));
                agent.updateRotation = true;
                return;
            }

            if(playerInSightRange == false) currentState = CreatureState.Wander;
        }

        if(currentState == CreatureState.AttackCreature)
        {
            if(Vector3.Distance(transform.position, targetCreature.transform.position) <= 5 && !dodging && !fearCooldown)
            {
                //Do the lunge attack
                //agent.Stop();
                StartCoroutine(LungeAttack(targetCreature.transform.position));
                return;
            }
        }

        isMoving = false;
        //coroutineRunning = false;
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

    private void OnTriggerEnter(Collider other)
    {
        if(currentState == CreatureState.Stun || !attacking) return;
        if (other.CompareTag("Player") && !isDead)
        {
            PlayerInteraction playerInteraction = other.GetComponent<PlayerInteraction>();
            if (playerInteraction != null)
            {
                agent.velocity = Vector3.zero; //STOP PLAYER MOMENTUM IMMEDIATELY SO THE SPIDER DOES NOT PUSH THE PLAYER
                playerInteraction.StaminaChange(-damageToPlayer, corpseParticleTransform.position);
                attacking = false;
                return;
            }
        }

        if(other.gameObject.layer == 9)
        {
            CreatureBehaviorScript c = other.gameObject.GetComponentInParent<CreatureBehaviorScript>();
            if(c && targettableCreatures.Contains(c.creatureData))
            {
                Vector3 cocoonSpawn = StructureManager.Instance.GetTileCenter(c.transform.position);
                if(cocoonSpawn != Vector3.zero) StructureManager.Instance.SpawnStructure(cocoonData.objectPrefab, cocoonSpawn);

                agent.velocity = Vector3.zero;
                c.TakeDamage(10);
                c.PlayHitParticle(c.transform.position);
            }
        }
    }

    IEnumerator LungeAttack(Vector3 target)
    {
        coroutineRunning = true;
        anim.Play("Attack");
        canLunge = false;
        agent.ResetPath();
        transform.LookAt(target);
        yield return new WaitForSeconds(0.4f);
        effectsHandler.MiscSound();
        //Lunge code here//
        coroutineRunning = true;
       
        if(currentState != CreatureState.Stun)
        {
            Vector3 lungeDirection = (target - transform.position).normalized;
            agent.velocity = lungeDirection * 30f; //better lunge
            print("I Lunged!!");
        }

        attacking = true;
        attackCollider.enabled = true;
        yield return new WaitForSeconds(0.1f);
        biteParticles.Play();
        yield return new WaitForSeconds(0.4f);
        //Stop the lunge//
        agent.velocity = Vector3.zero;
        attackCollider.enabled = false;
        attacking = false;

        yield return new WaitForSeconds(0.8f); //Agent can walk again
        agent.Resume();
        isMoving = false;
        coroutineRunning = false;
        interruptAction = false;
        yield return new WaitForSeconds(Random.Range(1.5f, 2f));
        canLunge = true;
    }

    IEnumerator StructureAttack()
    {
        coroutineRunning = true;
        anim.Play("Attack");
        yield return new WaitForSeconds(0.5f);
        //attacking = true;
        attackCollider.enabled = true;
        if(targetStructure)
        {

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
        biteParticles.Play();
        yield return new WaitForSeconds(0.3f);
        attackCollider.enabled = false;
        attacking = false;
        yield return new WaitForSeconds(1);
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

            FindNearbyStructure(5);
            if(!targetStructure) FindNearbyCreature(10);
            
        }
    }

    void FindNearbyStructure(float distance)
    {
        float closestDistance = distance;

        float distanceToStructure;

        List<StructureBehaviorScript> availableStructure = new List<StructureBehaviorScript>();
        foreach (var structure in structManager.allStructs)
        {
            if(!structure) continue;
            FarmLand tile = structure as FarmLand;
            distanceToStructure = Vector3.Distance(transform.position, structure.transform.position);
            if (targettableStructures.Contains(structure.structData) && !structure.absentFromFarmGrid && distanceToStructure < closestDistance && (!tile || (tile.crop && !tile.isWeed)))
            {
                availableStructure.Add(structure);
                closestDistance = distanceToStructure;
                continue;
            }

            if(homeDen) continue;

            SpiderDen den = structure as SpiderDen;
            if(den && !homeDen)
            {
                homeDen = den;
                patrolPoint = den.transform;
            }
        }

        if (availableStructure.Count > 0)
        {
            int r = Random.Range(0, availableStructure.Count);
            targetStructure = availableStructure[r];
        }
    }

    void FindNearbyCreature(float distance)
    {
        List<CreatureBehaviorScript> availableCreatures = new List<CreatureBehaviorScript>();
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, distance, 1 << 9);
        foreach(Collider collider in hitEnemies)
        {
            var creature = collider.GetComponentInParent<CreatureBehaviorScript>();
            if (creature != null && targettableCreatures.Contains(creature.creatureData) && Random.Range(0,10) > 5)
            {
                availableCreatures.Add(creature);
            }
        }

        if (availableCreatures.Count > 0)
        {
            int r = Random.Range(0, availableCreatures.Count);
            targetCreature = availableCreatures[r];
        }
    }

    IEnumerator FearCooldown()
    {
        fearCooldown = true;
        yield return new WaitForSeconds(1);
        if(currentState != CreatureState.Flee) fearCooldown = false;
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

    public override void NewPriorityTarget(StructureBehaviorScript newStruct)
    {
        if (currentState == CreatureState.Stun) return;
        interruptAction = true;
        targetStructure = newStruct;
    }

    public override void EnteredFireRadius(FireFearTrigger _fireSource, out bool successful)
    {
        successful = false;
        if(isDead) return;
        if(fireSource == null) effectsHandler.MiscSound2();
        fireSource = _fireSource;
        successful = true;
        fearObject.SetActive(true);
    }

    public override void NearLaventLeaf(GameObject laventObject)
    {
        if(!nearbyLavent.Contains(laventObject))
        {
            nearbyLavent.Add(laventObject);
        }
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
                    TakeDamage(5);
                    effectsHandler.MiscSound3();
                    laventParticles.Play();
                }
            }
        }
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
            fearObject.SetActive(false);
            persistAfterNewDay = false;
            if(homeDen) homeDen.outsideSpiders--;
            homeDen = null;
        }
    }

    public override bool CaughtByBugNet(out InventoryItemData item)
    {
        item = bugItem;

        if(isDead)
        {
            TakeDamage(999);
            return false;
        }

        return true;
    }

    void OnDestroy()
    {
        base.OnDestroy();
        PlayerInteraction.OnToolUse -= Dodge;
        if (!gameObject.scene.isLoaded) return; 
        if(homeDen) homeDen.outsideSpiders--;
    }
}
