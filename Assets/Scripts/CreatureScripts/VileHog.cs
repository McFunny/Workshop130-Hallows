using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class VileHog : CreatureBehaviorScript
{
    public Variant variant; // what variant of creature is this?

    public VileHog parent;
    public VileHog[] babies;

    //public List<CropData> desiredCrops; // what crops does this creature want to eat
    public List<CropData> undesiredCrops; // what crops does this creature ignore

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
    public float walkSpeed = 4;
    public float runSpeed = 8;
    public float chargeSpeed = 14;
    float accelerateSpeed;
    float thrusterSpeed = 30;
    bool faceTarget;

    private Vector3 despawnPos;

    private Coroutine trackPlayerRoutine, walkRoutine, chargeRoutine; 

    bool thrustersReady = false; //Cult hog only
    bool usingThrusters = false;

    public ParticleSystem exhaustL, exhaustR;
    public GameObject thrusterParticles;
    public GameObject armor;

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
        Chunky, //unused
        Tiny,
        Armored,
        Corrupted
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

        if(variant == Variant.Armored) thrustersReady = true;

        accelerateSpeed = agent.acceleration;

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

        if (Vector3.Distance(transform.position, target.position) < 2f)
        {
            agent.ResetPath();
            if(foundFarmTile && foundFarmTile.crop && foundFarmTile.harvestable)
            {
                coroutineRunning = true;
                StartCoroutine(DigUpCrop());
            }
            else
            {
                currentState = CreatureState.Wander;
            }
        }
    }

    void CropCheck()
    {
        List<FarmLand> availableLands = new List<FarmLand>();
        foreach (StructureBehaviorScript structure in structManager.allStructs)
        {
            FarmLand potentialFarmTile = structure as FarmLand;
            if (potentialFarmTile && !undesiredCrops.Contains(potentialFarmTile.crop) && potentialFarmTile.harvestable && !potentialFarmTile.rotted)
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
        if(digTimeElapsed >= 2f && foundFarmTile.crop)
        {
            heldItem = foundFarmTile.crop.cropYield;
            r.sprite = heldItem.icon;
            if(!foundFarmTile.crop.behavior || foundFarmTile.crop.behavior.WasFullyEaten(foundFarmTile, this) == true) foundFarmTile.CropDestroyed();
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
        usingThrusters = false;
        if(thrustersReady && armor)
        {
            usingThrusters = true;
            StartCoroutine(ThrusterRecharge());
        }

        float chargeTimeElapsed = 0;
        //
        anim.ResetTrigger("Recoiled");
        anim.ResetTrigger("Attacked");
        anim.ResetTrigger("Missed");
        anim.SetBool("ChargePrep", true);
        anim.SetBool("IsWalking", false);
        anim.SetBool("IsRunning", true);

        agent.speed = 0;
        agent.ResetPath();
        faceTarget = true;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        //bearTrapVulnerable = false;
        if(usingThrusters)
        {
            yield return new WaitForSeconds(0.4f/actionSpeedMod);
            agent.acceleration = thrusterSpeed;
            effectsHandler.PlayExtraSound(0);
            thrusterParticles.SetActive(true);
        }
        else yield return new WaitForSeconds(beginChargeTime/actionSpeedMod); //Beginning to charge

        //Actively Charging
        effectsHandler.MiscSound2();
        anim.SetBool("ChargePrep", false);
        dashParticles.Play();
        faceTarget = false;
        agent.speed = chargeSpeed;
        if(usingThrusters)
        {
            agent.speed += 12;
            chargeTimeElapsed += 1;
        }
        isCharging = true;
        attackHitbox.enabled = true;
        while(isCharging && chargeTimeElapsed < chargeTime)
        {
            chargeTimeElapsed += Time.deltaTime;
            if (NavMesh.SamplePosition(chargePosition.position, out var hit, 1.0f, NavMesh.AllAreas)) agent.SetDestination(hit.position);
            else if (agent.pathStatus != NavMeshPathStatus.PathComplete) agent.Move(transform.forward * agent.speed * Time.deltaTime);
            //agent.SetDestination(chargePosition.position);
            yield return null;
        }
        if(usingThrusters) thrusterParticles.SetActive(false);
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
        allColliders[0].enabled = false; //Untested method of catching them in beartraps post charge
        //bearTrapVulnerable = true;
        allColliders[0].enabled = true;

        agent.speed = runSpeed;

        yield return new WaitForSeconds(0.1f);
        if(currentState == CreatureState.Charging) currentState = CreatureState.Idle;
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
        if (isDead || !isCharging) return;
        if (other.CompareTag("Player"))
        {
            PlayerInteraction playerInteraction = other.GetComponent<PlayerInteraction>();
            if (playerInteraction != null)
            {
                int extraDamage = 0;
                if (usingThrusters) extraDamage += 15;
                playerInteraction.StaminaChange(damageToPlayer - extraDamage, corpseParticleTransform.position);
                playerInteraction.PlayerTrip();
                attackHitbox.enabled = false;
                if (!anim.GetBool("Recoiled")) anim.SetTrigger("Attacked");
                recoilTime = 1.7f;
                isCharging = false;
                return;
            }
        }
        if (other.gameObject.layer == 6)
        {
            var structure = other.GetComponentInParent<StructureBehaviorScript>();
            if (structure != null && (structure.isObstacle || !structure.destructable))
            {
                if (structure as PlacedHoe || structure as PlacedTorch) return;

                int extraDamage = 0;
                if (usingThrusters) extraDamage += 10;
                HitStructureParticle(structure.transform.position);
                if (!structure.destructable) //Hit a tree
                {
                    structure.TakeDamage(damageToStructure + extraDamage);
                    attackHitbox.enabled = false;
                    recoilTime = 2.5f;
                    if (!anim.GetBool("Attacked")) anim.SetTrigger("Recoiled");
                    isCharging = false;
                    agent.ResetPath();
                    agent.speed = 0;
                }
                else if (structure.health <= (damageToStructure + extraDamage)) //Broke it
                {
                    structure.TakeDamage(damageToStructure + extraDamage);
                    attackHitbox.enabled = false;
                    if (!anim.GetBool("Recoiled")) anim.SetTrigger("Attacked");
                    recoilTime = 1.7f;
                    isCharging = false;
                }
                else //Dealt damage
                {
                    structure.TakeDamage(damageToStructure + extraDamage);
                    attackHitbox.enabled = false;
                    if (!anim.GetBool("Attacked")) anim.SetTrigger("Recoiled");
                    recoilTime = 2.5f;
                    isCharging = false;
                    agent.ResetPath();
                    agent.speed = 0;
                }

                if (variant == Variant.Corrupted && Random.Range(0, 10) > 2) CorruptionExplosion();
                return;
            }

            if (other.TryGetComponent<Burrow>(out Burrow burrow)) burrow.TakeDamage(20);
        }

        if (other.gameObject.layer == 9)
        {
            var creature = other.GetComponentInParent<CreatureBehaviorScript>();
            if (creature != null && creature.shovelVulnerable && (creature.creatureData != creatureData || creature.health <= 0) && variant != Variant.Tiny)
            {
                float extraDamage = 0;
                if (usingThrusters) extraDamage += 50;
                creature.lastDamageTypeTaken = DamageType.HogCharge;
                creature.TakeDamage(50 + extraDamage);
                creature.PlayHitParticle(new Vector3(0, 0, 0));

                if (creature.health <= 0)
                {
                    AchievementManager.Instance.NotitfyCreatureKilledByCreature(creature.creatureData, this.creatureData);
                    if(other.TryGetComponent<PyreFly>(out PyreFly pyreFly))
                    {
                        //pyreFly.DropDisk();
                    }
                }

                if(creature.corpseType == CorpseParticleType.Stone)
                {
                    if(!anim.GetBool("Attacked")) anim.SetTrigger("Recoiled");
                    recoilTime = 2.1f;
                    isCharging = false;
                }

            }

            if (other.gameObject.layer == 0 || other.gameObject.layer == 7)
            {
                return; //causes wilderness issues

                attackHitbox.enabled = false;
                if (!anim.GetBool("Attacked")) anim.SetTrigger("Recoiled");
                recoilTime = 2f;
                isCharging = false;
                return;
            }

            var bug = other.GetComponentInParent<BugBehaviorScript>();
            if (bug != null)
            {
                bug.Struck();
            }
        }
    }

    public override bool OnBearTrapStun(StructureBehaviorScript b)
    {
        if (currentState != CreatureState.Stun)
        {
            ResetCharging();
            StartCoroutine(BearTrapHold(b));
            return true;
        }
        return false;
    }

    private IEnumerator BearTrapHold(StructureBehaviorScript b)
    {
        while (b && b.health > 0)
        {
            yield return null;
        }
        //StartCoroutine(IdleSoundTimer());
        currentState = CreatureState.Wander;
        bearTrapVulnerable = true;
        
    }

    void ResetCharging() //For when its charge is interrupted
    {
        currentState = CreatureState.Stun;
        faceTarget = false;
        coroutineRunning = false;
        StopTrackingPlayer();
        if(walkRoutine != null)
        {
            StopCoroutine(walkRoutine);
            walkRoutine = null;
        }
        agent.ResetPath();
        agent.velocity = Vector3.zero;
        anim.SetBool("IsWalking", false);
        anim.SetBool("IsRunning", false);
        anim.SetBool("ChargePrep", false);

        if(usingThrusters) thrusterParticles.SetActive(false);
        usingThrusters = false;
        isCharging = false;
        attackHitbox.enabled = false;
        dashParticles.Stop();
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        chargeParticles.Stop();
        agent.speed = runSpeed;
    }

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

            if(variant == Variant.Corrupted) StartCoroutine(CorpseExplosionTimer());
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

    public override void OnCorpseDamage()
    {
        if(health <= 0 && canCorpseBreak)
        {
            anim.Play("DeathRecoil", -1, 0f);
        }
    }

    IEnumerator ThrusterRecharge()
    {
        thrustersReady = false;
        exhaustL.Play();
        exhaustR.Play();
        int time = 0;
        while(time < 15)
        {
            if(!armor) time = 15;
            time++;
            yield return new WaitForSeconds(1);
        }
        thrustersReady = true;
        exhaustL.Stop();
        exhaustR.Stop();
    }

    IEnumerator CorpseExplosionTimer()
    {
        if(!inWilderness) yield return new WaitForSeconds(Random.Range(15, 45));
        else yield return new WaitForSeconds(Random.Range(0f, 2f));
        CorruptionExplosion();
    }

    void CorruptionExplosion()
    {
        canCorpseBreak = true;
        TakeDamage(999);
        CorruptionManager.Instance.CorruptionExplosion(transform.position, 5);
        //stagger player
        if(Vector3.Distance(transform.position, PlayerInteraction.Instance.playerFeet.position) <= 5) PlayerInteraction.Instance.PlayerTrip();

        GameObject corpseParticle = ParticlePoolManager.Instance.GrabCorpseParticle(corpseType);
        if(corpseParticle)
        {
            if(corpseParticleTransform) corpseParticle.transform.position = corpseParticleTransform.position;
            else corpseParticle.transform.position = transform.position;
        }
    }

}
