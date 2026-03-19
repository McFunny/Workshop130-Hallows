using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FanaticNPC : NPC, ITalkable
{
    public float sellMultiplier = 1;
    List<StoreItem> storeItems = new List<StoreItem>();

    public InventoryItemData bathBomb, loamTrinket;

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
            else if(GameSaveData.Instance.apo_wasKidnapped && !GameSaveData.Instance.fan_ApoGoneComment)
            {
                currentPath = 6;
                currentType = PathType.Misc;
                GameSaveData.Instance.fan_ApoGoneComment = true;
                NPCManager.Instance.fanSpoke = true;

            }
            else if(!GameSaveData.Instance.fan_giveBombs && !PlayerInventoryHolder.Instance.IsInventoryFull())
            {
                GameSaveData.Instance.fan_giveBombs = true;
                currentPath = 5;
                currentType = PathType.Misc;
                itemsToGive.Add(new ItemWithAmount(bathBomb, 3));
                dailyQuest = null;
                NPCManager.Instance.fanSpoke = true;
            }
            else if (AbleToGiveTrinketQuest())
            {
                int questIndex = 1 + GameSaveData.Instance.trinketSlotsGiven;
                //QuestManager.Instance.AddQuest(QuestDatabase.Instance.GetTutorialQuest(305));
                QuestManager.Instance.AddQuest(QuestDatabase.Instance.UniqueFetchQuests[questIndex]);
                currentPath = GameSaveData.Instance.trinketSlotsGiven;
                currentType = PathType.Quest;
                dailyQuest = null;
            }
            else
            {
                if (CompletedQuest())
                {
                    currentPath = QuestCompletedDialogue();
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

    public int QuestCompletedDialogue() //Reference lastCompletedQuestIndex to get which quest it is/what type it is, and give specific remarks here!!
    {
        if(lastCompletedQuestIndex < 0)
        {
            return 0;
        }

        int questIndex = GameSaveData.Instance.trinketSlotsGiven;

        //Remark about completing the bug trinket quest here
        if(GameSaveData.Instance.trinketSlotsGiven < 3 && QuestManager.Instance.CompareQuests(QuestManager.Instance.activeQuests[lastCompletedQuestIndex], QuestDatabase.Instance.UniqueFetchQuests[questIndex]))
        {
            if(GameSaveData.Instance.trinketSlotsGiven == 1) 
            {
                itemsToGive.Add(new ItemWithAmount(loamTrinket, 1));
                return 1;
            }
            //GameSaveData.Instance.trinketSlotsGiven++; //Handled in the item behavior

            return GameSaveData.Instance.trinketSlotsGiven;
        }

        return 0;
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
            currentPath = QuestCompletedDialogue();
            currentType = PathType.QuestComplete;
        }
        else if (item.ID == 163)
        {
            currentPath = 1;
            currentType = PathType.ItemSpecific;
        }

        else
        {
            currentPath = RemarkOnItem(item);
            if(currentPath >= 0)
            {
                currentType = PathType.ItemPath;
            }
            else
            {
                currentPath = 0;
                currentType = PathType.ItemSpecific;
            }
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

    bool AbleToGiveTrinketQuest()
    {
        int questIndex = 1 + GameSaveData.Instance.trinketSlotsGiven;

        if(GameSaveData.Instance.trinketSlotsGiven != 0) return false; //Fanatic only gives 1 quest now. Rest of the slots are bought from the MM 

        if(GameSaveData.Instance.trinketSlotsGiven >= 3) return false;

        if(GameSaveData.Instance.trinketSlotsGiven == 0) 
        {
            if(GameSaveData.Instance.ras_askedForNet && GameSaveData.Instance.tinkMet && QuestManager.Instance.FindSameQuest(QuestDatabase.Instance.UniqueFetchQuests[questIndex]) == -1) return true;
            else return false;
        }
        else if(GameSaveData.Instance.trinketSlotsGiven <= GameSaveData.Instance.siegesCleared && QuestManager.Instance.FindSameQuest(QuestDatabase.Instance.UniqueFetchQuests[questIndex]) == -1) return true;
        return false;
    }

    public override bool ExclamationCheck()
    {
        if(base.ExclamationCheck() == false)
        {
            if(AbleToGiveTrinketQuest())
            {
                exclamationObject.SetActive(true);
                return true;
            }
            else
            {
                exclamationObject.SetActive(false);
                return false;
            }
        }
        exclamationObject.SetActive(true);
        return true;
    }

    /*bool AbleToCompleteTrinketQuest()
    {
        if(GameSaveData.Instance.cul_gaveCrock) return false;
        int questNum = QuestManager.Instance.FindSameQuest(QuestDatabase.Instance.GetTutorialQuest(304));
        if(questNum == -1) return false;
        if(QuestManager.Instance.activeQuests[questNum].progress >= QuestManager.Instance.activeQuests[questNum].maxProgress)
        {
            QuestManager.Instance.activeQuests[questNum].alreadyCompleted = true;
            return true;
        }

        return false;
    }*/

    public override bool ActionCheck1() //Spa Check
    {
        if(Random.Range(0,100) > 95) return true;
        else return false;
    }
}
