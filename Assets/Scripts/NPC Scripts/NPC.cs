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

    public Character character;

    [HideInInspector] public int currentPath = -1; //-1 means default path
    [HideInInspector] public PathType currentType;

    [HideInInspector] public StoreItem lastInteractedStoreItem;
    [HideInInspector] public bool hasSpokeToday, hasEatenToday = false;

    [HideInInspector] public bool hasBeenFed = false; //outdated
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

    public virtual void OnConvoEnd()
    {
        currentPath = -1;
    }

    public virtual void BeginWorking(){}

    public virtual void StopWorking(){}

    public virtual void ShotAt(){}

    public virtual bool ActionCheck1()
    {
        return true;
    }
    public virtual bool ActionCheck2()
    {
        return true;
    }
    public virtual bool ActionCheck3()
    {
        return true;
    }

    public bool CompletedQuest()
    {
        //Check if player completed any quest non item related

        for(int i = 0; i < QuestManager.Instance.activeQuests.Count; i++)
        {
            if(QuestManager.Instance.activeQuests[i].alreadyCompleted || QuestManager.Instance.activeQuests[i].isMajorQuest) continue;

            if(QuestManager.Instance.activeQuests[i].assignee == character && QuestManager.Instance.activeQuests[i].progress == QuestManager.Instance.activeQuests[i].maxProgress)
            {
                QuestManager.Instance.activeQuests[i].alreadyCompleted = true;
                PlayerInteraction.Instance.currentMoney += QuestManager.Instance.activeQuests[i].mintReward;
                PlayerInteraction.Instance.totalMoneyEarned += QuestManager.Instance.activeQuests[i].mintReward;
                return true;
            }
        }

        return false;
    }

    public bool CompletedQuestWithItem()
    {
        //Check the item the player is holding

        for(int i = 0; i < QuestManager.Instance.activeQuests.Count; i++)
        {
            if(QuestManager.Instance.activeQuests[i].alreadyCompleted || QuestManager.Instance.activeQuests[i].isMajorQuest) continue;

            if(QuestManager.Instance.activeQuests[i].assignee == character && QuestManager.Instance.activeQuests[i].progress == QuestManager.Instance.activeQuests[i].maxProgress)
            {
                FetchQuest fq = QuestManager.Instance.activeQuests[i] as FetchQuest;
                if(fq != null && HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData == fq.desiredItem && HotbarDisplay.currentSlot.AssignedInventorySlot.StackSize == fq.amount)
                {
                    QuestManager.Instance.activeQuests[i].alreadyCompleted = true;
                    PlayerInteraction.Instance.currentMoney += QuestManager.Instance.activeQuests[i].mintReward;
                    PlayerInteraction.Instance.totalMoneyEarned += QuestManager.Instance.activeQuests[i].mintReward;

                    HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(fq.amount);
                    PlayerInventoryHolder.Instance.UpdateInventory();
                    return true;
                }

                GrowQuest gq = QuestManager.Instance.activeQuests[i] as GrowQuest;
                if(gq != null && HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData == gq.desiredItem && HotbarDisplay.currentSlot.AssignedInventorySlot.StackSize == gq.amount)
                {
                    QuestManager.Instance.activeQuests[i].alreadyCompleted = true;
                    PlayerInteraction.Instance.currentMoney += QuestManager.Instance.activeQuests[i].mintReward;
                    PlayerInteraction.Instance.totalMoneyEarned += QuestManager.Instance.activeQuests[i].mintReward;

                    HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(gq.amount);
                    PlayerInventoryHolder.Instance.UpdateInventory();
                    return true;
                }
            }
        }

        return false;
    }
}

public enum Character
{
    Null,
    MistMerchant,
    Botanist,
    Rascal,
    LumberJack,
    Apothocary,
    Tinkerer,
    Culinarian,
    Tavern,
    Traveler,
    Fanatic,
    GraveDigger,
    Butcher,
    Carpenter
}
