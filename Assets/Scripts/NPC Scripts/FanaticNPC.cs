using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FanaticNPC : NPC, ITalkable
{
    public float sellMultiplier = 1;
    List<StoreItem> storeItems = new List<StoreItem>();

    public InventoryItemData bathBomb;

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
            if (!GameSaveData.Instance.fanMet)
            {
                currentPath = -1;
                currentType = PathType.Default;
                GameSaveData.Instance.fanMet = true;
            }
            else if(!GameSaveData.Instance.fan_giveBombs && !PlayerInventoryHolder.Instance.IsInventoryFull())
            {
                GameSaveData.Instance.fan_giveBombs = true;
                currentPath = 5;
                currentType = PathType.Misc;
                itemsToGive.Add(new ItemWithAmount(bathBomb, 3));
                dailyQuest = null;
            }
            else
            {
                if (CompletedQuest())
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
                else if (NPCManager.Instance.fanSpoke)
                {
                    int i = Random.Range(0, dialogueText.alreadySpoken.Length);
                    currentPath = i;
                    currentType = PathType.AlreadySpoken;
                }
                if (currentPath == -1)
                {
                    int i = Random.Range(0, dialogueText.fillerPaths.Length);
                    currentPath = i;
                    NPCManager.Instance.fanSpoke = true;
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
        else if (item.ID == 163)
        {
            currentPath = 1;
            currentType = PathType.ItemSpecific;
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


    public override void PlayerLeftRadius()
    {
        if (lastInteractedStoreItem)
        {
            lastInteractedStoreItem = null;
        }
        if(movementHandler.isWorking) shopUI.shopImgObj.SetActive(false);
        base.PlayerLeftRadius();
    }

    public override void EmptyShopItem()
    {
        lastInteractedStoreItem.Empty();
        lastInteractedStoreItem = null;
    }

 

    public override void BeginWorking()
    {
        FindObjectOfType<Spa>().ActivateSpa();
        /*if (!assignedStall) return;
        storeItems = assignedStall.storeItems;
        RefreshStore();*/
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
    }

    public override bool ActionCheck1() //Spa Check
    {
        if(Random.Range(0,100) > 95) return true;
        else return false;
    }
}
