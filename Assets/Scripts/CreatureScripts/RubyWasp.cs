using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RubyWasp : CreatureBehaviorScript
{
    public RubyWaspSwarm homeSwarm;
    public CrimsonMothNest homeNest;

    public float accelerationRate, maxVelocity;

    public Vector3 targetPos;
    public Pollinator targetMoth;
    int unstickAttempts = 0;

    public List<FireFearTrigger> fireSources;

    public GameObject fearObject; //The particle system
    Vector3 fearedObjectPosition; //Where the lavent leaf is

    private Vector3 localOffset; //Used to simulate parenting to the player
    private Quaternion rotationOffset;

    private float wanderStrength = 2f;
    private float wanderFrequency = 1f;
    private float noiseOffset;

    private bool coroutineRunning = false;
    bool stuckOnPlayer;

    public enum CreatureState
    {
        Wander,
        Chase,
        Attack,
        Stuck,
        Stun,
        Flee,
        Dead
    }

    public CreatureState currentState;

    void Start()
    {
        base.Start();

        accelerationRate += Random.Range(-3, 3);
        transform.position = new Vector3(transform.position.x, transform.position.y + Random.Range(0, .4f), transform.position.z);
        StartCoroutine(RefreshDestination());
        StartCoroutine(ScoutForMoth());

        targetPos = transform.position;
        noiseOffset = Random.Range(0f, 1000f);
    }

    void LateUpdate()
    {
        base.Update();
        //if(!homeSwarm && !homeNest) Destroy(gameObject); //Should never happen unless morning hit

        float distance = Vector3.Distance(player.position, transform.position);
        playerInSightRange = distance <= sightRange;
        playerInAttackRange = distance <= attackRange;

        CheckState(currentState);
    }

    public void CheckState(CreatureState currentState)
    {
        switch (currentState)
        {
            case CreatureState.Wander:
                Wander();
                break;
            case CreatureState.Chase:
                Chase();
                break;
            case CreatureState.Attack:
                Attack();
                break;
            case CreatureState.Stuck:
                Stuck();
                break;
            case CreatureState.Stun:
                //
                break;
            case CreatureState.Flee:
                Flee();
                break;
            case CreatureState.Dead:
                //Dead();
                break;

            default:
                Debug.LogError("Unknown state: " + currentState);
                break;
        }
    }

    void AddForceToBug(Vector3 target, bool useOldMethod)
    {
        if(useOldMethod)
        {
            Vector3 dir = (transform.position - target).normalized;
            dir *= -1f;
            rb.AddForce(dir * (accelerationRate + 5));
            rb.drag = .2f;
            return;
        }


        Vector3 toTarget = (target - transform.position).normalized;
        Vector3 desiredVelocity = toTarget * maxVelocity;
        if(currentState == CreatureState.Attack) desiredVelocity = toTarget * 2;
        Vector3 steering = desiredVelocity - rb.velocity;

        // Boost steering force when misaligned
        float alignment = Vector3.Dot(rb.velocity.normalized, toTarget);
        float misalignment = 1f - Mathf.Clamp01(alignment);

        float steerBoost = Mathf.Lerp(1f, 1.5f, misalignment); // was 1f to 2f+
        rb.AddForce(steering * accelerationRate * steerBoost);

        //Dynamic drag
        float drag = Mathf.Lerp(0.2f, 1f, 1f - alignment); // alignment ∈ [-1, 1]
        rb.drag = Mathf.Clamp(drag, 0.2f, 1f);

        //Damping if moving away
        if(currentState == CreatureState.Attack)
        {
            if (alignment < 0f) rb.velocity *= 0.95f;
        }
        else if (alignment < -0.3f) rb.velocity *= 0.97f;

        Vector3 sideDir = Vector3.Cross(rb.velocity.normalized, Vector3.up);

        // Extra randomness
        float noise = Mathf.PerlinNoise(Time.time * wanderFrequency + noiseOffset, 0f);
        float wanderForce = (noise - 0.5f) * 2f * wanderStrength;

        // Apply sideways flutter force
        rb.AddForce(sideDir * wanderForce);
    }

    void Wander()
    {
        if(coroutineRunning) return;
        if(playerInSightRange && (MainMenuScript.currentFileMode != FileMode.Cozy || fireSources.Count == 0)) currentState = CreatureState.Chase;

        AddForceToBug(targetPos, true);

        LimitVelocity();

        SmoothLookAt(targetPos);

        if(targetMoth && Vector3.Distance(targetMoth.transform.position, transform.position) < 2) targetMoth.TakeDamage(999);
    }

    void Chase()
    {
        if(!playerInSightRange /*|| fireSources.Count > 0*/) currentState = CreatureState.Wander;
        else if(playerInAttackRange) currentState = CreatureState.Attack;

        AddForceToBug(player.position, false);

        LimitVelocity();

        SmoothLookAt(player.position);

        //if(Vector3.Distance(transform.position, player.position))
    }

    void Attack()
    {
        SmoothLookAt(player.position);

        if(coroutineRunning) return;

        if(!playerInAttackRange)
        {
            currentState = CreatureState.Chase;
            return;
        }

        coroutineRunning = true;
        StartCoroutine(AttackRoutine());
    }

    void Stuck()
    {
        SmoothLookAt(player.position);

        // Get only the Y-axis (horizontal) rotation from the camera
        Quaternion horizontalRotation = Quaternion.Euler(0f, PlayerInteraction.Instance.mainCam.transform.eulerAngles.y, 0f);

        // Apply position and rotation based on horizontal rotation only
        transform.position = PlayerInteraction.Instance.mainCam.transform.position + horizontalRotation * localOffset;
        transform.rotation = horizontalRotation * rotationOffset;
    }
    //PlayerInteraction.Instance.mainCam.transform

    void Flee()
    {
        AddForceToBug(targetPos, true);

        LimitVelocity();

        SmoothLookAt(targetPos);
    }

    IEnumerator RefreshDestination()
    {
        while(health > 0)
        {
            yield return new WaitForSeconds(Random.Range(0.5f, 1f));
            if(currentState == CreatureState.Wander)
            {
                if(fireSources.Count > 0)
                {
                    float distFromFire = 0;
                    float minDistance = 25;
                    int foundIndex = -1;
                    for(int i = 0; i < fireSources.Count; i++)
                    {
                        if(!fireSources[i] || fireSources[i].gameObject.activeInHierarchy == false) //remove fire if its deactivated
                        {
                            fireSources.RemoveAt(i);
                            i--;
                            continue;
                        }

                        distFromFire = Vector3.Distance(fireSources[i].transform.position, transform.position);

                        if(distFromFire < minDistance) //if this is the closest fire its the new target
                        {
                            foundIndex = i;
                            minDistance = distFromFire;
                        }
                    }
                    if(foundIndex > -1)
                    {
                        targetPos = GetRandomPointNearby(fireSources[foundIndex].transform.position); //Go to the nearest fire
                        continue;
                    }
                }
                if(targetMoth) targetPos = targetMoth.transform.position;
                else if(homeSwarm) targetPos = GetRandomPointNearby(homeSwarm.transform.position); //Follow the swarm
                else if(homeNest)
                {
                    if(TimeManager.Instance.isDay && Vector3.Distance(homeNest.transform.position, transform.position) < 5)
                    {
                        Destroy(gameObject);
                        homeNest.heldWasps++;
                    }
                    else targetPos = GetRandomPointNearby(homeNest.transform.position); //Stay by the nest
                } 
                else targetPos = StructureManager.Instance.GetRandomNearbyTile(GridType.Farm, 25, transform.position); //Random movement
            }

            if(currentState == CreatureState.Flee)
            {
                if(Vector3.Distance(targetPos, transform.position) < 2) 
                {
                    currentState = CreatureState.Wander;
                    fearObject.SetActive(false);
                }
            }
        }
    }

    IEnumerator AttackRoutine()
    {
        anim.Play("Sting");
        yield return new WaitForSeconds(0.15f);
        if(playerInAttackRange)
        {
            PlayerInteraction.Instance.StaminaChange(-damageToPlayer);
            if(Random.Range(0, 10) > 4) //It got stuck!
            {
                currentState = CreatureState.Stuck;
                rb.velocity = Vector3.zero;
                rb.isKinematic = true;
                allColliders[0].isTrigger = true;
                anim.Play("StuckIdle");
                StartCoroutine(StuckRoutine());
                transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

                // Use only horizontal (Y) rotation to calculate offset
                Quaternion horizontalRotation = Quaternion.Euler(0f, PlayerInteraction.Instance.mainCam.transform.eulerAngles.y, 0f);

                // Calculate offset using horizontal rotation only
                localOffset = Quaternion.Inverse(horizontalRotation) * (transform.position - PlayerInteraction.Instance.mainCam.transform.position);

                // Calculate rotation offset relative to horizontal rotation
                rotationOffset = Quaternion.Inverse(horizontalRotation) * transform.rotation;
                yield break;
            }
        }
        yield return new WaitForSeconds(0.8f);
        coroutineRunning = false;
    }

    IEnumerator StuckRoutine()
    {
        unstickAttempts = 0;
        int attemptsNeeded = Random.Range(2, 8);
        anim.SetBool("Unstuck", false);

        stuckOnPlayer = true;
        PlayerMovement.Instance.ApplySpeedMod(new MovementSpeedModifiers(gameObject, 0.9f, "Wasp", true));

        while(unstickAttempts < attemptsNeeded)
        {
            unstickAttempts++;
            yield return new WaitForSeconds(Random.Range(2f, 4f));
            anim.Play("StuckPull");
            yield return new WaitForSeconds(0.3f);
            Vector3 dir = (transform.position - player.position).normalized;
            PlayerMovement.limitMaxVelocity = false;
            if(PlayerMovement.restrictMovementTokens == 0) PlayerInteraction.Instance.GetComponent<PlayerMovement>().ApplyForceToPlayer(800, dir);
            yield return new WaitForSeconds(0.2f);
            PlayerMovement.limitMaxVelocity = true;

            //if(fireSources.Count > 0) unstickAttempts += 30;
        }
        rb.isKinematic = false;
        allColliders[0].isTrigger = false;
        anim.SetBool("Unstuck", true);
        currentState = CreatureState.Wander;

        stuckOnPlayer = false;
        PlayerMovement.Instance.RemoveSpeedMod(gameObject);
        yield return new WaitForSeconds(3f);
        coroutineRunning = false;
    }

    Vector3 GetRandomPointNearby(Vector3 target)
    {
        float x = Random.Range(-2f, 2f);
        float z = Random.Range(-2f, 2f);
        return new Vector3(target.x + x, target.y, target.z + z);
    }

    void SmoothLookAt(Vector3 targetPos)
    {
        targetPos.y = transform.position.y;
        // Calculate the direction vector to the target
        Vector3 direction = targetPos - transform.position;

        // Create a Quaternion representing the target rotation
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // Smoothly interpolate towards the target rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5 * Time.deltaTime);
    }

    void LimitVelocity()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        float currentMaxVelocity = maxVelocity;
        //if(currentState == CreatureState.Attack) currentMaxVelocity = attackMaxVelocity;

        // Limit velocity if needed
        if (flatVel.magnitude > currentMaxVelocity)
        {
            Vector3 limitedVel = flatVel.normalized * currentMaxVelocity;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    public override void EnteredFireRadius(FireFearTrigger _fireSource, out bool successful)
    {
        if(!fireSources.Contains(_fireSource))
        {
            fireSources.Add(_fireSource);
            successful = true;
        }
        else successful = false;
    }

    public override void NearLaventLeaf(Vector3 pos)
    {
        if(currentState == CreatureState.Flee) return;
        if(currentState == CreatureState.Stuck)
        {
            unstickAttempts = 99;
            return;
        }
        currentState = CreatureState.Flee; //Will need adjustments when its stuck
        //Vector3 distanceBehind = Vector3.Distance(transform.position, pos);
        targetPos = transform.position + (-transform.forward * 15);
        fearObject.SetActive(true);
    }

    IEnumerator ScoutForMoth()
    {
        while(health > 0)
        {
            yield return new WaitForSeconds(5);
            if(targetMoth) continue;
            Collider[] nearbyMoths = Physics.OverlapSphere(transform.position, 15f, 1 << 9);
            foreach(Collider collider in nearbyMoths)
            {
                Pollinator moth = collider.gameObject.GetComponentInParent<Pollinator>();
                if(moth) targetMoth = moth;
                break;
            }
        }
    }

    void OnDestroy()
    {
        base.OnDestroy();
        if(homeSwarm) homeSwarm.wasps.Remove(gameObject);

        if(stuckOnPlayer)
        {
            PlayerMovement.limitMaxVelocity = true;
            PlayerMovement.Instance.RemoveSpeedMod(gameObject);
        }
    }

    public override void OnDeath()
    {
        base.OnDeath();
        currentState = CreatureState.Dead;
        if (!isDead)
        {
            anim.SetBool("Unstuck", false);
            anim.Play("StuckIdle");
            rb.velocity = Vector3.zero;
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.AddForce(-transform.forward * 70, ForceMode.Impulse);
            rb.AddForce(-Vector3.up * 20);
            isDead = true;
            StopAllCoroutines();
            StartCoroutine(DeathTimer());
            if(health < -20)
            {
                canCorpseBreak = true;
                TakeDamage(100);
            }
            else canCorpseBreak = false;
        }
    }

    IEnumerator DeathTimer()
    {
        yield return new WaitForSeconds(3);
        canCorpseBreak = true;
        TakeDamage(100);
    }

    void OnCollisionEnter(Collision other)
    {
        if((other.gameObject.layer == 7 || other.gameObject.layer == 6) && health <= 0)
        {
            canCorpseBreak = true;
            TakeDamage(100);
        }
    }

    public override bool CaughtByBugNet(out InventoryItemData item)
    {
        item = null;
        TakeDamage(999);
        return false;
    }

    public override bool OnStun(float duration) // For the resin pole trap
    {
        if (currentState != CreatureState.Stun && currentState != CreatureState.Stuck)
        {
            currentState = CreatureState.Stun;
            rb.velocity = Vector3.zero;
            anim.Play("StuckIdle");
            return true;
        }
        return false;
    }

    public override void NewPriorityTarget(StructureBehaviorScript newStruct)
    {
        if (currentState == CreatureState.Stun || currentState == CreatureState.Stuck) return;
        targetPos = newStruct.transform.position;
    }
}
