using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotanistNPC : NPC, ITalkable
{
    public InventoryItemData fertalizerT, fertalizerG, fertalizerI;
    public InventoryItemData s_carrot, s_tuber, s_drake, s_stalk, s_bean, s_ginger, s_spores, s_timber; //seeds

    public float sellMultiplier = 1;
    //public InventoryItemData[] possibleSoldItems;
    public InventoryItemData[] commonSeeds, rareSeeds, fertalizers;
    //public float[] itemWeight; //likelyness of being sold, from 0 - 1
    List<StoreItem> storeItems = new List<StoreItem>();
    //WaypointScript shopUI;

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
                currentPath = 5;
                currentType = PathType.Misc;
                itemsToGive.Add(new ItemWithAmount(s_timber, 10));
            }
            else if(CompletedQuest())
            {
                currentPath = 0;
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

    /*public void Talk() //progress what they are saying or start new conversation
    {
        if(!dialogueController.FreeToSpeak(this)) return;
        anim.SetTrigger("IsTalking");
        movementHandler.TalkToPlayer();
        dialogueController.currentTalker = this;
        dialogueController.DisplayNextParagraph(dialogueText, currentPath, currentType);
    }*/

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
            currentPath = 0;
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

        /*else if(item.staminaValue > 0)
        {
            currentPath = 0;
            currentType = PathType.ItemRecieved;
            if(!NPCManager.Instance.botanistFed)
            {
                currentPath = 0;
                currentType = PathType.ItemRecieved;
                NPCManager.Instance.botanistFed = true;
                anim.SetTrigger("TakeItem");
            }
            else
            {
                currentPath = 1;
                currentType = PathType.ItemRecieved;
            }
            //Its consumable and giftable
        }*/
        else
        {
            currentPath = 0;
            currentType = PathType.ItemSpecific;
        }

        Talk();

        interactSuccessful = true;
    }

    public override void PurchaseAttempt(StoreItem item)
    {
        if(dialogueController.IsInterruptable() == false)
        {
            return;
        } 
        if(lastInteractedStoreItem == item)
        {
            //check price, then give item
            if(PlayerInteraction.Instance.currentMoney < lastInteractedStoreItem.cost)
            {
                currentPath = 3; //no money!?!?!?
            }
            else if(PlayerInventoryHolder.Instance.IsInventoryFull(item.itemData, 1))
            {
                currentPath = 4; //No space in inventory
            }
            else
            {
                currentPath = 2; //item sold
                shopUI.shopImgObj.SetActive(false);
                if (assignedStall && assignedStall.displaySign)
                {
                    assignedStall.displaySign.ResetDisplay();
                }
            }
            anim.SetTrigger("IsTalking");
        }
        else
        {
            dialogueController.restartDialogue = true;
            currentPath = 1; //item selected
            anim.SetTrigger("IsTalking");
            if(lastInteractedStoreItem) shopUI.shopImgObj.SetActive(false);
            lastInteractedStoreItem = item;
            shopUI.shopTarget = item.arrowObject.transform;
            shopUI.shopImgObj.SetActive(true);
            if(assignedStall && assignedStall.displaySign)
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
        if(lastInteractedStoreItem) shopUI.shopImgObj.SetActive(false);
        lastInteractedStoreItem = null;
        int i;
        float r;
        int currentItem = 0;
        InventoryItemData newItem;

        InventoryItemData rareSeedForSale;
        i = Random.Range(0, rareSeeds.Length);
        rareSeedForSale = rareSeeds[i];

        questCrops.Clear();

        List <InventoryItemData> allSeeds = new List<InventoryItemData>();
        allSeeds.AddRange(commonSeeds);
        allSeeds.AddRange(rareSeeds);

        if (QuestManager.Instance.activeQuests.Count > 0)
        {
            for (int j = 0; j < QuestManager.Instance.activeQuests.Count; j++)
            {
                if (QuestManager.Instance.activeQuests[j] is GrowQuest gQuest)
                {
                    for (int k = 0; k < allSeeds.Count; k++) 
                    {
                        if( allSeeds[k] == gQuest.desiredCrop.cropSeed)
                        {
                            questCrops.Add(gQuest.desiredCrop.cropSeed);
                        }
                    }
                }

            }
        }

            List<InventoryItemData> commonSeedsForSale = new List<InventoryItemData>();
        while (commonSeedsForSale.Count < 3)
        {
            if (questCrops.Count > 0)
            {
                i = Random.Range(0, 4);
                if (i > 0)
                {
                    i = Random.Range(0, questCrops.Count);
                    if (!commonSeedsForSale.Contains(questCrops[i]))
                    {
                        commonSeedsForSale.Add(questCrops[i]);
                        questCrops.Remove(questCrops[i]);
                    }
                }
                else
                {
                    i = Random.Range(0, commonSeeds.Length);
                    if (!commonSeedsForSale.Contains(commonSeeds[i])) commonSeedsForSale.Add(commonSeeds[i]);
                }
            }
            else
            { 
                i = Random.Range(0, commonSeeds.Length);
                if (!commonSeedsForSale.Contains(commonSeeds[i])) commonSeedsForSale.Add(commonSeeds[i]);
            }
            
        }
        int sellRareSeed = Random.Range(0, 2);
        foreach (StoreItem item in storeItems)
        {
            newItem = null;

            if (currentItem < 6)
            {
                i = Random.Range(0, commonSeedsForSale.Count - 1);
                newItem = commonSeedsForSale[i];
            }
            
            else if (currentItem < 9)
            {
                
                if (sellRareSeed == 0) newItem = rareSeedForSale;
                else newItem = commonSeedsForSale[commonSeedsForSale.Count - 1];

            }
            else
            {
                i = Random.Range(0, fertalizers.Length);
                newItem = fertalizers[i];
            }
            /*do
            {
                i = Random.Range(0, possibleSoldItems.Length);
                r = Random.Range(0f,1f);
                if(r < itemWeight[i]) newItem = possibleSoldItems[i];
            }
            while(!newItem); */
            int newCost = (int) (newItem.value * sellMultiplier);
            item.RefreshItem(newItem, newCost);
            item.seller = this;
            currentItem++;
        }
        currentItem = 0;
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

