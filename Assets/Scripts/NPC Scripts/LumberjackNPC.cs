using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LumberjackNPC : NPC, ITalkable
{
    //public InventoryItemData key, paleCarrot;

    public float sellMultiplier = 1;
    public InventoryItemData[] possibleSoldItems;
    public float[] itemWeight; //likelyness of being sold, from 0 - 1
    List<StoreItem> storeItems = new List<StoreItem>();
    WaypointScript shopUI;

    void Awake()
    {
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
        if(dialogueController.IsTalking() == false)
        {
            if(GameSaveData.Instance.rascalMentionedKey && !GameSaveData.Instance.lumber_choppedTree)
            {
                if(!GameSaveData.Instance.lumber_offersDeal)
                {
                    GameSaveData.Instance.lumber_offersDeal = true;
                    currentPath = 0;
                    currentType = PathType.Quest;
                }
                else if(!GameSaveData.Instance.lumber_choppedTree)
                {
                    if(!startedDialogue)
                    {
                        //Asks if the player wants to hand over the money
                        currentPath = 2;
                        currentType = PathType.Quest;
                    }
                    else if(PlayerInteraction.Instance.currentMoney >= 200)
                    {
                        //Takes money
                        PlayerInteraction.Instance.currentMoney -= 200;
                        currentPath = 3;
                        currentType = PathType.Quest;
                        GameSaveData.Instance.lumber_choppedTree = true;
                        print("I took ur money");
                        anim.SetTrigger("TakeItem");
                    }
                    else
                    {
                        currentPath = 1;
                        currentType = PathType.Quest;
                    }
                }
            }
            else
            {
                if(NPCManager.Instance.lumberjackSpoke)
                {
                    interactSuccessful = false;
                    return;
                }
                if(currentPath == -1)
                {
                    int i = Random.Range(0, dialogueText.fillerPaths.Length);
                    currentPath = i;
                    NPCManager.Instance.lumberjackSpoke = true;
                }
                currentType = PathType.Filler;
            }
        }
        Talk();
        interactSuccessful = true;
    }

    public void Talk()
    {
        anim.SetTrigger("IsTalking");
        movementHandler.TalkToPlayer();
        dialogueController.currentTalker = this;
        dialogueController.DisplayNextParagraph(dialogueText, currentPath, currentType);
        startedDialogue = true;
    }

    public override void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        if(dialogueController.IsInterruptable() == false)
        {
            interactSuccessful = true;
            return;
        } 

        else if(item.staminaValue > 0)
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

    public override void OnConvoEnd()
    {
        currentPath = -1;
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

}

