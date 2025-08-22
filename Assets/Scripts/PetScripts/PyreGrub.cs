using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class PyreGrub : PetBehaviorScript, IInteractable
{
    public Material litMat, extinguishedMat;
    public GameObject pyreFire;

    public PetState currentState;

    [Header("Debug tool to test out states")]
    public PetState forceState;

    public enum PetState
    {
        Decide, //Make a choice on the next action
        AwaitPlayer, //When the player is gone in the crypt/wilderness
        Idle,
        Follow, //Follow the player
        Ball, //Attack hare/crow/bug
        Flee,
        Pet,
        Eat //Pet goes to bowl to eat
    }

    /*public void CheckState(PetState currentState)
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
    */



    /////IInteractable nonsense/////

    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        if(!alreadyPet)
        {
            //StateSwitch(PetState.Pet);
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
            //StateSwitch(PetState.Flee);
            effectsHandler.MiscSound();
            StopCoroutine(DripEffects());
            StartCoroutine(DripEffects());
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
    ///////////////////////////////
}
