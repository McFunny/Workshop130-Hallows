using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

public class CulinarianNPC : NPC, ITalkable
{
    public float sellMultiplier = 1;
    public InventoryItemData[] possibleSoldItems;
    public float[] itemWeight; //likelyness of being sold, from 0 - 1
    List<StoreItem> storeItems = new List<StoreItem>();
    //WaypointScript shopUI;

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
            if (!GameSaveData.Instance.culMet)
            {
                currentPath = -1;
                currentType = PathType.Default;
                GameSaveData.Instance.culMet = true;
            }
            else if (AbleToGiveCrockPotQuest())
            {
                QuestManager.Instance.AddQuest(QuestDatabase.Instance.GetTutorialQuest(304));
                currentPath = 0;
                currentType = PathType.Quest;
                dailyQuest = null;
            }
            else if(AbleToCompleteCrockPotQuest())
            {
                currentPath = 1;
                currentType = PathType.QuestComplete;
                GameSaveData.Instance.cul_gaveCrock = true;
                GiveRewards(QuestDatabase.Instance.GetTutorialQuest(304).itemRewards);
                itemsToGive.Add(new ItemWithAmount(barterDatabase.uniqueTransactions[2].itemForSale, 5));
            }
            else
            {
                if(CompletedQuest())
                {
                    currentPath = 0;
                    currentType = PathType.QuestComplete;
                }
                else if(dailyQuest != null)
                {
                    currentPath = QuestDatabase.Instance.GetQuestPath(character);
                    currentType = PathType.GivingDaily;
                    GivePlayerDailyQuest();
                }
                else if (NPCManager.Instance.culinarianSpoke)
                {
                    int i = Random.Range(0, dialogueText.alreadySpoken.Length);
                    currentPath = i;
                    currentType = PathType.AlreadySpoken;
                }
                if (currentPath == -1)
                {
                    int i = Random.Range(0, dialogueText.fillerPaths.Length);
                    currentPath = i;
                    currentType = PathType.Filler;
                    NPCManager.Instance.culinarianSpoke = true;
                }
               
            }
        }
        Talk();
        interactSuccessful = true;
    }

    /*public void Talk()
    {
        if(!dialogueController.FreeToSpeak(this)) return;
        anim.SetTrigger("IsTalking");
        movementHandler.TalkToPlayer();
        dialogueController.currentTalker = this;
        dialogueController.DisplayNextParagraph(dialogueText, currentPath, currentType);
        startedDialogue = true;
    }*/

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
            currentPath = 0;
            currentType = PathType.QuestComplete;
        }

        else if (item.ID == 163)
        {
            currentPath = 1;
            currentType = PathType.ItemSpecific;
        }

        else if (item.ID == 285)
        {
            currentPath = 2;
            currentType = PathType.ItemSpecific;
        }

        else
        {
            currentPath = RemarkOnItem(item);
            if(currentPath >= 0)
            {
                currentType = PathType.ItemPath;
            }
            else
            {
                currentPath = 0;
                currentType = PathType.ItemSpecific;
            }
        }

        //code for the item being edible
        Talk();

        interactSuccessful = true;
    }

    public override void PlayerLeftRadius()
    {
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
        InventoryItemData newItem;
        int x = 0; //iterations
        List<int> selectedTrades = new List<int>(); //Make sure no repeats
        foreach (StoreItem item in storeItems)
        {
            newItem = null;

            if(x < 4) //For crock pot, sugar, and oil
            {
                if(!GameSaveData.Instance.cul_gaveCrock && x == 0) //No crockpot until quest is done
                {
                    x++;
                    continue;
                }
                if((!GameSaveData.Instance.cul_gaveCrock || CookingDatabase.Instance.AllRecipesUnlocked()) && x == 3) //No more recipes until quest is done and all are unlocked
                {
                    x++;
                    continue;
                }
                item.RefreshItem(barterDatabase.uniqueTransactions[x].itemForSale, barterDatabase.uniqueTransactions[x].mintCost, barterDatabase.uniqueTransactions[x].itemsRequired,
                    barterDatabase.uniqueTransactions[x].amountForSale);
                item.seller = this;
                x++;
                continue;
            }
            
            do
            {
                i = Random.Range(0, barterDatabase.transactions.Count);
                r = Random.Range(0f, 100f);
                if (r < barterDatabase.transactions[i].barterChance && !selectedTrades.Contains(i) && barterDatabase.transactions[i].siegesRequired <= GameSaveData.Instance.siegesCleared)
                {
                    if(i == 8 && GameSaveData.Instance.deadHenIDs.Count >= 4) continue; //No eggs if no hens
                    newItem = barterDatabase.transactions[i].itemForSale;
                    selectedTrades.Add(i);
                } 
            }
            while (!newItem);
            int newCost = (int)(barterDatabase.transactions[i].mintCost * sellMultiplier);
            item.RefreshItem(newItem, newCost, barterDatabase.transactions[i].itemsRequired, barterDatabase.transactions[i].amountForSale);
            item.seller = this;
            x++;
        }
    }

    public override void PurchaseSuccess(InventoryItemData item, out bool uniqueDialogue)
    {
        uniqueDialogue = false;
        int questNum = QuestManager.Instance.FindSameQuest(QuestDatabase.Instance.GetTutorialQuest(304));
        if(questNum == -1 || QuestManager.Instance.activeQuests[questNum].progress >= QuestManager.Instance.activeQuests[questNum].maxProgress) return;

        foreach(Barter barter in barterDatabase.transactions) if(barter.itemForSale == item)
        {
            QuestManager.Instance.activeQuests[questNum].progress++;
            break;
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
        if (assignedStall.barterSign) assignedStall.barterSign.LeaveShop();
    }

    public override bool ExclamationCheck()
    {
        if(base.ExclamationCheck() == false)
        {
            if(AbleToGiveCrockPotQuest())
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

    bool AbleToGiveCrockPotQuest()
    {
        if(!GameSaveData.Instance.cul_gaveCrock && GameSaveData.Instance.rascalMentionedKey && QuestManager.Instance.FindSameQuest(QuestDatabase.Instance.GetTutorialQuest(304)) == -1) return true;
        return false;
    }

    bool AbleToCompleteCrockPotQuest()
    {
        if(GameSaveData.Instance.cul_gaveCrock) return false;
        int questNum = QuestManager.Instance.FindSameQuest(QuestDatabase.Instance.GetTutorialQuest(304));
        if(questNum == -1) return false;
        if(QuestManager.Instance.activeQuests[questNum].progress >= QuestManager.Instance.activeQuests[questNum].maxProgress)
        {
            QuestManager.Instance.activeQuests[questNum].alreadyCompleted = true;
            return true;
        }

        return false;
    }
}
