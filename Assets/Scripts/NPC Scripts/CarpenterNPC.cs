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
            }
            else
            {
                if (CompletedQuest())
                {
                    currentPath = 0;
                    currentType = PathType.QuestComplete;
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

        if (CompletedQuestWithItem())
        {
            currentPath = 0;
            currentType = PathType.QuestComplete;
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
        int i;
        float r;
        int newCost = 0;
        InventoryItemData newItem;
        int x = 0; //iterations
        foreach (StoreItem item in storeItems)
        {
            newItem = null;

            if(x < 2)
            {
                if(x == 0)
                {
                    newItem = woodBarter.itemForSale;
                    item.RefreshItem(newItem, 0, woodBarter.itemsRequired);
                } 
                if(x == 1)
                {
                    newItem = gloomStalkBarter.itemForSale;
                    item.RefreshItem(newItem, 0, gloomStalkBarter.itemsRequired);
                }
                item.seller = this;
                item.clearUponPurchase = false;

                x++;
                continue;
            }

            do
            {
                i = Random.Range(0, barterDatabase.transactions.Count);
                r = Random.Range(0f, 100f);
                if (r < barterDatabase.transactions[i].barterChance) newItem = barterDatabase.transactions[i].itemForSale;
            }
            while (!newItem);
            newCost = (int)(barterDatabase.transactions[i].mintCost * sellMultiplier);
            item.RefreshItem(newItem, newCost, barterDatabase.transactions[i].itemsRequired);
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
}
