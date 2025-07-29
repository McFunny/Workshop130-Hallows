using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PetCat : PetBehaviorScript, IInteractable
{
    public InventoryItemData heldItem;
    public List<ItemWithAmount> possibleGiftItems = new List<ItemWithAmount>();

    //private Coroutine idleRoutine, walkRoutine, currentRoutine; 

    public PetState currentState;

    public enum PetState
    {
        Idle,
        Follow, //Follow the player
        ChaseCreature, //Attack hare/crow/bug
        Sit, //Sit still and watch
        BegForFood //Sits and stares at bowl
    }

    public void CheckState(PetState currentState)
    {
        switch (currentState)
        {
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

            case PetState.BegForFood:
                BegForFood();
                break;

            default:
                Debug.LogError("Unknown state: " + currentState);
                break;
        }
    }

    void Update()
    {
        if(currentState != PetState.Follow)
        {
            agent.speed = walkSpeed;
        }

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

    void Idle()
    {
        if(!isMoving && currentState == PetState.Idle)
        {
            float distance = Vector3.Distance(player.position, transform.position);
            if(distance > followDistance)
            {
                currentState = PetState.Follow;
                currentRoutine = null;
                return;
            }
        }

        if(currentRoutine == null)
        {
            target = GetRandomPointAround(transform.position, 10);
            currentRoutine = StartCoroutine(MoveToPoint(target));
        }
    }

    void Follow()
    {
        if(!isMoving && currentState == PetState.Follow && currentRoutine == null)
        {
            float distance = Vector3.Distance(player.position, transform.position);
            if(distance < followDistance && TownGate.Instance.location == PlayerLocation.InFarm)
            {
                int r = Random.Range(0,100);
                if(r > 80)
                {
                    currentState = PetState.Idle;
                }
                return;
            }

            if(distance > 10) agent.speed = runSpeed;
            else agent.speed = walkSpeed;

            target = GetRandomPointAround(player.position, 5);
            currentRoutine = StartCoroutine(MoveToPoint(target));
        }
    }

    void ChaseCreature()
    {
        //
    }

    void Sit()
    {
        //
    }

    void BegForFood()
    {
        //
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
            currentRoutine = StartCoroutine(FollowRoutine());
            isMoving = false;
            return;
        }
        
        isMoving = false;
        currentRoutine = null;
    }

    IEnumerator IdleRoutine()
    {
        yield return new WaitForSeconds(Random.Range(5f, 30f));
        currentRoutine = null;
    }

    IEnumerator FollowRoutine()
    {
        yield return new WaitForSeconds(Random.Range(2f, 5f));
        currentRoutine = null;
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
        if(!alreadyPet)
        {
            alreadyPet = true;
            FriendPointsChange(15);
            effectsHandler.PlaySound(effectsHandler.petSound);
        }
        interactSuccessful = true;
    }

    public void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        if(foodDiet.Contains(item) && hunger < 50)
        {
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            PlayerInventoryHolder.Instance.UpdateInventory();
            EatFood(item);
        }
        interactSuccessful = true;
    }
    
    public void EndInteraction(){}

    public void ToggleHighlight(bool enabled){}

    public void ReturnFocalPoint(out Transform point)
    {
        if(focalPoint) point = focalPoint;
        else point = transform;
    }
    ///////////////////////////////
}
