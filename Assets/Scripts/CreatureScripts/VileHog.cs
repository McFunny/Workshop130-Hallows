using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class VileHog : CreatureBehaviorScript
{
    public Variant variant; // what variant of creature is this?

    public VileHog parent;
    public VileHog[] babies;

    public List<CropData> desiredCrops; // what crops does this creature want to eat

    FarmLand foundFarmTile;
    InventoryItemData heldItem;

    public InventoryItemData foxGlove, dare;

    private StructureBehaviorScript targetStructure;

    private bool isMoving = false;
    private bool coroutineRunning = false;
    private Transform target;
    private bool attackingPlayer = false;

    [HideInInspector] public NavMeshAgent agent;
    public Collider attackHitbox;
    public Transform chargePosition;
    public SpriteRenderer r;
    public ParticleSystem chargeParticles, dashParticles;

    float beginChargeTime = 1f; // Time it takes to initiate a charge
    float chargeTime = 2f; // Time it takes to complete a charge
    private bool isCharging = false;
    float recoilTime = 2;
    float fleeTimeLeft = 0;

    bool holdingCrop;
    float walkSpeed = 4;
    float runSpeed = 8;
    float chargeSpeed = 14;
    bool faceTarget;

    private Vector3 despawnPos;

    private Coroutine trackPlayerRoutine, walkRoutine, chargeRoutine; 

    public enum CreatureState
    {
        SpawnIn,
        Idle,
        Wander,
        Charging,
        WalkTowards,
        FetchCrop,
        Stun,
        Flee,
        Die,
        FollowParent
    }

    public enum Variant
    {
        Normal,
        Chunky,
        Tiny
    }

    public CreatureState currentState;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        if(babies.Length > 0)
        {
            for(int i = 0; i < babies.Length; i++)
            {
                babies[i].transform.SetParent(null);
            }
        }

        base.Start();
        attackHitbox.enabled = false;
        
        agent.enabled = false;
        agent.enabled = true;

        int r = Random.Range(0, NightSpawningManager.Instance.despawnPositions.Length);
        despawnPos = NightSpawningManager.Instance.despawnPositions[r].position;
        targetStructure = null;
        StartCoroutine(IdleSoundTimer());

    }

    public void Spawn()
    {
        if(inWilderness)
        {
            currentState = CreatureState.WalkTowards;
        }
        else if (!isMoving)
        {
            Vector3 randomPoint = StructureManager.Instance.GetRandomTile();
            walkRoutine = StartCoroutine(MoveToPoint(randomPoint));
        }
    }

    void Update()
    {
        if (health <= 0) isDead = true;

        if (!isDead && currentState != CreatureState.Stun)
        {
            float distance = Vector3.Distance(player.position, transform.position);
            playerInSightRange = distance <= sightRange;
            playerInAttackRange = distance <= attackRange;

            /*if (playerInSightRange && currentState != CreatureState.Charging && currentState != CreatureState.WalkTowards && !holdingCrop)
            {
                currentState = CreatureState.WalkTowards;
            }*/

            CheckState(currentState);

            if(faceTarget && target)
            {
                Vector3 targetPosition = target.position;

                Vector3 direction = targetPosition - transform.position;
                direction.y = 0;
                Quaternion toRotation = Quaternion.LookRotation(direction);

                transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, 3.5f * Time.deltaTime);
            }
        }
        else
        {
            attackHitbox.enabled = false;
        }

    }

    public void CheckState(CreatureState currentState)
    {
        switch (currentState)
        {
            case CreatureState.SpawnIn:
                Spawn();
                anim.SetBool("IsWalking", true);
                break;

            case CreatureState.Idle:
                Idle();
                anim.SetBool("IsWalking", false);
                anim.SetBool("IsRunning", false);
                break;

            case CreatureState.Wander:
                Wander();
                anim.SetBool("IsWalking", true);
                anim.SetBool("IsRunning", false);
                break;

            case CreatureState.WalkTowards:
                WalkTowards();
                anim.SetBool("IsWalking", false);
                anim.SetBool("IsRunning", true);
                break;

            case CreatureState.Flee:
                Flee();
                break;

            case CreatureState.Charging:
                Charging();
                break;

            case CreatureState.FetchCrop:
                FetchCrop();
                anim.SetBool("IsWalking", false);
                anim.SetBool("IsRunning", true);
                break;

            case CreatureState.Stun:
                anim.SetBool("IsWalking", false);
                anim.SetBool("IsRunning", false);
                break;

            case CreatureState.Die:
                // OnDeath();
                break;

            case CreatureState.FollowParent:
                FollowParent();
                break;

            default:
                Debug.LogError("Unknown state: " + currentState);
                break;
        }
    }

    #region WanderingFunctions
    public void Wander()
    {
        if (playerInSightRange || inWilderness)
        {
            currentState = CreatureState.WalkTowards;
            return;
        }

        if (!isMoving && currentState == CreatureState.Wander)
        {
            agent.speed = walkSpeed;
            Vector3 randomPoint;
            if(inWilderness) randomPoint = GetRandomPointAround(transform.position, 10f);
            else randomPoint = StructureManager.Instance.GetRandomTile();
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

        if (TimeManager.Instance.isDay && !inWilderness && Tutorial.Instance == null) destination = despawnPos;

        agent.destination = destination;

        float timeSpent = 0; //to make sure it doesnt get stuck

        while ((agent.pathPending || agent.remainingDistance > agent.stoppingDistance) && timeSpent < 3)
        {
            timeSpent += Time.deltaTime;
            if (playerInSightRange)
            {
                currentState = CreatureState.WalkTowards;
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


    private void WalkTowards()
    {
        if (trackPlayerRoutine == null)
        {
            trackPlayerRoutine = StartCoroutine(TrackPlayer());
            target = player;
            agent.speed = runSpeed;
        }
        if(target == null)
        {
            if(playerInSightRange)
            {
                target = player;
            }
            else
            {
                currentState = CreatureState.Wander;
            }
            return;
        }

        if (target && playerInAttackRange)
        {
            StopTrackingPlayer();
            currentState = CreatureState.Charging;
        }
        else if (!playerInSightRange && !inWilderness)
        {
            currentState = CreatureState.Wander;
            target = null;
            StopTrackingPlayer();
        }
    }

    private IEnumerator TrackPlayer()
    {
        while ((playerInSightRange || inWilderness) && currentState == CreatureState.WalkTowards)
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

    private void Flee()
    {
        if(coroutineRunning) return;
        anim.SetBool("IsWalking", false);
        anim.SetBool("IsRunning", true);
        Vector3 runTo = transform.position + ((transform.position - player.transform.position + new Vector3(Random.Range(-3, 3), 0, Random.Range(-3, 3)) * 1));
        agent.destination = runTo;
        if(agent.speed > 0 && agent.speed != runSpeed) agent.speed = runSpeed;
        fleeTimeLeft -= Time.deltaTime;
        if(fleeTimeLeft <= 0)
        {
            if(heldItem) StartCoroutine(ConsumeItem());
            else currentState = CreatureState.Wander;
        }
    }

    void FetchCrop()
    {
        if(coroutineRunning)
        {
            return;
        }
        if (!foundFarmTile || !foundFarmTile.crop || playerInAttackRange || !target)
        {
            target = player.transform;
            currentState = CreatureState.WalkTowards;
            return;
        }

        /*if(CheckForObstacle(transform) != null)
        {
            targetStructure = CheckForObstacle(transform);
            target = targetStructure.transform;
            agent.destination = target.position;
        }*/

        if (Vector3.Distance(transform.position, target.position) < 2f)
        {
            agent.ResetPath();
            if(foundFarmTile && foundFarmTile.crop && foundFarmTile.harvestable)
            {
                currentState = CreatureState.Wander;
            }
            else
            {
                coroutineRunning = true;
                StartCoroutine(DigUpCrop());
            }
        }
        /*else if(agent.destination != target.position)
        {
            agent.destination = target.position;
        }*/
    }

    void CropCheck()
    {
        List<FarmLand> availableLands = new List<FarmLand>();
        foreach (StructureBehaviorScript structure in structManager.allStructs)
        {
            FarmLand potentialFarmTile = structure as FarmLand;
            if (potentialFarmTile && desiredCrops.Contains(potentialFarmTile.crop) && potentialFarmTile.harvestable)
            {
                availableLands.Add(potentialFarmTile);
            }
        }
        if (availableLands.Count > 0)
        {
            int r = Random.Range(0, availableLands.Count);
            foundFarmTile = availableLands[r];
            target = foundFarmTile.transform;
            currentState = CreatureState.FetchCrop;
            agent.destination = target.position;
        }
    }

    IEnumerator DigUpCrop()
    {
        float digTimeElapsed = 0;
        anim.Play("Dig");
        effectsHandler.Idle1();
        while(foundFarmTile && foundFarmTile.crop && foundFarmTile.harvestable && !isDead && digTimeElapsed < 2f)
        {
            digTimeElapsed += Time.deltaTime;
            agent.SetDestination(target.position);
            yield return null;
        }
        if(digTimeElapsed >= 2f)
        {
            heldItem = foundFarmTile.crop.cropYield;
            r.sprite = heldItem.icon;
            foundFarmTile.CropDestroyed();
            foundFarmTile = null;
            
            fleeTimeLeft = Random.Range(6, 12);
            currentState = CreatureState.Flee;
        }
        else currentState = CreatureState.Wander;
        coroutineRunning = false;
    }

    IEnumerator ConsumeItem()
    {
        anim.SetBool("IsRunning", false);
        anim.Play("Chew");
        effectsHandler.Idle2();
        agent.SetDestination(transform.position);
        coroutineRunning = true;
        yield return new WaitForSeconds(2.3f);
        if(heldItem == foxGlove)
        {
            TakeDamage(999);
        }
        else if(heldItem == dare)
        {
            ApplyStatusEffect(StatusDatabase.Instance.GetStatus(StatusEffectName.Dare), 30);
        }
        else
        {
            currentState = CreatureState.Wander;
            health = maxHealth;
        }
        r.sprite = null;
        heldItem = null;
        coroutineRunning = false;
    }
    #endregion

    void Charging()
    {
        if(chargeRoutine != null || coroutineRunning) return;

        chargeRoutine = StartCoroutine(ChargeRoutine());
    }

    IEnumerator ChargeRoutine()
    {
        float chargeTimeElapsed = 0;
        //
        anim.SetBool("ChargePrep", true);
        anim.SetBool("IsWalking", false);
        anim.SetBool("IsRunning", true);
        agent.speed = 0;
        agent.ResetPath();
        faceTarget = true;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        bearTrapVulnerable = false;
        yield return new WaitForSeconds(beginChargeTime); //Beginning to charge

        //Actively Charging
        effectsHandler.MiscSound2();
        anim.SetBool("ChargePrep", false);
        dashParticles.Play();
        faceTarget = false;
        agent.speed = chargeSpeed;
        isCharging = true;
        attackHitbox.enabled = true;
        while(isCharging && chargeTimeElapsed < chargeTime)
        {
            chargeTimeElapsed += Time.deltaTime;
            agent.SetDestination(chargePosition.position);
            yield return null;
        }
        attackHitbox.enabled = false;
        dashParticles.Stop();
        if(chargeTimeElapsed >= chargeTime)
        {
            recoilTime = 1.5f;
            if(!anim.GetBool("Attacked") && !anim.GetBool("Recoiled")) 
            {
                anim.SetTrigger("Missed");
                chargeParticles.Play();
            }
        }
        if(anim.GetBool("Recoiled")) agent.speed = 0;
        isCharging = false;
        agent.ResetPath();
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        anim.SetBool("IsRunning", false);
        yield return new WaitForSeconds(0.5f);
        chargeParticles.Stop();
        yield return new WaitForSeconds(recoilTime); //Charge Cooldown
        bearTrapVulnerable = true;

        //Should probably flee for about 5 seconds or so to prevent constant charging
        agent.speed = runSpeed;
        currentState = CreatureState.Idle;
        chargeRoutine = null;

    }


    private void Idle()
    {
        if (playerInSightRange)
        {
            currentState = CreatureState.WalkTowards;
            return;
        }

        if (!coroutineRunning)
        {
            int r = Random.Range(0, 13);
            if(r < 5 && variant != Variant.Tiny && !inWilderness)
            {
                CropCheck();
            }
            else if (r < 8)
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
        if(isDead || !isCharging) return;
        if (other.CompareTag("Player"))
        {
            PlayerInteraction playerInteraction = other.GetComponent<PlayerInteraction>();
            if (playerInteraction != null)
            {
                playerInteraction.StaminaChange(damageToPlayer);
                attackHitbox.enabled = false;
                if(!anim.GetBool("Recoiled")) anim.SetTrigger("Attacked");
                recoilTime = 1.7f;
                isCharging = false;
                return;
            }
        }
        if(other.gameObject.layer == 6)
        {
            var structure = other.GetComponentInParent<StructureBehaviorScript>();
            if (structure != null && structure.isObstacle)
            {
                if(!structure.destructable) //Hit a tree
                {
                    attackHitbox.enabled = false;
                    recoilTime = 3f;
                    if(!anim.GetBool("Attacked")) anim.SetTrigger("Recoiled");
                    isCharging = false;
                }
                else if(structure.health <= damageToStructure) //Broke it
                {
                    structure.TakeDamage(damageToStructure);
                    attackHitbox.enabled = false;
                    if(!anim.GetBool("Recoiled")) anim.SetTrigger("Attacked");
                    recoilTime = 1.7f;
                    isCharging = false;
                }
                else //Dealth damage
                {
                    structure.TakeDamage(damageToStructure);
                    attackHitbox.enabled = false;
                    if(!anim.GetBool("Attacked")) anim.SetTrigger("Recoiled");
                    recoilTime = 3f;
                    isCharging = false;
                }
                return;
            }           
        }

        if(other.gameObject.layer == 9)
        {
            var creature = other.GetComponentInParent<CreatureBehaviorScript>();
            if (creature != null && creature.shovelVulnerable && (creature.creatureData != creatureData || creature.health <= 0) && variant != Variant.Tiny)
            {
                creature.TakeDamage(30);
                creature.PlayHitParticle(new Vector3(0,0,0));
            }
        }

        if(other.gameObject.layer == 0 || other.gameObject.layer == 7)
        {
            return; //causes wilderness issues

            attackHitbox.enabled = false;
            if(!anim.GetBool("Attacked")) anim.SetTrigger("Recoiled");
            recoilTime = 2f;
            isCharging = false;
            return;
        }
    }

    public override bool OnBearTrapStun(StructureBehaviorScript b)
    {
        if (currentState != CreatureState.Stun)
        {
            StartCoroutine(BearTrapHold(b));
            faceTarget = false;
            agent.destination = transform.position;
            agent.ResetPath();
            return true;
        }
        return false;
    }

    private IEnumerator BearTrapHold(StructureBehaviorScript b)
    {
        currentState = CreatureState.Stun;
        coroutineRunning = false;
        StopTrackingPlayer();
        if(walkRoutine != null)
        {
            StopCoroutine(walkRoutine);
            walkRoutine = null;
        }
        agent.ResetPath();
        anim.SetBool("IsWalking", false);
        anim.SetBool("IsRunning", false);
        while (b && b.health > 0)
        {
            yield return null;
        }
        //StartCoroutine(IdleSoundTimer());
        currentState = CreatureState.Wander;
        bearTrapVulnerable = true;
        
    }

    /*public override bool OnStun(float duration)
    {
        if (currentState != CreatureState.Stun)
        {
            StartCoroutine(Stun(duration));
            StopCoroutine(ChargeRoutine());
            faceTarget = false;
            agent.destination = transform.position;
            agent.ResetPath();
            return true;
        }
        return false;
    }

    private IEnumerator Stun(float duration)
    {
        currentState = CreatureState.Stun;
        coroutineRunning = false;
        StopTrackingPlayer();
        if(walkRoutine != null)
        {
            StopCoroutine(walkRoutine);
            walkRoutine = null;
        }
        agent.ResetPath();
        anim.SetBool("IsWalking", false);
        anim.SetBool("IsRunning", false);
        yield return new WaitForSeconds(duration);
        //StartCoroutine(IdleSoundTimer());
        currentState = CreatureState.Wander;
        bearTrapVulnerable = true;
    }*/

    public override void OnDamage()
    {
        if(health > 0) effectsHandler.OnHit();
        if(currentState == CreatureState.FollowParent)
        {
            fleeTimeLeft = Random.Range(10, 12);
            currentState = CreatureState.Flee;
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
            dashParticles.Stop();
            chargeParticles.Stop();
            StopAllCoroutines();
        }
    }

    public void OnDestroy()
    {
        base.OnDestroy();
        if(!gameObject.scene.isLoaded) return;
        if(heldItem && health < 0)
        {
            GameObject droppedItem = ItemPoolManager.Instance.GrabItem(heldItem);
            droppedItem.transform.position = corpseParticleTransform.position;
        }
    }

    IEnumerator IdleSoundTimer()
    {
        while(health > 0)
        {
            int i = Random.Range(2,5);
            effectsHandler.RandomIdle();
            yield return new WaitForSeconds(i);
        }
    }

    public override void NewPriorityTarget(StructureBehaviorScript newStruct)
    {
        if(targetStructure && targetStructure == newStruct) return;
        targetStructure = newStruct;
        target = newStruct.transform;
        if(currentState == CreatureState.Idle || currentState == CreatureState.Wander || currentState == CreatureState.WalkTowards) currentState = CreatureState.Charging;
        
    }

    public void FollowParent()
    {
        //follow parent at a set distance
        if (parent.isDead)
        {
            fleeTimeLeft = Random.Range(6, 12);
            currentState = CreatureState.Flee;
            return;
        }

        if (!coroutineRunning)
        {
            agent.speed = runSpeed;
            walkRoutine = StartCoroutine(TrackParent());
        }

        if(agent.velocity.sqrMagnitude > 0) anim.SetBool("IsRunning", true);
        else anim.SetBool("IsRunning", false);
        
    }

    private IEnumerator TrackParent()
    {
        coroutineRunning = true;
        while (currentState == CreatureState.FollowParent && parent)
        {
            agent.destination = parent.transform.position;
            yield return new WaitForSeconds(1.2f); // update destination every 0.5 seconds to prevent overloading it
        }
        coroutineRunning = false;
    }

}
