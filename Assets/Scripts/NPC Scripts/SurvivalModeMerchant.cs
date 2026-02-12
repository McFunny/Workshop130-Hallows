using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SurvivalModeMerchant : NPC, ITalkable
{
    private InventoryItemData lastSeenItem;
    private SurvivalMechantBarterDatabase survivalBarterDatabase;

    public float sellMultiplier = 1;
    public StoreItem[] storeItems;

    ItemDisplaySign displaySign;

    [SerializeField] private List<InventoryItemData> allowedShopItems = new List<InventoryItemData>();
    [SerializeField] private List<InventoryItemData> starterItems = new List<InventoryItemData>();
    [SerializeField] private List<InventoryItemData> tierOneItems = new List<InventoryItemData>();
    [SerializeField] private List<InventoryItemData> tierTwoItems = new List<InventoryItemData>();
    [SerializeField] private List<InventoryItemData> tierThreeItems = new List<InventoryItemData>();

    private List<InventoryItemData> purchasedShopItems = new List<InventoryItemData>();


    //Find a way to get feedback on when a dialogue tree is finished by calling an event/delegate.

    void Start()
    {
        shopUI = FindObjectOfType<WaypointScript>();
        survivalBarterDatabase = barterDatabase as SurvivalMechantBarterDatabase;
        allowedShopItems.AddRange(starterItems);
        StartCoroutine(DelayedStart());
        TimeManager.OnHourlyUpdate += HourlyUpdate;
        for(int i = 0; i < storeItems.Length; i++)
        {
            storeItems[i].seller = this;
        }

        if (displaySign) displaySign.UpdateNPCName(this);

       

    }

    public void AddItemsToAllowedItems(int tier)
    {
        switch (tier)
        {
            case 1:
                allowedShopItems.AddRange(tierOneItems);
                break;
            case 2:
                allowedShopItems.AddRange(tierTwoItems);
                break;
            case 3:
                allowedShopItems.AddRange(tierThreeItems);
                break;
            default:
                break;
        }
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(2);

        //Reenable this when survival mode is fixed because rn its broken
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
                SurvivalModeManager.Instance.TotalMintsEarned += (int)(slot.StackSize * (slot.ItemData.value * slot.ItemData.sellValueMultiplier));
            }
            Talk();
        }
        interactSuccessful = true;
    }

    public override void PurchaseSuccess(InventoryItemData item, out bool uniqueDialogue)
    {
        uniqueDialogue = false;
        if(!GameSaveData.Instance.boughtItemIDs.Contains(item.ID)) GameSaveData.Instance.boughtItemIDs.Add(item.ID);
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

            //First 6 items are always there
            if (x < 6)
            {
                item.seller = this;

                newItem = survivalBarterDatabase.alwaysThere[x].itemForSale;
                item.RefreshItem(newItem, survivalBarterDatabase.alwaysThere[x].mintCost, survivalBarterDatabase.alwaysThere[x].itemsRequired, survivalBarterDatabase.alwaysThere[x].amountForSale);

                x++;
                continue;
            }

            //Next 3 are random seed items
            //No repeats and make sure they are allowed items
            else if (x < 9)
            {
                do
                {

                    i = Random.Range(0, survivalBarterDatabase.seeds.Count);
                    r = Random.Range(0f, 100f);
                    if (r < survivalBarterDatabase.seeds[i].barterChance && !selectedTrades.Contains(survivalBarterDatabase.seeds[i].itemForSale.ID) && allowedShopItems.Contains(survivalBarterDatabase.seeds[i].itemForSale))
                    {
                        newItem = survivalBarterDatabase.seeds[i].itemForSale;
                        selectedTrades.Add(survivalBarterDatabase.seeds[i].itemForSale.ID);
                    }
                }
                while (!newItem);
                newCost = (int)(survivalBarterDatabase.seeds[i].mintCost * sellMultiplier);
                item.RefreshItem(newItem, newCost, survivalBarterDatabase.seeds[i].itemsRequired, survivalBarterDatabase.seeds[i].amountForSale);
                item.seller = this;

                x++;
            }
            //Next 3 are random structure items
            //No repeats and make sure they are allowed items
            else if (x < 12)
            {
                do
                {

                    i = Random.Range(0, survivalBarterDatabase.structures.Count);
                    r = Random.Range(0f, 100f);

                    if (r < survivalBarterDatabase.structures[i].barterChance && !selectedTrades.Contains(survivalBarterDatabase.structures[i].itemForSale.ID) && allowedShopItems.Contains(survivalBarterDatabase.structures[i].itemForSale))
                    {
                        newItem = survivalBarterDatabase.structures[i].itemForSale;
                        selectedTrades.Add(survivalBarterDatabase.structures[i].itemForSale.ID);
                    }
                }
                while (!newItem);
                newCost = (int)(survivalBarterDatabase.structures[i].mintCost * sellMultiplier);
                item.RefreshItem(newItem, newCost, survivalBarterDatabase.structures[i].itemsRequired, survivalBarterDatabase.structures[i].amountForSale);
                item.seller = this;
                x++;
            }
            //Next 3 are random furniture items
            //No repeats and make sure they are allowed items
            else if (x < 15)
            {
                do
                {
                    i = Random.Range(0, survivalBarterDatabase.furniture.Count);
                    r = Random.Range(0f, 100f);
                    if (r < survivalBarterDatabase.furniture[i].barterChance && !selectedTrades.Contains(survivalBarterDatabase.furniture[i].itemForSale.ID))
                    {
                        newItem = survivalBarterDatabase.furniture[i].itemForSale;
                        selectedTrades.Add(survivalBarterDatabase.furniture[i].itemForSale.ID);
                    }
                }
                while (!newItem);
                newCost = (int)(survivalBarterDatabase.furniture[i].mintCost * sellMultiplier);
                item.RefreshItem(newItem, newCost, survivalBarterDatabase.furniture[i].itemsRequired, survivalBarterDatabase.furniture[i].amountForSale);
                item.seller = this;
                x++;
            }
            //Last 5 are special items
            else if (x < 20)
            {
                if (!GameSaveData.Instance.boughtItemIDs.Contains(survivalBarterDatabase.specialObjs[x - 15].itemForSale.ID))//(!purchasedShopItems.Contains(survivalBarterDatabase.specialObjs[x - 15].itemForSale))
                {
                    newItem = survivalBarterDatabase.specialObjs[x - 15].itemForSale;
                    item.RefreshItem(newItem, survivalBarterDatabase.specialObjs[x - 15].mintCost, survivalBarterDatabase.specialObjs[x - 15].itemsRequired, survivalBarterDatabase.specialObjs[x - 15].amountForSale);
                    item.seller = this;
                }
                else //Get a random trinket instead
                {
                    do
                    {
                        i = Random.Range(0, survivalBarterDatabase.trinkets.Count);
                        r = Random.Range(0f, 100f);
                        if (r < survivalBarterDatabase.trinkets[i].barterChance)
                        {
                            newItem = survivalBarterDatabase.trinkets[i].itemForSale;
                            selectedTrades.Add(survivalBarterDatabase.trinkets[i].itemForSale.ID);
                        }
                    }
                    while (!newItem);
                    newCost = (int)(survivalBarterDatabase.trinkets[i].mintCost * sellMultiplier);
                    item.RefreshItem(newItem, newCost, survivalBarterDatabase.trinkets[i].itemsRequired, survivalBarterDatabase.trinkets[i].amountForSale);
                    item.seller = this;
                }
                x++;
            }
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
