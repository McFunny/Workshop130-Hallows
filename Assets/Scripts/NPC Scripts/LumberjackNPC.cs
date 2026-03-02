using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LumberjackNPC : NPC, ITalkable
{
    public InventoryItemData papers, treeNut, wood;

    public float sellMultiplier = 1;
    public InventoryItemData[] possibleSoldItems;
    public float[] itemWeight; //likelyness of being sold, from 0 - 1
    List<StoreItem> storeItems = new List<StoreItem>();
    //WaypointScript shopUI;

    public Quest treeQuest;

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
        if(dialogueController.IsTalking() == false && dialogueController.FreeToSpeak(this))
        {
            if(!GameSaveData.Instance.lumberMet)
            {
                currentPath = -1;
                currentType = PathType.Default;
                GameSaveData.Instance.lumberMet = true;
            }
            else if(GameSaveData.Instance.rascalMentionedKey && !GameSaveData.Instance.lumber_offersDeal)
            {
                GameSaveData.Instance.lumber_offersDeal = true; //He will now start selling his papers at his shop
                currentPath = 0;
                currentType = PathType.Quest;
                QuestManager.Instance.AddQuest(QuestDatabase.Instance.GetMainQuest(4));
                QuestManager.Instance.ForceRemoveQuest(QuestDatabase.Instance.GetMainQuest(3));

                /*else if(!GameSaveData.Instance.lumber_choppedTree)
                {
                    if(!startedDialogue)
                    {
                        //Asks if the player wants to hand over the money
                        currentPath = 2;
                        currentType = PathType.Quest;
                    }
                    else if(PlayerInteraction.Instance.currentMoney >= 400)
                    {
                        //Takes money
                        PlayerInteraction.Instance.currentMoney -= 400;
                        currentPath = 3;
                        currentType = PathType.Quest;
                        GameSaveData.Instance.lumber_choppedTree = true;
                        print("I took ur money");
                        anim.SetTrigger("TakeItem");

                        QuestManager.Instance.ForceCompleteQuest(treeQuest);
                    }
                    else
                    {
                        currentPath = 1;
                        currentType = PathType.Quest;
                    }
                }*/
            }
            else
            {
                if(CompletedQuest())
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
                else if(NPCManager.Instance.lumberjackSpoke)
                {
                    int i = Random.Range(0, dialogueText.alreadySpoken.Length);
                    currentPath = i;
                    currentType = PathType.AlreadySpoken;
                }
                if(currentPath == -1)
                {
                    int i = Random.Range(0, dialogueText.fillerPaths.Length);
                    currentPath = i;
                    NPCManager.Instance.lumberjackSpoke = true;
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
        if(dialogueController.IsInterruptable() == false || tItem || !dialogueController.FreeToSpeak(this))
        {
            interactSuccessful = false;
            return;
        } 

        if(CompletedQuestWithItem())
        {
            currentPath = 0;
            currentType = PathType.QuestComplete;
        }

        else if(item == papers)
        {
            currentPath = 1;
            currentType = PathType.ItemSpecific;
            AchievementManager.Instance.CompleteProgressWithEnum(ACHKey.Lumberjack_Paper);
        }

        else if(item == treeNut)
        {
            currentPath = 2;
            currentType = PathType.ItemSpecific;
        }
        else if (item.ID == 163)
        {
            currentPath = 3;
            currentType = PathType.ItemSpecific;
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
                if(item.itemData == papers) currentPath = 5; //papers sold
                else currentPath = 2; //item sold
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
        if (lastInteractedStoreItem)
        {
            lastInteractedStoreItem = null;
        }
        if(movementHandler.isWorking) shopUI.shopImgObj.SetActive(false);
        base.PlayerLeftRadius();
    }

    /*public override void EmptyShopItem() //For when a player bought smth
    {
        if(!lastInteractedStoreItem.clearUponPurchase) return;
        lastInteractedStoreItem.Empty();
        lastInteractedStoreItem = null;
    }*/

    public override void RefreshStore()
    {
        //if(lastInteractedStoreItem) lastInteractedStoreItem.arrowObject.SetActive(false);
        if(lastInteractedStoreItem) shopUI.shopImgObj.SetActive(false);
        lastInteractedStoreItem = null;
        int i;
        float r;
        InventoryItemData newItem;
        int x = 0; //iterations
        foreach (StoreItem item in storeItems)
        {
            if(x == 0) newItem = papers;
            else
            {
                newItem = wood;
                item.clearUponPurchase = false;
            }
            
            int newCost = (int) (newItem.value * sellMultiplier);
            item.RefreshItem(newItem, newCost);
            item.seller = this;

            x++;
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

    public override bool ActionCheck1() //To check if he starts selling papers
    {
        if(GameSaveData.Instance.lumber_offersDeal) return true;
        return false;
    }

    public override bool ActionCheck2() //To check if he should stay at his shop for longer
    {
        if(GameSaveData.Instance.lumber_choppedTree || !GameSaveData.Instance.lumber_offersDeal) return false;
        return true;
    }
}

