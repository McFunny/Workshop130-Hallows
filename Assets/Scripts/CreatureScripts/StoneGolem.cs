using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class StoneGolem : CreatureBehaviorScript
{
    public Variant variant;

    public bool knockBackVulnerable = true; // Disable when being knocked back, damaged, and grabbing rock

    public Collider swipeCollider, slamCollider, knockBackCollider;
    bool attacking = false; //Used for both slam and swipe
    bool knockedBack = false; // Used for when it is actively being pushed back
    bool canHurtStructure = false; // Used specifically during knockback for hitting structures

    private Coroutine knockBackRoutine, slamRoutine, swipeRoutine, damageRoutine; //Check which coroutine is running to determine damage to player

    bool isMoving, coroutineRunning; //ISMOVING TRACKS IF THE MOVEMENT COROUTINE IS PLAYING. COROUTINERUNNING CHECKS IF ANY *OTHER* COROUTINE IS RUNNING

    [HideInInspector] public NavMeshAgent agent;
    float baseSpeed;

    private Vector3 despawnPos;

    public List<StructureObject> targettableStructures;
    private StructureBehaviorScript targetStructure;

    bool interruptAction = false;

    public StructureObject burrowData, rockData;

    public Transform knockBackPoint;

    public ParticleSystem slamParticles, skidParticles, crashedParticles, damagedParticles;

    public GameObject headLight, deathParticles;

    public enum CreatureState
    {
        Idle,
        Wander,
        AttackPlayer,
        AttackStructure,
        Knockback, //Being pushed back
        Damaged //Doing the damaged animation + stun
    }

    public enum Variant
    {
        Normal,
        Goliath
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

        StartCoroutine(ScanForTargets());

        baseSpeed = agent.speed;

        if(variant == Variant.Goliath) knockBackVulnerable = false;

    }

    void Update()
    {
        if (health <= 0) isDead = true;

        if(isDead) return;

        if(agent.velocity.magnitude > 0.5f) anim.SetBool("IsMoving", true);
        else anim.SetBool("IsMoving", false);

        float distance = Vector3.Distance(player.position, transform.position);
        playerInSightRange = distance <= sightRange;
        playerInAttackRange = distance <= attackRange;

        if (!isDead)
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

            case CreatureState.Knockback:
                break;

            case CreatureState.Damaged:
                break;

            case CreatureState.AttackPlayer:
                Wander();
                break;

            default:
                Debug.LogError("Unknown state: " + currentState);
                break;
        }
    }

    public override void TakeDamage(float damage)
    {
        if(lastDamageTypeTaken == DamageType.Shovel && damage >= 40)
        {
            if(variant == Variant.Goliath) currentState = CreatureState.AttackPlayer;
            if(!knockBackVulnerable) return;
            //Knockback
            knockBackRoutine = StartCoroutine(KnockbackRoutine());
            lastDamageTypeTaken = DamageType.Null;
            return;
        }

        if(lastDamageTypeTaken == DamageType.Mine || lastDamageTypeTaken == DamageType.PyreflyExplosion)
        {
            //Recoil
            TakeRealDamage(100);
            lastDamageTypeTaken = DamageType.Null;
            return;
        }

        if(lastDamageTypeTaken == DamageType.HogCharge || lastDamageTypeTaken == DamageType.FrostProjectile || lastDamageTypeTaken == DamageType.Cannonball)
        {
            //Recoil
            TakeRealDamage(50);
            lastDamageTypeTaken = DamageType.Null;
            return;
        }
    }

    public override void TakeDamage(float damage, Vector3 source)
    {
        source.y = 0;
        if(lastDamageTypeTaken == DamageType.Shovel && damage >= 40) transform.LookAt(source);
        TakeDamage(damage);
    }

    void TakeRealDamage(float damage)
    {
        health -= damage;

        damagedParticles.Play();

        if(damageRoutine == null && health > 0) damageRoutine = StartCoroutine(DamagedRoutine());

        if(health <= 0 && !isDead)
        {
            base.TakeDamage(999);
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


        if(currentState == CreatureState.Wander && targetStructure) currentState = CreatureState.AttackStructure;

        if(CheckForObstacle(transform) != null)
        {
            StructureBehaviorScript obstacle = CheckForObstacle(transform);
            if(targettableStructures.Contains(obstacle.structData) && targetStructure != obstacle)
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
                Vector3 randomPoint;
                if(!patrolPoint) randomPoint = StructureManager.Instance.GetRandomTile();
                else randomPoint = PointAroundPatrolPoint(7);
                StartCoroutine(MoveToPoint(randomPoint, Random.Range(7f, 11f)));
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

                if(Vector3.Distance(transform.position, targetStructure.transform.position) < 5f) interruptAction = true;
            }

            else if(currentState == CreatureState.AttackPlayer) ///Attacking Player
            {
                if(!playerInSightRange)
                {
                    currentState = CreatureState.Wander;
                }
                else 
                {
                    StartCoroutine(MoveToPoint(player.position, 1f));
                }
            }
        }
        
        if(playerInAttackRange && currentState == CreatureState.AttackPlayer)
        {
            interruptAction = true;
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
            if(Vector3.Distance(targetStructure.transform.position, transform.position) < 5f)
            {
                agent.Stop();
                slamRoutine = StartCoroutine(SlamAttack());
                return;
            }
        }

        if(currentState == CreatureState.AttackPlayer)
        {
            if(playerInAttackRange)
            {
                agent.Stop();
                if(Random.Range(0,10) < 2) slamRoutine = StartCoroutine(SlamAttack());
                else swipeRoutine = StartCoroutine(SwipeAttack());
                return;
            }

            if(playerInSightRange == false) currentState = CreatureState.Wander;
        }


        isMoving = false;
        interruptAction = false;
    }

    IEnumerator IdleSoundTimer()
    {
        while(health > 0)
        {
            int i = Random.Range(6,10);
            effectsHandler.RandomIdle();
            yield return new WaitForSeconds(i);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isDead)
        {
            if(!attacking) return;

            PlayerInteraction playerInteraction = other.GetComponent<PlayerInteraction>();
            if (playerInteraction != null)
            {
                if(slamRoutine != null) playerInteraction.StaminaChange(damageToPlayer - 20); //Slam
                else 
                {
                    playerInteraction.StaminaChange(damageToPlayer); //Swipe
                    PlayerInteraction.Instance.PlayerTrip();
                }
                attacking = false;
                return;
            }
        }

        if(other.gameObject.layer == 9)
        {
            CreatureBehaviorScript c = other.gameObject.GetComponentInParent<CreatureBehaviorScript>();
            if(c)
            {
                if(attacking)
                {
                    c.TakeDamage(70);
                    ParticlePoolManager.Instance.GrabOrangeHitParticle().transform.position = c.corpseParticleTransform.position;
                    c.PlayHitParticle(c.transform.position);
                }
                else if(knockedBack)
                {
                    c.TakeDamage(20);
                    ParticlePoolManager.Instance.GrabOrangeHitParticle().transform.position = c.corpseParticleTransform.position;
                    c.PlayHitParticle(c.transform.position);
                }
            }
        }

        if(other.gameObject.layer == 6)
        {
            StructureBehaviorScript structure = other.gameObject.GetComponent<StructureBehaviorScript>();
            if(structure)
            {
                if(knockedBack)
                {
                    if(structure as PlacedHoe || structure as PlacedTorch) return;

                    if(!structure.destructable || structure.isObstacle) knockedBack = false;

                    if(!canHurtStructure) return;
                    HitStructureParticle(structure.transform.position);
                    structure.TakeDamage(15);
                    return;
                }

                if(attacking)
                {
                    HitStructureParticle(structure.transform.position);
                    structure.TakeDamage(damageToStructure);
                }
            }
        }
    }

    IEnumerator SwipeAttack()
    {
        coroutineRunning = true;
        anim.Play("Swipe");

        transform.LookAt(player.position);
        effectsHandler.RandomIdle();
        yield return new WaitForSeconds(0.7f);
        attacking = true;
        swipeCollider.enabled = true;
        effectsHandler.MiscSound3();
        yield return new WaitForSeconds(0.2f);
        swipeCollider.enabled = false;
        attacking = false;
        yield return new WaitForSeconds(0.8f);

        swipeRoutine = null;
        isMoving = false;
        coroutineRunning = false;
        interruptAction = false;

        agent.isStopped = false;
    }

    IEnumerator SlamAttack() //For hitting structures
    {
        coroutineRunning = true;
        anim.Play("Slam");
        effectsHandler.RandomIdle();

        if(targetStructure) transform.LookAt(targetStructure.transform.position);
        yield return new WaitForSeconds(2.3f);
        attacking = true;
        slamCollider.enabled = true;
        slamParticles.Play();
        effectsHandler.MiscSound();
        yield return new WaitForSeconds(0.2f);
        slamCollider.enabled = false;
        attacking = false;

        if(Vector3.Distance(player.position, transform.position) < 12) PlayerInteraction.Instance.PlayerTrip();

        Collider[] hitStructures = Physics.OverlapSphere(transform.position, 20, 1 << 6);
        foreach(Collider collider in hitStructures)
        {
            StructureBehaviorScript structure = collider.gameObject.GetComponentInParent<StructureBehaviorScript>();
            if(structure && structure.structData && structure.structData == burrowData)
            {
                HitStructureParticle(structure.transform.position);
                structure.TakeDamage(99);
            }
        }
        yield return new WaitForSeconds(1.5f);

        isMoving = false;
        coroutineRunning = false;
        interruptAction = false;

        slamRoutine = null;
        agent.isStopped = false;
        
    }

    IEnumerator KnockbackRoutine()
    {
        CancelAttackRoutines(false);
        effectsHandler.RandomIdle();

        knockBackVulnerable = false;
        canHurtStructure = false;

        float timeElapsed = 0;
        float maxTime = 1f;
        float minTimeForDamage = 0.35f; //If under this recoil time, no damage

        currentState = CreatureState.Knockback;

        coroutineRunning = true;
        isMoving = false;
        //
        anim.Play("golemKnockback");
        agent.speed = 8;
        agent.ResetPath();
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        agent.updateRotation = false;  
        skidParticles.Play();
        
        knockedBack = true;
        knockBackCollider.enabled = true;

        while(knockedBack && timeElapsed < maxTime)
        {
            if(timeElapsed >= minTimeForDamage && !canHurtStructure) canHurtStructure = true;

            timeElapsed += Time.deltaTime;
            if (NavMesh.SamplePosition(knockBackPoint.position, out var hit, 1.0f, NavMesh.AllAreas)) agent.SetDestination(hit.position);
            else if (agent.pathStatus != NavMeshPathStatus.PathComplete) agent.Move(transform.forward * agent.speed * Time.deltaTime);
            //agent.SetDestination(chargePosition.position);
            yield return null;
        }

        skidParticles.Stop();


        if(timeElapsed < maxTime)
        {
            effectsHandler.MiscSound2();
            crashedParticles.Play();
            if(timeElapsed < minTimeForDamage) anim.Play("golemSlightRecoil"); //Play slight recoil anim
            else
            {
                TakeRealDamage(50);
                yield break;
            }
        }

        knockedBack = false;
        canHurtStructure = false;
        agent.ResetPath();
        agent.velocity = Vector3.zero;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        agent.destination = transform.position;
        agent.updateRotation = true;

        yield return new WaitForSeconds(0.5f);
        currentState = CreatureState.AttackPlayer;
        knockBackRoutine = null;
        knockBackVulnerable = true;
        isMoving = false;
        coroutineRunning = false;
        interruptAction = false;
        knockBackCollider.enabled = false;

        agent.speed = baseSpeed;

    }

    IEnumerator DamagedRoutine()
    {
        currentState = CreatureState.Damaged;
        coroutineRunning = true;
        anim.Play("golemKnockbackForward");

        if(health > 0) effectsHandler.OnHit();

        CancelAttackRoutines(true);

        agent.velocity = Vector3.zero;

        yield return new WaitForSeconds(1.3f);
        agent.isStopped = false;
        currentState = CreatureState.AttackPlayer;
        isMoving = false;
        coroutineRunning = false;
        interruptAction = false;
        knockBackVulnerable = true;

        damageRoutine = null;
    }

    IEnumerator DeathRoutine()
    {
        anim.Play("Death");
        headLight.SetActive(false);
        yield return new WaitForSeconds(2f);
        attacking = true;
        slamCollider.enabled = true;
        deathParticles.SetActive(true);
        deathParticles.gameObject.transform.parent = null;
        effectsHandler.MiscSound();
        yield return new WaitForSeconds(0.1f);
        Vector3 rockSpawn = StructureManager.Instance.CheckTile(corpseParticleTransform.position);
        if(rockSpawn != Vector3.zero && Random.Range(0,4) >= 2) Instantiate(rockData.objectPrefab, rockSpawn, Quaternion.identity);
        canCorpseBreak = true;
        base.TakeDamage(999);
    }

    void CancelAttackRoutines(bool cancelKnockback)
    {
        if(cancelKnockback)
        {
            if(knockBackRoutine != null)
            {
                StopCoroutine(knockBackRoutine);
                agent.ResetPath();
                agent.Stop();
                agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
                agent.updateRotation = true;
                knockBackCollider.enabled = false;
                agent.speed = baseSpeed;
                canHurtStructure = false;
            }
            knockBackRoutine = null;
        }

        if(slamRoutine != null)
        {
            StopCoroutine(slamRoutine);
            attacking = false;
            slamCollider.enabled = false;
        }
        slamRoutine = null;

        if(swipeRoutine != null)
        {
            StopCoroutine(swipeRoutine);
            attacking = false;
            swipeCollider.enabled = false;
        }
        swipeRoutine = null;

    }

    IEnumerator ScanForTargets()
    {
        while(health > 0)
        {
            yield return new WaitForSeconds(5);
            if(targetStructure) continue;

            FindNearbyStructure(30);
            
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

            distanceToStructure = Vector3.Distance(transform.position, structure.transform.position);
            if (targettableStructures.Contains(structure.structData) && !structure.absentFromFarmGrid && distanceToStructure < closestDistance)
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

    public override void NewPriorityTarget(StructureBehaviorScript newStruct)
    {
        interruptAction = true;
        targetStructure = newStruct;
    }

    public override void OnDeath()
    {
        if (!isDead)
        {
            knockBackVulnerable = false;
            isDead = true;
            anim.Play("Death");
            base.OnDeath();
            agent.enabled = false;
            rb.isKinematic = true;
            rb.freezeRotation = true;
            StopAllCoroutines();
            StartCoroutine(DeathRoutine());
        }
    }

    void OnDestroy()
    {
        base.OnDestroy();
        if (!gameObject.scene.isLoaded) return; 
    }
}
