using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface IInteractable
{
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public void Interact(PlayerInteraction interactor, out bool interactSuccessful);

    public void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item);
    

    public void EndInteraction();
}
