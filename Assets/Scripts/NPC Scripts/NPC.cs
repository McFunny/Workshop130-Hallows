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
    public AudioClip[] happy, sad, neutral, angry, confused, shocked;

    public Transform eyeLine;

    public Character character;

    public NPCBarterDatabase barterDatabase;

    [HideInInspector] public int currentPath = -1; //-1 means default path
    [HideInInspector] public PathType currentType;

    [HideInInspector] public StoreItem lastInteractedStoreItem;
    [HideInInspector] public bool hasSpokeToday, hasEatenToday = false;

    [HideInInspector] public bool hasBeenFed = false; //outdated
    [HideInInspector] public bool startedDialogue = false; //to check if this is the first time the player spoke to them since entering their radius

    [HideInInspector] public NPCMovement movementHandler;

    [HideInInspector] public FaceCamera faceCamera;

    [HideInInspector] public ShopStall assignedStall;

    [HideInInspector] public List<ItemWithAmount> itemsToGive = new List<ItemWithAmount>();

    [HideInInspector] public WaypointScript shopUI;

    public Quest dailyQuest; //If given a quest today, they will hold it here and have an explanation overhead until its given

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

    public virtual void Talk()
    {
        if(!dialogueController.FreeToSpeak(this)) return;
        anim.SetTrigger("IsTalking");
        movementHandler.TalkToPlayer();
        dialogueController.currentTalker = this;
        dialogueController.DisplayNextParagraph(dialogueText, currentPath, currentType);
        startedDialogue = true;
    }

    public virtual void PurchaseAttempt(StoreItem item)
    {
        if (dialogueController.IsInterruptable() == false || !shopUI)
        {
            return;
        }
        if (lastInteractedStoreItem == item)
        {
            //Barter Price Check
            if(item.barterCost.Count > 0)
            {
                if(PlayerInventoryHolder.Instance.IsInventoryFull(item.itemData, 1))
                {
                    currentPath = 4; //No space in inventory
                }
                else if(item.CanAffordTrade())
                {
                    //item.CompleteTrade();
                    currentPath = 2; //item sold
                    shopUI.shopImgObj.SetActive(false);
                }
                else if (PlayerInteraction.Instance.currentMoney < lastInteractedStoreItem.cost)
                {
                    currentPath = 3; //no money!?!?!?
                }
                else currentPath = 6; //Not enough items to cover barter
            }

            //check price, then give item
            else if (PlayerInteraction.Instance.currentMoney < lastInteractedStoreItem.cost)
            {
                currentPath = 3; //no money!?!?!?
            }
            else if (PlayerInventoryHolder.Instance.IsInventoryFull(item.itemData, 1))
            {
                currentPath = 4; //No space in inventory
            }
            else
            {
                currentPath = 2; //item sold
                shopUI.shopImgObj.SetActive(false);
                if (assignedStall && assignedStall.displaySign) assignedStall.displaySign.ResetDisplay();
                if (assignedStall && assignedStall.barterSign) assignedStall.barterSign.ResetDisplay();
            }
            anim.SetTrigger("IsTalking");
        }
        else
        {
            dialogueController.restartDialogue = true;
            if(item.barterCost.Count == 0) currentPath = 1; //item selected
            else currentPath = 5; //barter item selected
            anim.SetTrigger("IsTalking");
            if (lastInteractedStoreItem) shopUI.shopImgObj.SetActive(false);
            lastInteractedStoreItem = item;
            shopUI.shopTarget = item.arrowObject.transform;
            shopUI.shopImgObj.SetActive(true);
            if (assignedStall && assignedStall.displaySign)
            {
                assignedStall.displaySign.DisplayItem(lastInteractedStoreItem.itemData);
            }
            if (assignedStall && assignedStall.barterSign) assignedStall.barterSign.DisplayTrade(lastInteractedStoreItem);

        }
        currentType = PathType.Misc;
        Talk();
    }

    public virtual void RefreshStore(){}

    public virtual void EmptyShopItem()//when an item is bought by the player
    {
        //if(lastInteractedStoreItem.clearUponPurchase == false) return;

        if(lastInteractedStoreItem.barterCost.Count == 0) lastInteractedStoreItem.CompletePurchase();
        else lastInteractedStoreItem.CompleteTrade();
        lastInteractedStoreItem = null;
    }
    
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

    public virtual string ReplacementString1()
    {
        return "";
    }

    public virtual string ReplacementString2()
    {
        return "";
    }

    public virtual string ReplacementString3()
    {
        return "";
    }

    public bool CompletedQuest()
    {
        //Check if player completed any quest non item related

        for(int i = 0; i < QuestManager.Instance.activeQuests.Count; i++)
        {
            if(QuestManager.Instance.activeQuests[i].alreadyCompleted || QuestManager.Instance.activeQuests[i].isMajorQuest) continue;

            var type = QuestManager.Instance.activeQuests[i].GetType();

            if(type.Equals(typeof(FetchQuest))) continue;

            if(type.Equals(typeof(GrowQuest)))
            {
                GrowQuest gQ = QuestManager.Instance.activeQuests[i] as GrowQuest;
                if(gQ.amount == 0)
                {
                    QuestManager.Instance.activeQuests[i].alreadyCompleted = true;
                    PlayerInteraction.Instance.GainMints(QuestManager.Instance.activeQuests[i].mintReward, true);
                    //Spawn Items

                    return true;
                }
                else continue;
            }

            if(QuestManager.Instance.activeQuests[i].assignee == character && QuestManager.Instance.activeQuests[i].progress == QuestManager.Instance.activeQuests[i].maxProgress)
            {
                QuestManager.Instance.activeQuests[i].alreadyCompleted = true;
                PlayerInteraction.Instance.GainMints(QuestManager.Instance.activeQuests[i].mintReward, true);
                //Spawn Items

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

            if(QuestManager.Instance.activeQuests[i].assignee == character)
            {
                FetchQuest fq = QuestManager.Instance.activeQuests[i] as FetchQuest;
                if(fq != null && HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData == fq.desiredItem && HotbarDisplay.currentSlot.AssignedInventorySlot.StackSize >= fq.amount)
                {
                    QuestManager.Instance.activeQuests[i].alreadyCompleted = true;
                    PlayerInteraction.Instance.currentMoney += QuestManager.Instance.activeQuests[i].mintReward;
                    PlayerInteraction.Instance.totalMoneyEarned += QuestManager.Instance.activeQuests[i].mintReward;

                    HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(fq.amount);
                    PlayerInventoryHolder.Instance.UpdateInventory();
                    return true;
                }

                GrowQuest gq = QuestManager.Instance.activeQuests[i] as GrowQuest;
                if(gq != null && HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData == gq.desiredItem && HotbarDisplay.currentSlot.AssignedInventorySlot.StackSize >= gq.amount && gq.progress == gq.maxProgress)
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

    public void ReturnFocalPoint(out Transform focalPoint)
    {
        focalPoint = eyeLine;
    }
}

public enum Character
{
    Null,
    MistMerchant,
    Botanist,
    Rascal,
    Lumberjack,
    Apothocary,
    Tinkerer,
    Culinarian,
    Tavernkeep,
    Traveler,
    Fanatic,
    Gravedigger,
    Butcher,
    Craftsman
}
