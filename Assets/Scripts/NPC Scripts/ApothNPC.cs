using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

public class ApothNPC : NPC, ITalkable
{
    public float sellMultiplier = 1;
    List<StoreItem> storeItems = new List<StoreItem>();

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
            if (!GameSaveData.Instance.apothMet)
            {
                currentPath = -1;
                currentType = PathType.Default;
                GameSaveData.Instance.apothMet = true;
            }
            else
            {
                if(CompletedQuest())
                {
                    currentPath = 0;
                    currentType = PathType.QuestComplete;
                }
                else if(!GameSaveData.Instance.apo_explainedSiege && GameSaveData.Instance.apo_readScroll)
                {
                    GameSaveData.Instance.apo_explainedSiege = true;
                    currentPath = 0;
                    currentType = PathType.Quest;
                    QuestManager.Instance.AddQuest(QuestDatabase.Instance.MainQuests[9]);
                }
                else if(dailyQuest != null)
                {
                    currentPath = QuestDatabase.Instance.GetQuestPath(character);
                    currentType = PathType.GivingDaily;
                    GivePlayerDailyQuest();
                }
                else if (NPCManager.Instance.apothSpoke)
                {
                    int i = Random.Range(0, dialogueText.alreadySpoken.Length);
                    currentPath = i;
                    currentType = PathType.AlreadySpoken;
                }
                if (currentPath == -1)
                {
                    int i = Random.Range(0, dialogueText.fillerPaths.Length);
                    currentPath = i;
                    NPCManager.Instance.apothSpoke = true;
                    currentType = PathType.Filler;
                }
               
            }
        }
        Talk();
        interactSuccessful = true;
    }


    public override void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        ToolItem tItem = item as ToolItem;
        if (dialogueController.IsInterruptable() == false || tItem || !dialogueController.FreeToSpeak(this))
        {
            interactSuccessful = false;
            return;
        }

        //Add special text for showing him his own tree papers

        if(CompletedQuestWithItem())
        {
            currentPath = 0;
            currentType = PathType.QuestComplete;
        }

        else if (item.ID == 163)
        {
            currentPath = 1;
            currentType = PathType.ItemSpecific;
            GameSaveData.Instance.apo_readScroll = true; //Have this be set later in the day, when she stops working or when the game saves at night
        }

        else if (item.staminaValue > 0)
        {
            currentPath = 0;
            currentType = PathType.ItemRecieved;
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

    public override void PlayerLeftRadius()
    {
        if (lastInteractedStoreItem)
        {
            lastInteractedStoreItem = null;
        }
        if(movementHandler.isWorking) shopUI.shopImgObj.SetActive(false);
        base.PlayerLeftRadius();
    }

    public override void RefreshStore()
    {
        //if(lastInteractedStoreItem) lastInteractedStoreItem.arrowObject.SetActive(false);
        if (lastInteractedStoreItem) shopUI.shopImgObj.SetActive(false);
        lastInteractedStoreItem = null;
        int i;
        float r;
        int newCost = 0;
        InventoryItemData newItem;
        int x = 0; //iterations

        //List<int> selectedTrades = new List<int>(); //Make sure no repeats
        foreach (StoreItem item in storeItems)
        {
            newItem = null;

            if(x < 2 && CanSellSiegeSeeds())
            {
                if(x == 0)
                {
                    newItem = barterDatabase.uniqueTransactions[0].itemForSale;
                    newCost = (int)(barterDatabase.uniqueTransactions[0].mintCost * sellMultiplier);
                    item.RefreshItem(newItem, newCost, barterDatabase.uniqueTransactions[0].itemsRequired, barterDatabase.uniqueTransactions[0].amountForSale);
                } 
                if(x == 1)
                {
                    newItem = barterDatabase.uniqueTransactions[1].itemForSale;
                    newCost = (int)(barterDatabase.uniqueTransactions[1].mintCost * sellMultiplier);
                    item.RefreshItem(newItem, newCost, barterDatabase.uniqueTransactions[1].itemsRequired, barterDatabase.uniqueTransactions[1].amountForSale);
                }
                item.seller = this;
                //item.clearUponPurchase = false;

                x++;
                continue;
            }

            do
            {
                i = Random.Range(0, barterDatabase.transactions.Count);
                r = Random.Range(0f, 100f);
                if (r < barterDatabase.transactions[i].barterChance/* && !selectedTrades.Contains(i)*/)
                {
                    newItem = barterDatabase.transactions[i].itemForSale;
                    //selectedTrades.Add(i);
                }
            }
            while (!newItem);
            newCost = (int)(barterDatabase.transactions[i].mintCost * sellMultiplier);
            item.RefreshItem(newItem, newCost, barterDatabase.transactions[i].itemsRequired, barterDatabase.transactions[i].amountForSale);
            item.seller = this;

            x++;
        }
    }

    public override void BeginWorking()
    {
        if (!assignedStall) return;
        storeItems = assignedStall.storeItems;
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
    }

    bool CanSellSiegeSeeds()
    {
        if(GameSaveData.Instance.siegeCropInHand || SiegeManager.Instance.siegeCropOnFarm || !GameSaveData.Instance.apo_readScroll) return false;
        return true;
    }

    public override bool ActionCheck1()
    {
        if (GameSaveData.Instance.townTreeCleared1) return true;
        return false;
    }
}
