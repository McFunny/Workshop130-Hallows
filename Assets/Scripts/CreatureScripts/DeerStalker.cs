using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DeerStalker : CreatureBehaviorScript
{
    public LayerMask biteCheckMask;
    
    public Variant variant; // what variant of creature is this?

    bool hasTransformed = false;

    public Animator animTransformed; //anim of the tainted deer

    public GameObject deer,taintedDeer;

    private StructureBehaviorScript targetStructure;
    List<StructureBehaviorScript> hitStructures = new List<StructureBehaviorScript>();

    //public ParticleSystem transformParticles;

    private bool isMoving = false;
    private bool coroutineRunning = false;
    private Transform target;
    public float walkSpeed = 4;
    public float runSpeed = 13;
    float transformedSightRange = 30;
    float fleeTimeLeft = 0;

    [HideInInspector] public NavMeshAgent agent;
    public Collider attackHitbox;
    public Transform head;
    private bool recoilCooldown = false; //To prevent stunlocking
    private bool recoiling = false;
    bool emoting = false;
    bool canTurn;

    bool hitPlayer = false;
    bool hitStruct = false;

    private Vector3 despawnPos;

    private Coroutine trackPlayerRoutine, walkRoutine; 

    public ParticleSystem transformationParticles;

    //Its purpose is a player attacker only. Only attacks structures that impede it
    //Still needs Idle anim variance and transform particles
    //Occasionally will laugh after a bite or chase, giving the player a chance to run or hit

    public enum CreatureState
    {
        SpawnIn,
        Idle,
        Wander,
        Transformation,
        ChaseTarget,
        AttackInFront,
        Stun,
        Die,
        Flee
    }

    public enum Variant
    {
        Normal,
        Pure //Wont Transformation
    }

    public CreatureState currentState;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        base.Start();
        attackHitbox.enabled = false;
        
        agent.enabled = false;
        agent.enabled = true;

        if(hasTransformed)
        {
            taintedDeer.SetActive(true);
            deer.SetActive(false);
        }
        else
        {
            taintedDeer.SetActive(false);
            deer.SetActive(true);
        }

        int r = Random.Range(0, NightSpawningManager.Instance.despawnPositions.Length);
        despawnPos = NightSpawningManager.Instance.despawnPositions[r].position;
        targetStructure = null;

        StartCoroutine(IdleSoundTimer());
    }

    public void Spawn()
    {
        if(inWilderness)
        {
            currentState = CreatureState.ChaseTarget;
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

            if(currentState != CreatureState.Flee && !coroutineRunning && fleeTimeLeft > 0) currentState = CreatureState.Flee;

            if(currentState == CreatureState.ChaseTarget || currentState == CreatureState.Flee)
            {
                agent.speed = runSpeed;
            }
            else agent.speed = walkSpeed;

            if(agent.velocity.sqrMagnitude > 0) 
            {
                if(agent.speed == runSpeed)
                {
                    anim.SetBool("IsRunning", true);
                    animTransformed.SetBool("IsRunning", true);
                }
                else
                {
                    anim.SetBool("IsRunning", false);
                    animTransformed.SetBool("IsRunning", false);

                    anim.SetBool("IsMoving", true);
                    animTransformed.SetBool("IsMoving", true);
                }
            }
            else
            {
                anim.SetBool("IsMoving", false);
                animTransformed.SetBool("IsMoving", false);
            }

            CheckState(currentState);
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
                break;

            case CreatureState.Idle:
                Idle();
                break;

            case CreatureState.Wander:
                Wander();
                break;

            case CreatureState.Transformation:
                Transformation();
                break;

            case CreatureState.ChaseTarget:
                if(hasTransformed) ChaseTarget();
                else WalkTowardsPlayer();
                break;

            case CreatureState.AttackInFront:
                Attack();
                break;

            case CreatureState.Stun:
                break;

            case CreatureState.Die:
                // OnDeath();
                break;

            case CreatureState.Flee:
                Flee();
                break;

            default:
                Debug.LogError("Unknown state: " + currentState);
                break;
        }
    }

    #region WanderingFunctions
    public void Wander()
    {
        if ((playerInSightRange && hasTransformed) || inWilderness)
        {
            if(variant == Variant.Pure)
            {
                //fleeTimeLeft = Random.Range(1,4);
            }
            else currentState = CreatureState.ChaseTarget;
            return;
        }

        if (!isMoving && currentState == CreatureState.Wander)
        {
            Vector3 randomPoint;
            if(inWilderness) randomPoint = GetRandomPointAround(transform.position, 5);
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
        if(Random.Range(0,10) > 5) anim.SetTrigger("AltIdle");
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
            if (playerInSightRange && variant != Variant.Pure)
            {
                if(hasTransformed)
                {
                    currentState = CreatureState.ChaseTarget;
                    isMoving = false;
                    coroutineRunning = false;
                    walkRoutine = null;
                }
                else
                {
                    currentState = CreatureState.Transformation;
                    isMoving = false;
                    coroutineRunning = false;
                    walkRoutine = null;
                }
                
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
            currentState = CreatureState.Idle;
        }
        walkRoutine = null;
    }

    void ChaseTarget()
    {
        if (trackPlayerRoutine == null)
        {
            trackPlayerRoutine = StartCoroutine(TrackPlayer());
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

        //head.LookAt(target);
        targetStructure = CheckForObstacle(corpseParticleTransform);
        if(targetStructure)
        {
            target = targetStructure.transform;
        }

        float dist = Vector3.Distance(transform.position, target.position);


        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 toTarget = Vector3.Normalize(target.position - transform.position);

        if (target && dist < 6 && Vector3.Dot(forward, toTarget) > .7f)
        {
            StopTrackingPlayer();
            currentState = CreatureState.AttackInFront;
        }
        else if (!playerInSightRange && !inWilderness && target == player)
        {
            StartCoroutine(LosePlayer());
            currentState = CreatureState.Idle;
            target = null;
            StopTrackingPlayer();
            //Code for losing sight
        }
    }

    private void WalkTowardsPlayer() //Movement for passive Deer in the wilderness
    {
        if (trackPlayerRoutine == null)
        {
            trackPlayerRoutine = StartCoroutine(TrackPlayer());
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (playerInSightRange && variant != Variant.Pure)
        {
            StopTrackingPlayer();
            currentState = CreatureState.Transformation;
        }
        else if (!playerInSightRange && !inWilderness)
        {
            currentState = CreatureState.Wander;
            StopTrackingPlayer();
        }
    }

    private IEnumerator TrackPlayer()
    {
        while ((playerInSightRange || inWilderness))
        {
            if(!target) target = player;
            agent.destination = target.position;
            yield return new WaitForSeconds(0.05f);
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
        Vector3 runTo = transform.position + ((transform.position - player.transform.position + new Vector3(Random.Range(-3, 3), 0, Random.Range(-3, 3)) * 1));
        agent.destination = runTo;
        fleeTimeLeft -= Time.deltaTime;
        if(fleeTimeLeft <= 0)
        {
            currentState = CreatureState.Wander;
        }
    }
    #endregion
    private void Attack()
    {
        if (coroutineRunning || recoiling)
        {
            if(canTurn) transform.LookAt(player.position);
            return;
        }

        transform.LookAt(player.position);

        StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        coroutineRunning = true;
        canTurn = true;
        animTransformed.Play("Attack");

        yield return new WaitForSeconds(0.1f);
        if(!target) target = player;
        float dist = Vector3.Distance(target.position, transform.position);
        float lungeForce = 25;
        if(currentState != CreatureState.Stun && dist > 3.5f)
        {
            //canTurn = false;
            //Vector3 lungeDirection = transform.forward;
            Vector3 lungeDirection = GetPointOnUnitSphereCap(transform.forward, 30);
            if(dist > 5.5f)
            {
                lungeForce = 35;
                canTurn = false;
                print("LargeLunge");
            }
            else print("MiniLunge");
            agent.velocity = lungeDirection * lungeForce; 
        }
        else print("No Lunge");

        
        yield return new WaitForSeconds(0.5f);
        canTurn = false;
        attackHitbox.enabled = true;
        agent.SetDestination(transform.position);
        yield return new WaitForSeconds(0.1f);
        agent.velocity = Vector3.zero;
        attackHitbox.enabled = false;
        if(hitPlayer && (hitStructures.Count == 0 || CanSeePlayer()))
        {
            PlayerInteraction.Instance.StaminaChange(damageToPlayer);
            hitPlayer = false;
            animTransformed.SetBool("AttackSuccessful", true);
            yield return new WaitForSeconds(1.5f);
            animTransformed.SetBool("AttackSuccessful", false);
        }
        else if(hitStructures.Count > 0)
        {
            for(int i = 0; i < hitStructures.Count; i++)
            {
                if(hitStructures[i]) hitStructures[i].TakeDamage(damageToStructure);
            }
            //targetStructure.TakeDamage(damageToStructure);
            targetStructure = null;
            //hitStruct = false;
        }
        /*else
        {
            if(Random.Range(0,10) > 8)
            {
                StartCoroutine(Laugh());
                yield return new WaitForSeconds(1.2f);
            }
        }*/
        hitStructures.Clear();
        yield return new WaitForSeconds(0.4f);
        coroutineRunning = false;
        if(currentState != CreatureState.Stun) currentState = CreatureState.ChaseTarget;
    }

    //////Code to help add randomness to the lunge
    public Vector3 GetPointOnUnitSphereCap(Quaternion targetDirection, float angle)
    {
        var angleInRad = Random.Range(0.0f,angle) * Mathf.Deg2Rad;
        var PointOnCircle = (Random.insideUnitCircle.normalized)*Mathf.Sin(angleInRad);
        var V = new Vector3(PointOnCircle.x,PointOnCircle.y,Mathf.Cos(angleInRad));
        return targetDirection*V;
    }
    public Vector3 GetPointOnUnitSphereCap(Vector3 targetDirection, float angle)
    {
        return GetPointOnUnitSphereCap(Quaternion.LookRotation(targetDirection), angle);
    }
    ///////////

    bool CanSeePlayer()
    {
        RaycastHit hit;
        if (Physics.Raycast(corpseParticleTransform.position, corpseParticleTransform.forward, out hit, 15, biteCheckMask))
        {
            if(hit.transform.gameObject.layer == 10) return true;
            else return false;
        }
        return false;
    }

    void Transformation()
    {
        if(coroutineRunning) return;

        StartCoroutine(Transforming());
    }

    IEnumerator Transforming()
    {
        coroutineRunning = true;
        anim.Play("Transform");
        agent.destination = transform.position;
        agent.ResetPath();
        transformationParticles.Play();
        yield return new WaitForSeconds(1.5f);
        if(health <= 0) yield break;
        deer.SetActive(false);
        taintedDeer.SetActive(true);
        animTransformed.Play("Transform");
        effectsHandler.MiscSound();
        yield return new WaitForSeconds(1f);
        hasTransformed = true;
        currentState = CreatureState.Wander;
        sightRange = transformedSightRange;
        coroutineRunning = false;
    }

    private void Idle()
    {
        if (playerInSightRange && !emoting && variant != Variant.Pure)
        {
            if(hasTransformed) currentState = CreatureState.ChaseTarget;
            else currentState = CreatureState.Transformation;
            return;
        }

        if (!coroutineRunning)
        {
            agent.ResetPath();
            int r = Random.Range(0, 13);
            if (r < 3)
            {
                StartCoroutine(Emote());
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

    IEnumerator Emote()
    {
        emoting = true;
        coroutineRunning = true;
        if(hasTransformed)
        {
            StartCoroutine(Laugh());
        }
        else
        {
            StartCoroutine(EatEmote());
        }
        while (emoting)
        {
            yield return null;
        }
        coroutineRunning = false;
    }

    IEnumerator Laugh()
    {
        emoting = true;
        animTransformed.Play("Laugh");
        yield return new WaitForSeconds(1.2f);
        emoting = false;
    }
    IEnumerator EatEmote()
    {
        emoting = true;
        anim.Play("Eat");
        yield return new WaitForSeconds(1.8f);
        emoting = false;
    }
    IEnumerator LosePlayer()
    {
        emoting = true;
        coroutineRunning = true;
        animTransformed.Play("LosePlayer");
        yield return new WaitForSeconds(2.5f);
        emoting = false;
        coroutineRunning = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isDead)
        {
            PlayerInteraction playerInteraction = other.GetComponent<PlayerInteraction>();
            if (playerInteraction != null)
            {
                //playerInteraction.StaminaChange(damageToPlayer);
                hitPlayer = true;
                //attackHitbox.enabled = false;
            }
        }

        if(other.gameObject.layer == 6)
        {
            var structure = other.GetComponentInParent<StructureBehaviorScript>();
            /*if (structure != null && targetStructure && structure == targetStructure)
            {
                hitStruct = true;
                return;
            }   */ 

            if(structure && structure.isObstacle && structure.destructable && !hitStructures.Contains(structure))
            {
                hitStructures.Add(structure);
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
            anim.SetTrigger("Recoiling");
            return true;
        }
        return false;
    }

    private IEnumerator BearTrapHold(StructureBehaviorScript b)
    {
        currentState = CreatureState.Stun;
        coroutineRunning = false;
        StopTrackingPlayer();
        StopCoroutine(AttackRoutine());
        attackHitbox.enabled = false;
        if(walkRoutine != null)
        {
            StopCoroutine(walkRoutine);
            walkRoutine = null;
        }
        if(hasTransformed)
        {
            animTransformed.Play("TrapStart");
            animTransformed.SetBool("IsTrapped", true);
        }
        else
        {
            anim.Play("TrapStart");
            anim.SetBool("IsTrapped", true);
        }
        while (b && b.health > 0)
        {
            yield return null;
        }
        anim.SetBool("IsTrapped", false);
        animTransformed.SetBool("IsTrapped", false);
        currentState = CreatureState.Wander;
    }

    /*public override bool OnStun(float duration)
    {
        if (currentState != CreatureState.Stun)
        {
            StartCoroutine(Stun(duration));
            agent.destination = transform.position;
            agent.ResetPath();
            anim.SetTrigger("Recoiling");
            return true;
        }
        return false;
    }

    private IEnumerator Stun(float duration)
    {
        currentState = CreatureState.Stun;
        coroutineRunning = false;
        StopTrackingPlayer();
        StopCoroutine(AttackRoutine());
        attackHitbox.enabled = false;
        if(walkRoutine != null)
        {
            StopCoroutine(walkRoutine);
            walkRoutine = null;

            if(hasTransformed)
            {
                anim.Play("TrapStart");
            }
            else
            {
                animTransformed.Play("TrapStart");
            }
        }
        yield return new WaitForSeconds(duration);
        //StartCoroutine(IdleSoundTimer());
        currentState = CreatureState.Wander;
    } */

    public override void OnDeath()
    {
        if (!isDead)
        {
            isDead = true;
            anim.Play("Death");
            animTransformed.Play("Death");
            base.OnDeath();
            agent.enabled = false;
            rb.isKinematic = true;
            rb.freezeRotation = true;
            StopAllCoroutines();
            StartCoroutine(DeathTimer());
        }
    }

    public override void OnDamage()
    {
        if(variant == Variant.Pure)
        {
            effectsHandler.OnHit();
            fleeTimeLeft = Random.Range(3,7);
            StopTrackingPlayer();
            if(walkRoutine != null)
            {
                StopCoroutine(walkRoutine);
                walkRoutine = null;
                coroutineRunning = false;
            }
            return;
        }
        if(!recoilCooldown && hasTransformed && !isDead && (currentState == CreatureState.ChaseTarget))
        {
            recoilCooldown = true;
            effectsHandler.OnHit();
            animTransformed.Play("Hit", -1, 0.18f);
            StartCoroutine(RecoilCooldown());
            target = player;
        }
        else if(!hasTransformed && currentState != CreatureState.Stun)
        {
            StopTrackingPlayer();
            if(walkRoutine != null)
            {
                StopCoroutine(walkRoutine);
                walkRoutine = null;
                coroutineRunning = false;
            }
            currentState = CreatureState.Transformation;
        }
    }

    IEnumerator DeathTimer()
    {
        if(hasTransformed) yield return new WaitForSeconds(2);
        else yield return new WaitForSeconds(0.5f);
        health = 0;
        canCorpseBreak = true;
    }

    IEnumerator RecoilCooldown()
    {
        recoiling = true;
        coroutineRunning = true;
        currentState = CreatureState.Stun;
        StopTrackingPlayer();
        StopCoroutine(AttackRoutine());
        attackHitbox.enabled = false;
        yield return new WaitForSeconds(0.6f);
        currentState = CreatureState.ChaseTarget;
        coroutineRunning = false;
        recoiling = false;
        yield return new WaitForSeconds(5);
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

    public override void NewPriorityTarget(StructureBehaviorScript newStruct)
    {
        if(targetStructure) return;
        targetStructure = newStruct;
        if(currentState == CreatureState.Idle || currentState == CreatureState.Wander) currentState = CreatureState.ChaseTarget;
        
    }
}
