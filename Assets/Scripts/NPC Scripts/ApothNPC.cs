using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

public class ApothNPC : NPC, ITalkable
{
    public float sellMultiplier = 1;
    List<StoreItem> storeItems = new List<StoreItem>();

    bool currentlyReadingScroll = false;
    bool justGaveTissueQuest = false;

    Barter siegeSeedBarter;

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

        if(!GameSaveData.Instance.apo_wasKidnapped && GameSaveData.Instance.siegesCleared == 1)
        {
            GameSaveData.Instance.apo_wasKidnapped = true;
        }
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
                dailyQuest = null;
            }
            else
            {
                if(CompletedQuest())
                {
                    currentPath = QuestCompletedDialogue();
                    currentType = PathType.QuestComplete;
                }
                else if(!GameSaveData.Instance.apo_explainedSiege && GameSaveData.Instance.apo_readScroll)
                {
                    GameSaveData.Instance.apo_explainedSiege = true;
                    currentPath = 0;
                    currentType = PathType.Quest;
                    QuestManager.Instance.AddQuest(QuestDatabase.Instance.MainQuests[9]);
                }
                else if(!GameSaveData.Instance.apo_thanked && GameSaveData.Instance.apo_rescued) //Apo commented on being rescued
                {
                    GameSaveData.Instance.apo_thanked = true;
                    currentPath = 8;
                    currentType = PathType.Misc;
                    QuestManager.Instance.ForceCompleteQuest(QuestDatabase.Instance.GetMainQuest(16)); //Ideally this is called after the apoth is freed from her cage
                }
                else if(!GameSaveData.Instance.apo_gaveTissueQuest && GameSaveData.Instance.siegesCleared == 2) //Apo gives quest for azure rot cure
                {
                    GameSaveData.Instance.apo_gaveTissueQuest = true;
                    currentPath = 1;
                    currentType = PathType.Quest;
                    QuestManager.Instance.AddQuest(QuestDatabase.Instance.UniqueFetchQuests[0]);
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
            currentlyReadingScroll = true;
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            PlayerInventoryHolder.Instance.UpdateInventory();
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

        List<int> selectedTrades = new List<int>(); //Make sure no repeats
        foreach (StoreItem item in storeItems)
        {
            newItem = null;

            if(x < 2 && CanSellSiegeSeeds())
            {
                int index = GameSaveData.Instance.siegesCleared * 2;
                if(x == 0) //Small siege seeds
                {
                    newItem = barterDatabase.uniqueTransactions[index].itemForSale;
                    newCost = (int)(barterDatabase.uniqueTransactions[index].mintCost * sellMultiplier);
                    item.RefreshItem(newItem, newCost, barterDatabase.uniqueTransactions[index].itemsRequired, barterDatabase.uniqueTransactions[index].amountForSale);
                } 
                if(x == 1) //Titan seeds
                {
                    siegeSeedBarter = barterDatabase.uniqueTransactions[index + 1];

                    newItem = siegeSeedBarter.itemForSale;
                    newCost = (int)(siegeSeedBarter.mintCost * sellMultiplier);

                    int seedsNeededMod = GameSaveData.Instance.siegesLost + 1;
                    if(seedsNeededMod > 4) seedsNeededMod = 4; //Discount wont be less than 1/4th of base price

                    siegeSeedBarter.itemsRequired[0].amount = siegeSeedBarter.itemsRequired[0].amount/seedsNeededMod; //Have the cost be less depending on how many times the player failed the siege

                    item.RefreshItem(newItem, newCost, siegeSeedBarter.itemsRequired, siegeSeedBarter.amountForSale);
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
                if (r < barterDatabase.transactions[i].barterChance && !selectedTrades.Contains(i) && barterDatabase.transactions[i].siegesRequired <= GameSaveData.Instance.siegesCleared)
                {
                    newItem = barterDatabase.transactions[i].itemForSale;
                    selectedTrades.Add(i);
                }
            }
            while (!newItem);
            newCost = (int)(barterDatabase.transactions[i].mintCost * sellMultiplier);
            item.RefreshItem(newItem, newCost, barterDatabase.transactions[i].itemsRequired, barterDatabase.transactions[i].amountForSale);
            item.ChangeAmountGiven(barterDatabase.transactions[i].amountGiven);
            item.seller = this;

            x++;
        }
    }

    public override void PurchaseSuccess(InventoryItemData item, out bool uniqueDialogue)
    {
        uniqueDialogue = false;

        int index = GameSaveData.Instance.siegesCleared * 2;
        if(item == barterDatabase.uniqueTransactions[index + 1].itemForSale)
        {
            GameSaveData.Instance.siegeCropInHand = true;
            if(!GameSaveData.Instance.apo_explainedTitanSeed)
            {
                GameSaveData.Instance.apo_explainedTitanSeed = true;
                ExtraInformation();
                uniqueDialogue = true;
            }
        }
    }

    public int QuestCompletedDialogue() //Reference lastCompletedQuestIndex to get which quest it is/what type it is, and give specific remarks here!!
    {
        if(lastCompletedQuestIndex < 0)
        {
            return 0;
        }

        //Remark about completing the Tissue quest here
        if(QuestManager.Instance.CompareQuests(QuestManager.Instance.activeQuests[lastCompletedQuestIndex], QuestDatabase.Instance.UniqueFetchQuests[0]))
        {
            QuestManager.Instance.AddQuest(QuestDatabase.Instance.UniqueGrowQuests[2]); //Add the "Grow Puripulp Quest" quest
            if(justGaveTissueQuest) return 3;
            else return 1; 
        }

        //Remark about completing the Puripulp quest here
        if(QuestManager.Instance.CompareQuests(QuestManager.Instance.activeQuests[lastCompletedQuestIndex], QuestDatabase.Instance.UniqueGrowQuests[2])) 
        {
            GameSaveData.Instance.apo_gaveCure = true;
            CraftingDatabase.Instance.UnlockRecipe(7); // unlocks the purification flask
            return 2;
        } 

        return 0;
    }

    void ExtraInformation()
    {
        currentPath = 7; //Explaining Siege
        currentType = PathType.Misc;

        dialogueController.restartDialogue = true;
        Talk();

    }

    public override int VerifySchedule()
    {
        if(GameSaveData.Instance.apo_wasKidnapped && !GameSaveData.Instance.apo_rescued) return 2;
        return 1;
    }

    public override void BeginWorking()
    {
        if (!assignedStall) return;
        storeItems = assignedStall.storeItems;
        RefreshStore();
    }

    public override void StopWorking()
    {
        if(currentlyReadingScroll)
        {
            currentlyReadingScroll = false;
            GameSaveData.Instance.apo_readScroll = true;
        }

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

    protected override void HourUpdate()
    {
        base.HourUpdate();
        if(currentlyReadingScroll && (TimeManager.Instance.currentHour == 8 || TimeManager.Instance.currentHour == 19 || TimeManager.Instance.currentHour == 20))
        {
            currentlyReadingScroll = false;
            GameSaveData.Instance.apo_readScroll = true;
        }

        if((TimeManager.Instance.currentHour == 8 || TimeManager.Instance.currentHour == 9) && !GameSaveData.Instance.apo_wasKidnapped && GameSaveData.Instance.siegesCleared == 1)
        {
            GameSaveData.Instance.apo_wasKidnapped = true;
        }

        if(TimeManager.Instance.currentHour == 8)
        {
            justGaveTissueQuest = false;
        }

        
    }

    public override bool ExclamationCheck()
    {
        if(base.ExclamationCheck() == false)
        {
            if((!GameSaveData.Instance.apo_gaveTissueQuest && GameSaveData.Instance.siegesCleared == 2) || (!GameSaveData.Instance.apo_explainedSiege && GameSaveData.Instance.apo_readScroll))
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

    bool CanSellSiegeSeeds()
    {
        if(GameSaveData.Instance.siegesCleared == 1 && !GameSaveData.Instance.apo_rescued) return false; // apo has not been kidnapped yet or is kidnapped
        if(GameSaveData.Instance.siegesCleared == 2 && !GameSaveData.Instance.apo_gaveCure) return false; // apo has not given the azure rot cure yet
        if(GameSaveData.Instance.siegeCropInHand || SiegeManager.Instance.siegeCropOnFarm || !GameSaveData.Instance.apo_readScroll) return false;
        if(GameSaveData.Instance.siegesCleared >= 4) return false; //All sieges done
        return true;
    }

    public override bool ActionCheck1()
    {
        if (GameSaveData.Instance.townTreeCleared1) return true;
        return false;
    }
}
