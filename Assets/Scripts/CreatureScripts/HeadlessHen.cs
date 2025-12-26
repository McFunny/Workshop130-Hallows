using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadlessHen : CreatureBehaviorScript
{
    Vector3 despawnPos;

    public float moveSpeed = 8f;
    public float flyingSpeed = 6f;
    float currentSpeed;
    public float directionChangeInterval = 2f;
    public float randomTurnStrength = 30f;
    public float panicTurnChance = 0.2f;
    public float maxDistanceFromHome = 30f;
    public float turnSpeed = 360f;

    float wanderStrength = 1.5f;   // how much randomness while chasing
    float wanderFrequency = 1.2f;  // how fast the randomness changes

    private Vector3 moveDirection;
    private Vector3 targetDirection;
    private float nextDirectionChangeTime;
    private Vector3 homePosition;

    bool idling = false;
    bool attackCooldown = false;
    bool isAttacking = false;

    public LayerMask groundMask;

    public List<StructureObject> jumpableStructures;

    Coroutine currentRoutine;

    public GameObject surroundParticles, slamParticles;

    public Collider slamCollider;

    public GameObject egg;


    public enum CreatureState
    {
        Wander,
        AttackCrop,
        AttackPlayer,
        Stunned,
        Dead
    }

    public CreatureState currentState;

    void Start()
    {
        base.Start();
        currentState = CreatureState.Wander;
        despawnPos = NightSpawningManager.Instance.despawnPositions[Random.Range(0, NightSpawningManager.Instance.despawnPositions.Length)].position;

        if(!NightSpawningManager.Instance.allCreatures.Contains(this))NightSpawningManager.Instance.allCreatures.Add(this);

        PickStartingDirection();

        StartCoroutine(RefreshWanderPoint());
        StartCoroutine(IdleTimer());
        StartCoroutine(IdleSoundTimer());

        wanderFrequency = Random.Range(1f, 2f);
        wanderStrength = Random.Range(1f, 2.5f);
    }

    void FixedUpdate()
    {
        base.Update();
        if(isDead) return;

        if(GroundedCheck())
        {
            currentSpeed = moveSpeed;
            if(rb.velocity.magnitude > 0.5f) anim.SetBool("IsMoving", true);
            else anim.SetBool("IsMoving", false);

            anim.SetBool("IsFlying", false);
        }
        else
        {
            currentSpeed = flyingSpeed;
            anim.SetBool("IsFlying", true);
        }

        rb.angularVelocity = Vector3.zero;

        CheckState(currentState);
    }

    private void CheckState(CreatureState state)
    {
        switch (state)
        {
            case CreatureState.Wander:
                Wander();
                break;
            case CreatureState.Stunned:
                //Stunned();
                break;
            case CreatureState.Dead:
                //
                break;
            case CreatureState.AttackCrop:
                AttackCrop();
                break;

            case CreatureState.AttackPlayer:
                AttackPlayer();
                break;
        }
    }

    void Wander()
    {
        TryJumpOverObstacle();

        if(CloseToCropCheck() && !attackCooldown && currentState != CreatureState.AttackPlayer)
        {
            currentState = CreatureState.AttackCrop;
            return;
        }

        float distance = Vector3.Distance(transform.position, homePosition);

        // Decide target direction
        if(!inWilderness && TimeManager.Instance.isDay)
        {
            targetDirection = (despawnPos - transform.position).normalized;
        }
        else if (distance > maxDistanceFromHome)
        {
            targetDirection = (homePosition - transform.position).normalized;
        }
        else if (Time.time >= nextDirectionChangeTime)
        {
            // Pick a new random target direction
            float angle = Random.Range(0f, 360f);
            if(GroundedCheck()) targetDirection = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad));
            nextDirectionChangeTime = Time.time + directionChangeInterval + Random.Range(-0.5f, 1f);
        }

        if(idling) return; //Have it idle for a bit

        // Gradually rotate moveDirection toward targetDirection
        moveDirection = Vector3.RotateTowards(moveDirection, targetDirection, Mathf.Deg2Rad * turnSpeed * Time.fixedDeltaTime, 0f).normalized;

        // Move
        rb.velocity = moveDirection * currentSpeed + new Vector3(0, rb.velocity.y, 0);

        // Smooth rotate Rigidbody
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDirection, Vector3.up);
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRot, turnSpeed * Time.fixedDeltaTime));
        }
    }

    void PickStartingDirection()
    {
        nextDirectionChangeTime = Time.time + directionChangeInterval;

        Vector3 newDir;
        if (Random.value < panicTurnChance)
        {
            // Sharp random panic turn
            newDir = Random.insideUnitSphere;
            newDir.y = 0;
        }
        else
        {
            // Semi-random wander direction in world space
            float angle = Random.Range(0f, 360f);
            newDir = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad));
        }

        moveDirection = newDir.normalized;
    }

    IEnumerator RefreshWanderPoint()
    {
        while(health > 0)
        {
            if(!inWilderness) homePosition = StructureManager.Instance.GetRandomTile();
            if(inWilderness || Random.Range(0, 15) == 0)
            {
                homePosition = player.position;
                continue;
            }
            yield return new WaitForSeconds(Random.Range(15f, 30f));
        }
    }

    void AttackCrop()
    {
        rb.velocity = Vector3.zero;

        if(currentRoutine == null)
        {
            currentRoutine = StartCoroutine(AttackCropRoutine());
            StartCoroutine(AttackCooldownTimer(5));
        }
    }

    IEnumerator AttackCropRoutine()
    {
        anim.Play("CropAttack");
        yield return new WaitForSeconds(0.9f);
        Collider[] nearbyTiles = Physics.OverlapSphere(transform.position, 1.5f, 1 << 6);
        foreach(Collider collider in nearbyTiles)
        {
            FarmLand tile = collider.gameObject.GetComponentInParent<FarmLand>();
            if(tile)
            {
                if(tile.crop && tile.crop.behavior) 
                {
                    if(tile.harvestable)
                    {
                        tile.crop.behavior.OnConsumed(this);
                    }
                    else tile.crop.behavior.OnConsumedBeforeMaturity(this);
                }
                if(tile.health <= damageToStructure) tile.CropDestroyed();
                else tile.TakeDamage(damageToStructure);
            }
        }

        if(Vector3.Distance(player.position, transform.position) < 1.8f) PlayerInteraction.Instance.StaminaChange(8);

        surroundParticles.SetActive(true);
        effectsHandler.PlaySound(effectsHandler.miscSound3);
        yield return new WaitForSeconds(1.2f);
        if(currentState != CreatureState.AttackPlayer) currentState = CreatureState.Wander;
        currentRoutine = null;
    }

    void AttackPlayer()
    {
        TryJumpOverObstacle();

        /*targetDirection = (player.position - transform.position).normalized;
        
        Quaternion targetRot = Quaternion.LookRotation(targetDirection, Vector3.up);
        rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRot, 400 * Time.fixedDeltaTime));

        if(currentRoutine != null) 
        {
            rb.velocity = Vector3.zero;
            return;
        }

        if(Vector3.Distance(player.position, transform.position) < attackRange && !attackCooldown)
        {
            currentRoutine = StartCoroutine(AttackPlayerRoutine());
            StartCoroutine(AttackCooldownTimer(2));
        }

        // Move
        rb.velocity = targetDirection * currentSpeed + new Vector3(0, rb.velocity.y, 0);*/

        /////////////////////////
        /// 
        Vector3 toPlayer = (player.position - transform.position);
        float distance = toPlayer.magnitude;

        float wanderStrength = 1.5f;   // how much randomness while chasing
        float wanderFrequency = 1.2f;  // how fast the randomness changes

        // Base chase direction
        Vector3 chaseDir = toPlayer.normalized;

        if (distance > attackRange + 4)
        {
             // Calculate small lateral offset perpendicular to chaseDir
            Vector3 perpendicular = Vector3.Cross(Vector3.up, chaseDir).normalized;
            float wanderAmount = (Mathf.PerlinNoise(Time.time * wanderFrequency, 0f) - 0.5f) * 2f; // -1 to 1
            Vector3 wanderOffset = perpendicular * wanderAmount * wanderStrength;

            // Add offset while keeping mostly toward player
            chaseDir = (chaseDir + wanderOffset).normalized;
        }
        else
        {
            // Close enough to attack → move directly
            chaseDir = chaseDir.normalized;
        }

        if(currentRoutine != null) 
        {
            rb.velocity = Vector3.zero;
            return;
        }

        if(distance < attackRange && !attackCooldown)
        {
            currentRoutine = StartCoroutine(AttackPlayerRoutine());
            StartCoroutine(AttackCooldownTimer(2));
        }

        // Apply velocity
        rb.velocity = chaseDir * currentSpeed + new Vector3(0, rb.velocity.y, 0);

        Quaternion targetRot = Quaternion.LookRotation(chaseDir, Vector3.up);
        rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRot, 360 * Time.fixedDeltaTime));
    }

    IEnumerator AttackPlayerRoutine()
    {
        anim.Play("SlamAttack");
        rb.velocity = Vector3.zero;
        effectsHandler.PlaySound(effectsHandler.miscSound2);
        yield return new WaitForSeconds(0.7f);
        //if(Vector3.Distance(player.position, transform.position) < attackRange) PlayerInteraction.Instance.StaminaChange(damageToPlayer);
        slamCollider.enabled = true;
        isAttacking = true;

        slamParticles.SetActive(true);
        effectsHandler.PlaySound(effectsHandler.miscSound3);
        yield return new WaitForSeconds(0.1f);
        slamCollider.enabled = false;
        isAttacking = false;
        yield return new WaitForSeconds(0.8f);
        currentRoutine = null;
    }

    IEnumerator AttackCooldownTimer(float time)
    {
        attackCooldown = true;
        yield return new WaitForSeconds(time);
        attackCooldown = false;
    }

    public override void OnDamage()
    {
        //play aggro sound
        //Tell all other hens to fly to this spot
        effectsHandler.OnHit();
        StartCoroutine(AlertHens());

    }

    IEnumerator AlertHens()
    {
        yield return new WaitForSeconds(0.5f);
        if(Vector3.Distance(player.position, transform.position) > 15 || health <= 0) yield break;
        foreach( CreatureBehaviorScript creature in NightSpawningManager.Instance.allCreatures)
        {
            HeadlessHen hen = creature as HeadlessHen;
            if(hen && Random.Range(0,5) != 0)
            {
                hen.AggroToPlayer();
            }
        }
        AggroToPlayer();
    }

    public void AggroToPlayer()
    {
        if(currentState == CreatureState.AttackPlayer || isDead) return;

        if(currentState == CreatureState.Wander) rb.AddForce(Vector3.up * 700, ForceMode.Impulse);

        currentState = CreatureState.AttackPlayer;
        
        effectsHandler.PlaySound(effectsHandler.miscSound);
    }

    void OnTriggerEnter(Collider other)
    {
        if(isAttacking && !isDead)
        {
            if(other.gameObject.layer == 10) PlayerInteraction.Instance.StaminaChange(damageToPlayer);

            if(other.gameObject.layer == 6)
            {
                StructureBehaviorScript structure = other.gameObject.GetComponentInParent<StructureBehaviorScript>();
                if(structure) structure.TakeDamage(damageToStructure);
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
            //rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            rb.freezeRotation = true;
            StopAllCoroutines();
            canCorpseBreak = true;
            effectsHandler.loopingSource.Stop();
        }
    }

    public override void OnCorpseDamage()
    {
        if(health <= 0 && canCorpseBreak)
        {
            anim.Play("DeathRecoil", -1, 0f);
        }
    }

    private bool GroundedCheck()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -Vector3.up, out hit, 0.5f, groundMask))
        {
            print("Grounded");
            return true;
        }
        else
        {
            print("Flying");
            return false;
        }
    }

    bool CloseToCropCheck()
    {
        Vector3 origin = transform.position;
        RaycastHit hit;
        if (Physics.Raycast(origin, transform.forward, out hit, 1.3f, 1 << 6))
        {
            FarmLand tile = hit.collider.GetComponentInParent<FarmLand>(); //To check if its a tree because trees arent "obstacles"
            if(tile && tile.crop && !tile.isWeed && tile.currentUpgrade != FarmLand.FarmTileUpgrade.Corrupt)
            {
                return true;
            }
        }
        return false;
    }

    void TryJumpOverObstacle()
    {
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        RaycastHit hit;
        if (Physics.Raycast(origin, transform.forward, out hit, 3, 1 << 6))
        {
            StructureBehaviorScript obstacle = hit.collider.GetComponentInParent<StructureBehaviorScript>(); //To check if its a tree because trees arent "obstacles"
            if(obstacle && jumpableStructures.Contains(obstacle.structData))
            {
                Vector3 force = Vector3.up * 110;
                rb.AddForce(force, ForceMode.Impulse);
                //print("FLYYYY");
            }
        }
    }

    IEnumerator IdleTimer()
    {
        while(health > 0)
        {
            yield return new WaitForSeconds(Random.Range(7f, 18f));
            idling = true;
            if(Random.Range(0,10) > 8 && !inWilderness && NightSpawningManager.Instance.ReportTotalOfCreature(creatureData) < creatureData.spawnCap) 
                Instantiate(egg, corpseParticleTransform.position, Quaternion.identity);
            yield return new WaitForSeconds(Random.Range(1f, 3f));
            idling = false;
        }
    }

    IEnumerator IdleSoundTimer()
    {
        while(health > 0)
        {
            int i = Random.Range(3,8);
            effectsHandler.RandomIdle();
            yield return new WaitForSeconds(i);
        }
    }
}
