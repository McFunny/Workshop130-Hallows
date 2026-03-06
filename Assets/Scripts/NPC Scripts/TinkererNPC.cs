 using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

public class TinkererNPC : NPC, ITalkable
{
    //public InventoryItemData[] upgrades; //Save this for later

    public float sellMultiplier = 1;
    public InventoryItemData[] possibleSoldItems;
    public float[] itemWeight; //likelyness of being sold, from 0 - 1
    List<StoreItem> storeItems = new List<StoreItem>();

    public InventoryItemData assembly, ticket;

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
                itemsToGive.Add(new ItemWithAmount(assembly, 1));
            }
            else
            {
                if(CompletedQuest()) //ADD UNIQUE FUNCTION TO GIVE UNIQUE DIALOGUE THAT IS QUEST DEPENDENT
                {
                    currentPath = QuestCompletedDialogue();
                    currentType = PathType.QuestComplete;
                }
                else if(GameSaveData.Instance.tink_newWares)
                {
                    GameSaveData.Instance.tink_newWares = false;
                    currentPath = 8;
                    currentType = PathType.Misc;
                }
                else if(GameSaveData.Instance.trinketSlotsGiven > 0 && !GameSaveData.Instance.tink_foundTrinketRecipes)
                {
                    GameSaveData.Instance.tink_foundTrinketRecipes = true;
                    currentPath = 9;
                    currentType = PathType.Misc;
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
        else if(item == ticket)
        {
            if(GameSaveData.Instance.tink_explainedTickets)
            {
                GameSaveData.Instance.tink_explainedTickets = true;
                currentPath = 7;
                currentType = PathType.Misc;
            }
            else
            {
                currentPath = 2;
                currentType = PathType.ItemSpecific;
            }
        }
        else if (item.ID == 191)
        {
            currentPath = 3;
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

    public override void PurchaseSuccess(InventoryItemData item, out bool uniqueDialogue)
    {
        uniqueDialogue = false;
        if(item == barterDatabase.uniqueTransactions[0].itemForSale)
        {
            GameSaveData.Instance.watergunObtained = true;
            QuestManager.Instance.ForceRemoveQuest(QuestDatabase.Instance.GetMainQuest(7));
            return;
        }
        if(item == barterDatabase.uniqueTransactions[1].itemForSale)
        {
            GameSaveData.Instance.testerObtained = true;
            return;
        }


        if (item == barterDatabase.uniqueTransactions2[0].itemForSale)
        {
            GameSaveData.Instance.upg_can = true;
            PlayerInteraction.Instance.playerUpgrades.GainWaterStorage();
            AchievementManager.Instance.AddProgressWithEnum(ACHKey.Gilded_Gadgets);
        }
        if (item == barterDatabase.uniqueTransactions2[1].itemForSale)
        {
            GameSaveData.Instance.upg_hoe = true;
            AchievementManager.Instance.AddProgressWithEnum(ACHKey.Gilded_Gadgets);
        }
        if (item == barterDatabase.uniqueTransactions2[2].itemForSale)
        {
            GameSaveData.Instance.upg_torch = true;
            AchievementManager.Instance.AddProgressWithEnum(ACHKey.Gilded_Gadgets);
        }
        if (item == barterDatabase.uniqueTransactions2[3].itemForSale)
        {
            GameSaveData.Instance.upg_scythe = true;
            AchievementManager.Instance.AddProgressWithEnum(ACHKey.Gilded_Gadgets);
        }

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

    /*public override void EmptyShopItem()
    {
        lastInteractedStoreItem.Empty();
        lastInteractedStoreItem = null;
    }*/

    public override void RefreshStore()
    {
        //if(lastInteractedStoreItem) lastInteractedStoreItem.arrowObject.SetActive(false);
        if (lastInteractedStoreItem) shopUI.shopImgObj.SetActive(false);
        lastInteractedStoreItem = null;
        int newCost = 0;
        float r;
        int b;
        InventoryItemData newItem;

        List<int> selectedTrades = new List<int>(); //Make sure no repeats

       
        for (int i = 0; i < storeItems.Count; i++)
        {
            //////

            newItem = null;

            if(storeItems.Count == 4)// upgrades
            {
                if (i == 0 && GameSaveData.Instance.upg_can) continue;
                if (i == 1 && GameSaveData.Instance.upg_hoe) continue;
                if (i == 2 && GameSaveData.Instance.upg_torch) continue;
                if (i == 3 && GameSaveData.Instance.upg_scythe) continue;

                newItem = barterDatabase.uniqueTransactions2[i].itemForSale;
                newCost = (int)(barterDatabase.uniqueTransactions2[i].mintCost * sellMultiplier);
                storeItems[i].RefreshItem(newItem, newCost, barterDatabase.uniqueTransactions2[i].itemsRequired, barterDatabase.uniqueTransactions2[i].amountForSale);
                storeItems[i].seller = this;
                continue;
            }

            if (i == 0 && !GameSaveData.Instance.watergunObtained && GameSaveData.Instance.tinkMet)
            {
                newItem = barterDatabase.uniqueTransactions[0].itemForSale;
                newCost = (int)(barterDatabase.uniqueTransactions[0].mintCost * sellMultiplier);
                storeItems[i].RefreshItem(newItem, newCost, barterDatabase.uniqueTransactions[0].itemsRequired, barterDatabase.uniqueTransactions[0].amountForSale);
                storeItems[i].seller = this;
                continue;
            }

            if (i == 1 && !GameSaveData.Instance.testerObtained && GameSaveData.Instance.tinkMet)
            {
                newItem = barterDatabase.uniqueTransactions[1].itemForSale;
                newCost = (int)(barterDatabase.uniqueTransactions[1].mintCost * sellMultiplier);
                storeItems[i].RefreshItem(newItem, newCost, barterDatabase.uniqueTransactions[1].itemsRequired, barterDatabase.uniqueTransactions[1].amountForSale);
                storeItems[i].seller = this;
                continue;
            } // Wait until the tool is done

            do
            {
                b = Random.Range(0, barterDatabase.transactions.Count);
                r = Random.Range(0f, 100f);
                if (r < barterDatabase.transactions[b].barterChance && barterDatabase.transactions[b].siegesRequired <= GameSaveData.Instance.siegesCleared)
                {
                    if(selectedTrades.Contains(b) && Random.Range(0, 10) > 4) continue; //Repeats are less likely but not impossible
                    newItem = barterDatabase.transactions[b].itemForSale;
                    selectedTrades.Add(b);
                }
            }
            while (!newItem);
            newCost = (int)(barterDatabase.transactions[b].mintCost * sellMultiplier);
            storeItems[i].RefreshItem(newItem, newCost, barterDatabase.transactions[b].itemsRequired, barterDatabase.transactions[b].amountForSale);
            storeItems[i].ChangeAmountGiven(barterDatabase.transactions[b].amountGiven);
            storeItems[i].seller = this;
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

    public override bool ExclamationCheck()
    {
        if(base.ExclamationCheck() == false)
        {
            if(!GameSaveData.Instance.tinkMet || GameSaveData.Instance.tink_newWares || (GameSaveData.Instance.trinketSlotsGiven > 0 && !GameSaveData.Instance.tink_foundTrinketRecipes))
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
        exclamationObject.SetActive(true);
        return true;
    }

    public override bool ActionCheck1()
    {
        if (GameSaveData.Instance.townTreeCleared1) return true;
        return false;
    }

    public override bool ActionCheck2()
    {
        if(GameSaveData.Instance.upg_can && GameSaveData.Instance.upg_hoe && GameSaveData.Instance.upg_scythe && GameSaveData.Instance.upg_torch) 
        {
            AchievementManager.Instance.CompleteProgressWithEnum(ACHKey.Gilded_Gadgets);
            return false;
        }
        if (GameSaveData.Instance.siegesCleared >= 1) return true;
        return false;
    }
}
