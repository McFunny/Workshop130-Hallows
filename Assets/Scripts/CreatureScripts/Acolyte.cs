using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Acolyte : CreatureBehaviorScript
{
    public Collider attackCollider;
    bool isCharging = false;
    bool grabbedPlayer, slammedPlayer;

    bool isMoving, coroutineRunning; //ISMOVING TRACKS IF THE MOVEMENT COROUTINE IS PLAYING. COROUTINERUNNING CHECKS IF ANY *OTHER* COROUTINE IS RUNNING
    
    [HideInInspector] public NavMeshAgent agent;

    private Vector3 despawnPos;

    public List<StructureObject> targettableStructures;
    private StructureBehaviorScript targetStructure;

    public List<CreatureObject> targettableCreatures;
    private CreatureBehaviorScript targetCreature;

    public GameObject ballProjectile;
    public Transform bulletSpawn;


    bool interruptAction = false;
    bool spellCooldown = false;
    bool faceTarget;
    bool recoilCooldown = false;
    bool chargeCooldown = false;

    float baseSpeed;
    public float strafeSpeed;
    public float chargeSpeed;
    float chargeTimeElapsed = 0;
    bool strafeLeft = false;

    private Coroutine chargeRoutine, grabRoutine, spellRoutine;

    public Transform strafePointL, strafePointR, chargePoint, playerHoldPoint;

    public Transform modelGrounded;
    Vector3 modelFloatPos;
    public GameObject model;

    public ParticleSystem teleportParticles, missChargeParticles;


    public enum CreatureState
    {
        Idle,
        Wander,
        AttackPlayer,
        Charging,
        AttackStructure,
        ReviveCreature, //Like grubs that get too close or unlit pyreflies
        Stun
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


        StartCoroutine(ScanForTargets());

        baseSpeed = agent.speed;

        modelFloatPos = model.transform.position;
    }

    void Update()
    {
        if (health <= 0 && !grabbedPlayer) isDead = true;

        if(isDead) return;

        if(currentState != CreatureState.Charging)
        {
            float newSpeed = baseSpeed;
            if(currentState == CreatureState.AttackPlayer) newSpeed = strafeSpeed;

            if(spellCooldown) newSpeed -= 3;

            agent.speed = newSpeed;
        }

        FloatAnimToggle();


        float distance = Vector3.Distance(player.position, transform.position);
        if(!playerInSightRange) playerInSightRange = distance <= sightRange;
        else playerInSightRange = distance <= sightRange + 10;
        playerInAttackRange = distance <= attackRange;

        if (!isDead && currentState != CreatureState.Stun)
        {
            CheckState(currentState);
        }

        if(faceTarget)
        {
            Vector3 targetPosition = player.position;

            Vector3 direction = targetPosition - transform.position;
            direction.y = 0;
            Quaternion toRotation = Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, 15f * Time.deltaTime);
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
            
            case CreatureState.Charging:
                //Charge();
                break;

            case CreatureState.AttackPlayer:
                Wander();
                break;

            case CreatureState.ReviveCreature:
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
        if(coroutineRunning) return; 


        if(currentState == CreatureState.Wander && targetStructure)
        {
            currentState = CreatureState.AttackStructure;
        }

        /*if(currentState == CreatureState.Wander && targetCreature)
        {
            currentState = CreatureState.ReviveCreature;
        }*/

        if((currentState == CreatureState.Wander && playerInSightRange) || playerInAttackRange)
        {
            currentState = CreatureState.AttackPlayer;
        }

        if (!isMoving)
        {
            if(currentState == CreatureState.Wander) //Wandering
            {
                agent.updateRotation = true;
                Vector3 randomPoint;
                if(!patrolPoint) randomPoint = StructureManager.Instance.GetRandomTile();
                else randomPoint = PointAroundPatrolPoint(7);
                StartCoroutine(MoveToPoint(randomPoint, Random.Range(2f, 3.5f)));
            }

            else if(currentState == CreatureState.AttackPlayer) ///isCharging Player
            {
                agent.updateRotation = false;
                faceTarget = true;
                if(playerInSightRange)
                {
                    float r = Random.Range(0, 10);
                    //transform.LookAt(player.position);

                    Vector3 strafePos;

                    if(r > 7) strafeLeft = !strafeLeft;

                    if(strafeLeft) strafePos = strafePointL.position;
                    else strafePos = strafePointR.position;

                    Vector3 retreatDir = (transform.position - player.position).normalized;

                    if(Vector3.Distance(transform.position, player.position) < 10) strafePos += retreatDir * 5;

                    if(playerInAttackRange) StartCoroutine(MoveToPoint(strafePos, Random.Range(0.7f,0.9f)));

                    else StartCoroutine(MoveToPoint(player.position, 0.3f));
                }
                else 
                {
                    StartCoroutine(MoveToPoint(player.position, 0.3f));
                }

            }

            else if(currentState == CreatureState.AttackStructure) ///isCharging Structure
            {
                agent.updateRotation = true;
                if(!targetStructure)
                {
                    targetStructure = null;
                    currentState = CreatureState.Wander;
                    return;
                }
                StartCoroutine(MoveToPoint(targetStructure.transform.position, 1.5f));

                if(Vector3.Distance(transform.position, targetStructure.transform.position) < 5) interruptAction = true;
            }

            /*else if(currentState == CreatureState.ReviveCreature) ///Reviving Creature
            {
                if(!targetCreature)
                {
                    currentState = CreatureState.Wander;
                    return;
                }
                agent.updateRotation = true;
                StartCoroutine(MoveToPoint(targetCreature.transform.position, 10));

                if(Vector3.Distance(transform.position, targetCreature.transform.position) <= 5) interruptAction = true;
            }*/
            
        }
    }

    private IEnumerator WaitAround()
    {
        coroutineRunning = true;
        float r = Random.Range(2f, 4.5f);
        float timeElapsed = 0;
        agent.ResetPath();

        while(timeElapsed < r)
        {
            yield return new WaitForSeconds(0.1f);
            timeElapsed += 0.1f;
            if(playerInSightRange) timeElapsed += 0.3f;
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
            destination = despawnPos;
        } 

        agent.destination = destination;

        float timeSpent = 0; //to make sure it doesnt get stuck

        while ((agent.pathPending || agent.remainingDistance > agent.stoppingDistance) && timeSpent < maxTime)
        {
            timeSpent += Time.deltaTime;

            //FaceTarget();

            if(interruptAction) timeSpent += 30;
            yield return null;
        }

        FinishedMoving();
    }

    /*void FaceTarget()
    {
        if(agent.updateRotation == false && faceTarget)
        {
            Vector3 directionToTarget = player.position - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 1.2f * Time.deltaTime);
        }
    }*/

    void FinishedMoving()
    {
        if(!playerInSightRange)
        {
            faceTarget = false;
            agent.updateRotation = true;
        }

        if (currentState == CreatureState.Wander)
        {
            currentState = CreatureState.Idle;
        }

        if(currentState == CreatureState.AttackStructure && targetStructure)
        {
            if(Vector3.Distance(targetStructure.transform.position, transform.position) < 10f && !spellCooldown)
            {
                agent.Stop();
                spellRoutine = StartCoroutine(ThrowSpell());
            }
        }

        if(currentState == CreatureState.AttackPlayer)
        {
            if(playerInAttackRange && Random.Range(0,10) == 1 && !isCharging && StructureManager.Instance.ValidateGridType(transform.position, GridType.Farm) 
                && !chargeCooldown && TimeManager.Instance.currentHour != 7)
            {
                //Do the lunge attack
                //agent.Stop();
                currentState = CreatureState.Charging;
                StartCoroutine(LungeAttack());
                agent.updateRotation = true;
                return;
            }
            else if(playerInSightRange && Random.Range(0,10) > 2 && !spellCooldown)
            {
                spellRoutine = StartCoroutine(ThrowSpell());
                //return;
            }

            if(playerInSightRange == false) currentState = CreatureState.Wander;
        }

        isMoving = false;
        //coroutineRunning = false;
        interruptAction = false;
    }

    IEnumerator ThrowSpell()
    {
        spellCooldown = true;
        anim.Play("CultSpell");
        effectsHandler.RandomIdle();
        yield return new WaitForSeconds(0.6f);
        effectsHandler.MiscSound2();
        GameObject newBullet = Instantiate(ballProjectile, bulletSpawn.position, bulletSpawn.rotation);
        newBullet.GetComponent<ShadowProjectile>().sourceCreature = this;
        //newBullet.transform.position = bulletSpawn;
        //newBullet.transform.rotation = corpseParticleTransform.rotation;
        //Vector3 dir = new Vector3(Random.Range(-1,1), 0, Random.Range(-1,1));
        //newBullet.GetComponent<Rigidbody>().AddForce(Vector3.up * Random.Range(40,60));
        newBullet.GetComponent<Rigidbody>().AddForce(bulletSpawn.forward * Random.Range(40,90));

        if(!targetStructure && Random.Range(0, 15) == 1 || (Vector3.Distance(transform.position, player.position) < 10 && Random.Range(0, 4) == 1)) 
        {
            teleportParticles.Play();
            yield return new WaitForSeconds(0.5f);
            Teleport();
        }
        if(health > 75) yield return new WaitForSeconds(Random.Range(1f, 3f));
        else yield return new WaitForSeconds(Random.Range(0.5f, 2f));
        spellCooldown = false;

        spellRoutine = null;
    }

    void Teleport()
    {
        effectsHandler.MiscSound();
        Vector3 newPos = StructureManager.Instance.GetRandomClearTile();
        teleportParticles.Play();
        if(newPos != Vector3.zero) transform.position = newPos;
    }

    IEnumerator IdleSoundTimer()
    {
        while(health > 0)
        {
            int i = Random.Range(5,15);
            effectsHandler.RandomIdle();
            yield return new WaitForSeconds(i);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(isDead || !isCharging) return;
        if (other.CompareTag("Player"))
        {
            PlayerInteraction playerInteraction = other.GetComponent<PlayerInteraction>();
            if (playerInteraction != null && !playerInteraction.TripCheck() && PlayerMovement.restrictMovementTokens == 0)
            {
                faceTarget = false;
                grabbedPlayer = true;
                grabRoutine = StartCoroutine(GrabPlayer());
                chargeTimeElapsed -= 0.5f;
                return;
            }
        }
        if(other.gameObject.layer == 6)
        {
            if(!grabbedPlayer) return;
            var structure = other.GetComponentInParent<StructureBehaviorScript>();
            if (structure != null && (structure.isObstacle || !structure.destructable))
            {
                if(structure as PlacedHoe || structure as PlacedTorch) return;

                HitStructureParticle(structure.transform.position);
                isCharging = false;
                return;
            }       
        }
    }

    IEnumerator LungeAttack()
    {
        chargeTimeElapsed = 0;
        float chargeTime = 1.5f;
        //
        anim.SetBool("IsGrabbing", true);
        anim.SetBool("GrabSuccess", false);
        anim.Play("CultGrabStart");
        agent.speed = 0;
        agent.ResetPath();
        agent.updateRotation = false;
        faceTarget = true;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;

        chargeCooldown = true;

        effectsHandler.PlayExtraSound(1, 1f);

        yield return new WaitForSeconds(0.5f); //Beginning to charge

        //Actively Charging

        //faceTarget = false;
        agent.speed = chargeSpeed;

        isCharging = true;
        attackCollider.enabled = true;
        while(isCharging && chargeTimeElapsed < chargeTime)
        {
            chargeTimeElapsed += Time.deltaTime;
            if (NavMesh.SamplePosition(chargePoint.position, out var hit, 1.0f, NavMesh.AllAreas)) agent.SetDestination(hit.position);
            else if (agent.pathStatus != NavMeshPathStatus.PathComplete) agent.Move(transform.forward * agent.speed * Time.deltaTime);
            yield return null;
        }
        attackCollider.enabled = false;

        faceTarget = false;

        if(chargeTimeElapsed >= chargeTime) //Throw/miss
        {
            if(grabbedPlayer) //Throw
            {
                anim.SetBool("GrabSuccess", true);
                anim.SetBool("IsGrabbing", false);
            }
            else //Miss
            {
                anim.SetBool("GrabSuccess", false);
                anim.SetBool("IsGrabbing", false);
            }
        }
        else //Slam
        {
            anim.SetBool("GrabSuccess", true);
            anim.SetBool("IsGrabbing", false);
            slammedPlayer = true;
            //PlayerInteraction.Instance.StaminaChange(damageToPlayer);
            //PlayerInteraction.Instance.PlayerTrip();
        }
        agent.speed = 0;
        isCharging = false;
        agent.ResetPath();
        agent.velocity = Vector3.zero;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;

        yield return new WaitForSeconds(0.2f);
        grabbedPlayer = false;

        if(anim.GetBool("GrabSuccess") == false)
        {
            modelFloatPos = model.transform.position;
            model.transform.position = modelGrounded.position;
            yield return new WaitForSeconds(0.5f);
            missChargeParticles.Play();
            effectsHandler.PlayExtraSound(0, 1f);
        }
        yield return new WaitForSeconds(1.5f);

        if(anim.GetBool("GrabSuccess") == false)
        {
            yield return new WaitForSeconds(1f);
            model.transform.position = modelFloatPos;
        }
        else
        {
            teleportParticles.Play();
            yield return new WaitForSeconds(0.5f);
            Teleport();
        }
        if(currentState == CreatureState.Charging) currentState = CreatureState.Wander;
        
        chargeRoutine = null;
        agent.updateRotation = true;

        interruptAction = false;

        chargeCooldown = false;
        isMoving = false;
    }

    IEnumerator GrabPlayer()
    {
        foreach(Collider collider in allColliders)
        {
            collider.isTrigger = true;
        }

        effectsHandler.PlayExtraSound(2, 1f);

        PlayerInteraction.Instance.ToggleTrip(true);
        PlayerMovement.restrictMovementTokens++;
        PlayerCam.Instance.NewObjectOfInterest(corpseParticleTransform.position);

        Vector3 playerStartPos = new Vector3(PlayerInteraction.Instance.transform.position.x, PlayerInteraction.Instance.transform.position.y, PlayerInteraction.Instance.transform.position.z);

        while(grabbedPlayer)
        {
            PlayerInteraction.Instance.transform.position = playerHoldPoint.position;
            yield return null;
        }
        PlayerInteraction.Instance.ToggleTrip(false);
        PlayerCam.Instance.ClearObjectOfInterest();


        yield return new WaitForSeconds(0.05f);

        //Player is still floating

        PlayerInteraction.Instance.transform.position = new Vector3(playerHoldPoint.position.x, playerStartPos.y, playerHoldPoint.position.z);

        if(!slammedPlayer)  PlayerInteraction.Instance.StaminaChange(Mathf.Floor(damageToPlayer * .75f));
        else  PlayerInteraction.Instance.StaminaChange(damageToPlayer);

        slammedPlayer = false;
        PlayerMovement.restrictMovementTokens--;

        yield return new WaitForSeconds(0.2f);

        if(!slammedPlayer) 
        {
            PlayerInteraction.Instance.PlayerTrip();
            effectsHandler.MiscSound3();
        }
        else 
        {
            PlayerInteraction.Instance.PlayerTripNoKnockback();
            effectsHandler.PlayExtraSound(0, 1f);
        }

        yield return new WaitForSeconds(1);

        foreach(Collider collider in allColliders)
        {
            collider.isTrigger = false;
        }

        grabRoutine = null;
    }

    IEnumerator ScanForTargets()
    {
        while(health > 0)
        {
            yield return new WaitForSeconds(10);
            if(targetStructure) continue;

            //FindNearbyStructure(15);
            if(!targetStructure) FindNearbyCorpse(15);
            
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
        }

        if (availableStructure.Count > 0)
        {
            int r = Random.Range(0, availableStructure.Count);
            targetStructure = availableStructure[r];
        }
    }

    void FindNearbyCorpse(float distance)
    {
        List<CreatureBehaviorScript> availableCreatures = new List<CreatureBehaviorScript>();
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, distance, 1 << 9);
        foreach(Collider collider in hitEnemies)
        {
            var creature = collider.GetComponentInParent<CreatureBehaviorScript>();
            if (creature != null && targettableCreatures.Contains(creature.creatureData) && creature.isDead)
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

    public override void NewPriorityTarget(StructureBehaviorScript newStruct)
    {
        if (currentState == CreatureState.Stun) return;
        interruptAction = true;
        targetStructure = newStruct;
    }

    void FloatAnimToggle()
    {
        bool idleFloat = true;
        if(agent.velocity.magnitude > 0.4f && currentState != CreatureState.Charging && currentState != CreatureState.AttackPlayer) idleFloat = false;

        anim.SetBool("IsMoving", !idleFloat);
    }

    public override void OnDamage()
    {
        if(recoilCooldown || currentState == CreatureState.Charging) return;

        if(spellRoutine != null) StopCoroutine(spellRoutine);
        spellRoutine = null;

        anim.Play("CultHit");

        StartCoroutine(RecoilRoutine());
    }

    IEnumerator RecoilRoutine()
    {
        recoilCooldown = true;
        spellCooldown = true;
        chargeCooldown = true;

        yield return new WaitForSeconds(0.5f);

        spellCooldown = false;
        chargeCooldown = false;

        yield return new WaitForSeconds(2);

        recoilCooldown = false;
    }

    public override void OnDeath()
    {
        if (!isDead)
        {
            isDead = true;
            anim.Play("CultDie");
            base.OnDeath();
            agent.enabled = false;
            rb.isKinematic = true;
            rb.freezeRotation = true;

            if(grabRoutine != null)
            {
                PlayerMovement.restrictMovementTokens = 0;
                PlayerInteraction.Instance.ToggleTrip(false);
            }

            StopAllCoroutines();

            model.transform.position = modelGrounded.position;
        }
    }

    public override void OnCorpseDamage()
    {
        if(health <= 0 && canCorpseBreak)
        {
            anim.Play("DeathRecoil", -1, 0f);
        }
    }

    void OnDestroy()
    {
        base.OnDestroy();
        if (!gameObject.scene.isLoaded) return; 
    }
}
