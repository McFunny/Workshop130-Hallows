using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotanistNPC : NPC, ITalkable
{
    public InventoryItemData fertalizerT, fertalizerG, fertalizerI;
    public InventoryItemData s_carrot, s_tuber, s_drake, s_stalk, s_bean, s_ginger, s_spores, s_timber; //seeds

    public float sellMultiplier = 1;
    List<StoreItem> storeItems = new List<StoreItem>();

    bool willExplainPollen = false;
    bool willExplainTrellis = false;

    public CropData timberCrop;


    public List<InventoryItemData> questCrops = new List<InventoryItemData>();

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
        if(dialogueController.IsTalking() == false && dialogueController.FreeToSpeak(this)) //Makes sure to not interrupt an existing dialogue branch
        {
            if(!GameSaveData.Instance.botMet) //Introduction Check
            {
                currentPath = -1;
                currentType = PathType.Default;
                GameSaveData.Instance.botMet = true;
                dailyQuest = null;
            }
            else if(!GameSaveData.Instance.bot_giveSeeds && !PlayerInventoryHolder.Instance.IsInventoryFull())
            {
                GameSaveData.Instance.bot_giveSeeds = true;
                currentPath = 7;
                currentType = PathType.Misc;
                itemsToGive.Add(new ItemWithAmount(s_timber, 10));
                QuestManager.Instance.AddQuest(QuestDatabase.Instance.UniqueGrowQuests[0]); //Add the "Grow TimberEar Quest" quest
                dailyQuest = null;
            }
            else if(CompletedQuest())
            {
                currentPath = QuestCompletedDialogue();
                currentType = PathType.QuestComplete;
            }
            else if(GameSaveData.Instance.bot_newWares)
            {
                GameSaveData.Instance.bot_newWares = false;
                currentPath = 11;
                currentType = PathType.Misc;
            }
            else if(!GameSaveData.Instance.bot_giveScytheQuest && !PlayerInventoryHolder.Instance.IsInventoryFull() && timberCrop.amountHarvested > 3)
            {
                GameSaveData.Instance.bot_giveScytheQuest = true;
                currentPath = 9;
                currentType = PathType.Misc;
                itemsToGive.Add(new ItemWithAmount(s_stalk, 10));
                QuestManager.Instance.AddQuest(QuestDatabase.Instance.UniqueGrowQuests[1]); //Add the "Grow Gloomstalk Quest" quest
                dailyQuest = null;
            }
            else if(dailyQuest != null)
            {
                currentPath = QuestDatabase.Instance.GetQuestPath(character);
                currentType = PathType.GivingDaily;
                GivePlayerDailyQuest();
            }
            else if(movementHandler.isWorking) //Working Dialogue
            {
                if(TimeManager.Instance.dayNum == 1)
                {
                    currentPath = 10;
                    currentType = PathType.Misc;   
                }
                else
                {
                    currentPath = 0;
                    currentType = PathType.Misc;
                }
            }
            else if(NPCManager.Instance.botanistSpoke) //Say nothing if already given flavor text
            {
                int i = Random.Range(0, dialogueText.alreadySpoken.Length);
                currentPath = i;
                currentType = PathType.AlreadySpoken;
            }
            else //if(currentPath == -1) //Give 1 daily flavor text
            {
                int i = Random.Range(0, dialogueText.fillerPaths.Length);
                currentPath = i;
                currentType = PathType.Filler;
                NPCManager.Instance.botanistSpoke = true;
            }
        }
        Talk();
        interactSuccessful = true;
    }

    public int QuestCompletedDialogue() //Reference lastCompletedQuestIndex to get which quest it is/what type it is, and give specific remarks here!!
    {
        if(lastCompletedQuestIndex < 0)
        {
            return 0;
        }

        //Remark about completing the timber ear quest here
        if(QuestManager.Instance.CompareQuests(QuestManager.Instance.activeQuests[lastCompletedQuestIndex], QuestDatabase.Instance.UniqueGrowQuests[0]))
        {
            QuestManager.Instance.AddQuest(QuestDatabase.Instance.GetTutorialQuest(302)); //Add the "go barter for the wood" quest
            return 2; //Unfort this means no random timber ear quests
        }

        //Remark about completing the Gloomstalk quest here
        if(QuestManager.Instance.CompareQuests(QuestManager.Instance.activeQuests[lastCompletedQuestIndex], QuestDatabase.Instance.UniqueGrowQuests[1])) //Unfort this means no random gloomstalk quests
        {
            GameSaveData.Instance.scytheObtained = true;
            return 4;
        } 

        //Remark about completing a grow quest here
        if(QuestManager.Instance.activeQuests[lastCompletedQuestIndex] as GrowQuest != null) return 1;

        //Remark about completing the pollination quest here
        if(QuestManager.Instance.CompareQuests(QuestManager.Instance.activeQuests[lastCompletedQuestIndex], QuestDatabase.Instance.GetTutorialQuest(301))) return 3;

        return 0;
    }

    public override void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        ToolItem tItem = item as ToolItem;
        if(dialogueController.IsInterruptable() == false || tItem || !dialogueController.FreeToSpeak(this))
        {
            interactSuccessful = false;
            Talk();
            return;
        } 

        if(CompletedQuestWithItem())
        {
            currentPath = QuestCompletedDialogue();
            currentType = PathType.QuestComplete;
        }

        else if(item == fertalizerI || item == fertalizerT || item == fertalizerG)
        {
            currentPath = 1;
            currentType = PathType.ItemSpecific;
        }

        else if (item.ID == 163)
        {
            currentPath = 8;
            currentType = PathType.ItemSpecific;
        }

        else if(IsItemASeed(item) > -1)
        {
            currentPath = IsItemASeed(item);
            currentType = PathType.ItemSpecific;
        }

        else
        {
            currentPath = 0;
            currentType = PathType.ItemSpecific;
        }

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
        if (assignedStall && assignedStall.displaySign && movementHandler.isWorking)
        {
            assignedStall.displaySign.ResetDisplay();
        }
        base.PlayerLeftRadius();
    }

    public override void RefreshStore()
    {
        if (lastInteractedStoreItem) shopUI.shopImgObj.SetActive(false);
        lastInteractedStoreItem = null;
        int i;
        float r;
        int newCost = 0;
        InventoryItemData newItem;
        int x = 0; //iterations

        questCrops.Clear();

        if (QuestManager.Instance.activeQuests.Count > 0)
        {
            for (int j = 0; j < QuestManager.Instance.activeQuests.Count; j++)
            {
                if (QuestManager.Instance.activeQuests[j] is GrowQuest gQuest)
                {
                    for (int k = 0; k < barterDatabase.transactions.Count; k++) 
                    {
                        if( barterDatabase.transactions[k].itemForSale == gQuest.desiredCrop.cropSeed)
                        {
                            questCrops.Add(gQuest.desiredCrop.cropSeed);
                        }
                    }
                }
            }
        }
        
        foreach (StoreItem item in storeItems)
        {
            newItem = null;
            int extraItems = 0;
            newCost = -1;

            if(x < 3 || x > 8) //For guaranteed stuff to sell
            {
                if(x < 3)
                {
                    if(TimeManager.Instance.dayNum == 1) //Only sell a few carrot seeds the first day
                    {
                        item.RefreshItem(barterDatabase.uniqueTransactions2[0].itemForSale, barterDatabase.uniqueTransactions2[0].mintCost, barterDatabase.uniqueTransactions2[0].itemsRequired,
                        barterDatabase.uniqueTransactions2[0].amountForSale);
                        item.seller = this;
                        return;
                    }
                    int sack = Random.Range(0, 3);
                    item.RefreshItem(barterDatabase.uniqueTransactions[sack].itemForSale, barterDatabase.uniqueTransactions[sack].mintCost, barterDatabase.uniqueTransactions[sack].itemsRequired,
                    barterDatabase.uniqueTransactions[sack].amountForSale);
                }
                if(x > 8)
                {

                    item.RefreshItem(barterDatabase.uniqueTransactions[x - 6].itemForSale, barterDatabase.uniqueTransactions[x - 6].mintCost, barterDatabase.uniqueTransactions[x - 6].itemsRequired,
                    barterDatabase.uniqueTransactions[x - 6].amountForSale);
                }
                item.seller = this;
                //item.clearUponPurchase = false;

                x++;
                continue;
            }

            if (questCrops.Count > 0 && questCrops[0] != null) //If there are any quests that need crops, make this more likely
            {
                for (int k = 0; k < barterDatabase.transactions.Count; k++) 
                {
                    if (barterDatabase.transactions[k].itemForSale == questCrops[0])
                    {
                        newItem = barterDatabase.transactions[k].itemForSale;
                        newCost = (int)(barterDatabase.transactions[k].mintCost * sellMultiplier);
                        extraItems += 5;
                        questCrops.Remove(questCrops[0]);
                        break;
                    }
                }
            }

            do
            {

                i = Random.Range(0, barterDatabase.transactions.Count);
                r = Random.Range(0f, 100f);
                if (r < barterDatabase.transactions[i].barterChance && !newItem && barterDatabase.transactions[i].siegesRequired <= GameSaveData.Instance.siegesCleared)
                {
                    newItem = barterDatabase.transactions[i].itemForSale;
                }
            }
            while (!newItem);
            extraItems += Random.Range(1, 6);
            if(newCost == -1) newCost = (int)(barterDatabase.transactions[i].mintCost * sellMultiplier);
            item.RefreshItem(newItem, newCost, barterDatabase.transactions[i].itemsRequired, barterDatabase.transactions[i].amountForSale + extraItems);
            item.seller = this;

            x++;
        }
    }

    public override void PurchaseSuccess(InventoryItemData item, out bool uniqueDialogue)
    {
        uniqueDialogue = false;
        if(!GameSaveData.Instance.bot_explainedPollen)
        {
            CropItem seed = item as CropItem;
            if(seed && seed.cropData.requirePollination)
            {
                willExplainPollen = true;
                ExtraInformation();
                uniqueDialogue = true;
                return;
            }
        }
        if(!GameSaveData.Instance.bot_explainedTrellis)
        {
            CropItem seed = item as CropItem;
            if(seed && seed.requireTrellis)
            {
                willExplainTrellis = true;
                ExtraInformation();
                uniqueDialogue = true;
                return;
            }
        }
    }

    public override void BeginWorking()
    {
        if(!assignedStall) return;
        storeItems = assignedStall.storeItems;
        if (assignedStall.displaySign)
        {
            assignedStall.displaySign.UpdateNPCName(this);
        }
        RefreshStore();
    }

    public override void StopWorking()
    {
        if(!assignedStall || storeItems.Count == 0) return;
        for(int i = 0; i < storeItems.Count; i++)
        {
            storeItems[i].Empty();
        }
        if(lastInteractedStoreItem)
        {
            lastInteractedStoreItem = null;
        }
        shopUI.shopImgObj.SetActive(false);
        if (assignedStall.displaySign)
        {
            assignedStall.displaySign.LeaveShop();
        }
    }

    void ExtraInformation()
    {
        if(willExplainPollen) //Explain Pollination and give quest
        {
            willExplainPollen = false; 
            QuestManager.Instance.AddQuest(QuestDatabase.Instance.GetTutorialQuest(301)); //Add the "Pollinate" quest

            currentPath = 0;
            currentType = PathType.Quest;

            //PlayerCam.Instance.NewObjectOfInterest(eyeLine.position);
            dialogueController.restartDialogue = true;
            Talk();
            GameSaveData.Instance.bot_explainedPollen = true;
            return;
        }
        if(willExplainTrellis)
        {
            willExplainTrellis = false; 

            currentPath = 8; //Explaining trellis
            currentType = PathType.Misc;

            //PlayerCam.Instance.NewObjectOfInterest(eyeLine.position);
            dialogueController.restartDialogue = true;
            Talk();
            GameSaveData.Instance.bot_explainedTrellis = true;
        }

    }

    public override bool ExclamationCheck()
    {
        if(base.ExclamationCheck() == false)
        {
            if(!GameSaveData.Instance.bot_giveSeeds || (!GameSaveData.Instance.bot_giveScytheQuest && timberCrop.amountHarvested > 3) || GameSaveData.Instance.bot_newWares)
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

    public int IsItemASeed(InventoryItemData item)
    {
        if(item == s_carrot) return 2;
        if(item == s_tuber) return 3;
        if(item == s_drake) return 4;
        if(item == s_stalk) return 5;
        if(item == s_bean) return 6;
        if(item == s_ginger) return 7;
        if(item == s_spores) return 8;


        return -1;
    }

}

