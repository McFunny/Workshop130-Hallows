using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class PetCat : PetBehaviorScript, IInteractable
{
    public InventoryItemData heldItem;
    public SpriteRenderer itemR;
    public List<ItemWithAmount> possibleGiftItems = new List<ItemWithAmount>();
    public List<CreatureObject> targettableCreatures = new List<CreatureObject>();

    public Transform headPivot;
    Vector3 starePoint;
    Quaternion defaultHeadRotation;
    //bool lookFreely;

    Table targetTable; //For Sitting
    BugBehaviorScript targetBug;
    CreatureBehaviorScript targetCreature;
    StructureBehaviorScript targetStructure;

    public LayerMask BugCreatureMask, PlayerStructureMask;

    public PetState currentState;

    [Header("Debug tool to test out states")]
    public PetState forceState;

    public enum PetState
    {
        Decide, //Make a choice on the next action
        AwaitPlayer, //When the player is gone in the crypt/wilderness
        Idle,
        Follow, //Follow the player
        ChaseCreature, //Attack hare/crow/bug
        Sit, //Sit still and watch
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

            case PetState.Sit:
                Sit();
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
        if(TimeManager.Instance.currentHour == 8) FindItem();
        base.Start();
        StartCoroutine(CheckSurroundings());
    }

    void Update()
    {
        base.Update();

        /*if(currentState != PetState.Follow && currentState != PetState.ChaseCreature && currentState != PetState.Flee)
        {
            agent.speed = walkSpeed;
        }*/

        if(currentState == PetState.Idle || currentState == PetState.Sit) LookAtObject();

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

    protected virtual void OnHour()
    {
        base.OnHour();
        if(TimeManager.Instance.currentHour == 8)
        {
            FindItem();
        }
    }

    void StateSwitch(PetState newState)
    {
        //Leaving Old State Effects
        if(currentState == PetState.Idle)
        {
            anim.Play("Idle"); //Reset the anim
            anim.SetBool("IsSitting", false);
            StopCoroutine(IdleRoutine());
            currentRoutine = null;
            isMoving = false;
        }

        if(currentState == PetState.ChaseCreature)
        {
            targetBug = null;
            targetCreature = null;
            anim.Play("Idle"); //Reset the anim
            if(currentRoutine != null) StopCoroutine(currentRoutine);
            currentRoutine = null;
            isMoving = false;
        }

        if(currentState == PetState.Pet)
        {
            anim.SetBool("IsSitting", false);
        }

        if(currentState == PetState.Sit && newState == PetState.Pet)
        {
            if(targetTable && Vector3.Distance(targetTable.transform.position, transform.position) < 3.5f) //Play the animation and effects but dont change the state
            {
                alreadyPet = true;
                FriendPointsChange(25, true);
                effectsHandler.PlaySound(effectsHandler.petSound);
                anim.Play("CatPet");
                return;
            }
            else //Stop and pet the cat if its on its way to sit
            {
                if(currentRoutine != null) StopCoroutine(currentRoutine);
                currentRoutine = null;
                isMoving = false;
            }
        }

        //Change the State
        agent.velocity = Vector3.zero;
        agent.ResetPath();
        headPivot.rotation = transform.rotation;
        currentState = newState;

        //Entering New State Effects
        if(currentState != PetState.Follow && currentState != PetState.ChaseCreature && currentState != PetState.Flee && currentState != PetState.Eat)
        {
            agent.speed = walkSpeed;
        }
    }

    void Decide()
    {
        if(isMoving || currentRoutine != null || TimeManager.Instance.stopTime) return; //Wait until all coroutines are done to avoid overlap

        if(forceState != PetState.Decide)
        {
            StateSwitch(forceState);
            return;
        }

        if(TownGate.Instance.location != PlayerLocation.InFarm) //Make sure pet is following when not in farm
        {
            if(TownGate.Instance.location != PlayerLocation.InTown) //Player is not within reach, so stay still
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

        if(hunger <= 25 && EatCheck())
        {
            StateSwitch(PetState.Eat);
            return;
        }

        float positiveActionChance = (friendshipLevel + 1) * .75f;
        if(!TimeManager.Instance.isDay) positiveActionChance *= 2;
        float r = Random.Range(0, 100f);

        if(r < positiveActionChance)
        {
            //currentState = PetState.ChaseCreature;
            StateSwitch(PetState.ChaseCreature);
            return;
        }

        r = Random.Range(0, 100);

        if(r < 80) StateSwitch(PetState.Idle);
        else if(r < 95) StateSwitch(PetState.Sit);
        else
        {
            StateSwitch(PetState.Follow);
            forceFollows = 10;
        }
    }

    void AwaitPlayer()
    {
        if(TownGate.Instance.location == PlayerLocation.InFarm || TownGate.Instance.location == PlayerLocation.InTown) StateSwitch(PetState.Follow);
    }

    void Idle()
    {
        if(!isMoving)
        {
            float distance = Vector3.Distance(player.position, spawnOrigin);
            if(distance > followDistance)
            {
                StateSwitch(PetState.Follow);
                currentRoutine = null;
                forceFollows = 5;
                return;
            }
        }

        if(currentRoutine == null)
        {
            target = StructureManager.Instance.GetRandomTile();
            target = GetRandomPointAround(target, 3);
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
        if(!targetBug && !targetCreature)
        {
            agent.speed = runSpeed;
            Collider[] hitTargets = Physics.OverlapSphere(transform.position, 80f, BugCreatureMask);
            foreach(Collider collider in hitTargets)
            {
                BugBehaviorScript bug = collider.gameObject.GetComponentInParent<BugBehaviorScript>();
                if(bug && Random.Range(0,10) > 6)
                {
                    targetBug = bug;
                    target = bug.gameObject.transform.position;
                }

                CreatureBehaviorScript creature = collider.gameObject.GetComponentInParent<CreatureBehaviorScript>();
                if(creature && Random.Range(0,10) > 3 && creature.health > 0 && targettableCreatures.Contains(creature.creatureData))
                {
                    targetCreature = creature;
                    target = creature.gameObject.transform.position;
                    targetBug = null;
                    return;
                }

                if(targetBug) return;
            }
            StateSwitch(PetState.Idle);
            return;
        }

        if(Vector3.Distance(target, transform.position) < 1.5f)
        {
            //print("Cat close enough to Target");
            interruptAction = true;
            //return;
        }

        if(currentRoutine == null && !isMoving)
        {
            currentRoutine = StartCoroutine(MoveToPoint(target, 0.5f));
        }
    }

    void Sit()
    {
        if(!targetTable)
        {
            Collider[] hitStructures = Physics.OverlapSphere(transform.position, 80f, 1 << 6);
            foreach(Collider collider in hitStructures)
            {
                Table table = collider.gameObject.GetComponentInParent<Table>();
                if(table && table.HasOpenSocket() && Random.Range(0,10) > 3)
                {
                    targetTable = table;
                    return;
                }
            }
            StateSwitch(PetState.Idle);
            return;
        }


        if(!interruptAction && targetTable && Vector3.Distance(targetTable.transform.position, transform.position) < 3.5f && isMoving)
        {
            print("Cat close enough to table");
            if(currentRoutine == null) FinishedMoving();
            else interruptAction = true;
            return;
        }

        if(currentRoutine == null && !isMoving && targetTable)
        {
            currentRoutine = StartCoroutine(MoveToPoint(targetTable.transform.position, 10));
        }
    }

    void Flee()
    {
        if(!isMoving && currentRoutine == null)
        {
            agent.speed = runSpeed;
            // 1. Calculate direction to target
            Vector3 directionToTarget = transform.position - target;

            // 2. Normalize the direction
            Vector3 normalizedDirection = directionToTarget.normalized;

            // 3. Calculate the flee position
            Vector3 fleePosition = transform.position + normalizedDirection * 20;

            // 4. Set the agent's destination
            currentRoutine = StartCoroutine(MoveToPoint(fleePosition, 4));
        }
    }

    void Pet()
    {
        if(!isMoving && currentRoutine == null)
        {
            alreadyPet = true;
            FriendPointsChange(25, true);
            effectsHandler.PlaySound(effectsHandler.petSound);
            agent.ResetPath();
            currentRoutine = StartCoroutine(TimerRoutine(4));
            anim.Play("CatPet");
        }
    }

    void Eat()
    {
        if(!targetStructure) //If there is no bowl, then they should not be in this state
        {
            StateSwitch(PetState.Decide);
            return;
        }
        if(!interruptAction && targetStructure && Vector3.Distance(targetStructure.transform.position, transform.position) < 1.5f) //Are they close enough? If so, begin eating
        {
            print("Cat close enough to Dish");
            if(currentRoutine == null) FinishedMoving();
            else interruptAction = true;
            return;
        }
        if(!isMoving && currentRoutine == null) //Move to the dish
        {
            agent.speed = runSpeed;
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

        if(currentState == PetState.Sit && targetTable)
        {
            Transform sitPos = targetTable.GrabOpenSocketTransform();
            if(sitPos)
            {
                if(Vector3.Distance(targetTable.transform.position, transform.position) < 4f)
                {
                    agent.Stop();
                    transform.DOJump(sitPos.position, 1, 1, 0.5f);
                    currentRoutine = StartCoroutine(SitRoutine());
                }
                else currentRoutine = null;
            }
            else 
            {
                StateSwitch(PetState.Idle);
                currentRoutine = null;
            }

            isMoving = false;
            return;
        }

        if(currentState == PetState.ChaseCreature)
        {
            if(targetCreature) target = targetCreature.transform.position;
            if(targetBug) target = targetBug.transform.position;
            if(Vector3.Distance(target, transform.position) < 3f)
            {
                anim.Play("CatAttack1");
                agent.ResetPath();
                currentRoutine = StartCoroutine(FollowRoutine()); //Just to buy the animation some time

                effectsHandler.PlayExtraSound(Random.Range(0, effectsHandler.extraSounds.Length));
                if(targetBug)
                {
                    if(Random.Range(0,10) < (friendshipLevel + 1)/2 && !heldItem)
                    {
                        heldItem = targetBug.bugItem;
                        itemR.sprite = heldItem.icon;
                        Destroy(targetBug.gameObject);
                    }
                    else targetBug.Struck();
                }
                if(targetCreature)
                {
                    targetCreature.TakeDamage(25);
                    targetCreature.PlayHitParticle(targetCreature.transform.position);
                }
                targetBug = null;
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

        if(currentState == PetState.Eat && targetStructure) //Cat Reached the Bowl
        {
            PetBowl bowl = targetStructure as PetBowl;
            if(bowl && Vector3.Distance(targetStructure.transform.position, transform.position) < 1f && bowl.ContainsEdibleItem(foodDiet))
            {
                agent.Stop();
                anim.Play("CatEat");
                bowl.RemoveItem(out InventoryItemData itemEaten);
                EatFood(itemEaten);
                currentRoutine = StartCoroutine(TimerRoutine(3));
                isMoving = false;
                targetStructure = null;
                return;
            }
            if(!bowl) targetStructure = null;
        }
        
        isMoving = false;
        currentRoutine = null;
    }

    protected void FinishedCoroutine()
    {
        if(currentState == PetState.Pet)
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
        float time = Random.Range(2f, 15);
        if(time > 10)
        {
            anim.SetBool("IsSitting", true);
            if(Random.Range(0, 10) > 2) anim.Play("CatSit");
            else anim.Play("CatClean");
            time += 10;
        }
        while(t < time)
        {
            yield return new WaitForSeconds(1);
            t++;
            /*Collider[] hitTargets = Physics.OverlapSphere(transform.position, 5, BugCreatureMask); //Check if it should flee from nearby creatures
            foreach(Collider collider in hitTargets)
            {
                CreatureBehaviorScript creature = collider.gameObject.GetComponentInParent<CreatureBehaviorScript>();
                if(creature && creature.health > 0)
                {
                    if(!targettableCreatures.Contains(creature.creatureData))
                    {
                        creatureNear = true;
                        target = creature.gameObject.transform.position;
                        t += time;
                        break;
                    }
                    else
                    {
                        float positiveActionChance = (friendshipLevel + 1) * .75f;
                        if(!TimeManager.Instance.isDay) positiveActionChance *= 2;

                        if(Random.Range(0, 10f) < friendshipLevel + 1)
                        {
                            creatureNear = true;
                            targetCreature = creature;
                            target = creature.gameObject.transform.position;
                            t += time;
                            break;
                        }
                    }
                }
            }*/
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

    IEnumerator SitRoutine()
    {
        //transform.position = sitPos.position;
        anim.Play("CatAttack1");
        yield return new WaitForSeconds(.5f);
        transform.Rotate(0, 180, 0);

        anim.SetBool("IsSitting", true);
        anim.Play("CatLoaf");
        agent.enabled = false;
        targetTable.usedByPet = true;
        yield return new WaitForSeconds(Random.Range(25f, 90f));
        anim.SetBool("IsSitting", false);
        yield return new WaitForSeconds(2);
        targetTable.usedByPet = false;
        targetTable = null;
        agent.enabled = true;
        StateSwitch(PetState.Decide);
        currentRoutine = null;

        FriendPointsChange(6, true);
    }

    IEnumerator TimerRoutine(float time)
    {
        yield return new WaitForSeconds(time);
        FinishedCoroutine();
    }

    IEnumerator CheckSurroundings()
    {
        bool checkStructures = false;
        Collider[] hitTargets = new Collider[10];
        int numColliders;
        while(true)
        {
            yield return new WaitForSeconds(1f);

            if(currentState == PetState.Idle || currentState == PetState.Follow)
            {
                numColliders = Physics.OverlapSphereNonAlloc(transform.position, 5, hitTargets, BugCreatureMask);
                for (int i = 0; i < numColliders; i++)
                {
                    CreatureBehaviorScript creature = hitTargets[i].gameObject.GetComponentInParent<CreatureBehaviorScript>();
                    if(creature && creature.health > 0)
                    {
                        if(!targettableCreatures.Contains(creature.creatureData))
                        {
                            target = creature.gameObject.transform.position;
                            StateSwitch(PetState.Flee);
                            break;
                        }
                        else
                        {
                            float positiveActionChance = (friendshipLevel + 1) * .75f;
                            if(!TimeManager.Instance.isDay) positiveActionChance *= 2;

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
            //if(checkStructures)
            //{
                checkStructures = false;
                numColliders = Physics.OverlapSphereNonAlloc(transform.position, 8, hitTargets, PlayerStructureMask);
                for (int i = 0; i < numColliders; i++)
                {
                    StructureBehaviorScript structure = hitTargets[i].gameObject.GetComponentInParent<StructureBehaviorScript>();
                    if(structure || hitTargets[i].gameObject.layer == 10) 
                    {
                        dist = Vector3.Distance(transform.position, hitTargets[i].gameObject.transform.position);
                        if(dist < minDist && Random.Range(0, 10) > 2)
                        {
                            minDist = dist;
                            closestTarget = hitTargets[i].gameObject.transform.position;
                        }
                    }
                }

                if(closestTarget != Vector3.zero)
                {
                    starePoint = closestTarget;
                    print(starePoint);
                }
            //}
            //else checkStructures = true;
        }
    }

    void LookAtObject()
    {
        //Check the Dot
        if(starePoint == Vector3.zero) return;

        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 toTarget = Vector3.Normalize(starePoint - transform.position);

        if (Vector3.Dot(forward, toTarget) > .1f /*|| (currentState == PetState.Idle || currentState == PetState.Sit || starePoint == Vector3.zero)*/)
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

    void FindItem()
    {
        if(Random.Range(0f, 100f) < (friendshipLevel + 1) * 8.5f)
        {
            int x = 0;
            InventoryItemData chosenItem = null;
            while(x < 30 && chosenItem == null)
            {
                int r = Random.Range(0, possibleGiftItems.Count);
                if(Random.Range(0, 100) < possibleGiftItems[r].amount) chosenItem = possibleGiftItems[r].item;
            }
            if(chosenItem)
            {
                itemR.sprite = chosenItem.icon;
                heldItem = chosenItem;
            }
        }
    }

    bool EatCheck()
    {
        var foundBowls = FindObjectsByType<PetBowl>(FindObjectsSortMode.None);
        if(foundBowls.Length == 0) return false;
        for(int i = 0; i < foundBowls.Length; i++)
        {
            if(foundBowls[i].ContainsEdibleItem(foodDiet))
            {
                targetStructure = foundBowls[i];
                return true;
            }
        }
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
            FriendPointsChange(10, true);
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
            return;
        }
        if(hunger < 100)
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
        showStats = enabled;
    }

    public void ReturnFocalPoint(out Transform point)
    {
        if(focalPoint) point = focalPoint;
        else point = transform;
    }
    ///////////////////////////////
}
