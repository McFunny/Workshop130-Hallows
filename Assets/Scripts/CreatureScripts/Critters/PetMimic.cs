using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PetMimic : CritterBehaviorScript
{
    public Collider attackHitbox;

    int pacesUntilIdle = 5; //How many times does this wander before trying to idle
    int pacesUntilCalm = 0;

    float originalSpeed;
    float fleeSpeed = 15;
    float coolDownSpeed = 8;

    private bool coroutineRunning = false;
    bool hasTarget, attacking, attackCooldown, speedCooldown;

    public InventoryItemData fiberItem, flaskItem;
    public float growthProgress = 0;
    int maxGrowth = 100;
    public GameObject harvestReadyObj;

    float baseAttack = 15;

    public CropData trap;

    [HideInInspector] public CreatureBehaviorScript targetCreature; //Pheromone afflicted creature

    public enum CritterState
    {
        Emerge, //Left Burrow
        Wander, //if it runs into a structure or player, will swipe at it
        Idle, //Will resume wandering after x amount of time or it was struck
        Die,
        ChaseTarget,
        Eat //Eating at the trough
    }

    public CritterState currentState;
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        base.Start();
        StartCoroutine(IdleSoundTimer());

        originalSpeed = agent.speed;

        StartCoroutine(ScanForScentedTargets());
    }

    //////////////ICritter Stuff\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\
    public CritterData GetCritterData(){ return new CritterData(creatureData.id, friendshipLevel, friendPoints, health, hunger, thirst, name, growthProgress);} //For saving purposes

    public void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        if(growthProgress >= maxGrowth)
        {
            //Shear it
            ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);
            if(Random.Range(0, 10) < friendshipLevel + 1) ItemPoolManager.Instance.GrabItem(flaskItem).transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
            for(int i = 0; i < 4; i++)
            {
                if(Random.Range(0, i) == 0) ItemPoolManager.Instance.GrabItem(fiberItem).transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
            }
            harvestReadyObj.SetActive(false);
            growthProgress = 0;
            FriendPointsChange(10, true);
        }
        else if(!alreadyPet)
        {
            alreadyPet = true;
            FriendPointsChange(25, true);
            effectsHandler.PlaySound(effectsHandler.petSound);
            interactSuccessful = true;
            return;
        }
        interactSuccessful = false;
    }

    public void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        if(hunger < 100 && item.foodForCritters.Count >= 0 || item.foodForCritters.Contains(critterType))
        {
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            PlayerInventoryHolder.Instance.UpdateInventory();
            EatFood(item);
            interactSuccessful = true;
            return;
        }
        interactSuccessful = true;
    }

    public override void LoadData(CritterData c)
    {
        base.LoadData(c);
        growthProgress = c.extraVar;
        if(growthProgress >= maxGrowth) harvestReadyObj.SetActive(true);
    }

    ////////Critter Specific Stuff///////////
    /// 
    public void CheckState(CritterState currentState)
    {
        if(behaviorDelay) return;
        switch (currentState)
        {
            case CritterState.Emerge:
                EmergeFromTile();
                break;

            case CritterState.Wander:
                Wander();
                break;

            case CritterState.Idle:
                Idle();
                break;

            case CritterState.Die:
                break;

            case CritterState.ChaseTarget:
                ChaseTarget();
                break;

            case CritterState.Eat:
                Eat();
                break;

            default:
                Debug.LogError("Unknown state: " + currentState);
                break;
        }
    }

    void Update()
    {
        if (health <= 0)
        {
            isDead = true;
            return;
        }

        CheckState(currentState);

        if(agent.velocity.sqrMagnitude > 0) anim.SetBool("IsMoving", true);
        else anim.SetBool("IsMoving", false);

        if(speedCooldown)
        {
            agent.speed = coolDownSpeed * actionSpeedMod;
        }
        else
        {
            if(pacesUntilCalm > 0 && agent.speed != fleeSpeed) agent.speed = fleeSpeed * actionSpeedMod;
            if(pacesUntilCalm <= 0 && agent.speed != originalSpeed) agent.speed = originalSpeed * actionSpeedMod;
        }
    }

    protected override void OnHour()
    {
        base.OnHour();
        growthProgress += Random.Range(5f, 8f);
        if(growthProgress > maxGrowth)
        {
            growthProgress = maxGrowth;
            harvestReadyObj.SetActive(true);
        }

        if(TimeManager.Instance.currentHour == 8) SpawnTrap();
    }

    private void Idle()
    {
        if (!coroutineRunning)
        {
            currentState = CritterState.Wander;
        }
    }

    private IEnumerator WaitAround()
    {
        coroutineRunning = true;
        float r = Random.Range(6f, 18f);
        yield return new WaitForSeconds(r);
        coroutineRunning = false;

        if(playerFollowTokens <= 0 && Vector3.Distance(player.position, transform.position) < 30 && Random.Range(0, 100) < friendshipLevel * 7)
        {
            playerFollowTokens = Random.Range(4, 8);
        }
    }

    private void Wander()
    {
        if(coroutineRunning) return;

        if(ScentedTargetExists())
        {
            currentState = CritterState.ChaseTarget;
            return;
        }

        if (hasTarget && !agent.pathPending && agent.remainingDistance < agent.stoppingDistance + 1f)
        {
            hasTarget = false;
            pacesUntilIdle--;
            if(playerFollowTokens > 0) playerFollowTokens--;
            if(pacesUntilCalm > 0) pacesUntilCalm--;
               
            if (pacesUntilIdle <= 0)
            {
                pacesUntilIdle = Random.Range(5, 11);
                StartCoroutine(WaitAround());
                currentState = CritterState.Idle;
                effectsHandler.Idle1();
                return;
            }
        }
        else if (!hasTarget && !attackCooldown)
        {
            hasTarget = true;
            Vector3 wanderDirection = transform.forward;

               
            float randomAngle = Random.Range(-70f, 70f); //random offset for random movement

            wanderDirection = Quaternion.Euler(0, randomAngle, 0) * wanderDirection;

            Vector3 newDestination = transform.position + wanderDirection * 5;

            if(Vector3.Distance(BarnManager.Instance.barnSource.position, transform.position) > 30) newDestination = GetRandomPointAround(BarnManager.Instance.barnSource.position, 20);
            else if(playerFollowTokens > 0) target = GetRandomPointAround(player.position, 20);

           
            agent.SetDestination(newDestination);
        }
    }

    void EmergeFromTile()
    {
        if(coroutineRunning) return;
        anim.SetBool("IsBuried", false);
        StartCoroutine(EmergeCoroutine());
    }

    void ChaseTarget()
    {
        if(!ScentedTargetExists())
        {
            currentState = CritterState.Wander;
            return;
        }

        if(!coroutineRunning && !attackCooldown)
        {
            if((CheckForPlayer(corpseParticleTransform)))
            {
                StartCoroutine(SwipeTarget());
                hasTarget = false;
            }
            else if(targetCreature && Vector3.Distance(transform.position, targetCreature.transform.position) < attackRange)
            {
                StartCoroutine(SwipeTarget());
                hasTarget = false;
            }
        }

        if(targetCreature) agent.SetDestination(targetCreature.transform.position);
        else if(StatusEffectManager.Instance.FindStatusOnPlayer(StatusEffectName.MimicScent)) agent.SetDestination(player.position);
        //else if(targetCreature) agent.SetDestination(targetCreature.transform.position);
    }

    void Eat()
    {
        if (!coroutineRunning)
        {
            if(!targetObject)
            {
                currentState = CritterState.Idle;
                return;
            }
            target = targetObject.position;
            StartCoroutine(MoveToPoint(target, 10));
            coroutineRunning = true;
        }
        else if (Vector3.Distance(transform.position, target) < 2f)
        {
            interruptAction = true;
        }
    }

    protected override void FinishedMoving()
    {
        if(currentState == CritterState.Eat && targetObject) //Cat Reached the Bowl
        {
            Trough trough = targetObject.GetComponent<Trough>();
            if(!trough) //Trough is gone
            {
                targetObject = null;
                isMoving = false;
                currentRoutine = null;
                return;
            }
            bool isEating = false, isDrinking = false;
            if(hunger <= hunger/4 && trough.HasEdibleItem(critterType)) isEating = true;
            if(thirst <= thirst/4 && trough.waterLevel > 0) isDrinking = true;

            if(Vector3.Distance(targetObject.transform.position, transform.position) < 1.5f && (isEating || isDrinking))
            {
                agent.velocity = Vector3.zero;
                agent.ResetPath();
                if(isEating)
                {
                    trough.EatItem(critterType, out InventoryItemData itemEaten);
                    EatFood(itemEaten);
                }
                else
                {
                    trough.WaterLevelChange(-1);
                    thirst = maxThirst;
                    FriendPointsChange(5, true);
                }
                currentRoutine = StartCoroutine(EatRoutine());
                isMoving = false;
                targetObject = null;
                target = Vector3.zero;
                return;
            }
            else if(!isEating && !isDrinking) targetObject = null;
        }

        coroutineRunning = false;
    }

    IEnumerator EmergeCoroutine()
    {
        coroutineRunning = true;
        health = maxHealth;
        yield return new WaitForSeconds(1.5f);
        effectsHandler.Idle2();
        yield return new WaitForSeconds(0.5f);
        agent.enabled = true;
        currentState = CritterState.Wander;
        coroutineRunning = false;
    }

    IEnumerator SwipeTarget()
    {
        coroutineRunning = true;
        anim.SetTrigger("IsAttackingPlayer");
        StartCoroutine(AttackCooldown());
        speedCooldown = true;
        yield return new WaitForSeconds(0.7f);
        effectsHandler.MiscSound();
        attacking = true;
        attackHitbox.enabled = true;
        yield return new WaitForSeconds(0.3f);
        attackHitbox.enabled = false;
        yield return new WaitForSeconds(1);
        speedCooldown = false;

        attacking = false;
        coroutineRunning = false;
    }

    IEnumerator AttackCooldown()
    {
        attackCooldown = true;
        float t = Random.Range(2.5f, 3.5f);
        float p = 0;
        while(p < t)
        {
            if(currentState == CritterState.Wander)
            {
                if(targetCreature) agent.SetDestination(targetCreature.transform.position);
                else if(StatusEffectManager.Instance.FindStatusOnPlayer(StatusEffectName.MimicScent)) agent.SetDestination(player.position);
                //else if(targetCreature) agent.SetDestination(targetCreature.transform.position);
                else agent.SetDestination(player.position);
            }
            yield return new WaitForSeconds(0.2f);
            p += 0.2f;
        }
        attackCooldown = false;
    }

    IEnumerator EatRoutine()
    {
        yield return new WaitForSeconds(5);
        currentState = CritterState.Wander;
        coroutineRunning = false;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (attacking && !isDead)
        {
            PlayerInteraction playerInteraction = other.GetComponent<PlayerInteraction>();
            if (playerInteraction != null)
            {
                playerInteraction.StaminaChange(damageToPlayer);
                attackHitbox.enabled = false;
            }

            if(targetCreature && other.GetComponentInParent<CreatureBehaviorScript>() == targetCreature)
            {
                float damageDealt = baseAttack + (5 * (friendshipLevel + 1));
                targetCreature.TakeDamage(damageDealt);
                targetCreature.PlayHitParticle(targetCreature.transform.position);

                FriendPointsChange(5, true);
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
            agent.enabled = false;
            rb.isKinematic = true;
            rb.freezeRotation = true;
            StopAllCoroutines();

            PopupHandler.Instance.names.Enqueue(name);
            PopupHandler.Instance.AddToQueue(PopupHandler.Instance.critterDiedPopup);
        }
    }

    public override void OnDamage()
    {
        if(currentState == CritterState.Idle)
        {
            currentState = CritterState.Wander;
        }
        pacesUntilCalm = Random.Range(4, 8);
    }

    public override void OnCorpseDamage()
    {
        if(health <= 0 && canCorpseBreak)
        {
            anim.Play("DeathRecoil", -1, 0f);
        }
    }

    IEnumerator ScanForScentedTargets()
    {
        while(health > 0)
        {
            yield return new WaitForSeconds(3);
            if(targetCreature) continue;

            Collider[] hitEnemies = Physics.OverlapSphere(transform.position, 100f, 1 << 9); //Make radius much larger for critter variant
            foreach(Collider collider in hitEnemies)
            {
                var creature = collider.GetComponentInParent<CreatureBehaviorScript>();
                if (creature != null && StatusEffectManager.Instance.FindStatusOnCreature(StatusEffectName.MimicScent, creature))
                {
                    targetCreature = creature;
                    continue;
                }
            }
        }
    }

    bool ScentedTargetExists()
    {
        if(targetCreature && StatusEffectManager.Instance.FindStatusOnCreature(StatusEffectName.MimicScent, targetCreature)) return true; //Creature has it

        if(StatusEffectManager.Instance.FindStatusOnPlayer(StatusEffectName.MimicScent)) return true; //Player has it

        return false;
    }

    void SpawnTrap()
    {
        if(Random.Range(0, 100) <= (friendshipLevel + 1) * 8) StructureManager.Instance.PopulateCrop(1, 1, trap);

        if(Random.Range(0, 100) <= (friendshipLevel + 1) * 8) StructureManager.Instance.PopulateCrop(1, 1, trap);
    }
}
