using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Well : MonoBehaviour, IInteractable
{
    public InventoryItemData waterCan;
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }
    public void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        interactSuccessful = true;
    }

    public void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        if(item != waterCan || PlayerInteraction.Instance.waterHeld == 10)
        {
            interactSuccessful = false;
            return;
        }
        interactSuccessful = true;
        PlayerInteraction.Instance.waterHeld = 10;
        
    }
    
    public void EndInteraction()
    {
       
    }
}
