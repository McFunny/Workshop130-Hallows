using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wisp : CreatureBehaviorScript
{
    Vector3 despawnPos;

    public float moveSpeed = 4f;
    public float stunnedSpeed = 0.5f;
    public float fleeSpeed = 12f;
    float currentSpeed;
    public float turnSpeed = 360f;

    public float hazeDistance = 2; // Distance it will use ice breath attack


    private Vector3 moveDirection;
    private Vector3 targetDirection;
    Vector3 targetPos;

    public List<StructureObject> targettableStructures;
    StructureBehaviorScript targetStructure;

    List<FireFearTrigger> nearbyFires = new List<FireFearTrigger>();

    bool attackCooldown = false;
    bool isAttacking = false;
    bool isFrosting = false;
    bool pauseFromLight;
    float fleeTimeLeft;

    Coroutine currentRoutine;

    public ParticleSystem frostParticles;
    public Collider frostCollider, punchCollider;

    public Material hiddenMat, visibleMat;
    SkinnedMeshRenderer[] allChildRenderers;
    public ParticleSystem hurtParticles;
    public GameObject deathParticles;
    bool currentlyVisible = true;
    public Renderer[] renderers;
    public Color nearbyColor, hiddenColor;

    public enum CreatureState
    {
        Wander,
        FrostStructure, //Moving to structure to frost area
        AttackPlayer, //Moving to player to Punch
        ExtinguishFlame, //Covering eyes to extinguish fire
        Stunned,
        BeingCaptured,
        Flashed, //Stunned for a moment by the player torch
        Dead
    }

    public CreatureState currentState;

    void Start()
    {
        base.Start();
        currentState = CreatureState.Wander;
        despawnPos = NightSpawningManager.Instance.despawnPositions[Random.Range(0, NightSpawningManager.Instance.despawnPositions.Length)].position;

        StartCoroutine(RefreshBehavior());
        StartCoroutine(IdleSoundTimer());
        StartCoroutine(CheckNearbyFires());

        if(!inWilderness) targetPos = StructureManager.Instance.GetRandomTile();

        allChildRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();

        VisibilityChange(false);
    }

    void FixedUpdate()
    {
        base.Update();
        if(isDead) return;

        if(currentState == CreatureState.ExtinguishFlame)
        {
            currentSpeed = stunnedSpeed;
            anim.SetBool("CoverEyes", true);
            anim.SetBool("Fleeing", false);
        }
        else if(fleeTimeLeft > 0)
        {
            fleeTimeLeft -= Time.deltaTime;
            currentSpeed = fleeSpeed;
            anim.SetBool("CoverEyes", false);
            anim.SetBool("Fleeing", true);
        }
        else 
        {
            currentSpeed = moveSpeed;
            anim.SetBool("CoverEyes", false);
            anim.SetBool("Fleeing", false);
        }

        rb.angularVelocity = Vector3.zero;

        if(!currentlyVisible) UpdateMaterial();

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
            case CreatureState.FrostStructure:
                FrostStructure();
                break;
            case CreatureState.AttackPlayer:
                AttackPlayer();
                break;
            case CreatureState.ExtinguishFlame:
                ExtinguishFlame();
                break;
            case CreatureState.Flashed:
                Flashed();
                break;
            case CreatureState.BeingCaptured:
                //
                break;
        }
    }

    void Wander()
    {
        if(targetStructure) targetPos = targetStructure.transform.position;

        float distance = Vector3.Distance(transform.position, targetPos);

        if(distance <= hazeDistance)
        {
            if(targetStructure)
            {
                currentState = CreatureState.FrostStructure;
                return;
            }
            if(!inWilderness) 
            {
                if(fleeTimeLeft > 0) targetPos = NightSpawningManager.Instance.RandomMistPosition();
                else targetPos = StructureManager.Instance.GetRandomTile();
            }
            else
            {
                targetPos = player.position;
            }
        }

        // Decide target direction
        if(!inWilderness && TimeManager.Instance.isDay)
        {
            targetDirection = (despawnPos - transform.position).normalized;
        }
        else if(inWilderness)
        {
            currentState = CreatureState.AttackPlayer;
            return;
        }
        else
        {
            targetDirection = (targetPos - transform.position).normalized;
        }

        // Gradually rotate moveDirection toward targetDirection
        //moveDirection = Vector3.RotateTowards(moveDirection, targetDirection, Mathf.Deg2Rad * turnSpeed * Time.fixedDeltaTime, 0f).normalized;

        // Move
        rb.velocity = targetDirection * currentSpeed + new Vector3(0, rb.velocity.y, 0);

        // Smooth rotate Rigidbody
        if (targetDirection != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(targetDirection, Vector3.up);
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRot, turnSpeed * Time.fixedDeltaTime));
        }
    }

    IEnumerator RefreshBehavior()
    {
        while(health > 0)
        {
            //have it randomly target crop or player
            yield return new WaitForSeconds(Random.Range(4f, 15f));
            if(fleeTimeLeft > 0 || currentState != CreatureState.Wander || targetStructure) continue;

            float r = Random.Range(0, 100);
            if(r < 60) FindStructure();
            else currentState = CreatureState.AttackPlayer;
        }
    }

    void FindStructure()
    {
        List<StructureBehaviorScript> availableStructures = new List<StructureBehaviorScript>();
        foreach (var structure in structManager.allStructs)
        {
            if(!structure || !targettableStructures.Contains(structure.structData) || structure.absentFromFarmGrid) continue;

            FarmLand tile = structure as FarmLand;
            if (tile && tile.crop && !tile.isWeed && tile.currentUpgrade != FarmLand.FarmTileUpgrade.Corrupt && !tile.isFrosted)
            {
                if(Random.Range(0,3) == 0 || availableStructures.Count == 0) availableStructures.Add(structure); //Crops have less likely chance to be chosen
                continue;
            }

            IWaterHolder wHolder = structure as IWaterHolder;
            if (wHolder != null && wHolder.CanBeFrozen())
            {
                availableStructures.Add(structure); 
                continue;
            }
        }

        if (availableStructures.Count > 0)
        {
            int r = Random.Range(0, availableStructures.Count);
            targetStructure = availableStructures[r];
        }
        else currentState = CreatureState.AttackPlayer;
    }

    void FrostStructure()
    {
        rb.velocity = Vector3.zero;

        if(currentRoutine == null)
        {
            currentRoutine = StartCoroutine(FrostStructureRoutine());
            StartCoroutine(AttackCooldownTimer(6));
        }
    }

    IEnumerator FrostStructureRoutine()
    {
        anim.Play("ghoulBlow");
        VisibilityChange(true);
        yield return new WaitForSeconds(0.8f);
        if(currentState != CreatureState.FrostStructure) yield break;

        frostParticles.Play();
        isFrosting = true;
        frostCollider.enabled = true;
        yield return new WaitForSeconds(1.2f);
        isFrosting = false;
        frostCollider.enabled = false;
        frostParticles.Stop();
        yield return new WaitForSeconds(0.8f);
        ResetToFlee();
        currentRoutine = null;
    }

    void AttackPlayer()
    {
        Vector3 toPlayer = (player.position - transform.position);
        float distance = toPlayer.magnitude;

        // Base chase direction
        Vector3 chaseDir = toPlayer.normalized;

        chaseDir = chaseDir.normalized;

        chaseDir.y = 0;

        Quaternion targetRot = Quaternion.LookRotation(chaseDir, Vector3.up);

        /*if(currentRoutine != null) 
        {
            rb.velocity = Vector3.zero;

            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRot, 360 * Time.fixedDeltaTime));
            return;
        }*/

        if(distance < attackRange && !attackCooldown && currentRoutine == null)
        {
            currentRoutine = StartCoroutine(AttackPlayerRoutine());
            StartCoroutine(AttackCooldownTimer(2));
        }

        // Apply velocity
        if(distance > 0.7f) rb.velocity = chaseDir * currentSpeed + new Vector3(0, rb.velocity.y, 0);
        else rb.velocity = Vector3.zero;

        rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRot, 360 * Time.fixedDeltaTime));
    }

    IEnumerator AttackPlayerRoutine()
    {
        VisibilityChange(true);
        anim.SetTrigger("Punching");
        rb.velocity = Vector3.zero;
        effectsHandler.PlaySound(effectsHandler.extraSounds[0]);
        yield return new WaitForSeconds(1f);
        effectsHandler.PlaySound(effectsHandler.extraSounds[2]);
        punchCollider.enabled = true;
        isAttacking = true;

        //effectsHandler.PlaySound(effectsHandler.miscSound3);
        yield return new WaitForSeconds(0.1f);
        punchCollider.enabled = false;
        isAttacking = false;
        yield return new WaitForSeconds(0.5f);
        //Exit current state to flee away
        ResetToFlee();
        currentRoutine = null;
    }

    void ResetToFlee()
    {
        VisibilityChange(false);
        targetStructure = null;
        currentState = CreatureState.Wander;
        fleeTimeLeft = Random.Range(4f, 6f);
        targetPos = NightSpawningManager.Instance.RandomMistPosition();
    }

    void VisibilityChange(bool visible)
    {
        if(visible == currentlyVisible || (!visible && nearbyFires.Count > 0)) return;
        currentlyVisible = visible;
        shovelVulnerable = visible;
        effectsHandler.PlaySound(effectsHandler.extraSounds[3]);
        hurtParticles.Play();
        for(int i = 0; i < allChildRenderers.Length; i++)
        {
            if(visible) allChildRenderers[i].material = visibleMat;
            else allChildRenderers[i].material = hiddenMat;
        }
    }

    void UpdateMaterial()
    {
        for(int i = 0; i < renderers.Length; i++)
        {
            Material mat = renderers[i].material;
            if(currentlyVisible || mat == visibleMat) return;
            float dist = Vector3.Distance(player.position, transform.position);
            Color newColor = Color.Lerp(nearbyColor, hiddenColor, dist/20);
            mat.SetColor("_Fresnel_Color", newColor);
        }
    }

    IEnumerator AttackCooldownTimer(float time)
    {
        attackCooldown = true;
        yield return new WaitForSeconds(time);
        attackCooldown = false;
        
    }

    void ExtinguishFlame()
    {
        if(pauseFromLight) 
        {
            if(currentRoutine == null) currentRoutine = StartCoroutine(PauseFromLight());
            return;
        }
        if(currentRoutine != null)
        {
            rb.velocity = Vector3.zero;
            return;
        }

        float distance = Vector3.Distance(transform.position, targetPos);

        if(distance <= hazeDistance + 1 || !targetStructure)
        {
            if(targetStructure)
            {
                currentState = CreatureState.FrostStructure;
            }
            else
            {
                currentState = CreatureState.Wander;
            }
            return;
        }
        targetDirection = (targetPos - transform.position).normalized;

        // Move
        rb.velocity = targetDirection * currentSpeed + new Vector3(0, rb.velocity.y, 0);

        // Smooth rotate Rigidbody
        if (targetDirection != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(targetDirection, Vector3.up);
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRot, turnSpeed * Time.fixedDeltaTime));
        }
    }

    IEnumerator PauseFromLight()
    {
        pauseFromLight = false;
        rb.velocity = Vector3.zero;
        yield return new WaitForSeconds(1.2f);
        currentRoutine = null;
    }

    void Flashed()
    {
        rb.velocity = Vector3.zero;

        if(currentRoutine == null)
        {
            currentRoutine = StartCoroutine(FlashedCoroutine());
        }
    }

    IEnumerator FlashedCoroutine()
    {
        anim.SetBool("Shocked", true);
        effectsHandler.PlaySound(effectsHandler.extraSounds[4]);
        yield return new WaitForSeconds(2);
        anim.SetBool("Shocked", false);
        ResetToFlee();
        currentRoutine = null;
    }

    public override void EnteredFireRadius(FireFearTrigger _fireSource, out bool successful)
    {
        if(!nearbyFires.Contains(_fireSource)) nearbyFires.Add(_fireSource);
        successful = true;

        if((currentState == CreatureState.Wander || (currentState == CreatureState.AttackPlayer && !attackCooldown)) && fleeTimeLeft <= 0)
        {
            VisibilityChange(true);
            if(_fireSource.priority >= 5) //Player torch
            {
                currentState = CreatureState.Flashed;
                return;
            }

            var structure = _fireSource.gameObject.GetComponentInParent<StructureBehaviorScript>();
            IFireHolder fHolder = structure as IFireHolder;
            if (fHolder != null && fHolder.CanBeExtinguished())
            {
                targetStructure = structure;
                targetPos = targetStructure.transform.position;

                pauseFromLight = true;
                currentState = CreatureState.ExtinguishFlame;
            }
        }

        //Pause for a moment to cover eyes, then proceed to target
    }

    public override void OnDamage()
    {
        effectsHandler.OnHit();
        hurtParticles.Play();
        //Maybe teleport? Or flee at least
        effectsHandler.PlaySound(effectsHandler.extraSounds[3]);

        if(Vector3.Distance(transform.position, player.position) < 9 && currentState == CreatureState.ExtinguishFlame)
        {
            if(Random.Range(0,10) > 6) currentState = CreatureState.AttackPlayer;
            else ResetToFlee();
        }
    }



    void OnTriggerEnter(Collider other)
    {
        if(isAttacking)
        {
            if(other.gameObject.layer == 10) 
            {
                PlayerInteraction.Instance.StaminaChange(damageToPlayer);
                isAttacking = false;
                effectsHandler.PlaySound(effectsHandler.extraSounds[1]);
            }
        }
        if(isFrosting)
        {
            if(other.gameObject.layer == 10) PlayerInteraction.Instance.ApplyStatusEffect(StatusDatabase.Instance.GetStatus(StatusEffectName.Frost), 10);

            var farmTile = other.GetComponentInParent<FarmLand>();
            if(farmTile)
            {
                farmTile.RecieveFrost();
                return;
            }

            var structure = other.GetComponentInParent<StructureBehaviorScript>();

            IWaterHolder wHolder = structure as IWaterHolder;
            if (wHolder != null && wHolder.CanBeFrozen())
            {
                wHolder.Freeze();
                return;
            }

            IFireHolder fHolder = structure as IFireHolder;
            if (fHolder != null && fHolder.CanBeExtinguished())
            {
                fHolder.ExternalExtinguish();
                return;
            }
        }
    }

    public override void OnDeath()
    {
        if (!isDead)
        {
            deathParticles.SetActive(true);
            deathParticles.transform.parent = null;
            Destroy(gameObject);
        }
    }

    IEnumerator IdleSoundTimer()
    {
        while(health > 0)
        {
            int i = Random.Range(3,8);
            //effectsHandler.RandomIdle();
            yield return new WaitForSeconds(i);
        }
    }

    IEnumerator CheckNearbyFires()
    {
        float distFromFire = 0;
        while(true)
        {
            yield return new WaitForSeconds(1);
            if(nearbyFires.Count == 0) 
            {
                if(currentState == CreatureState.Wander && currentlyVisible) VisibilityChange(false);
                continue;
            }

            for(int i = 0; i < nearbyFires.Count; ++i)
            {
                if(nearbyFires[i] != null) distFromFire = Vector3.Distance(nearbyFires[i].transform.position, transform.position);
                if(nearbyFires[i] == null || nearbyFires[i].gameObject.activeInHierarchy == false || distFromFire > nearbyFires[i].fleeRange)
                {
                    nearbyFires.RemoveAt(i);
                    --i;
                }
            }
        }
    }
}
