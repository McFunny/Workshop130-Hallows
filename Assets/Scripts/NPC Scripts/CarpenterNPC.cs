using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

public class CarpenterNPC : NPC, ITalkable
{
    public float sellMultiplier = 1;
    public InventoryItemData[] possibleSoldItems;
    public float[] itemWeight; //likelyness of being sold, from 0 - 1
    List<StoreItem> storeItems = new List<StoreItem>();

    public InventoryItemData chest;

    public Barter woodBarter, gloomStalkBarter;

    public InventoryItemData timberEar, stalk, bundle, timber, rocks;

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
            if (!GameSaveData.Instance.carpMet)
            {
                currentPath = -1;
                currentType = PathType.Default;
                GameSaveData.Instance.carpMet = true;
            }
            else if(!GameSaveData.Instance.cm_giveChest && !PlayerInventoryHolder.Instance.IsInventoryFull())
            {
                GameSaveData.Instance.cm_giveChest = true;
                currentPath = 7;
                currentType = PathType.Misc;
                itemsToGive.Add(new ItemWithAmount(chest, 1));
                dailyQuest = null;
            }
            else
            {
                if (CompletedQuest())
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
                else if (NPCManager.Instance.carpSpoke)
                {
                    int i = Random.Range(0, dialogueText.alreadySpoken.Length);
                    currentPath = i;
                    currentType = PathType.AlreadySpoken;
                }
                if (currentPath == -1)
                {
                    int i = Random.Range(0, dialogueText.fillerPaths.Length);
                    currentPath = i;
                    NPCManager.Instance.carpSpoke = true;
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

        if (CompletedQuestWithItem())
        {
            currentPath = 0;
            currentType = PathType.QuestComplete;
        }
        else if(item == timberEar || item == stalk)
        {
            currentPath = 1;
            currentType = PathType.ItemSpecific;
        }
        else if(item == timber || item == bundle || item == rocks)
        {
            currentPath = 2;
            currentType = PathType.ItemSpecific;
        }
        else if (item.ID == 163)
        {
            currentPath = 3;
            currentType = PathType.ItemSpecific;
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

    /*public override void PurchaseAttempt(StoreItem item)
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
                shopUI.shopImgObj.SetActive(false);
                if (assignedStall.displaySign)
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
    }*/

    public override void PlayerLeftRadius()
    {
        if(movementHandler.isWorking) shopUI.shopImgObj.SetActive(false);
        base.PlayerLeftRadius();
    }

    /*public override void EmptyShopItem() //when an item is bought by the player
    {
        if(lastInteractedStoreItem.clearUponPurchase == false) return;
        lastInteractedStoreItem.Empty();
        lastInteractedStoreItem = null;
    }*/

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

            if(x < 3)
            {
                if(x == 0)
                {
                    newItem = woodBarter.itemForSale;
                    item.RefreshItem(newItem, 0, woodBarter.itemsRequired, 99);
                } 
                if(x == 1)
                {
                    newItem = gloomStalkBarter.itemForSale;
                    item.RefreshItem(newItem, 0, gloomStalkBarter.itemsRequired, 99);
                }
                if(x == 2)
                {
                    newItem = barterDatabase.uniqueTransactions[0].itemForSale;
                    newCost = (int)(barterDatabase.uniqueTransactions[0].mintCost * sellMultiplier);
                    item.RefreshItem(newItem, newCost, barterDatabase.uniqueTransactions[0].itemsRequired, barterDatabase.uniqueTransactions[0].amountForSale);
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
            item.seller = this;

            x++;

            /*
            do
            {
                i = Random.Range(0, possibleSoldItems.Length);
                r = Random.Range(0f, 1f);
                if (r < itemWeight[i]) newItem = possibleSoldItems[i];
            }
            while (!newItem);
            int newCost = (int)(newItem.value * sellMultiplier);
            item.RefreshItem(newItem, newCost);
            item.seller = this;
            */
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
        shopUI.shopImgObj.SetActive(false);
        base.StopWorking();
    }

    public override bool ExclamationCheck()
    {
        if(base.ExclamationCheck() == false)
        {
            if(!GameSaveData.Instance.cm_giveChest)
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
        return true;
    }
}
