using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RascalNPC : NPC, ITalkable
{
    public InventoryItemData key, paleCarrot;

    public float sellMultiplierMin, sellMultiplierMax;
    public InventoryItemData[] possibleSoldItems;
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
        anim.SetBool("IsLeaning", true);
        shopUI = FindObjectOfType<WaypointScript>();
    }

    public override void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        if(dialogueController.IsTalking() == false)
        {
            if(!GameSaveData.Instance.rascalMet)
            {
                currentPath = -1;
                currentType = PathType.Default;
                GameSaveData.Instance.rascalMet = true;
            }
            else if(!GameSaveData.Instance.rascalWantsFood)
            {
                currentPath = 1;
                currentType = PathType.Quest;
                GameSaveData.Instance.rascalWantsFood = true; 
            }
            else
            {
                if(NPCManager.Instance.rascalSpoke)
                {
                    interactSuccessful = false;
                    return;
                }
                if(currentPath == -1)
                {
                    int i = Random.Range(0, dialogueText.fillerPaths.Length);
                    currentPath = i;
                }
                //currentPath = -1;
                currentType = PathType.Filler;
                NPCManager.Instance.rascalSpoke = true;
            }
        }
        Talk();
        interactSuccessful = true;
    }

    public void Talk()
    {
        //anim.SetTrigger("IsTalking");
        dialogueController.currentTalker = this;
        dialogueController.DisplayNextParagraph(dialogueText, currentPath, currentType);
    }

    public override void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        ToolItem tItem = item as ToolItem;
        if(dialogueController.IsInterruptable() == false || tItem)
        {
            interactSuccessful = false;
            return;
        } 

        if(item == paleCarrot && GameSaveData.Instance.rascalWantsFood == true && GameSaveData.Instance.rascalMentionedKey == false)
        {
            currentPath = 2;
            currentType = PathType.Quest;
            GameSaveData.Instance.rascalMentionedKey = true;
            //anim.SetTrigger("TakeItem");
        }

        else if(item == key)
        {
            currentPath = 1;
            currentType = PathType.ItemSpecific;
        }
        else if(item.staminaValue > 0)
        {
            currentPath = 0;
            currentType = PathType.ItemRecieved;
            /*
            if(!NPCManager.Instance.rascalFed)
            {
                currentPath = 0;
                currentType = PathType.ItemRecieved;
                NPCManager.Instance.rascalFed = true;
                //anim.SetTrigger("TakeItem");
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
            else if(PlayerInventoryHolder.Instance.IsInventoryFull())
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
            float sellMultiplier = Random.Range(sellMultiplierMin, sellMultiplierMax);
            int newCost = (int) (newItem.value * sellMultiplier);
            item.RefreshItem(newItem, newCost);
            item.seller = this;
        }
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

    public override bool ActionCheck1()
    {
        if(GameSaveData.Instance.rascalWantsFood /*&& Random.Range(0,10) > 4*/) return true;
        return false;
    }

}
