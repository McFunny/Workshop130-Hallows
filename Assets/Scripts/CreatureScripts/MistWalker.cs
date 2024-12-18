using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;

public class MistWalker : CreatureBehaviorScript
{
    public List<StructureObject> targettableStructures;

    private StructureBehaviorScript targetStructure;
    public List<StructureBehaviorScript> availableStructure = new List<StructureBehaviorScript>();

    private bool isMoving = false;
    private bool isBeingAttacked = false; // For priority target tracking (seed shooter)
    private bool coroutineRunning = false;
    private Transform target;
    private bool attackingPlayer = false;

    [HideInInspector] public NavMeshAgent agent;
    public AnimEvents animEvents;
    public Collider lungeAttackHitbox;
    public float lungeCooldown = 6f; // Time between lunges
    public float lungeRange = 9f; // Distance at which it will lunge
    private bool canLunge = true;
    private bool recoilCooldown = false; //To prevent stunlocking
    private bool isRecoiling = false;

    private Vector3 despawnPos;

    private Coroutine trackPlayerRoutine; 

    private FireFearTrigger fireSource;

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
        Trapped,
        FleeFromFire
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
        currentState = CreatureState.SpawnIn;
        StartCoroutine(IdleSoundTimer());
    }

    void OnDestroy()
    {
        StructureBehaviorScript.OnStructuresUpdated -= UpdateStructureList;
        if (animEvents) animEvents.OnColliderChange -= ColliderChange;
        if (animEvents) animEvents.OnFloatChange -= WalkSpeedToggle;
    }

    public override void OnSpawn()
    {
        if (!isMoving)
        {
            Vector3 randomPoint = StructureManager.Instance.GetRandomTile();
            StartCoroutine(MoveToPoint(randomPoint));
        }
    }

    /*private void TargetImbuedScarecrow(GameObject structure)
    {
        if (currentState == CreatureState.AttackStructure)
        {
            if (targetStructure == structure.GetComponent<StructureBehaviorScript>())
            {
                return;
            }
        }
        float distance = Vector3.Distance(transform.position, structure.transform.position);
        if (distance < 25f)
        {
            targetStructure = structure.GetComponent<StructureBehaviorScript>();
            target = structure.transform;
            currentState = CreatureState.WalkTowardsPriorityStructure;
        }
    } */

    private void UpdateStructureList()
    {
        availableStructure.Clear();
        foreach (var structure in structManager.allStructs)
        {
            if (targettableStructures.Contains(structure.structData))
                availableStructure.Add(structure);
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

        if (!isDead && currentState != CreatureState.Stun && currentState != CreatureState.Trapped)
        {
            if(fireSource)
            {
                float distFromFire = Vector3.Distance(fireSource.transform.position, transform.position);

                if(currentState != CreatureState.FleeFromFire && !coroutineRunning) currentState = CreatureState.FleeFromFire;

                if(fireSource.gameObject.activeSelf == false || distFromFire > fireSource.fleeRange)
                {
                    fireSource = null;
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

            if (playerInSightRange && currentState != CreatureState.AttackPlayer && currentState != CreatureState.WalkTowardsPlayer)
            {
                currentState = CreatureState.WalkTowardsPlayer;
            }

            CheckState(currentState);
        }
        else
        {
            lungeAttackHitbox.enabled = false;
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
                OnSpawn();
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
                WalkTowardsPriorityStructure();
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

            case CreatureState.Trapped:
                Trapped();
                anim.SetBool("IsWalking", false);
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
        if (playerInSightRange)
        {
            currentState = CreatureState.WalkTowardsPlayer;
            return;
        }

        if (!isMoving && currentState == CreatureState.Wander)
        {
            Vector3 randomPoint = GetRandomPointAround(transform.position, 5f);
            StartCoroutine(MoveToPoint(randomPoint));
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

        if (TimeManager.Instance.isDay) destination = despawnPos;

        agent.destination = destination;

        float timeSpent = 0; //to make sure it doesnt get stuck

        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance || timeSpent > 100)
        {
            timeSpent += 0.01f;
            if (playerInSightRange)
            {
                currentState = CreatureState.WalkTowardsPlayer;
                isMoving = false;
                coroutineRunning = false;
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
    }


    private void WalkTowardsClosestStructure()
    {
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
        else if (Vector3.Distance(transform.position, targetStructure.transform.position) < 4f)//(!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 1f)
        {
            agent.ResetPath();
            currentState = CreatureState.AttackStructure;
        }
        else if(target == null || agent.destination != target.position)
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

        foreach (var structure in availableStructure)
        {
            if (structure == null) continue;

            distanceToStructure = Vector3.Distance(transform.position, structure.transform.position);

            r = Random.Range(0,10); //to add randomness to what they chose to seek out

            if(structure.wealthValue == 0) continue; //to prevent mistwalkers from targetting dirt without a crop

            if (distanceToStructure < closestDistance && r > 1)
            {
                closestDistance = distanceToStructure;
                closestStructure = structure;
            }
        }
        return closestStructure;
    }

    private void WalkTowardsPriorityStructure()
    {
        if (targetStructure == null)
        {
            currentState = CreatureState.Wander;
            return;
        }

        if (target != targetStructure.transform)
        {
            target = targetStructure.transform;
            agent.destination = target.position;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 3f)
        {
            agent.ResetPath();
            currentState = CreatureState.AttackStructure;
        }
    }

    private void WalkTowardsPlayer()
    {
        if (trackPlayerRoutine == null)
        {
            trackPlayerRoutine = StartCoroutine(TrackPlayer());
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= 3f || (distanceToPlayer <= lungeRange && canLunge))
        {
            StopTrackingPlayer();
            currentState = CreatureState.AttackPlayer;
        }
        else if (!playerInSightRange)
        {
            StopTrackingPlayer();
            currentState = CreatureState.Wander;
        }
    }

    private IEnumerator TrackPlayer()
    {
        while (playerInSightRange && currentState == CreatureState.WalkTowardsPlayer)
        {
            agent.destination = player.position;
            yield return new WaitForSeconds(0.5f); // update destination every 0.5 seconds to prevent overloading it
        }
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
    }
    #endregion

    #region AttackingFunctions
    private void AttackPlayer()
    {
        if (coroutineRunning || isRecoiling)
            return;

        transform.LookAt(player.position);

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= 6f)
        {
            StartCoroutine(SwipePlayer());
            transform.LookAt(player.position);
        }
        else if (distance > 6f && distance <= lungeRange && canLunge)
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
        coroutineRunning = true;
        anim.SetTrigger("IsAttacking");
        transform.LookAt(targetStructure.transform.position);

        yield return new WaitForSeconds(1f); 

        if (Vector3.Distance(transform.position, targetStructure.transform.position) < 5f)
        {
            coroutineRunning = true;
            targetStructure.TakeDamage(damageToStructure);
            transform.LookAt(targetStructure.transform.position);
            if (targetStructure != null && targetStructure.health <= 0) { targetStructure = null; }
            yield return new WaitForSeconds(3f);
            coroutineRunning = false;
        }
        else
        {
            currentState = CreatureState.WalkTowardsClosestStructure;
        }

        yield return new WaitForSeconds(2f); // Cooldown between attacks
        coroutineRunning = false;
    }

    private IEnumerator LungeAtPlayer()
    {
        coroutineRunning = true;
        attackingPlayer = true;

        anim.SetTrigger("IsLunging");
        canLunge = false;

        effectsHandler.MiscSound();
        recoilCooldown = true;

        yield return new WaitForSeconds(0.75f); 

       
        if(currentState != CreatureState.Stun)
        {
            Vector3 lungeDirection = (player.position - transform.position).normalized;
            agent.velocity = lungeDirection * 20f; //better lunge
        }

        yield return new WaitForSeconds(0.5f);

        attackingPlayer = false;
        agent.velocity = Vector3.zero;
        currentState = CreatureState.WalkTowardsPlayer;
        coroutineRunning = false;
        recoilCooldown = false;
        StartCoroutine(LungeCooldown());
    }

    private IEnumerator SwipePlayer()
    {
        coroutineRunning = true;
        attackingPlayer = true;

        anim.SetTrigger("IsAttacking");

        effectsHandler.MiscSound2();
        recoilCooldown = true;

        yield return new WaitForSeconds(0.5f); 

        if(currentState != CreatureState.Stun)
        {
            Vector3 lungeDirection = (player.position - transform.position).normalized;
            agent.velocity = lungeDirection * 8; 
        }

        yield return new WaitForSeconds(0.8f);
        agent.velocity = Vector3.zero;

        attackingPlayer = false;
        currentState = CreatureState.WalkTowardsPlayer;
        recoilCooldown = false;
        yield return new WaitForSeconds(0.5f); 
        coroutineRunning = false;
    }

    private IEnumerator LungeCooldown()
    {
        yield return new WaitForSeconds(lungeCooldown);
        canLunge = true;
    }
    #endregion

    private void Idle()
    {
        if (playerInSightRange)
        {
            currentState = CreatureState.WalkTowardsPlayer;
            return;
        }

        if (!coroutineRunning)
        {
            int r = Random.Range(0, 11);
            if (r < 2)
            {
                if (availableStructure.Count > 0)
                {
                    currentState = CreatureState.WalkTowardsClosestStructure;
                }
            }
            else if (r < 5)
            {
                StartCoroutine(WaitAround());
            }
            else if (r >= 6)
            {
                currentState = CreatureState.Wander;
            }
        }
    }

    

    private void OnTriggerEnter(Collider other)
    {
        if (attackingPlayer && other.CompareTag("Player") && !isDead)
        {
            PlayerInteraction playerInteraction = other.GetComponent<PlayerInteraction>();
            if (playerInteraction != null)
            {
                playerInteraction.StaminaChange(damageToPlayer);
                lungeAttackHitbox.enabled = false;
            }
        }
    }

    public override void OnStun(float duration)
    {
        if (currentState != CreatureState.Stun)
        {
            StartCoroutine(Stun(duration));
            agent.destination = transform.position;
            agent.ResetPath();
            anim.SetBool("IsWalking", false);
            anim.SetTrigger("IsRecoiling");
        }
    }

    private IEnumerator Stun(float duration)
    {
        currentState = CreatureState.Stun;
        coroutineRunning = false;
        StopAllCoroutines();
        yield return new WaitForSeconds(duration);
        currentState = CreatureState.Wander;
    }

    public override void OnDeath()
    {
        if (!isDead)
        {
            isDead = true;
            anim.SetTrigger("IsDead");
            base.OnDeath();
            agent.enabled = false;
            rb.isKinematic = true;
            agent.ResetPath();
            rb.freezeRotation = true;
            StopAllCoroutines();
        }
    }

    public override void OnDamage()
    {
        if(!recoilCooldown)
        {
            recoilCooldown = true;
            effectsHandler.OnHit();
            anim.SetTrigger("IsRecoiling");
            StartCoroutine(RecoilCooldown());
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
            yield return new WaitForSeconds(i);
            effectsHandler.RandomIdle();
        }
    }

    private void Trapped()
    {
        agent.ResetPath();
        rb.isKinematic = true;
    }

    public override void EnteredFireRadius(FireFearTrigger _fireSource)
    {
        fireSource = _fireSource;
    }

    public void WalkSpeedToggle(float _speed)
    {
        agent.speed = _speed;
    }

    public void ColliderChange(bool enabled)
    {
        lungeAttackHitbox.enabled = enabled;
    }
}
