using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotanistNPC : NPC, ITalkable
{
    public InventoryItemData fertalizerT, fertalizerG, fertalizerI;
    public InventoryItemData s_carrot, s_tuber, s_drake, s_stalk, s_bean, s_ginger, s_spores; //seeds

    public float sellMultiplier = 1;
    public InventoryItemData[] possibleSoldItems;
    public InventoryItemData[] commonSeeds, rareSeeds, fertalizers;
    public float[] itemWeight; //likelyness of being sold, from 0 - 1
    List<StoreItem> storeItems = new List<StoreItem>();
    WaypointScript shopUI;

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
        if(dialogueController.IsTalking() == false) //Makes sure to not interrupt an existing dialogue branch
        {
            if(!GameSaveData.Instance.botMet) //Introduction Check
            {
                currentPath = -1;
                currentType = PathType.Default;
                GameSaveData.Instance.botMet = true;
            }
            else if(movementHandler.isWorking) //Working Dialogue
            {
                currentPath = 0;
                currentType = PathType.Misc;
            }
            else if(NPCManager.Instance.botanistSpoke) //Say nothing if already given flavor text
            {
                interactSuccessful = false;
                return;
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

    public void Talk() //progress what they are saying or start new conversation
    {
        anim.SetTrigger("IsTalking");
        movementHandler.TalkToPlayer();
        dialogueController.currentTalker = this;
        dialogueController.DisplayNextParagraph(dialogueText, currentPath, currentType);
    }

    public override void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        ToolItem tItem = item as ToolItem;
        if(dialogueController.IsInterruptable() == false || tItem)
        {
            interactSuccessful = false;
            Talk();
            return;
        } 

        if(item == fertalizerI || item == fertalizerT || item == fertalizerG)
        {
            currentPath = 1;
            currentType = PathType.ItemSpecific;
        }

        else if(IsItemASeed(item) > -1)
        {
            currentPath = IsItemASeed(item);
            currentType = PathType.ItemSpecific;
        }

        else if(item.staminaValue > 0)
        {
            currentPath = 0;
            currentType = PathType.ItemRecieved;
            /*if(!NPCManager.Instance.botanistFed)
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
            }*/
            //Its consumable and giftable
        }
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
            
        }
        currentType = PathType.Misc;
        Talk();
    }

    public override void PlayerLeftRadius()
    {
        if(lastInteractedStoreItem)
        {
            lastInteractedStoreItem = null;
        }
        shopUI.shopImgObj.SetActive(false);
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

        List<InventoryItemData> commonSeedsForSale = new List<InventoryItemData>();
        while(commonSeedsForSale.Count < 2)
        {
            i = Random.Range(0, commonSeeds.Length);
            if(!commonSeedsForSale.Contains(commonSeeds[i])) commonSeedsForSale.Add(commonSeeds[i]);
        }

        foreach (StoreItem item in storeItems)
        {
            newItem = null;

            if(currentItem < 6)
            {
                i = Random.Range(0, commonSeedsForSale.Count);
                newItem = commonSeedsForSale[i];
            }
            else if(currentItem < 9) newItem = rareSeedForSale;
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

