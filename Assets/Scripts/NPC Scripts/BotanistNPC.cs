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
            }
            else if(!GameSaveData.Instance.bot_giveSeeds && !PlayerInventoryHolder.Instance.IsInventoryFull())
            {
                GameSaveData.Instance.bot_giveSeeds = true;
                currentPath = 7;
                currentType = PathType.Misc;
                itemsToGive.Add(new ItemWithAmount(s_timber, 10));
                QuestManager.Instance.AddQuest(QuestDatabase.Instance.UniqueGrowQuests[0]); //Add the "Grow TimberEar Quest" quest
            }
            else if(CompletedQuest()) //ADD UNIQUE FUNCTION TO GIVE UNIQUE DIALOGUE THAT IS QUEST DEPENDENT
            {
                currentPath = QuestCompletedDialogue();
                currentType = PathType.QuestComplete;
            }
            else if(movementHandler.isWorking) //Working Dialogue
            {
                currentPath = 0;
                currentType = PathType.Misc;
            }
            else if(NPCManager.Instance.botanistSpoke) //Say nothing if already given flavor text
            {
                int i = Random.Range(0, dialogueText.alreadySpoken.Length);
                currentPath = i;
                currentType = PathType.AlreadySpoken;
            }
            else if(currentPath == -1) //Give 1 daily flavor text
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

    public int QuestCompletedDialogue() 
    {
        if(lastCompletedQuestIndex < 0)
        {
            return 0;
        }
        //reference lastCompletedQuestIndex to get which quest it is/what type it is, and give specific remarks here!!

        //Remark about completing the timber ear quest here

        //Remark about completing a grow quest here

        //Remark about completing the pollination quest here

        else return 0;
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

            if(x == 0 || x > 8) //For guaranteed stuff to sell
            {
                if(x == 0)
                {
                    int sack = Random.Range(0, 2);
                    item.RefreshItem(barterDatabase.uniqueTransactions[sack].itemForSale, barterDatabase.uniqueTransactions[sack].mintCost, barterDatabase.uniqueTransactions[sack].itemsRequired,
                     barterDatabase.transactions[sack].amountForSale);
                } 
                if(x > 8)
                {

                    item.RefreshItem(barterDatabase.uniqueTransactions[x - 7].itemForSale, barterDatabase.uniqueTransactions[x - 7].mintCost, barterDatabase.uniqueTransactions[x - 7].itemsRequired,
                     barterDatabase.transactions[x - 7].amountForSale);
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
                if (r < barterDatabase.transactions[i].barterChance && !newItem)
                {
                    newItem = barterDatabase.transactions[i].itemForSale;
                }
            }
            while (!newItem);
            extraItems += Random.Range(0, 3);
            if(newCost == -1) newCost = (int)(barterDatabase.transactions[i].mintCost * sellMultiplier);
            item.RefreshItem(newItem, newCost, barterDatabase.transactions[i].itemsRequired, barterDatabase.transactions[i].amountForSale + extraItems);
            item.seller = this;

            x++;
        }
    }

    public override void PurchaseSuccess(InventoryItemData item)
    {
        if(!GameSaveData.Instance.bot_explainedPollen)
        {
            CropItem seed = item as CropItem;
            if(seed && seed.cropData.requirePollination)
            {
                willExplainPollen = true;
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

    public override void OnConvoEnd()
    {
        return; //CANNOT GIVE OUT QUEST UNTIL POLLINATOR POST IS IN
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
        }

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

