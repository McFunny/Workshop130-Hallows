using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class PetDog : PetBehaviorScript, IInteractable
{
    public InventoryItemData heldItem;
    public SpriteRenderer itemR;
    public List<ItemWithAmount> possibleGiftItems = new List<ItemWithAmount>();
    public List<CreatureObject> targettableCreatures = new List<CreatureObject>();
    public List<CreatureObject> fearedCreatures = new List<CreatureObject>();

    public Transform headPivot;
    Vector3 starePoint;
    Quaternion defaultHeadRotation;

    Vector3 oldJumpPos;
    //bool lookFreely;

    public LayerMask CreatureMask, PlayerStructureMask;

    float chanceToAttackAgain = 100;

    public PetState currentState;

    [Header("Debug tool to test out states")]
    public PetState forceState;

    public enum PetState
    {
        Decide, //Make a choice on the next action
        AwaitPlayer, //When the player is gone in the crypt/wilderness
        Idle,
        Follow, //Follow the player. High chance to follow during the night, and will retaliate if the player is hurt while following
        ChaseCreature, //Attack creature. May also just bark instead. Chance to attack when player is damaged if following
        Bury, //Buried a bone
        ChaseThrownItem, //Chase a bone the player threw. These bones can be thrown for the dog to eat, gaining some food + friendship, and makes it follow. If a ball, increases friendship but has daily cap
        Flee,
        Pet,
        Eat //Pet goes to bowl to eat
    }

    public void CheckState(PetState currentState)
    {
        switch (currentState)
        {
            case PetState.Decide:
                Decide();
                break;

            case PetState.AwaitPlayer:
                AwaitPlayer();
                break;

            case PetState.Idle:
                Idle();
                break;

            case PetState.Follow:
                Follow();
                break;

            case PetState.ChaseCreature:
                ChaseCreature();
                break;

            case PetState.Bury:
                //Bury();
                break;

            case PetState.ChaseThrownItem:
                //ChaseThrownItem();
                break;

            case PetState.Flee:
                Flee();
                break;

            case PetState.Pet:
                Pet();
                break;

            case PetState.Eat:
                Eat();
                break;

            default:
                Debug.LogError("Unknown state: " + currentState);
                break;
        }
    }

    void Awake()
    {
        defaultHeadRotation = headPivot.rotation;
    }

    void Start()
    {
        base.Start();
        StartCoroutine(CheckSurroundings());

        //agent.updateRotation = false;
    }

    void Update()
    {
        base.Update();

        if(currentState == PetState.Idle || currentState == PetState.Follow) LookAtObject();

        if(agent.velocity.magnitude < 0.2f)
        {
            anim.SetBool("IsWalking", false);
            anim.SetBool("IsRunning", false);
        }
        else if(agent.velocity.magnitude < walkSpeed + 1)
        {
            anim.SetBool("IsWalking", true);
            anim.SetBool("IsRunning", false);
        }
        else
        {
            anim.SetBool("IsWalking", false);
            anim.SetBool("IsRunning", true);
        }

        //
        CheckState(currentState);
    }

    protected override void OnHour() //Shouldnt this be on override?
    {
        base.OnHour();
        //Chance to bury bone here
    }

    void StateSwitch(PetState newState)
    {
        //Leaving Old State Effects
        if(currentState == PetState.Idle)
        {
            anim.Play("DogIdle"); //Reset the anim
            anim.SetBool("IsSitting", false);
            if(currentRoutine != null) StopCoroutine(currentRoutine);
            StopCoroutine(IdleRoutine());
            currentRoutine = null;
            isMoving = false;
        }

        if(currentState == PetState.Follow)
        {
            if(currentRoutine != null) StopCoroutine(currentRoutine);
            StopCoroutine(FollowRoutine());
            currentRoutine = null;
            isMoving = false;
        }

        if(currentState == PetState.ChaseCreature)
        {

            targetCreature = null;
            anim.Play("DogIdle"); //Reset the anim
            if(currentRoutine != null) StopCoroutine(currentRoutine);
            currentRoutine = null;
            isMoving = false;
        }

        if(currentState == PetState.Pet)
        {
            anim.SetBool("IsSitting", false);
        }

        if((currentState == PetState.ChaseThrownItem || currentState == PetState.Bury) && newState == PetState.Pet)
        {
            alreadyPet = true;
            FriendPointsChange(25, true);
            effectsHandler.PlaySound(effectsHandler.petSound);
            anim.Play("DogPet");
            return;
        }

        //Change the State
        agent.velocity = Vector3.zero;
        headPivot.rotation = transform.rotation;
        currentState = newState;

        //Entering New State Effects
        if(currentState != PetState.Follow && currentState != PetState.ChaseCreature && currentState != PetState.Flee/* && currentState != PetState.Eat*/)
        {
            agent.speed = walkSpeed;
        }
    }

    void Decide()
    {
        if(isMoving || currentRoutine != null || TimeManager.Instance.stopTime) return; //Wait until all coroutines are done to avoid overlap

        if(forceState != PetState.Decide) //For debugging and testing states
        {
            StateSwitch(forceState);
            return;
        }

        if((hunger <= 25 && EatCheck(false)) || (thirst <= 25 && EatCheck(true)))
        {
            StateSwitch(PetState.Eat);
            return;
        }

        if(TownGate.Instance.location != PlayerLocation.InFarm) //Make sure pet is following when not in farm
        {
            if(TownGate.Instance.location != PlayerLocation.InTown || friendshipLevel < 1) //Player is not within reach, so stay still
            {
                //currentState = PetState.AwaitPlayer;
                StateSwitch(PetState.AwaitPlayer);
                return;
            }
            //currentState = PetState.Follow;
            StateSwitch(PetState.Follow);
            forceFollows = 5;
            return;
        }

        float positiveActionChance = (friendshipLevel + 1) * .25f;
        if(hunger == 0) positiveActionChance = 0;
        float r = Random.Range(0, 100f);

        if(r < positiveActionChance)
        {
            //currentState = PetState.ChaseCreature;
            StateSwitch(PetState.ChaseCreature);
            return;
        }

        r = Random.Range(0, 100);

        if(r < 95 - positiveActionChance * 2) StateSwitch(PetState.Idle);
        else
        {
            StateSwitch(PetState.Follow);
            forceFollows = Random.Range(7, 13);
        }
    }

    void AwaitPlayer()
    {
        if(TownGate.Instance.location == PlayerLocation.InFarm || TownGate.Instance.location == PlayerLocation.InTown)
        {
            StateSwitch(PetState.Follow);
        } 
    }

    void Idle()
    {
        if(!isMoving)
        {
            float distance = Vector3.Distance(player.position, spawnOrigin);
            if(distance > followDistance && friendshipLevel >= 1)
            {
                StateSwitch(PetState.Follow);
                currentRoutine = null;
                forceFollows = 5;
                return;
            }
        }

        if(currentRoutine == null)
        {
            if(hunger == 0)
            {
                target = FindPetBowl();
                if(target == Vector3.zero) target = StructureManager.Instance.GetRandomTile();
            }
            else
            {
                target = StructureManager.Instance.GetRandomTile();
                target = GetRandomPointAround(target, 3);
            }
            currentRoutine = StartCoroutine(MoveToPoint(target, 5));
        }
    }

    void Follow()
    {
        if(!isMoving && currentRoutine == null)
        {
            float distance = Vector3.Distance(player.position, spawnOrigin);
            if(distance < followDistance && TownGate.Instance.location == PlayerLocation.InFarm && forceFollows <= 0)
            {
                int r = Random.Range(0,100);
                if(r > 80)
                {
                    StateSwitch(PetState.Decide);
                    return;
                }
            }
            float playerDistance = Vector3.Distance(player.position, transform.position);
            float pointRange = 5;
            if(playerDistance > 15)
            {
                pointRange = 1.5f;
                agent.speed = runSpeed;
            } 
            else agent.speed = walkSpeed;

            target = GetRandomPointAround(player.position, pointRange);
            currentRoutine = StartCoroutine(MoveToPoint(target, 3));
            forceFollows--;
        }
    }

    void ChaseCreature()
    {
        if(!targetCreature)
        {
            agent.speed = runSpeed;
            Collider[] hitTargets = Physics.OverlapSphere(transform.position, 80f, CreatureMask);
            foreach(Collider collider in hitTargets)
            {
                CreatureBehaviorScript creature = collider.gameObject.GetComponentInParent<CreatureBehaviorScript>();
                if(creature && Random.Range(0,10) > 3 && creature.health > 0 && targettableCreatures.Contains(creature.creatureData))
                {
                    targetCreature = creature;
                    target = creature.gameObject.transform.position;
                    return;
                }
            }
            StateSwitch(PetState.Idle);
            return;
        }

        if(Vector3.Distance(target, transform.position) < 1.5f)
        {
            interruptAction = true;
        }

        if(currentRoutine == null && !isMoving)
        {
            currentRoutine = StartCoroutine(MoveToPoint(target, 0.5f));
        }
    }

    void Flee()
    {
        if(!isMoving && currentRoutine == null)
        {
            anim.Play("DogBite");
            agent.speed = runSpeed;
            // 1. Calculate direction to target
            Vector3 directionToTarget = transform.position - target;

            // 2. Normalize the direction
            Vector3 normalizedDirection = directionToTarget.normalized;

            // 3. Calculate the flee position
            Vector3 fleePosition = transform.position + normalizedDirection * 40;

            // 4. Set the agent's destination
            currentRoutine = StartCoroutine(MoveToPoint(fleePosition, 4));
        }
    }

    void Pet()
    {
        if(!isMoving && currentRoutine == null)
        {
            alreadyPet = true;
            FriendPointsChange(15, true);
            effectsHandler.PlaySound(effectsHandler.petSound);
            agent.ResetPath();
            currentRoutine = StartCoroutine(TimerRoutine(4));
            anim.Play("DogPet");
        }
    }

    void Eat()
    {
        if(!targetStructure && currentRoutine == null) //If there is no bowl, then they should not be in this state
        {
            StateSwitch(PetState.Decide);
            return;
        }
        if(!interruptAction && targetStructure && Vector3.Distance(targetStructure.transform.position, transform.position) < 1.5f) //Are they close enough? If so, begin eating
        {
            //print("Cat close enough to Dish");
            if(currentRoutine == null) FinishedMoving();
            else interruptAction = true;
            return;
        }
        if(!isMoving && currentRoutine == null) //Move to the dish
        {
            //agent.speed = runSpeed;
            currentRoutine = StartCoroutine(MoveToPoint(targetStructure.transform.position, 8));
        }
    }

    protected override void FinishedMoving()
    {
        if(currentState == PetState.Idle)
        {
            currentRoutine = StartCoroutine(IdleRoutine());
            isMoving = false;
            return;
        }

        if(currentState == PetState.Follow)
        {
            if(Vector3.Distance(player.position, transform.position) < 10) currentRoutine = StartCoroutine(FollowRoutine());
            else currentRoutine = null;
            isMoving = false;
            return;
        }

        if(currentState == PetState.ChaseCreature)
        {
            if(targetCreature) target = targetCreature.transform.position;
            if(Vector3.Distance(target, transform.position) < 3f)
            {
                anim.Play("DogBite");
                agent.ResetPath();
                currentRoutine = StartCoroutine(FollowRoutine()); //Just to buy the animation some time

                effectsHandler.PlayExtraSound(Random.Range(0, effectsHandler.extraSounds.Length));
                if(targetCreature)
                {
                    targetCreature.TakeDamage(10);
                    targetCreature.PlayHitParticle(targetCreature.transform.position);
                }
                if(targetCreature && targetCreature.health > 0)
                {
                    if(chanceToAttackAgain > Random.Range(0, 100))
                    {
                        StartCoroutine(AttackCooldown());
                        chanceToAttackAgain -= 30 - (friendshipLevel * 2);
                        return;
                    }
                    else chanceToAttackAgain = 100;
                }
                targetCreature = null;
                isMoving = false;
                StateSwitch(PetState.Decide);
                return;
            }
        }

        if(currentState == PetState.Flee)
        {
            StateSwitch(PetState.Decide);
        }

        if(currentState == PetState.Eat && targetStructure) //Dog Reached the Bowl
        {
            PetBowl bowl = targetStructure as PetBowl;
            if(!bowl) //Bowl is gone
            {
                targetStructure = null;
                isMoving = false;
                currentRoutine = null;
                return;
            }
            bool isEating = false, isDrinking = false;
            if(hunger <= 25 && bowl.ContainsEdibleItem(foodDiet)) isEating = true;
            if(thirst <= 25 && bowl.containsWater) isDrinking = true;

            if(Vector3.Distance(player.position, transform.position) > 70f) transform.position = targetStructure.transform.position; //To get the pet unstuck if they got stuck

            if(Vector3.Distance(targetStructure.transform.position, transform.position) < 1.5f && (isEating || isDrinking))
            {
                agent.velocity = Vector3.zero;
                agent.ResetPath();
                anim.Play("DogEat");
                if(isEating)
                {
                    bowl.RemoveItem(out InventoryItemData itemEaten);
                    EatFood(itemEaten);
                }
                else
                {
                    bowl.WaterChange(false);
                    thirst = maxThirst;
                    FriendPointsChange(5, true);
                }
                currentRoutine = StartCoroutine(TimerRoutine(3));
                isMoving = false;
                targetStructure = null;
                return;
            }
            else if(!isEating && !isDrinking) targetStructure = null;
        }
        
        isMoving = false;
        currentRoutine = null;
    }

    protected void FinishedCoroutine()
    {
        if(currentState == PetState.Pet || currentState == PetState.Eat)
        {
            StateSwitch(PetState.Decide);
        }
        currentRoutine = null;
    }

    IEnumerator IdleRoutine()
    {
        agent.ResetPath();
        bool creatureNear = false;
        float t = 0;
        float time = Random.Range(2f, 10);
        if(time > 6)
        {
            if(Random.Range(0, 10) > 2)
            {
                anim.SetBool("IsSitting", true);
                anim.Play("DogSit");
                time += 5;
            }
            else
            {
                anim.Play("DogHowl");
                //Play a sound for it
            }
        }
        while(t < time)
        {
            yield return new WaitForSeconds(1);
            if(Random.Range(0,10) == 9) anim.SetTrigger("IsPanting");
            t++;
        }
        
        if(time > 10)
        {
            anim.SetBool("IsSitting", false);
            yield return new WaitForSeconds(1);

        }
        if(creatureNear)
        {
            if(targetCreature)
            {
                StateSwitch(PetState.ChaseCreature);
            }
            else 
            {
                StateSwitch(PetState.Flee);
            }
        }
        else StateSwitch(PetState.Decide);
        currentRoutine = null;
    }

    IEnumerator FollowRoutine()
    {
        yield return new WaitForSeconds(Random.Range(1f, 4f));
        currentRoutine = null;
    }

    IEnumerator TimerRoutine(float time)
    {
        yield return new WaitForSeconds(time);
        FinishedCoroutine();
    }

    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(1.5f);
        isMoving = false;
    }

    IEnumerator CheckSurroundings()
    {
        Collider[] hitTargets = new Collider[10];
        int numColliders;
        while(true)
        {
            yield return new WaitForSeconds(1f);

            if(currentState == PetState.Idle || currentState == PetState.Follow)
            {
                numColliders = Physics.OverlapSphereNonAlloc(transform.position, 5, hitTargets, CreatureMask);
                for (int i = 0; i < numColliders; i++)
                {
                    CreatureBehaviorScript creature = hitTargets[i].gameObject.GetComponentInParent<CreatureBehaviorScript>();
                    if(creature && creature.health > 0)
                    {
                        if(fearedCreatures.Contains(creature.creatureData) && Random.Range(0,10) > 4)
                        {
                            target = creature.gameObject.transform.position;
                            StateSwitch(PetState.Flee);
                            break;
                        }
                        else
                        {
                            float positiveActionChance = (friendshipLevel + 1) * 2.75f;
                            if(hunger == 0) positiveActionChance = 0;

                            if(Random.Range(0, 20f) < positiveActionChance)
                            {
                                targetCreature = creature;
                                target = creature.gameObject.transform.position;
                                StateSwitch(PetState.ChaseCreature);
                                break;
                            }
                        }
                    }
                }
            }

            Vector3 closestTarget = Vector3.zero;
            float dist = 0;
            float minDist = 100;
            numColliders = Physics.OverlapSphereNonAlloc(transform.position, 8, hitTargets, PlayerStructureMask);
            for (int i = 0; i < numColliders; i++)
            {
                StructureBehaviorScript structure = hitTargets[i].gameObject.GetComponentInParent<StructureBehaviorScript>();
                if(structure || hitTargets[i].gameObject.layer == 10) 
                {
                    dist = Vector3.Distance(transform.position, hitTargets[i].gameObject.transform.position);
                    if(dist < minDist && Random.Range(0, 10) > 1)
                    {
                        minDist = dist;
                        closestTarget = hitTargets[i].gameObject.transform.position;
                    }
                }
            }

            starePoint = closestTarget;
        }
    }

    void LookAtObject()
    {
        //Check the Dot
        //if(starePoint == Vector3.zero) return;

        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 toTarget = Vector3.Normalize(starePoint - transform.position);

        if (Vector3.Dot(forward, toTarget) > .1f && starePoint != Vector3.zero)
        {
            Vector3 direction = starePoint - headPivot.position;
            //direction.y = 0;
            Quaternion toRotation = Quaternion.LookRotation(direction);
            toRotation *= defaultHeadRotation;

            headPivot.rotation = Quaternion.Slerp(headPivot.rotation, toRotation, 2 * Time.deltaTime);
        }
        else
        {
            headPivot.rotation = Quaternion.Slerp(headPivot.rotation, transform.rotation, 2 * Time.deltaTime);
        }
    }

    protected override bool StopMovingEarlyCheck()
    {
        if(base.StopMovingEarlyCheck() == true) return true;
        return false;
    }

    /////IInteractable nonsense/////

    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        if(heldItem && PlayerInventoryHolder.Instance.AddToInventory(heldItem, 1))
        {
            heldItem = null;
            itemR.sprite = null;
            FriendPointsChange(15, true);
        }

        else if(!alreadyPet)
        {
            StateSwitch(PetState.Pet);
        }
        interactSuccessful = true;
    }

    public void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        if(item.ID == 2 && PlayerInteraction.Instance.waterHeld > 0 && (currentState == PetState.Idle || currentState == PetState.Follow))
        {
            PlayerInteraction.Instance.waterHeld--;
            interactSuccessful = true;
            target = player.position;
            StateSwitch(PetState.Flee);
            effectsHandler.MiscSound();
            StopCoroutine(DripEffects());
            StartCoroutine(DripEffects());
            thirst = maxThirst;
            return;
        }
        if(hunger < 100 && (foodDiet.Contains(item)))
        {
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            PlayerInventoryHolder.Instance.UpdateInventory();
            EatFood(item);
            interactSuccessful = true;
            return;
        }
        interactSuccessful = false;
    }
    
    public void EndInteraction(){}

    public void ToggleHighlight(bool enabled)
    {
        //showStats = enabled;
    }

    public void ReturnFocalPoint(out Transform point)
    {
        if(focalPoint) point = focalPoint;
        else point = transform;
    }
}
