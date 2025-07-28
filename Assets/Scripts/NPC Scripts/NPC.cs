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

    public GameObject exclamationObject;

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

    protected int lastCompletedQuestIndex = -1;
    private ToolTipScript toolTipScript;

    //REMEMBER TO CAST THIS AS THE CORRECT TYPE OF QUEST WHEN HANDING IT OUT!!!!!!!!
    public Quest dailyQuest = null; //If given a quest today, they will hold it here and have an explanation overhead until its given

    //private ToolTipScript toolTipScript;

    protected virtual void Awake()
    {
        if (dialogueController == null) dialogueController = FindFirstObjectByType<DialogueController>();
        dailyQuest = null;
        toolTipScript = GameObject.Find("BarterCanvas").GetComponent<ToolTipScript>();
    }

    void OnEnable()
    {
        TimeManager.OnHourlyUpdate += HourUpdate;
    }

    void OnDisable()
    {
        TimeManager.OnHourlyUpdate -= HourUpdate;
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

        ExclamationCheck();
    }

    protected virtual void HourUpdate()
    {
        ExclamationCheck();
    }

    public virtual void PurchaseAttempt(StoreItem item)
    {
        bool uniqueDialogue = false;
        if (dialogueController.IsInterruptable() == false || !shopUI)
        {
            Talk();
            return;
        }
        if (lastInteractedStoreItem == item)
        {
            //Barter Price Check
            if (item.barterCost.Count > 0)
            {
                if (PlayerInventoryHolder.Instance.IsInventoryFull(item.itemData, 1))
                {
                    currentPath = 4; //No space in inventory
                    toolTipScript.panel.SetActive(false);
                }
                else if (item.CanAffordTrade())
                {
                    //item.CompleteTrade();
                    currentPath = 2; //item sold
                    shopUI.shopImgObj.SetActive(false);
                    toolTipScript.panel.SetActive(false);
                    PurchaseSuccess(item.itemData, out uniqueDialogue);
                }
                else if (PlayerInteraction.Instance.currentMoney < lastInteractedStoreItem.cost)
                {
                    currentPath = 3; //no money!?!?!?
                    toolTipScript.panel.SetActive(false);
                }
                else
                {
                    currentPath = 6; //Not enough items to cover barter
                    toolTipScript.panel.SetActive(false);
                }
            }

            //check price, then give item
            else if (PlayerInteraction.Instance.currentMoney < lastInteractedStoreItem.cost)
            {
                currentPath = 3; //no money!?!?!?
                toolTipScript.panel.SetActive(false);
            }
            else if (PlayerInventoryHolder.Instance.IsInventoryFull(item.itemData, 1))
            {
                currentPath = 4; //No space in inventory
                toolTipScript.panel.SetActive(false);
            }
            else
            {
                currentPath = 2; //item sold
                PurchaseSuccess(item.itemData, out uniqueDialogue);
                shopUI.shopImgObj.SetActive(false);
                toolTipScript.panel.SetActive(false);
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
        if(!uniqueDialogue)
        {
            currentType = PathType.Misc;
            Talk();
        }
    }

    public virtual void PurchaseSuccess(InventoryItemData boughtItem, out bool uniqueDialogue)
    {
        uniqueDialogue = false;
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

        if (lastInteractedStoreItem)
        {
            lastInteractedStoreItem = null;
            //toolTipScript.panel.SetActive(false);
        }

        if (assignedStall && assignedStall.displaySign && movementHandler.isWorking)
        {
            assignedStall.displaySign.ResetDisplay();
            if (assignedStall.barterSign) assignedStall.barterSign.ResetDisplay();
        }
        
    }

    public virtual void GivePlayerItem(int id, int amount){}

    public virtual void OnConvoEnd()
    {
        currentPath = -1;
        ExclamationCheck();
    }

    public void GiveDailyQuest(Quest q)
    {
        if(q == null || q.name == "") return;
        dailyQuest = q;
        ExclamationCheck();
    }

    protected void GivePlayerDailyQuest()
    {
        if(dailyQuest == null || dailyQuest.name == "")
        {
            dailyQuest = null;
            return;
        }

        //Check to see if we have to identify the type of quest
        QuestManager.Instance.AddQuest(dailyQuest);
        if(dailyQuest != null && dailyQuest.questBehavior) dailyQuest.questBehavior.QuestAssigned(dailyQuest);
        dailyQuest = null;
    }

    public virtual bool ExclamationCheck() //Checks if the exclamation point should persist
    {
        if(!exclamationObject) return false;
        if((dailyQuest != null && dailyQuest.name != "") || QuestManager.Instance.CheckForFinishedNPCQuest(character)) //Need a way to call this hourly, otherwise this wont update when the player completes quests
        {
            exclamationObject.SetActive(true);
            return true;
        }
        else
        {
            exclamationObject.SetActive(false);
            return false;
        }
    }

    public virtual void BeginWorking(){}

    public virtual void StopWorking()
    {
        if (assignedStall && assignedStall.displaySign) assignedStall.displaySign.LeaveShop();
        if (assignedStall && assignedStall.barterSign) assignedStall.barterSign.LeaveShop();

        if (lastInteractedStoreItem) lastInteractedStoreItem = null;
    }

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
            if(QuestManager.Instance.activeQuests[i].alreadyCompleted /*|| QuestManager.Instance.activeQuests[i].isMajorQuest*/ || QuestManager.Instance.activeQuests[i].assignee != character) continue;
            if(QuestManager.Instance.activeQuests[i].isMajorQuest && QuestManager.Instance.activeQuests[i].maxProgress == 0) continue; //To prevent major quests from completing automatically

            var type = QuestManager.Instance.activeQuests[i].GetType();

            if(type.Equals(typeof(FetchQuest))) continue;

            if(type.Equals(typeof(GrowQuest)))
            {
                GrowQuest gQ = QuestManager.Instance.activeQuests[i] as GrowQuest;
                if(gQ.amount == 0 && gQ.progress == gQ.maxProgress)
                {
                    QuestManager.Instance.activeQuests[i].alreadyCompleted = true;
                    PlayerInteraction.Instance.GainMints(QuestManager.Instance.activeQuests[i].mintReward, true);
                    //Spawn Items
                    GiveRewards(QuestManager.Instance.activeQuests[i].itemRewards);

                    lastCompletedQuestIndex = i;
                    return true;
                }
                else continue;
            }

            if(QuestManager.Instance.activeQuests[i].progress == QuestManager.Instance.activeQuests[i].maxProgress)
            {
                QuestManager.Instance.activeQuests[i].alreadyCompleted = true;
                PlayerInteraction.Instance.GainMints(QuestManager.Instance.activeQuests[i].mintReward, true);
                //Spawn Items
                GiveRewards(QuestManager.Instance.activeQuests[i].itemRewards);

                lastCompletedQuestIndex = i;
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
                    PlayerInteraction.Instance.GainMints(QuestManager.Instance.activeQuests[i].mintReward, true);
                    //Spawn Items
                    GiveRewards(QuestManager.Instance.activeQuests[i].itemRewards);

                    HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(fq.amount); //Remember this only works with quests that need items less than their stack size
                    PlayerInventoryHolder.Instance.UpdateInventory();

                    lastCompletedQuestIndex = i;
                    return true;
                }

                GrowQuest gq = QuestManager.Instance.activeQuests[i] as GrowQuest;
                if(gq != null && HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData == gq.desiredItem && HotbarDisplay.currentSlot.AssignedInventorySlot.StackSize >= gq.amount && gq.progress == gq.maxProgress)
                {
                    QuestManager.Instance.activeQuests[i].alreadyCompleted = true;
                    PlayerInteraction.Instance.GainMints(QuestManager.Instance.activeQuests[i].mintReward, true);
                    //Spawn Items
                    GiveRewards(QuestManager.Instance.activeQuests[i].itemRewards);

                    HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(gq.amount); //Remember this only works with quests that need items less than their stack size
                    PlayerInventoryHolder.Instance.UpdateInventory();

                    lastCompletedQuestIndex = i;
                    return true;
                }
            }
        }

        return false;
    }

    void GiveRewards(List<InventoryItemData> rewards)
    {
        if(rewards.Count == 0 || rewards[0] == null) return;

        if(PlayerInventoryHolder.Instance.AddToInventory(rewards[0], rewards.Count)) return; //Gave all the rewards. Only does first item cuz quests should only give 1 type

        Vector3 itemPos = new Vector3(transform.position.x, transform.position.y + 2, transform.position.z);
        for(int i = 0; i < rewards.Count; i++)
        {
            GameObject droppedItem = ItemPoolManager.Instance.GrabItem(rewards[i]);
            droppedItem.transform.position = new Vector3(itemPos.x, itemPos.y + 1f, itemPos.z);

            Rigidbody itemRB = droppedItem.GetComponent<Rigidbody>();
            itemRB.AddForce(Vector3.up * 25);
            itemRB.AddForce(transform.forward * 50);
        }
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
    Apothecary,
    Tinkerer,
    Culinarian,
    Tavernkeep,
    Traveler,
    Fanatic,
    Gravedigger,
    Butcher,
    Craftsman,
    ElderMandrake
}
