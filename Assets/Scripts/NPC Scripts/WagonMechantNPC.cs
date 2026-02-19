using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WagonMerchantNPC : NPC, ITalkable
{
    private InventoryItemData lastSeenItem;
    public InventoryItemData barricade, shotGun, ammo, carrot, carrotSeeds, inventoryUpgrade, burntFood, rockPet, rockItem;
    [HideInInspector] public bool interactedWithLantern;
    bool remembersGift; //if true and the player tries to sell barricades, he gets mad
    bool metPlayerAtEntrace = false; //resets at new day
    bool talkingOutsideWagon = false; //if the merchant is talking outside of his wagon
    public AudioClip scareSound;
    public Transform townEntrancePos;
    public Transform merchantWagonPos;
    public Transform merchant;

    public MerchantLantern lantern;

    int wildernessPrice = 50;
    int wildernessUnlockThreshold = 2000;

    bool gaveFiller = false;


    public float sellMultiplier = 1;
    public InventoryItemData[] possibleSoldItems;
    public float[] itemWeight; //likelyness of being sold, from 0 - 1
    public StoreItem[] storeItems;

    public InventoryItemData[] soldPetItems;
    public InventoryItemData[] possibleSoldCritterItems;
    public float[] critterItemWeight; //likelyness of being sold, from 0 - 1
    public StoreItem[] storeCritterItems;

    public ItemDisplaySign displaySign;

    public float[] ticketThresholds;
    float mintsBeforeSale; // tracks how many mints player had before selling item to calculate how many mints were earned
    bool checkTicket;

    int buyRockAttempts = 0;

    void Start()
    {
        shopUI = FindObjectOfType<WaypointScript>();
        StartCoroutine(DelayedStart());
        TimeManager.OnHourlyUpdate += HourlyUpdate;
        for(int i = 0; i < storeItems.Length; i++)
        {
            storeItems[i].seller = this;
        }

        for(int i = 0; i < storeCritterItems.Length; i++)
        {
            storeCritterItems[i].seller = this;
        }

        /*if(lantern)
        {
            lantern.merchant = this;
            if(!GameSaveData.Instance.wildernessIntroduced && PlayerInteraction.Instance.totalMoneyEarned > wildernessUnlockThreshold) lantern.EnableSelf();
        }*/

        if (displaySign) displaySign.UpdateNPCName(this);

    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(2);
        RefreshStore();
    }

    void OnDestroy()
    {
        TimeManager.OnHourlyUpdate -= HourlyUpdate;
    }

    public override void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        if(dialogueController.IsTalking() == false && dialogueController.FreeToSpeak(this))
        {
            /*if(!GameSaveData.Instance.wildernessIntroduced && PlayerInteraction.Instance.totalMoneyEarned > wildernessUnlockThreshold) //Open Wilderness
            {
                currentPath = 8;
                currentType = PathType.Misc;
                GameSaveData.Instance.wildernessIntroduced = true;
                lantern.EnableSelf();
            }
            else*/ if(CompletedQuest())
            {
                currentPath = 0;
                currentType = PathType.QuestComplete;
            }
            else if(!GameSaveData.Instance.mm_giveBarricade && !PlayerInventoryHolder.Instance.IsInventoryFull())
            {
                GameSaveData.Instance.mm_giveBarricade = true;
                currentPath = 11;
                currentType = PathType.Misc;
                itemsToGive.Add(new ItemWithAmount(barricade, 4));
                remembersGift = true;
                gaveFiller = true;
            }
            else if(!gaveFiller)
            {
                if(GameSaveData.Instance.mm_sellOnlyRocks)
                {
                    currentPath = 23;
                    currentType = PathType.Misc;
                }
                else
                {
                    int i = Random.Range(0, dialogueText.fillerPaths.Length);
                    currentPath = i;
                    currentType = PathType.Filler;
                }
                gaveFiller = true;
            }
            else
            {
                currentPath = -1;
                currentType = PathType.Default;
            }
            lastSeenItem = null;
            interactedWithLantern = false;
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
        if(checkTicket) GiveTicketCheck();
        if(!GameSaveData.Instance.mm_giveBarricade && !PlayerInventoryHolder.Instance.IsInventoryFull()) //Make sure he gives the intro to the store before player can start selling
        {
            GameSaveData.Instance.mm_giveBarricade = true;
            currentPath = 11;
            currentType = PathType.Misc;
            itemsToGive.Add(new ItemWithAmount(barricade, 4));
            remembersGift = true;
            Talk();
            interactSuccessful = true;
            anim.SetTrigger("IsTalking");
            return;
        }

        else if (item.ID == 163) //Scroll
        {
            currentPath = 0;
            currentType = PathType.ItemSpecific;
            lastSeenItem = item;
            Talk();
            anim.SetTrigger("IsTalking");
        }

        else if (item as CropItem != null) //Seeds
        {
            currentPath = Random.Range(1,3);
            currentType = PathType.ItemSpecific;
            lastSeenItem = item;
            Talk();
            anim.SetTrigger("IsTalking");
        }

        else if (item == burntFood) //Burnt Food
        {
            currentPath = 3;
            currentType = PathType.ItemSpecific;
            lastSeenItem = item;
            Talk();
            anim.SetTrigger("IsTalking");
        }

        else if(item.sellValueMultiplier == 0 || item.value == 0 || item.sellValueMultiplier == 0)
        {
            //Cannot Buy
            lastSeenItem = item;
            currentPath = 1;
            currentType = PathType.Misc;
            Talk();

            anim.SetTrigger("IsTalking");
        }
        else if(remembersGift && item == barricade)
        {
            remembersGift = false;
            currentPath = 12;
            currentType = PathType.Misc;
            Talk();
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
                QuestManager.Instance.ForceCompleteQuest(QuestDatabase.Instance.GetMainQuest(13));
                checkTicket = true;
                mintsBeforeSale = PlayerInteraction.Instance.currentMoney;
            }
            Talk();
        }

        ToolItem tItem = item as ToolItem;
        if(dialogueController.IsInterruptable() == false || tItem)
        {
            interactSuccessful = false;
            return;
        } 
        interactSuccessful = true;
    }

    public override void PurchaseAttempt(StoreItem item)
    {
        if(checkTicket) GiveTicketCheck();
        if(dialogueController.IsInterruptable() == false)
        {
            Talk();
            return;
        }

        if(TimeManager.Instance.dayNum >= 3 && !GameSaveData.Instance.mm_introducedPets)
        {
            GameSaveData.Instance.mm_introducedPets = true;
            currentPath = 17;
        }

        else if(lastInteractedStoreItem == item)
        {
            //check price, then give item
            if(PlayerInteraction.Instance.currentMoney < lastInteractedStoreItem.cost)
            {
                currentPath = 6; //no money!?!?!?
            }
            else if(PlayerInventoryHolder.Instance.IsInventoryFull() && !item.itemData.cannotEnterInventory)
            {
                currentPath = 7; //No space in inventory
            }
            else //Item was sold
            {
                CritterItem c = item.itemData as CritterItem;
                if(c)
                {
                    if(c.petOverride) //item was a pet
                    {
                        if(GameSaveData.Instance.mm_soldPet)
                        {
                            currentPath = 16; //Not selling multiple pets
                            return;
                        }
                        else
                        {
                            if(item.itemData == rockPet)
                            {
                                switch(buyRockAttempts)
                                {
                                    case 0:
                                    currentPath = 20;
                                    break;
                                    case 1:
                                    currentPath = 21;
                                    break;
                                    case 2:
                                    currentPath = 22;
                                    break;
                                    default:
                                    currentPath = 15; //Pet sold
                                    GameSaveData.Instance.mm_sellOnlyRocks = true;
                                    break;
                                }
                                buyRockAttempts++;
                            }
                            else currentPath = 15; //Pet sold
                            //EmptyPetShop();
                        }
                    }
                    else //item was a critter
                    {
                        bool noHome = true;
                        foreach (StructureBehaviorScript structure in StructureManager.Instance.allStructs)
                        {
                            CritterPen pen = structure as CritterPen;
                            if(!pen) continue;
                            if(pen.type == c.homeType)
                            {
                                currentPath = 5; //item sold
                                noHome = false;
                                break;
                            }
                        }
                        if(noHome) currentPath = 18; //critter has no home
                    }
                    
                }
                else currentPath = 5; //item sold
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
    }

    /*public override void RefreshStore()
    {
        if(lastInteractedStoreItem) shopUI.shopImgObj.SetActive(false);
        lastInteractedStoreItem = null;
        int i;
        float r;
        int x = 0; //iterations
        InventoryItemData newItem;
        foreach (StoreItem item in storeItems)
        {
            newItem = null;
            if(GameSaveData.Instance.mm_sellOnlyRocks)
            {
                int rockCost = (int) (rockItem.value * sellMultiplier);
                item.RefreshItem(rockItem, rockCost);
                item.seller = this;
                x++;
                return;
            }
            do
            {
                i = Random.Range(0, possibleSoldItems.Length);
                r = Random.Range(0f,1f);
                if(r < itemWeight[i]) newItem = possibleSoldItems[i];

                if(x == 0 && !PlayerInteraction.Instance.playerUpgrades.gainedInventoryUpgrade) newItem = inventoryUpgrade;
                if(x == 1 && MainMenuScript.currentFileMode == FileMode.Cozy)
                {
                    newItem = ammo;

                    item.RefreshItem(newItem, (int) (newItem.value * sellMultiplier), new List<ItemWithAmount>(), 5);
                    item.seller = this;
                    x++;
                    continue;
                }
            }
            while(!newItem);
            int newCost = (int) (newItem.value * sellMultiplier);
            item.RefreshItem(newItem, newCost);
            item.seller = this;
            x++;
        }
        //For selling pets and critters
        if(TimeManager.Instance.dayNum < 3) return; //Wont give pets until third day
        x = 0;
        foreach (StoreItem item in storeCritterItems)
        {
            newItem = null;
            do
            {
                if(GameSaveData.Instance.mm_soldPet == false)
                {
                    if(x >= soldPetItems.Length) return;
                    newItem = soldPetItems[x];
                    continue;
                }
                else if(GameSaveData.Instance.townTreeCleared2 == false) return; //Wont sell if the barn is not unlocked

                i = Random.Range(0, possibleSoldCritterItems.Length);
                r = Random.Range(0f,1f);
                if(r < critterItemWeight[i]) newItem = possibleSoldCritterItems[i];
            }
            while(!newItem);
            int newCost = (int) (newItem.value * sellMultiplier);
            item.RefreshItem(newItem, newCost);
            item.seller = this;
            x++;
        }
        GameSaveData.Instance.mm_sellOnlyRocks = false;
    } */

    public override void RefreshStore()
    {
        if (lastInteractedStoreItem) shopUI.shopImgObj.SetActive(false);
        lastInteractedStoreItem = null;
        int newCost = 0;
        float r;
        int b;
        InventoryItemData newItem;

        List<int> selectedTrades = new List<int>(); //Make sure no repeats

        int extraItems = 0;

       
        for (int i = 0; i < storeItems.Length; i++)
        {
            //////

            newItem = null;
            if(GameSaveData.Instance.mm_sellOnlyRocks)
            {
                int rockCost = (int) (rockItem.value * sellMultiplier);
                storeItems[i].RefreshItem(rockItem, rockCost);
                storeItems[i].seller = this;
                return;
            }

            if(i == 0)
            {
                if(!PlayerInteraction.Instance.playerUpgrades.gainedInventoryUpgrade) 
                {
                    storeItems[i].RefreshItem(inventoryUpgrade, (int) inventoryUpgrade.value);
                    storeItems[i].seller = this;
                }
                else if(CanSellTrinketPouch())
                {
                    storeItems[i].RefreshItem(barterDatabase.uniqueTransactions[0].itemForSale, barterDatabase.uniqueTransactions[0].mintCost,
                        barterDatabase.uniqueTransactions[0].itemsRequired, barterDatabase.uniqueTransactions[0].amountForSale);
                    storeItems[i].seller = this;
                }
            }

            do
            {
                b = Random.Range(0, barterDatabase.transactions.Count);
                r = Random.Range(0f, 100f);
                if (r < barterDatabase.transactions[b].barterChance && barterDatabase.transactions[b].siegesRequired <= GameSaveData.Instance.siegesCleared)
                {
                    if(selectedTrades.Contains(b) && Random.Range(0, 10) > 4) continue; //Repeats are less likely but not impossible
                    newItem = barterDatabase.transactions[b].itemForSale;
                    selectedTrades.Add(b);
                }
            }
            while (!newItem);
            newCost = (int)(barterDatabase.transactions[b].mintCost * sellMultiplier);

            extraItems = 0;
            if(barterDatabase.transactions[b].amountForSale == 1) extraItems = Random.Range(0, 3);
            else if(barterDatabase.transactions[b].amountForSale == 3) extraItems = Random.Range(0, 6);
            if(GameSaveData.Instance.siegesCleared > 1 && extraItems > 0) extraItems += Random.Range(0, 4);

            storeItems[i].RefreshItem(newItem, newCost, barterDatabase.transactions[b].itemsRequired, barterDatabase.transactions[b].amountForSale + extraItems);
            storeItems[i].ChangeAmountGiven(barterDatabase.transactions[b].amountGiven);
            storeItems[i].seller = this;
        }

        //For selling pets and critters
        if(TimeManager.Instance.dayNum < 3) return; //Wont give pets until third day
        int x = 0;
        foreach (StoreItem item in storeCritterItems)
        {
            newItem = null;
            do
            {
                if(GameSaveData.Instance.mm_soldPet == false)
                {
                    if(x >= soldPetItems.Length) return;
                    newItem = soldPetItems[x];
                    continue;
                }
                else if(GameSaveData.Instance.townTreeCleared2 == false) return; //Wont sell if the barn is not unlocked

                int index = Random.Range(0, possibleSoldCritterItems.Length);
                r = Random.Range(0f,1f);
                if(r < critterItemWeight[index]) newItem = possibleSoldCritterItems[index];
            }
            while(!newItem);
            newCost = (int) (newItem.value * sellMultiplier);
            item.RefreshItem(newItem, newCost);
            item.seller = this;
            x++;
        }
        GameSaveData.Instance.mm_sellOnlyRocks = false;
    }

    /*public override void EmptyShopItem()
    {
        lastInteractedStoreItem.Empty();
        lastInteractedStoreItem = null;
    }*/

    void EmptyPetShop()
    {
        foreach (StoreItem item in storeCritterItems)
        {
            item.Empty();
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

        interactedWithLantern = false;
    }

    public void HourlyUpdate()
    {
        if(TimeManager.Instance.currentHour == 6)
        {
            metPlayerAtEntrace = false;
        }
        if(TimeManager.Instance.currentHour == 8)
        {
            RefreshStore();
            remembersGift = false;
            gaveFiller = false;
            //metPlayerAtEntrace = false;
        }
    }

    public void LanternInteraction()
    {
        if(dialogueController.IsTalking() == false)
        {
            if(!GameSaveData.Instance.wildernessIntroduced && PlayerInteraction.Instance.totalMoneyEarned > wildernessUnlockThreshold) //Open Wilderness
            {
                currentPath = 8;
                currentType = PathType.Misc;
                GameSaveData.Instance.wildernessIntroduced = true;
            }
            else //Are you sure you want to go/You cannot go yet
            {
                if(TimeManager.Instance.currentHour >= 17 || !TimeManager.Instance.isDay || WildernessManager.Instance.visitedWilderness) //Cannot go
                {
                    currentPath = 10;
                    currentType = PathType.Misc;
                }
                else if(!interactedWithLantern)
                {
                    currentPath = 9;
                    currentType = PathType.Misc;
                    interactedWithLantern = true;
                }
                else if(PlayerInteraction.Instance.currentMoney < wildernessPrice)
                {
                    currentPath = 6; //no money!?!?!?
                    currentType = PathType.Misc;
                }
                else
                {
                    StartCoroutine(TakeToWilderness());
                    return;
                }
            }
            lastSeenItem = null;
            dialogueController.SetInterruptable(false);

            anim.SetTrigger("IsTalking");
        }
        Talk();
    }

    IEnumerator TakeToWilderness()
    {
        //restrict movement and darken screen
        PlayerMovement.restrictMovementTokens++;
        FadeScreen.coverScreen = true;
        interactedWithLantern = false;
        PlayerInteraction.Instance.currentMoney -= wildernessPrice;
        yield return new WaitForSeconds(3);
        WildernessManager.Instance.EnterWilderness();
        FadeScreen.coverScreen = false;
        PlayerMovement.restrictMovementTokens--;
    }

    public void PlayerEnteredTown()
    {
        if(metPlayerAtEntrace) return;

        if(TimeManager.Instance.dayNum == 1 && (TimeManager.Instance.currentHour != 6 && TimeManager.Instance.currentHour != 7))
        {
            currentPath = 13;
            currentType = PathType.Misc;
        }
        else if(!GameSaveData.Instance.mm_giveGun && PlayerInventoryHolder.Instance.ReturnFreeSlots() >= 3)
        {
            int carrotsHeld = PlayerInventoryHolder.Instance.ReturnItemCountInPlayerInventory(carrot);
            if(carrotsHeld >= 8)
            {
                //bountiful harvest
                currentPath = 3;
                currentType = PathType.BranchingPaths;
            }
            else if(carrotsHeld >= 4)
            {
                //Decent
                currentPath = 2;
                currentType = PathType.BranchingPaths;
            }
            else if(carrotsHeld >= 1)
            {
                //Some
                currentPath = 1;
                currentType = PathType.BranchingPaths;
            }
            else
            {
                //No Harvest
                currentPath = 0;
                currentType = PathType.BranchingPaths;
                itemsToGive.Add(new ItemWithAmount(carrotSeeds, 4));
            }
            //currentPath = 14;
            //currentType = PathType.Misc;
            GameSaveData.Instance.mm_giveGun = true;
            itemsToGive.Add(new ItemWithAmount(shotGun, 1));
            if(MainMenuScript.currentFileMode == FileMode.Cozy) itemsToGive.Add(new ItemWithAmount(ammo, 20));
            else itemsToGive.Add(new ItemWithAmount(ammo, 10));
            //QuestManager.Instance.AddQuest(QuestDatabase.Instance.MainQuests[1]);
        }
        /*else if(!GameSaveData.Instance.wildernessIntroduced && PlayerInteraction.Instance.totalMoneyEarned > wildernessUnlockThreshold)
        {
            currentPath = 8;
            currentType = PathType.Misc;
            GameSaveData.Instance.wildernessIntroduced = true;
            lantern.EnableSelf();
        }*/
        else return;
        metPlayerAtEntrace = true;
        talkingOutsideWagon = true;
        AudioPoolManager.Instance.PlayClipAtPosition(scareSound, townEntrancePos.position);
        merchant.position = townEntrancePos.position;
        PlayerCam.Instance.NewObjectOfInterest(eyeLine.position);
        //dialogueController.SetInterruptable(false);
        dialogueController.restartDialogue = true;
        Talk();
        //StartCoroutine(WaitUntilDoneTalking());
    }

    public override void OnConvoEnd()
    {
        if(checkTicket) GiveTicketCheck();
        ////////////// Clear last seen item to stop player frustration at accidental purchasing
        if(lastInteractedStoreItem)
        {
            lastInteractedStoreItem = null;
        }
        if(lastSeenItem) lastSeenItem = null; 
        shopUI.shopImgObj.SetActive(false);
        //////////////

        if(currentPath == 11) //Finished store introduction
        {
           PopupHandler.Instance.AddToQueue(PopupHandler.Instance.bedTutorialPopup); 
           QuestManager.Instance.AddQuest(QuestDatabase.Instance.GetTutorialQuest(300)); //Add the "go buy seeds" quest
        }
        if(currentPath == 15) EmptyPetShop();

        if(!talkingOutsideWagon) return;
        QuestManager qm = QuestManager.Instance;
        if(currentType == PathType.BranchingPaths && !qm.CheckForQuest(QuestDatabase.Instance.GetMainQuest(2))) qm.AddQuest(QuestDatabase.Instance.GetMainQuest(1));
        talkingOutsideWagon = false;
        StartCoroutine(ReturnToWagon());
    }

    IEnumerator ReturnToWagon()
    {
        FadeScreen.coverScreen = true;
        PlayerMovement.restrictMovementTokens++;
        TimeManager.Instance.stopTime = false;
        yield return new WaitForSeconds(1.5f);
        merchant.position = merchantWagonPos.position;
        PlayerMovement.restrictMovementTokens--;
        FadeScreen.coverScreen = false;
    }

    [ContextMenu("Test")]
    public void Test()
    {
        print(PlayerInventoryHolder.Instance.ReturnItemCountInPlayerInventory(carrotSeeds));
    }

    void GiveTicketCheck()
    {
        checkTicket = false;
        float mintsEarned = PlayerInteraction.Instance.currentMoney - mintsBeforeSale;
        if(mintsEarned <= 0) return;
        GameSaveData sData = GameSaveData.Instance;

        sData.tTicketMintProgress += mintsEarned;

        int currentTier = CraftingDatabase.Instance.CurrentTier();

        //print("Mints Earned is " + mintsEarned);
        //print("Mint Progress is " + sData.tTicketMintProgress);
        //print("Current Tier is " + currentTier);

        if(currentTier == -1 || currentTier >= ticketThresholds.Length) return;

        if(sData.tTicketMintProgress >= ticketThresholds[currentTier])
        {
            while(sData.tTicketMintProgress >= ticketThresholds[currentTier])
            {
                sData.tTicketMintProgress -= ticketThresholds[currentTier];
                GameSaveData.Instance.tTicketsAvailable++;

                currentTier = CraftingDatabase.Instance.CurrentTier();
                if(currentTier == -1 || currentTier >= ticketThresholds.Length) return;
            }
        }
    }

    public void InteractWithTicketBox()
    {
        if(dialogueController.IsTalking() == true || GameSaveData.Instance.mm_introducedTickets || !GameSaveData.Instance.mm_giveBarricade) return;

        GameSaveData.Instance.mm_introducedTickets = true;

        currentPath = 19;
        currentType = PathType.Misc;
        lastSeenItem = null;
        dialogueController.SetInterruptable(false);
        anim.SetTrigger("IsTalking");
        Talk();
    }

    bool CanSellTrinketPouch()
    {
        if(GameSaveData.Instance.trinketSlotsGiven == 0 || GameSaveData.Instance.trinketSlotsGiven >= 3) return false;
        if(PlayerInteraction.Instance.playerUpgrades.gainedInventoryUpgrade) return false;

        if(GameSaveData.Instance.trinketSlotsGiven > GameSaveData.Instance.siegesCleared) return false;
        return true;
    }
    
}
