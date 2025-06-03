using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurvivalModeMerchant : NPC, ITalkable
{
    private InventoryItemData lastSeenItem;

    public float sellMultiplier = 1;
    public StoreItem[] storeItems;
    ItemDisplaySign displaySign;



    //Find a way to get feedback on when a dialogue tree is finished by calling an event/delegate.

    void Start()
    {
        shopUI = FindObjectOfType<WaypointScript>();
        StartCoroutine(DelayedStart());
        TimeManager.OnHourlyUpdate += HourlyUpdate;
        for(int i = 0; i < storeItems.Length; i++)
        {
            storeItems[i].seller = this;
        }

        if (displaySign) displaySign.UpdateNPCName(this);

    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(2);
        RefreshStore();
    }

    void OnDisable()
    {
        StopAllCoroutines();
        TimeManager.OnHourlyUpdate -= HourlyUpdate;
    }

    public override void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        if(dialogueController.IsTalking() == false && dialogueController.FreeToSpeak(this))
        {
            currentPath = -1;
            currentType = PathType.Default;

            lastSeenItem = null;
            dialogueController.SetInterruptable(false);

            anim.SetTrigger("IsTalking");
        }
        Talk();
        interactSuccessful = true;
    }

    public override void Talk()
    {
        if(!dialogueController.FreeToSpeak(this)) return;
        dialogueController.currentTalker = this;
        dialogueController.DisplayNextParagraph(dialogueText, currentPath, currentType);
    }

    public override void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        ToolItem tItem = item as ToolItem;
        if(dialogueController.IsInterruptable() == false || tItem)
        {
            interactSuccessful = false;
            //if(dialogueController.FreeToSpeak(this))Talk();
            return;
        } 

        if(TimeManager.Instance.currentHour == 6 || TimeManager.Instance.currentHour == 7);
        else
        {
            //Cannot Buy, only at morning
            lastSeenItem = item;
            currentPath = 9;
            currentType = PathType.Misc;
            Talk();

            anim.SetTrigger("IsTalking");
            interactSuccessful = true;
            return;
        }

        if(item.sellValueMultiplier == 0 || item.value == 0 || item as PlaceableItem)
        {
            //Cannot Buy
            lastSeenItem = item;
            currentPath = 6;
            currentType = PathType.Misc;
            Talk();

            anim.SetTrigger("IsTalking");
        }
        else
        {
            //Can Buy
            if(lastSeenItem != item)
            {
                //Are you sure?
                lastSeenItem = item;
                dialogueController.restartDialogue = true;
                if(HotbarDisplay.currentSlot.AssignedInventorySlot.StackSize > 1) currentPath = 8;
                else currentPath = 0;
                currentType = PathType.Misc;

                anim.SetTrigger("IsTalking");
            }
            else
            {
                print("Repeated item");
                //Sold, remove item and gain money
                currentPath = 7;
                currentType = PathType.Misc;

                anim.SetTrigger("Transaction");
                InventorySlot slot = HotbarDisplay.currentSlot.AssignedInventorySlot;
                SurvivalModeManager.Instance.mintsEarned += (int)(slot.StackSize * (slot.ItemData.value * slot.ItemData.sellValueMultiplier));
            }
            Talk();
        }
        interactSuccessful = true;
    }

    /*public override void PurchaseAttempt(StoreItem item)
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
                currentPath = 6; //no money!?!?!?
            }
            else if(PlayerInventoryHolder.Instance.IsInventoryFull())
            {
                currentPath = 7; //No space in inventory
            }
            else
            {
                currentPath = 5; //item sold
                shopUI.shopImgObj.SetActive(false);
                if(displaySign)
                {
                    displaySign.ResetDisplay();
                }
            }
            anim.SetTrigger("Transaction");
        }
        else
        {
            dialogueController.restartDialogue = true;
            currentPath = 4; //item selected
            anim.SetTrigger("IsTalking");
            if(lastInteractedStoreItem) shopUI.shopImgObj.SetActive(false);
            lastInteractedStoreItem = item;
            shopUI.shopTarget = item.arrowObject.transform;
            shopUI.shopImgObj.SetActive(true);
            if (displaySign) displaySign.DisplayItem(lastInteractedStoreItem.itemData);
            
        }
        currentType = PathType.Misc;
        Talk();
    }*/

    public override void RefreshStore()
    {
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

            if(x < 6)
            {
                item.seller = this;

                newItem = barterDatabase.uniqueTransactions[x].itemForSale;
                item.RefreshItem(newItem, barterDatabase.uniqueTransactions[x].mintCost, barterDatabase.uniqueTransactions[x].itemsRequired,  barterDatabase.uniqueTransactions[x].amountForSale);

                x++;
                continue;
            }

            do
            {
                i = Random.Range(0, barterDatabase.transactions.Count);
                r = Random.Range(0f, 100f);
                if (r < barterDatabase.transactions[i].barterChance && !selectedTrades.Contains(i))
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
        }
    }

    public override void PlayerLeftRadius()
    {
        if(lastInteractedStoreItem)
        {
            lastInteractedStoreItem = null;
        }
        if(lastSeenItem) lastSeenItem = null; 
        shopUI.shopImgObj.SetActive(false);
        if (displaySign) displaySign.ResetDisplay();
    }

    public void HourlyUpdate()
    {
        if(TimeManager.Instance.currentHour == 8)
        {
            RefreshStore();
        }
    }
    
}
