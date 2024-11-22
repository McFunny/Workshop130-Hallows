using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class NPC : MonoBehaviour, IInteractable
{
    //[SerializeField] private GameObject interactObject;
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public DialogueText dialogueText;
    public DialogueController dialogueController;
    public AudioClip happy, sad, neutral, angry, confused, shocked;

    [HideInInspector] public int currentPath = -1; //-1 means default path
    [HideInInspector] public PathType currentType;

    [HideInInspector] public StoreItem lastInteractedStoreItem;
    [HideInInspector] public bool hasSpokeToday, hasEatenToday = false;


    public void EndInteraction()
    {
        throw new System.NotImplementedException();
    }

    public abstract void Interact(PlayerInteraction interactor, out bool interactSuccessful);

    public abstract void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item);

    public virtual void PurchaseAttempt(StoreItem item){}

    public virtual void RefreshStore(){}

    public virtual void EmptyShopItem(){}
    
    public virtual void PlayerLeftRadius(){}
}
