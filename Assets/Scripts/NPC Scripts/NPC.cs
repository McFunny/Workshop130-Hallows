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
    [HideInInspector] public DialogueController dialogueController;
    public Animator anim;
    public AudioClip happy, sad, neutral, angry, confused, shocked;

    public Transform eyeLine;

    [HideInInspector] public int currentPath = -1; //-1 means default path
    [HideInInspector] public PathType currentType;

    [HideInInspector] public StoreItem lastInteractedStoreItem;
    [HideInInspector] public bool hasSpokeToday, hasEatenToday = false;

    [HideInInspector] public bool hasBeenFed = false;
    [HideInInspector] public bool startedDialogue = false; //to check if this is the first time the player spoke to them since entering their radius

    [HideInInspector] public NPCMovement movementHandler;

    [HideInInspector] public FaceCamera faceCamera;

    [HideInInspector] public ShopStall assignedStall;

    protected virtual void Awake()
    {
        if(dialogueController == null) dialogueController = FindFirstObjectByType<DialogueController>();
    }

    public void EndInteraction()
    {
        throw new System.NotImplementedException();
    }

    public void ToggleHighlight(bool enabled){}

    public abstract void Interact(PlayerInteraction interactor, out bool interactSuccessful);

    public abstract void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item);

    public virtual void PurchaseAttempt(StoreItem item){}

    public virtual void RefreshStore(){}

    public virtual void EmptyShopItem(){}
    
    public virtual void PlayerLeftRadius()
    {
        startedDialogue = false;
    }

    public virtual void GivePlayerItem(int id, int amount){}

    public virtual void OnConvoEnd(){}

    public virtual void BeginWorking(){}

    public virtual void StopWorking(){}

    public virtual void ShotAt(){}
}
