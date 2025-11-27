using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;

public class MistWalker : CreatureBehaviorScript
{
    public Variant variant; // what variant of creature is this?

    public List<StructureObject> targettableStructures;

    private StructureBehaviorScript targetStructure;
    public List<StructureBehaviorScript> availableStructure = new List<StructureBehaviorScript>();

    public List<CropData> undesiredCrops;

    private bool isMoving = false;
    private bool coroutineRunning = false;
    private Transform target;
    private bool attackingPlayer = false;

    [HideInInspector] public NavMeshAgent agent;
    public AnimEvents animEvents;
    public Collider lungeAttackHitbox;
    public float lungeCooldown = 6f; // Time between lunges
    public float lungeRange = 9f; // Distance at which it will lunge
    private bool canLunge = true;
    bool canAttack = true;
    bool fearCooldown;
    float attackCooldown = 0.7f; // Time between swipes
    bool canDoubleLunge = false;
    private bool recoilCooldown = false; //To prevent stunlocking
    private bool isRecoiling = false;

    private Vector3 despawnPos;

    private Coroutine trackPlayerRoutine, walkRoutine; 

    private FireFearTrigger fireSource;
    public GameObject fearParticle;

    public EquipEnemyArmor[] equippableArmor;

    public List<GameObject> foggedWalkers = new List<GameObject>();
    public GameObject foggedWalkerPrefab;

    public ParticleSystem feralLungeParticles;

    public enum CreatureState
    {
        SpawnIn,
        Idle,
        Wander,
        WalkTowardsClosestStructure,
        WalkTowardsPriorityStructure,
        WalkTowardsPlayer,
        AttackStructure,
        AttackPlayer,
        Stun,
        Die,
        FleeFromFire
    }

    public enum Variant
    {
        Normal,
        Strong,
        Fogged,
        FogMind,
        Corrupted
    }

    public CreatureState currentState;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (animEvents) animEvents.OnFloatChange += WalkSpeedToggle;
        if (animEvents) animEvents.OnColliderChange += ColliderChange;
    }

    void Start()
    {
        base.Start();
        lungeAttackHitbox.enabled = false;
        StructureBehaviorScript.OnStructuresUpdated += UpdateStructureList; // Update list when structures change
        UpdateStructureList();
        
        agent.enabled = false;
        agent.enabled = true;

        int r = Random.Range(0, NightSpawningManager.Instance.despawnPositions.Length);
        despawnPos = NightSpawningManager.Instance.despawnPositions[r].position;
        targetStructure = null;
        //currentState = CreatureState.SpawnIn;
        StartCoroutine(IdleSoundTimer());

        if(variant == Variant.Strong) canDoubleLunge = true;
        if (variant == Variant.FogMind) SpawnFoggedWalkers();
        if (variant == Variant.Fogged) canAttack = false;

        for(int i = 0; i < equippableArmor.Length; i++)
        {
            r = Random.Range(0,100);
            if(equippableArmor[i].chanceToEquip >= r)
            {
                equippableArmor[i].armorObject.SetActive(true);
                if(i == 0)
                {
                    //its a barrel, disable lunge
                    canLunge = false;
                }

                if(i == 2)
                {
                    //its a hive, lower visability
                    sightRange -= 5;
                }
            }
        }

        if(Random.Range(0,10) > 1) anim.SetBool("AltWalk", true);

        if(MainMenuScript.currentFileMode == FileMode.Cozy) canLunge = false;

        //if(!inWilderness && Random.Range(0,5) > 2) currentState = CreatureState.WalkTowardsClosestStructure; //causing issues I think
    }

   

    void OnDisable()
    {
        StructureBehaviorScript.OnStructuresUpdated -= UpdateStructureList;
        if (animEvents) animEvents.OnColliderChange -= ColliderChange;
        if (animEvents) animEvents.OnFloatChange -= WalkSpeedToggle;
    }

    public void Spawn()
    {
        if(inWilderness)
        {
            currentState = CreatureState.WalkTowardsPlayer;
        }
        else if (!isMoving)
        {
            Vector3 randomPoint = StructureManager.Instance.GetRandomTile();
            walkRoutine = StartCoroutine(MoveToPoint(randomPoint));
        }
    }

    private void UpdateStructureList()
    {
        availableStructure.Clear();
        foreach (var structure in structManager.allStructs)
        {
            if (structure && targettableStructures.Contains(structure.structData) && !structure.absentFromFarmGrid)
            {
                FarmLand f = structure as FarmLand;
                if(f && (!f.crop || undesiredCrops.Contains(f.crop) || f.isWeed || f.currentUpgrade == FarmLand.FarmTileUpgrade.Corrupt)) continue;
                
                availableStructure.Add(structure);
            }
                
        }

        if (availableStructure.Count > 0)
        {
            int r = Random.Range(0, availableStructure.Count);
            targetStructure = availableStructure[r];
        }
    }

    void Update()
    {
        if (health <= 0) isDead = true;

        if(currentState == CreatureState.FleeFromFire && !fearParticle.activeSelf) fearParticle.SetActive(true);
        else if(currentState != CreatureState.FleeFromFire && fearParticle.activeSelf) fearParticle.SetActive(false);

        if (!isDead && currentState != CreatureState.Stun)
        {
            if(fireSource)
            {
                float distFromFire = Vector3.Distance(fireSource.transform.position, transform.position);

                if(currentState != CreatureState.FleeFromFire && !coroutineRunning) currentState = CreatureState.FleeFromFire;

                if(fireSource.gameObject.activeInHierarchy == false || distFromFire > fireSource.fleeRange)
                {
                    fireSource = null;
                    StartCoroutine(FearCooldown());
                    currentState = CreatureState.Wander;
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

            if (playerInSightRange && currentState != CreatureState.AttackPlayer && currentState != CreatureState.WalkTowardsPlayer && (currentState != CreatureState.AttackStructure || playerInAttackRange) && variant != Variant.Fogged)
            {
                currentState = CreatureState.WalkTowardsPlayer;
            }

            CheckState(currentState);
        }
        else
        {
            lungeAttackHitbox.enabled = false;
            fearParticle.SetActive(false);
        }
    }


    private void OnDrawGizmos()
    {
        float attackRange = 3f;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    public void CheckState(CreatureState currentState)
    {
        switch (currentState)
        {
            case CreatureState.AttackPlayer:
                AttackPlayer();
                anim.SetBool("IsWalking", true);
                break;

            case CreatureState.SpawnIn:
                Spawn();
                anim.SetBool("IsWalking", true);
                break;

            case CreatureState.Idle:
                Idle();
                anim.SetBool("IsWalking", false);
                break;

            case CreatureState.Wander:
                Wander();
                anim.SetBool("IsWalking", true);
                break;

            case CreatureState.WalkTowardsClosestStructure:
                WalkTowardsClosestStructure();
                anim.SetBool("IsWalking", true);
                break;

            case CreatureState.WalkTowardsPriorityStructure:
                //WalkTowardsPriorityStructure();
                Debug.LogError("Should not be in this state");
                anim.SetBool("IsWalking", true);
                break;

            case CreatureState.WalkTowardsPlayer:
                WalkTowardsPlayer();
                anim.SetBool("IsWalking", true);
                break;

            case CreatureState.AttackStructure:
                AttackStructure();
                anim.SetBool("IsWalking", false);
                break;

            case CreatureState.Stun:
                anim.SetBool("IsWalking", false);
                break;

            case CreatureState.Die:
                // OnDeath();
                break;

            case CreatureState.FleeFromFire:
                FleeFromFire();
                anim.SetBool("IsWalking", true);
                break;

            default:
                Debug.LogError("Unknown state: " + currentState);
                break;
        }
    }

    #region WanderingFunctions
    public void Wander()
    {
        if ((playerInSightRange && variant != Variant.Fogged) || (inWilderness && !patrolPoint))
        {
            currentState = CreatureState.WalkTowardsPlayer;
            return;
        }

        if (!isMoving && currentState == CreatureState.Wander)
        {
            Vector3 randomPoint;
            if(!patrolPoint) randomPoint = GetRandomPointAround(transform.position, 5f);
            else randomPoint = PointAroundPatrolPoint(7);
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
            if (playerInSightRange && variant != Variant.Fogged)
            {
                currentState = CreatureState.WalkTowardsPlayer;
                isMoving = false;
                coroutineRunning = false;
                walkRoutine = null;
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


    private void WalkTowardsClosestStructure()
    {
        if(TimeManager.Instance.isDay)
        {
            currentState = CreatureState.Wander;
            return;
        }
        if (targetStructure == null || !targetStructure.gameObject.activeSelf)
        {
            targetStructure = FindClosestStructure();
            if (targetStructure != null)
            {
                target = targetStructure.transform;
                agent.destination = target.position;
            }
            else
            {
                currentState = CreatureState.Wander;
            }
        }
        if(CheckForObstacle(transform) != null)
        {
            targetStructure = CheckForObstacle(transform);
            target = targetStructure.transform;
            agent.destination = target.position;
        }
        else if (targetStructure && Vector3.Distance(transform.position, targetStructure.transform.position) < 4f)//(!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 1f)
        {
            agent.ResetPath();
            if (variant != Variant.Fogged) currentState = CreatureState.AttackStructure;
            else currentState = CreatureState.Wander;
        }
        else if((target == null || agent.destination != target.position) && targetStructure)
        {
            target = targetStructure.transform;
            agent.destination = target.position;
        }
    }

    private StructureBehaviorScript FindClosestStructure()
    {
        StructureBehaviorScript closestStructure = null;
        float closestDistance = Mathf.Infinity;

        float distanceToStructure;
        float r;

        bool foundCrop = false;

        foreach (var structure in availableStructure)
        {
            if (structure == null) continue;

            distanceToStructure = Vector3.Distance(transform.position, structure.transform.position);

            r = Random.Range(0,10); //to add randomness to what they chose to seek out

            if(structure.wealthValue == 0) continue; //to prevent mistwalkers from targetting dirt without a crop

            FarmLand foundTile = structure as FarmLand; //Mistwalkers will stop searching for non crops once they find one

            if ((distanceToStructure < closestDistance && r > 3 && (!foundCrop || foundTile)) || (foundTile && closestStructure is FarmLand == false))
            {
                closestDistance = distanceToStructure;
                closestStructure = structure;

                if(foundTile) foundCrop = true;
            }
        }
        return closestStructure;
    }

    private void WalkTowardsPlayer()
    {
        if (trackPlayerRoutine == null)
        {
            trackPlayerRoutine = StartCoroutine(TrackPlayer());
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if ((distanceToPlayer <= 3.3f || (distanceToPlayer <= lungeRange && canLunge)) && canAttack)
        {
            StopTrackingPlayer();
            currentState = CreatureState.AttackPlayer;
        }
        else if(!playerInAttackRange && CheckForObstacle(transform) != null)
        {
            StopTrackingPlayer();
            targetStructure = CheckForObstacle(transform);
            currentState = CreatureState.AttackStructure;
        }
        else if (!playerInSightRange && (!inWilderness || patrolPoint))
        {
            if(targetStructure)
            {
                currentState = CreatureState.WalkTowardsClosestStructure;
            }
            else currentState = CreatureState.Wander;
            StopTrackingPlayer();
        }
    }

    private IEnumerator TrackPlayer()
    {
        while ((playerInSightRange || inWilderness) && currentState == CreatureState.WalkTowardsPlayer)
        {
            agent.destination = player.position;
            yield return new WaitForSeconds(0.5f); // update destination every 0.5 seconds to prevent overloading it
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

    private void FleeFromFire()
    {
        Vector3 runTo = transform.position + ((transform.position - fireSource.transform.position + new Vector3(Random.Range(-3, 3), 0, Random.Range(-3, 3)) * 1));
        agent.destination = runTo;
        if(agent.speed > 0 && agent.speed != 5) agent.speed = 5;
    }
    #endregion

    #region AttackingFunctions
    private void AttackPlayer()
    {
        if (coroutineRunning || isRecoiling || fearCooldown)
            return;

        transform.LookAt(player.position);

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= attackRange && canAttack)
        {
            StartCoroutine(SwipePlayer());
            transform.LookAt(player.position);
        }
        else if (distance > attackRange && distance <= lungeRange && canLunge && 
        (!PlayerInteraction.Instance.torchLit || (PlayerInteraction.Instance.torchLit && HandItemManager.Instance.GetCurrentType() != ToolType.Torch)))
        {
            StartCoroutine(LungeAtPlayer());
        }
        else
        {
            currentState = CreatureState.WalkTowardsPlayer;
        }
    }

    private void AttackStructure()
    {
        if (targetStructure == null)
        {
            currentState = CreatureState.Wander;
        }
        else if (!coroutineRunning)
        {
            StartCoroutine(AttackingStructure());
        }
    }

    private IEnumerator AttackingStructure()
    {
        bool attackingPlant = false;
        if(targetStructure)
        {
            FarmLand farmTile = targetStructure as FarmLand;
            if(farmTile) attackingPlant = true;
        }

        coroutineRunning = true;
        if(attackingPlant) anim.SetTrigger("IsAttackingPlant");
        else anim.SetTrigger("IsAttacking");
        transform.LookAt(targetStructure.transform.position);

        yield return new WaitForSeconds(1f);
        if(!targetStructure || isRecoiling || health <= 0)
        {
            if(currentState != CreatureState.Stun) currentState = CreatureState.Idle;
            coroutineRunning = false;
            yield break;
        }

        if (Vector3.Distance(transform.position, targetStructure.transform.position) < 5f)
        {
            coroutineRunning = true;
            HitStructureParticle(targetStructure.transform.position);
            targetStructure.TakeDamage(damageToStructure);
            transform.LookAt(targetStructure.transform.position);
            if (targetStructure != null && targetStructure.health <= 0) { targetStructure = null; }
            //yield return new WaitForSeconds(3f);
            //coroutineRunning = false;
        }
        else
        {
            if(currentState != CreatureState.Stun) currentState = CreatureState.WalkTowardsClosestStructure;
        }

        yield return new WaitForSeconds(2f); // Cooldown between attacks
        coroutineRunning = false;
    }

    private IEnumerator LungeAtPlayer()
    {
        coroutineRunning = true;
        attackingPlayer = true;

        recoilCooldown = true;
        //anim.SetTrigger("IsLunging");
        if(variant == Variant.Corrupted) anim.Play("MistFeral", -1, 0);
        else anim.Play("MistLunge", -1, 0);
        canLunge = false;

        effectsHandler.MiscSound();

        if(variant == Variant.Corrupted) yield return new WaitForSeconds(0.45f); 
        else yield return new WaitForSeconds(0.75f); 

       
        if(currentState != CreatureState.Stun)
        {
            Vector3 lungeDirection = (player.position - transform.position).normalized;
            float lungeBoost = 20;
            if(variant == Variant.Corrupted) lungeBoost += 10;
            agent.velocity = lungeDirection * lungeBoost; //better lunge
        }

        if(variant == Variant.Corrupted) yield return new WaitForSeconds(0.35f); 
        else yield return new WaitForSeconds(0.5f);

        attackingPlayer = false;
        agent.velocity = Vector3.zero;
        if(canDoubleLunge && !isDead)
        {
            yield return new WaitForSeconds(0.1f);
            StartCoroutine(LungeAtPlayer());
            canDoubleLunge = false;
        }
        else
        {
            if(variant == Variant.Corrupted)
            {
                feralLungeParticles.Play();
                yield return new WaitForSeconds(1.75f); 
            }
            else yield return new WaitForSeconds(0.5f);
            if(currentState != CreatureState.Stun) currentState = CreatureState.WalkTowardsPlayer;
            coroutineRunning = false;
            recoilCooldown = false;
            StartCoroutine(LungeCooldown());
        }

    }

    private IEnumerator SwipePlayer()
    {
        coroutineRunning = true;
        attackingPlayer = true;
        canAttack = false;

        anim.SetTrigger("IsAttacking");

        effectsHandler.MiscSound2();
        recoilCooldown = true;

        yield return new WaitForSeconds(0.5f); 

        if(currentState != CreatureState.Stun)
        {
            Vector3 lungeDirection = (player.position - transform.position).normalized;
            agent.velocity = lungeDirection * 7; 
        }

        yield return new WaitForSeconds(0.8f);
        agent.velocity = Vector3.zero;

        attackingPlayer = false;
        if(currentState != CreatureState.Stun) currentState = CreatureState.WalkTowardsPlayer;
        recoilCooldown = false;
        yield return new WaitForSeconds(0.5f); 
        coroutineRunning = false;
        StartCoroutine(AttackCooldown());
    }

    private IEnumerator AttackCooldown()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private IEnumerator LungeCooldown()
    {
        if(variant == Variant.Strong)
        {
            float r = Random.Range(0, 100);
            if(r > 30) canDoubleLunge = true;
        }
        yield return new WaitForSeconds(lungeCooldown + Random.Range(-0.5f, 4f));
        canLunge = true;
    }
    #endregion

    private void Idle()
    {
        if (playerInSightRange && variant != Variant.Fogged)
        {
            currentState = CreatureState.WalkTowardsPlayer;
            return;
        }

        if (!coroutineRunning)
        {
            int r = Random.Range(0, 15);
            if (r < 6)
            {
                if (availableStructure.Count > 0)
                {
                    currentState = CreatureState.WalkTowardsClosestStructure;
                }
            }
            else if (r < 10)
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
        if(currentState == CreatureState.Stun) return;
        if (attackingPlayer && other.CompareTag("Player") && !isDead)
        {
            PlayerInteraction playerInteraction = other.GetComponent<PlayerInteraction>();
            if (playerInteraction != null)
            {
                playerInteraction.StaminaChange(damageToPlayer);
                attackingPlayer = false;
                //lungeAttackHitbox.enabled = false;
            }
        }
    }

    public override bool OnBearTrapStun(StructureBehaviorScript b)
    {
        if (currentState != CreatureState.Stun)
        {
            StartCoroutine(BearTrapHold(b));
            agent.destination = transform.position;
            agent.ResetPath();
            anim.SetBool("IsWalking", false);
            anim.SetTrigger("IsRecoiling");
            return true;
        }
        return false;
    }

    private IEnumerator BearTrapHold(StructureBehaviorScript b)
    {
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
        while (b && b.health > 0)
        {
            yield return new WaitForSeconds(1);
            currentState = CreatureState.Stun;
            anim.SetTrigger("IsRecoiling");
        }
        currentState = CreatureState.Wander;
    }

    public override void OnDeath()
    {
        if (!isDead)
        {
            isDead = true;
            anim.SetBool("IsDead", true);
            base.OnDeath();
            agent.enabled = false;
            rb.isKinematic = true;
            rb.freezeRotation = true;
            StopAllCoroutines();
            fearParticle.SetActive(false);
            if (variant == Variant.FogMind) { KillFogged(); }
        }
    }

    private void KillFogged()
    {
        for (int i = 0; i < foggedWalkers.Count; i++)
        {
            MistWalker walker = foggedWalkers[i].GetComponent<MistWalker>();
            walker.OnDeath();
        }
    }
    private void DestroyFogged()
    {
        foreach (GameObject fogged in foggedWalkers)
        {
            if (fogged == null) continue;

            MistWalker walker = fogged.GetComponent<MistWalker>();
            if (walker == null) continue;

            // Make sure the corpse breaks
            walker.health = walker.corpseHealth - 1; // Ensure it's below corpseHealth
            walker.OnCorpseDamage(); // Triggers corpse particle, item drops, etc.
        }
    }



    public override void OnDamage()
    {
        if (variant == Variant.Fogged) return;
        if(!recoilCooldown && !attackingPlayer)
        {
            recoilCooldown = true;
            effectsHandler.OnHit();
            anim.SetTrigger("IsRecoiling");
            StartCoroutine(RecoilCooldown());
            
        }
        if (variant == Variant.FogMind && !isDead) { SwapPlacesWithFogged(); }
    }

    private void SwapPlacesWithFogged()
    {
        int r = Random.Range(0, foggedWalkers.Count);
        Vector3 fogMindPosition = transform.position;
        Quaternion fogMindRotation = transform.rotation;
        Vector3 foggedPostion = foggedWalkers[r].transform.position;
        Quaternion foggedRotation = foggedWalkers[r].transform.rotation;

        transform.position = foggedPostion;
        transform.rotation = foggedRotation;
        foggedWalkers[r].transform.position = fogMindPosition;
        foggedWalkers[r].transform.rotation = fogMindRotation;
    }

    public override void OnCorpseDamage()
    {
        if (health <= 0 && canCorpseBreak)
        {
            anim.Play("MistDeathInteract", -1, 0f);
            if (variant == Variant.FogMind) { DestroyFogged(); }
        }
        else if (health <= 0 && !canCorpseBreak && variant == Variant.Fogged)
        {
            anim.Play("MistDeathInteract", -1, 0f);
        }
    }

    IEnumerator RecoilCooldown()
    {
        isRecoiling = true;
        yield return new WaitForSeconds(1);
        isRecoiling = false;
        yield return new WaitForSeconds(1);
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

    private void SpawnFoggedWalkers()
    {
        for (int i = 0; i < 3; i++)
        {
            GameObject walker = Instantiate(foggedWalkerPrefab, NightSpawningManager.Instance.RandomMistPosition(), Quaternion.identity);
            foggedWalkers.Add(walker);
        }
    }

    public override void EnteredFireRadius(FireFearTrigger _fireSource, out bool successful)
    {
        successful = false;
        if(variant == Variant.Corrupted) return;
        fireSource = _fireSource;
        successful = true;
    }

    public override void NewPriorityTarget(StructureBehaviorScript newStruct)
    {
        if(targetStructure) return;
        targetStructure = newStruct;
        if(currentState == CreatureState.Idle || currentState == CreatureState.Wander) currentState = CreatureState.WalkTowardsClosestStructure;
        
    }

    public void WalkSpeedToggle(float _speed)
    {
        agent.speed = _speed;
    }

    public void ColliderChange(bool enabled)
    {
        //print(enabled);
        lungeAttackHitbox.enabled = enabled;
    }

    IEnumerator FearCooldown()
    {
        fearCooldown = true;
        yield return new WaitForSeconds(1);
        if(currentState != CreatureState.FleeFromFire) fearCooldown = false;
    }
}
