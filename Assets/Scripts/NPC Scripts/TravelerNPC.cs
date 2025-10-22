using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TravelerNPC : NPC, ITalkable
{
    public float sellMultiplier = 1;
    public InventoryItemData[] possibleSoldItems;
    public float[] itemWeight; //likelyness of being sold, from 0 - 1
    List<StoreItem> storeItems = new List<StoreItem>();
    //WaypointScript shopUI;

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
            if (!GameSaveData.Instance.travMet)
            {
                currentPath = -1;
                currentType = PathType.Default;
                GameSaveData.Instance.travMet = true;
            }
            else
            {
                if(CompletedQuest()) //ADD UNIQUE FUNCTION TO GIVE UNIQUE DIALOGUE THAT IS QUEST DEPENDENT
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
                else if (NPCManager.Instance.travSpoke)
                {
                    
                    int i = Random.Range(0, dialogueText.alreadySpoken.Length);
                    currentPath = i;
                    currentType = PathType.AlreadySpoken;
                }
                else if(!GameSaveData.Instance.tra_askedForFood && GameSaveData.Instance.siegesCleared > 0) //Give knife quest
                {
                    GameSaveData.Instance.tra_askedForFood = true;
                    currentPath = 0;
                    currentType = PathType.Quest;
                    QuestManager.Instance.AddQuest(QuestDatabase.Instance.UniqueGrowQuests[2]); //Add the "Grow Tuber Quest" quest
                }
                else if (currentPath == -1)
                {
                    int i = Random.Range(0, dialogueText.fillerPaths.Length);
                    currentPath = i;
                    NPCManager.Instance.travSpoke = true;
                    currentType = PathType.Filler;
                }
               
            }
        }
        Talk();
        interactSuccessful = true;
    }

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

    public int QuestCompletedDialogue() //Reference lastCompletedQuestIndex to get which quest it is/what type it is, and give specific remarks here!!
    {
        if(lastCompletedQuestIndex < 0)
        {
            return 0;
        }

        //Remark about completing the knife/tuber quest here
        if(QuestManager.Instance.CompareQuests(QuestManager.Instance.activeQuests[lastCompletedQuestIndex], QuestDatabase.Instance.UniqueGrowQuests[2])) return 1;

        return 0;
    }

    public override bool ExclamationCheck()
    {
        if(base.ExclamationCheck() == false)
        {
            if(!GameSaveData.Instance.tra_askedForFood && !GameSaveData.Instance.kukriObtained)
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

}
