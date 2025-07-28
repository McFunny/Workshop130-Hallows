 using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

public class TinkererNPC : NPC, ITalkable
{
    // InventoryItemData papers;
    //public InventoryItemData[] upgrades; //Save this for later

  

    public float sellMultiplier = 1;
    public InventoryItemData watergun, scythe;
    public InventoryItemData[] possibleSoldItems;
    public float[] itemWeight; //likelyness of being sold, from 0 - 1
    List<StoreItem> storeItems = new List<StoreItem>();
    //WaypointScript shopUI;
    private int timesSetUpShop = 0;

    protected override void Awake() //Awake in NPC.cs assigns the dialoguecontroller
    {
        base.Awake();
        movementHandler = GetComponent<NPCMovement>();
        faceCamera = GetComponent<FaceCamera>();
        faceCamera.enabled = false;
    }

    void Start()
    {
        shopUI = FindObjectOfType<WaypointScript>();
    }

    public override void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        if (dialogueController.IsTalking() == false && dialogueController.FreeToSpeak(this))
        {
            if (!GameSaveData.Instance.tinkMet)
            {
                currentPath = -1;
                currentType = PathType.Default;
                GameSaveData.Instance.tinkMet = true;
                QuestManager.Instance.AddQuest(QuestDatabase.Instance.GetMainQuest(7));
                dailyQuest = null;
            }
            else
            {
                if(CompletedQuest()) //ADD UNIQUE FUNCTION TO GIVE UNIQUE DIALOGUE THAT IS QUEST DEPENDENT
                {
                    currentPath = QuestCompletedDialogue();
                    currentType = PathType.QuestComplete;
                }
                else if(dailyQuest != null)
                {
                    currentPath = QuestDatabase.Instance.GetQuestPath(character);
                    currentType = PathType.GivingDaily;
                    GivePlayerDailyQuest();
                }
                else if (NPCManager.Instance.tinkererSpoke)
                {
                    int i = Random.Range(0, dialogueText.alreadySpoken.Length);
                    currentPath = i;
                    currentType = PathType.AlreadySpoken;
                }
                if (currentPath == -1)
                {
                    int i = Random.Range(0, dialogueText.fillerPaths.Length);
                    currentPath = i;
                    NPCManager.Instance.tinkererSpoke = true;
                    currentType = PathType.Filler;
                }
              
            }
        }
        Talk();
        interactSuccessful = true;
    }

    public int QuestCompletedDialogue() //Reference lastCompletedQuestIndex to get which quest it is/what type it is, and give specific remarks here!!
    {
        return 0;
    }

    public override void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        ToolItem tItem = item as ToolItem;
        if (dialogueController.IsInterruptable() == false || tItem || !dialogueController.FreeToSpeak(this))
        {
            interactSuccessful = false;
            return;
        }

        if(CompletedQuestWithItem())
        {
            currentPath = QuestCompletedDialogue();
            currentType = PathType.QuestComplete;
        }
        else if (item.ID == 163)
        {
            currentPath = 1;
            currentType = PathType.ItemSpecific;
        }

        else if (item.staminaValue > 0)
        {
            currentPath = 0;
            currentType = PathType.ItemRecieved;
            /*
            if(!NPCManager.Instance.lumberjackFed)
            {
                currentPath = 0;
                currentType = PathType.ItemRecieved;
                NPCManager.Instance.lumberjackFed = true;
                anim.SetTrigger("TakeItem");
            }
            else
            {
                currentPath = 1;
                currentType = PathType.ItemRecieved;
            }
            */
            //Its consumable and giftable
        }

        else
        {
            currentPath = 0;
            currentType = PathType.ItemSpecific;
        }

        //code for the item being edible
        Talk();

        interactSuccessful = true;
    }

    public override void PurchaseAttempt(StoreItem item)
    {
        if (dialogueController.IsInterruptable() == false)
        {
            return;
        }
        if (lastInteractedStoreItem == item)
        {
            //check price, then give item
            if (PlayerInteraction.Instance.currentMoney < lastInteractedStoreItem.cost)
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
                if (item.itemData == watergun) 
                {
                    GameSaveData.Instance.watergunObtained = true;
                    QuestManager.Instance.ForceRemoveQuest(QuestDatabase.Instance.GetMainQuest(7));
                }
                if (item.itemData == scythe) 
                {
                    GameSaveData.Instance.scytheObtained = true;
                }
                shopUI.shopImgObj.SetActive(false);
            }
            anim.SetTrigger("IsTalking");
        }
        else
        {
            dialogueController.restartDialogue = true;
            currentPath = 1; //item selected
            anim.SetTrigger("IsTalking");
            if (lastInteractedStoreItem) shopUI.shopImgObj.SetActive(false);
            lastInteractedStoreItem = item;
            shopUI.shopTarget = item.arrowObject.transform;
            shopUI.shopImgObj.SetActive(true);
            if (assignedStall.displaySign)
            {
                assignedStall.displaySign.DisplayItem(lastInteractedStoreItem.itemData);
            }

        }
        currentType = PathType.Misc;
        Talk();
    }

    public override void PlayerLeftRadius()
    {
        if (lastInteractedStoreItem)
        {
            lastInteractedStoreItem = null;
        }
        if(movementHandler.isWorking) shopUI.shopImgObj.SetActive(false);
        if (assignedStall && assignedStall.displaySign && movementHandler.isWorking)
        {
            assignedStall.displaySign.ResetDisplay();
        }
        base.PlayerLeftRadius();
    }

    public override void EmptyShopItem()
    {
        lastInteractedStoreItem.Empty();
        lastInteractedStoreItem = null;
    }

    public override void RefreshStore()
    {
        //if(lastInteractedStoreItem) lastInteractedStoreItem.arrowObject.SetActive(false);
        if (lastInteractedStoreItem) shopUI.shopImgObj.SetActive(false);
        lastInteractedStoreItem = null;
        int t;
        float r;
        InventoryItemData newItem;


       
            for (int i = 0; i < storeItems.Count; i++)
            {
                if (i == 0 && !GameSaveData.Instance.watergunObtained && timesSetUpShop > 0 && GameSaveData.Instance.tinkMet)
                {
                    newItem = watergun;
                    int newCost = (int)(newItem.value * sellMultiplier);
                    storeItems[0].RefreshItem(newItem, newCost);
                    storeItems[0].seller = this;
                }
                else if (i == 0 && !GameSaveData.Instance.scytheObtained && timesSetUpShop == 0)
                {
                    newItem = scythe;
                    int newCost = (int)(newItem.value * sellMultiplier);
                    storeItems[0].RefreshItem(newItem, newCost);
                    storeItems[0].seller = this;
                }
                else
                {
                    newItem = null;
                    do
                    {
                        t = Random.Range(0, possibleSoldItems.Length);
                        r = Random.Range(0f, 1f);
                        if (r < itemWeight[t]) newItem = possibleSoldItems[t];
                    }
                    while (!newItem);
                    int newCost = (int)(newItem.value * sellMultiplier);
                    storeItems[i].RefreshItem(newItem, newCost);
                    storeItems[i].seller = this;
                }
            }

        if (timesSetUpShop == 0) { timesSetUpShop++; }
        else if (timesSetUpShop > 0) { timesSetUpShop = 0; }
    }

    protected override void HourUpdate()
    {
        base.HourUpdate();
        if(TimeManager.Instance.currentHour == 8)
        {
            timesSetUpShop = 0;
        }
    }
    

    public override void BeginWorking()
    {
        if (!assignedStall) return;
        storeItems = assignedStall.storeItems;
        if (assignedStall.displaySign)
        {
            assignedStall.displaySign.UpdateNPCName(this);
        }
        RefreshStore();
    }

    public override void StopWorking()
    {
        if (!assignedStall || storeItems.Count == 0) return;
        for (int i = 0; i < storeItems.Count; i++)
        {
            storeItems[i].Empty();
        }
        if (lastInteractedStoreItem)
        {
            lastInteractedStoreItem = null;
        }
        shopUI.shopImgObj.SetActive(false);
        if (assignedStall.displaySign)
        {
            assignedStall.displaySign.LeaveShop();
        }
    }

    public override bool ActionCheck1()
    {
        if (GameSaveData.Instance.townTreeCleared1) return true;
        return false;
    }
}
