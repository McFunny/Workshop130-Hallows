using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatConstruct : CreatureBehaviorScript
{
    //Turning the wheel
    public Transform wheel, wheelParent;
    float ballRadius = 1f; // In meters
    private Vector3 lastPosition;

    //Turning the torso
    public Transform torsoPivot; //For looking at player
    Vector3 starePoint;
    Quaternion defaultTorsoRotation;

    //Movement
    public Vector3 targetPos; //The direction where the bot is trying to push itself to move to
    Vector3 despawnPos;

    public float moveSpeed = 5f;           // Max movement speed
    public float acceleration = 10f;       // Force applied to reach speed
    public float stoppingDistance = 1.5f;  // Distance to stop chasing
    public float slowDownDistance = 4f;    // Start slowing down before stopping
    Rigidbody targetRb;
    bool movingToPlayer;

    [Header("Reaction Settings")]
    public float baseReactionTime = 0.3f;    // Base delay in seconds
    public float reactionTimeVariance = 0.2f; // ± random offset
    private Vector3 laggedTargetPosition;
    private float nextReactionTime;

    float recoilTimeLeft = 0;

    //Attacking
    public Collider attackCollider;
    bool isAttacking, attackCooldown;
    List<StructureBehaviorScript> hitStructures = new List<StructureBehaviorScript>();
    bool hitPlayer = false;
    public LayerMask attackCheckMask;

    //ShortCurcuit
    bool isWet;

    //Effects
    public GameObject scrapeParticles;
    public GameObject shockedParticles;
    public GameObject splashObject;
    public List<GameObject> optionalMeshes = new List<GameObject>();

    public enum CreatureState
    {
        Wander,
        Recoil,
        Attacking,
        Stunned,
        Dead
    }

    public CreatureState currentState;

    void Awake()
    {
        defaultTorsoRotation = torsoPivot.rotation;

        foreach (GameObject mesh in optionalMeshes)
        {
            if(Random.Range(0,10) > 7) mesh.SetActive(false);
        }
    }

    void Start()
    {
        base.Start();
        currentState = CreatureState.Wander;
        despawnPos = NightSpawningManager.Instance.despawnPositions[Random.Range(0, NightSpawningManager.Instance.despawnPositions.Length)].position;
        lastPosition = transform.position;
        anim.applyRootMotion = false;

        StartCoroutine(RefreshWanderPoint());
        StartCoroutine(IdleSoundTimer());
        targetRb = PlayerInteraction.Instance.GetComponent<Rigidbody>();
        ScheduleNextReaction();
    }

    void LateUpdate()
    {
        //base.Update();
        wheel.position = wheelParent.position;
        if(isDead) return;

        float distance = Vector3.Distance(player.position, transform.position);
        playerInSightRange = distance <= sightRange;
        playerInAttackRange = distance <= attackRange;

        LookAtObject();
        RollWheel();

        CheckState(currentState);
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        if(type == ToolType.WateringCan && PlayerInteraction.Instance.waterHeld > 0 && !isWet && !isDead)
        {
            PlayerInteraction.Instance.waterHeld--;
            splashObject.SetActive(true);
            HitWithWater();
            success = true;
        }
        else success = false;
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
            case CreatureState.Attacking:
                Attacking();
                break;
            case CreatureState.Dead:
                //
                break;
            case CreatureState.Recoil:
                Recoil();
                break;
        }
    }

    void Wander()
    {
        if(playerInAttackRange || CheckForObstacle(transform) != null) currentState = CreatureState.Attacking;
        else if(playerInSightRange && movingToPlayer) MoveToPoint();
        else
        {
            Vector3 dir = (transform.position - targetPos).normalized;
            dir *= -1f;
            rb.AddForce(dir * (acceleration * 4));
        }
    }

    void Attacking()
    {
        if(!playerInAttackRange) currentState = CreatureState.Wander;
        if(!attackCooldown)
        {
            if(Random.Range(0, 5) > 2) StartCoroutine(ScytheAttack());
            else StartCoroutine(SlowScytheAttack());
        }

        MoveToPoint();
    }

    void MoveToPoint()
    {
        float maxSpeed = moveSpeed / actionSpeedMod;

        float distanceToPlayer = Vector3.Distance(transform.position, targetPos);
        if (distanceToPlayer <= stoppingDistance)
        {
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, 0.2f);
            return;
        }

        // Update prediction only at reaction intervals
        if (Time.time >= nextReactionTime)
        {
            float distance = Vector3.Distance(transform.position, targetPos);
            float interceptTime = distance / Mathf.Max(maxSpeed, 0.01f);
            Vector3 predictedPosition = targetPos + targetRb.velocity * interceptTime;

            laggedTargetPosition = predictedPosition;

            ScheduleNextReaction();
        }

        // Move toward lagged target position
        Vector3 toTarget = laggedTargetPosition - transform.position;
        toTarget.y = 0;
        float distanceToLagged = toTarget.magnitude;

        Vector3 direction = toTarget.normalized;

        // Slow down near actual player
        float speedMultiplier = 1f;
        if (distanceToPlayer < slowDownDistance)
        {
            speedMultiplier = Mathf.InverseLerp(stoppingDistance, slowDownDistance, distanceToPlayer);
        }

        Vector3 desiredVelocity = direction * maxSpeed * speedMultiplier;
        Vector3 velocityDelta = desiredVelocity - rb.velocity;
        velocityDelta.y = 0;

        Vector3 force = velocityDelta * acceleration;
        rb.AddForce(force, ForceMode.Acceleration);
    }

    void ScheduleNextReaction()
    {
        float randomOffset = Random.Range(-reactionTimeVariance, reactionTimeVariance);
        nextReactionTime = Time.time + baseReactionTime + randomOffset;
    }

    IEnumerator RefreshWanderPoint()
    {
        if(!inWilderness) targetPos = StructureManager.Instance.GetRandomTile();
        yield return new WaitForSeconds(5);
        while(health > 0)
        {
            yield return new WaitForSeconds(0.1f);

            if(TimeManager.Instance.isDay && !inWilderness)
            {
                targetPos = despawnPos;
                movingToPlayer = false;
                continue;
            }
            else if(inWilderness || playerInSightRange)
            {
                targetPos = player.position;
                movingToPlayer = true;
                continue;
            }
            movingToPlayer = false;
            yield return new WaitForSeconds(Random.Range(1.5f, 3f));
            float x = Random.Range(-15f, 15f);
            float z = Random.Range(-15f, 15f);
            targetPos = new Vector3(transform.position.x + x, transform.position.y, transform.position.z + z);
        }
    }

    void Recoil()
    {
        if(recoilTimeLeft <= 0)
        {
            currentState = CreatureState.Wander;
        }
        recoilTimeLeft -= Time.deltaTime;
    }

    IEnumerator ScytheAttack()
    {
        anim.Play("AutoAttack1", -1, 0f);
        StartCoroutine(AttackCooldown(1.6f));
        yield return new WaitForSeconds(0.35f);
        effectsHandler.MiscSound2(); 
        isAttacking = true;
        attackCollider.enabled = true;
        yield return new WaitForSeconds(0.2f);
        DealDamage();
        isAttacking = false;
        attackCollider.enabled = false;
    }

    IEnumerator SlowScytheAttack()
    {
        anim.Play("AutoAttack2", -1, 0f);
        StartCoroutine(AttackCooldown(2f));
        yield return new WaitForSeconds(0.6f);
        effectsHandler.MiscSound2(); 
        isAttacking = true;
        attackCollider.enabled = true;
        rb.AddForce(-torsoPivot.up * Random.Range(30, 50), ForceMode.Impulse);
        yield return new WaitForSeconds(0.2f);
        DealDamage();
        isAttacking = false;
        attackCollider.enabled = false;
    }

    IEnumerator AttackCooldown(float duration)
    {
        attackCooldown = true;
        scrapeParticles.SetActive(false);
        yield return new WaitForSeconds(duration * actionSpeedMod);
        attackCooldown = false;
        scrapeParticles.SetActive(true);
    }

    void DealDamage()
    {
        if(hitPlayer && (hitStructures.Count == 0 || CanSeePlayer()))
        {
            PlayerInteraction.Instance.StaminaChange(-damageToPlayer);
            hitPlayer = false;
        }
        else if(hitStructures.Count > 0)
        {
            for(int i = 0; i < hitStructures.Count; i++)
            {
                if(hitStructures[i]) hitStructures[i].TakeDamage(damageToStructure);
            }
        }

        hitStructures.Clear();
    }

    bool CanSeePlayer()
    {
        RaycastHit hit;
        if (Physics.Raycast(corpseParticleTransform.position, corpseParticleTransform.forward, out hit, 15, attackCheckMask))
        {
            if(hit.transform.gameObject.layer == 10) return true;
            else return false;
        }
        return false;
    }


    void LookAtObject()
    {
        if(playerInSightRange) starePoint = PlayerInteraction.Instance.transform.position;
        else starePoint = targetPos;

        Vector3 direction = starePoint - torsoPivot.position;
        direction.y = 0;
        Quaternion toRotation = Quaternion.LookRotation(direction);
        toRotation *= defaultTorsoRotation;

        torsoPivot.rotation = Quaternion.Slerp(torsoPivot.rotation, toRotation, 8 * Time.deltaTime);
    }

    void RollWheel()
    {

        Vector3 velocity = rb.velocity;

        // Ignore very small movement (to prevent jitter)
        if (velocity.magnitude > 0.01f)
        {
            // Movement direction (projected onto XZ plane)
            Vector3 moveDir = velocity.normalized;
            moveDir.y = 0;

            // Rotation axis: perpendicular to movement direction
            Vector3 rotationAxis = Vector3.Cross(moveDir, Vector3.up);

            // Distance moved this frame = speed * deltaTime
            float distance = velocity.magnitude * Time.deltaTime;

            // Degrees to rotate the ball based on the distance and radius
            float rotationDegrees = (distance / (2 * Mathf.PI * ballRadius)) * 360f;

            // Apply rotation
            wheel.Rotate(rotationAxis, -rotationDegrees, Space.World);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(!isAttacking) return;

        GameObject hitObject = other.gameObject;


        if(hitObject.layer == 10)
        {
            //PlayerInteraction.Instance.StaminaChange(-damageToPlayer);
            hitPlayer = true;
        }

        StructureBehaviorScript structure = hitObject.GetComponentInParent<StructureBehaviorScript>(); //To check if its a tree because trees arent "obstacles"
        if(structure && structure.isObstacle && structure.destructable && !hitStructures.Contains(structure))
        {
            hitStructures.Add(structure);
        } 
        
    }

    public override void TakeDamage(float damage, Vector3 source)
    {
        TakeDamage(damage);
        effectsHandler.MiscSound(); 
        recoilTimeLeft = Random.Range(0.5f, 1f);
        Vector3 dir = (transform.position - source).normalized;
        rb.AddForce(dir * Random.Range(50, 80), ForceMode.Impulse);

        if(currentState == CreatureState.Wander || currentState == CreatureState.Recoil)
        {
            currentState = CreatureState.Recoil;
            anim.Play("AutoKnockback");
        }
        
    }

    public override void OnDeath()
    {
        if (!isDead)
        {
            effectsHandler.loopingSource.Stop();
            scrapeParticles.SetActive(false);
            anim.SetLayerWeight(0, 0);
            anim.SetLayerWeight(1, 1);
            isDead = true;
            anim.Play("AutoDie");
            base.OnDeath();
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            StopAllCoroutines();

            torsoPivot.rotation = defaultTorsoRotation;

            transform.LookAt(new Vector3(targetPos.x, transform.position.y, targetPos.z));
            if(isWet || Random.Range(0,10) > 6) StartCoroutine(Explode());
        }
    }

    IEnumerator Explode()
    {
        yield return new WaitForSeconds(1);
        ParticlePoolManager.Instance.GrabExplosionParticle().transform.position = corpseParticleTransform.position;
        if(Vector3.Distance(transform.position, PlayerInteraction.Instance.transform.position) < 4f)
        {
            PlayerInteraction.Instance.StaminaChange(-20);
            PlayerInteraction.Instance.PlayerTrip();
        }

        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, 8f, 1 << 9);
        foreach(Collider collider in hitEnemies)
        {
            var creature = collider.GetComponentInParent<CreatureBehaviorScript>();
            if (creature != null && creature.shovelVulnerable)
            {
                creature.TakeDamage(45);
                creature.PlayHitParticle(creature.transform.position);
            }
        }
        TakeDamage(999);
    }

    public override void OnCorpseDamage()
    {
        if (health <= 0 && canCorpseBreak)
        {
            anim.Play("AutoDieInteract", -1, 0f);
        }
    }

    public override void HitWithWater()
    {
        ParticlePoolManager.Instance.GrabElecZapParticle().transform.position = transform.position;
        TakeDamage(25);
        if(!isWet) StartCoroutine(ShortCircuiting());
    }

    IEnumerator ShortCircuiting()
    {
        isWet = true;
        rb.velocity = Vector3.zero;
        StartCoroutine(PlayShortCircuitAudio());
        shockedParticles.SetActive(true);
        actionSpeedMod += 0.4f;
        yield return new WaitForSeconds(Random.Range(10, 20));
        isWet = false;
        shockedParticles.SetActive(false);
        actionSpeedMod -= 0.4f;
    }

    IEnumerator PlayShortCircuitAudio()
    {
        while(isWet)
        {
            effectsHandler.PlayExtraSound(Random.Range(0, effectsHandler.extraSounds.Length), 0.2f);
            yield return new WaitForSeconds(Random.Range(0.9f, 1.5f));
        }
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
}
