using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WagonMerchantNPC : NPC, ITalkable
{
    private InventoryItemData lastSeenItem;
    public InventoryItemData barricade, shotGun, ammo, carrot, carrotSeeds;
    [HideInInspector] public bool interactedWithLantern;
    bool remembersGift; //if true and the player tries to sell barricades, he gets mad
    bool metPlayerAtEntrace = false; //resets at new day
    bool talkingOutsideWagon = false; //if the merchant is talking outside of his wagon
    public AudioClip scareSound;
    public Transform townEntrancePos;
    public Transform merchantWagonPos;
    public Transform merchant;

    public MerchantLantern lantern;

    //public Animator anim;

    public float sellMultiplier = 1;
    public InventoryItemData[] possibleSoldItems;
    public float[] itemWeight; //likelyness of being sold, from 0 - 1
    public StoreItem[] storeItems;
    //WaypointScript shopUI;
    public ItemDisplaySign displaySign;

    [TextArea(5,10)]
    public string[] carrotComments;

    //Find a way to get feedback on when a dialogue tree is finished by calling an event/delegate.

    void Start()
    {
        shopUI = FindObjectOfType<WaypointScript>();
        RefreshStore();
        TimeManager.OnHourlyUpdate += HourlyUpdate;
        for(int i = 0; i < storeItems.Length; i++)
        {
            storeItems[i].seller = this;
        }

        if(lantern)
        {
            lantern.merchant = this;
            if(!GameSaveData.Instance.wildernessIntroduced && PlayerInteraction.Instance.totalMoneyEarned > 1000) lantern.EnableSelf();
        }

        if (displaySign) displaySign.UpdateNPCName(this);

    }

    void OnDestroy()
    {
        TimeManager.OnHourlyUpdate -= HourlyUpdate;
    }

    public override void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        if(dialogueController.IsTalking() == false && dialogueController.FreeToSpeak(this))
        {
            if(!GameSaveData.Instance.wildernessIntroduced && PlayerInteraction.Instance.totalMoneyEarned > 1000) //Open Wilderness
            {
                currentPath = 8;
                currentType = PathType.Misc;
                GameSaveData.Instance.wildernessIntroduced = true;
                lantern.EnableSelf();
            }
            else if(CompletedQuest())
            {
                currentPath = 0;
                currentType = PathType.QuestComplete;
            }
            else if(!GameSaveData.Instance.mm_giveBarricade && !PlayerInventoryHolder.Instance.IsInventoryFull())
            {
                GameSaveData.Instance.mm_giveBarricade = true;
                currentPath = 11;
                currentType = PathType.Misc;
                itemsToGive.Add(new ItemWithAmount(barricade, 2));
                remembersGift = true;
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
        ToolItem tItem = item as ToolItem;
        if(dialogueController.IsInterruptable() == false || tItem)
        {
            interactSuccessful = false;
            //if(dialogueController.FreeToSpeak(this))Talk();
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
            print("I Ran");
            if(lastSeenItem != item)
            {
                print("I have not seen this item yet");
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
                print("Repeated item");
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
    }

    public override void RefreshStore()
    {
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
            //metPlayerAtEntrace = false;
        }
    }

    public void LanternInteraction()
    {
        if(dialogueController.IsTalking() == false)
        {
            if(!GameSaveData.Instance.wildernessIntroduced && PlayerInteraction.Instance.totalMoneyEarned > 1000) //Open Wilderness
            {
                currentPath = 8;
                currentType = PathType.Misc;
                GameSaveData.Instance.wildernessIntroduced = true;
            }
            else //Are you sure you want to go/You cannot go yet
            {
                if(TimeManager.Instance.currentHour >= 17 || !TimeManager.Instance.isDay || WildernessManager.Instance.visitedWilderness)
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
            itemsToGive.Add(new ItemWithAmount(ammo, 6));
            //QuestManager.Instance.AddQuest(QuestDatabase.Instance.MainQuests[1]);
        }
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
        if(currentPath == 11) PopupHandler.Instance.AddToQueue(PopupHandler.Instance.bedTutorialPopup);
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
    
}
