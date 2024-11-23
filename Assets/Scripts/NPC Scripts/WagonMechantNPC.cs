using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WagonMerchantNPC : NPC, ITalkable
{
    private InventoryItemData lastSeenItem;

    public Animator anim;

    public float sellMultiplier = 1;
    public InventoryItemData[] possibleSoldItems;
    public float[] itemWeight; //likelyness of being sold, from 0 - 1
    public StoreItem[] storeItems;
    WaypointScript shopUI;

    //Find a way to get feedback on when a dialogue tree is finished by calling an event/delegate.

    void Start()
    {
        shopUI = FindObjectOfType<WaypointScript>();
        RefreshStore();
        TimeManager.OnHourlyUpdate += HourlyUpdate;
    }

    public override void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        if(dialogueController.IsTalking() == false)
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

    public void Talk()
    {
        dialogueController.currentTalker = this;
        dialogueController.DisplayNextParagraph(dialogueText, currentPath, currentType);
    }

    public override void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        if(dialogueController.IsInterruptable() == false)
        {
            interactSuccessful = true;
            return;
        } 
        if(item.sellValueMultiplier == 0 || item.value == 0)
        {
            //Cannot Buy
            lastSeenItem = item;
            currentPath = 1;
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
                if(HotbarDisplay.currentSlot.AssignedInventorySlot.StackSize > 1) currentPath = 3;
                else currentPath = 0;
                currentType = PathType.Misc;

                anim.SetTrigger("IsTalking");
            }
            else
            {
                //Sold, remove item and gain money
                currentPath = 2;
                currentType = PathType.Misc;

                anim.SetTrigger("Transaction");
            }
            Talk();
        }
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
                currentPath = 6; //no money!?!?!?
            }
            else
            {
                currentPath = 5; //item sold
                //item.arrowObject.SetActive(false);
                shopUI.shopImgObj.SetActive(false);
            }
            anim.SetTrigger("Transaction");
            //lastInteractedStoreItem = null;
        }
        else
        {
            dialogueController.restartDialogue = true;
            currentPath = 4; //item selected
            anim.SetTrigger("IsTalking");
            //if(lastInteractedStoreItem) lastInteractedStoreItem.arrowObject.SetActive(false);
            if(lastInteractedStoreItem) shopUI.shopImgObj.SetActive(false);
            lastInteractedStoreItem = item;
            //item.arrowObject.SetActive(true);
            shopUI.shopTarget = item.arrowObject.transform;
            shopUI.shopImgObj.SetActive(true);
            
        }
        currentType = PathType.Misc;
        Talk();
    }

    public override void RefreshStore()
    {
        //if(lastInteractedStoreItem) lastInteractedStoreItem.arrowObject.SetActive(false);
        if(lastInteractedStoreItem) shopUI.shopImgObj.SetActive(false);
        lastInteractedStoreItem = null;
        int i;
        float r;
        InventoryItemData newItem;
        foreach (StoreItem item in storeItems)
        {
            newItem = null;
            do
            {
                i = Random.Range(0, possibleSoldItems.Length);
                r = Random.Range(0f,1f);
                if(r < itemWeight[i]) newItem = possibleSoldItems[i];
            }
            while(!newItem);
            int newCost = (int) (newItem.value * sellMultiplier);
            item.RefreshItem(newItem, newCost);
            item.seller = this;
        }
    }

    public override void EmptyShopItem()
    {
        lastInteractedStoreItem.Empty();
        lastInteractedStoreItem = null;
    }

    public override void PlayerLeftRadius()
    {
        if(lastInteractedStoreItem)
        {
            //lastInteractedStoreItem.arrowObject.SetActive(false);
            shopUI.shopImgObj.SetActive(false);
            lastInteractedStoreItem = null;
        }
        if(lastSeenItem) lastSeenItem = null;
    }

    public void HourlyUpdate()
    {
        //update store at night. Change to perform when not in view of the player 
        if(TimeManager.Instance.currentHour == 8)
        {
            RefreshStore();
        }
    }
    
}
