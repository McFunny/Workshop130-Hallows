using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PetCat : PetBehaviorScript, IInteractable
{
    public InventoryItemData heldItem;
    public SpriteRenderer itemR;
    public List<ItemWithAmount> possibleGiftItems = new List<ItemWithAmount>();
    public List<CreatureObject> targettableCreatures = new List<CreatureObject>();

    //private Coroutine idleRoutine, walkRoutine, currentRoutine; 

    Table targetTable; //For Sitting
    BugBehaviorScript targetBug;
    CreatureBehaviorScript targetCreature;

    public LayerMask BugCreatureMask;

    public PetState currentState;

    public enum PetState
    {
        Decide, //Make a choice on the next action
        AwaitPlayer, //When the player is gone in the crypt/wilderness
        Idle,
        Follow, //Follow the player
        ChaseCreature, //Attack hare/crow/bug
        Sit, //Sit still and watch
        Flee,
        Pet
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

            default:
                Debug.LogError("Unknown state: " + currentState);
                break;
        }
    }

    void Start()
    {
        if(TimeManager.Instance.currentHour == 8) FindItem();
        base.Start();
    }

    void Update()
    {
        base.Update();

        /*if(currentState != PetState.Follow && currentState != PetState.ChaseCreature && currentState != PetState.Flee)
        {
            agent.speed = walkSpeed;
        }*/

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
            StopCoroutine(IdleRoutine());
        }

        //Change the State
        agent.ResetPath();
        currentState = newState;

        //Entering New State Effects
        if(currentState != PetState.Follow && currentState != PetState.ChaseCreature && currentState != PetState.Flee)
        {
            agent.speed = walkSpeed;
        }
    }

    void Decide()
    {
        if(isMoving || currentRoutine != null || TimeManager.Instance.stopTime) return; //Wait until all coroutines are done to avoid overlap

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


        if(targetTable && Vector3.Distance(targetTable.transform.position, transform.position) < 3.5f)
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
                    transform.position = sitPos.position;
                    transform.Rotate(0, 180, 0);
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
        
        isMoving = false;
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
            anim.Play("CatSit");
            time += 10;
        }
        while(t < time)
        {
            yield return new WaitForSeconds(1);
            t++;
            Collider[] hitTargets = Physics.OverlapSphere(transform.position, 5, BugCreatureMask); //Check if it should flee from nearby creatures
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
            }
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
            alreadyPet = true;
            FriendPointsChange(25, true);
            effectsHandler.PlaySound(effectsHandler.petSound);
        }
        interactSuccessful = true;
    }

    public void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        if(hunger < 100)
        {
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            PlayerInventoryHolder.Instance.UpdateInventory();
            EatFood(item);
        }
        interactSuccessful = true;
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
