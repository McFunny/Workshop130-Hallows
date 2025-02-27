using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TavernNPC : NPC, ITalkable
{
    List<Quest> possibleQuests = new List<Quest>();
    public List<FetchQuest> possibleFetchQuests;
    public List<HuntQuest> possibleHuntQuests;
    public List<GrowQuest> possibleGrowQuests;

    public List<Character> possibleFetchAssignees, possibleHuntAssignees, possibleGrowAssignees;

    protected override void Awake() //Awake in NPC.cs assigns the dialoguecontroller
    {
        base.Awake();
        movementHandler = GetComponent<NPCMovement>();
        faceCamera = GetComponent<FaceCamera>();
        faceCamera.enabled = false;
    }

    void Start()
    {
        possibleQuests.AddRange(possibleFetchQuests);
        possibleQuests.AddRange(possibleHuntQuests);
        possibleQuests.AddRange(possibleGrowQuests);
    }

    public override void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        if (dialogueController.IsTalking() == false) //Makes sure to not interrupt an existing dialogue branch
        {
            if (!GameSaveData.Instance.barMet) //Introduction Check
            {
                currentPath = -1;
                currentType = PathType.Default;
                GameSaveData.Instance.barMet = true;
            }
            else if(CompletedQuest())
            {
                currentPath = 0;
                currentType = PathType.QuestComplete;
            }
            /*else if (movementHandler.isWorking) //Working Dialogue
            {
                currentPath = 0;
                currentType = PathType.Misc;
            }*/
            else if (NPCManager.Instance.barkeepSpoke) //Say nothing if already given flavor text
            {
                interactSuccessful = false;
                return;
            }
            else if (currentPath == -1) //Give 1 daily flavor text
            {
                if(/*Random.Range(0, 10) >= 6 &&*/ CanGiveQuest())
                {
                    GiveQuest();
                    int i = Random.Range(0, dialogueText.questPaths.Length);
                    currentPath = i;
                    currentType = PathType.Quest;
                    NPCManager.Instance.barkeepSpoke = true;
                }
                else
                {
                    int i = Random.Range(0, dialogueText.fillerPaths.Length);
                    currentPath = i;
                    currentType = PathType.Filler;
                    NPCManager.Instance.barkeepSpoke = true;
                }
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
        if (dialogueController.IsInterruptable() == false || tItem)
        {
            interactSuccessful = false;
            Talk();
            return;
        }

        if(CompletedQuestWithItem())
        {
            currentPath = 0;
            currentType = PathType.QuestComplete;
        }


        else if (item.staminaValue > 0)
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

    void GiveQuest()
    {
       // QuestManager.Instance.activeQuests.Add(possibleQuests[Random.Range(0, possibleQuests.Count)]);
        QuestManager.Instance.AddQuest(possibleQuests[Random.Range(0, possibleQuests.Count)]);
        int questNum = QuestManager.Instance.activeQuests.Count - 1;//To grab the newly added quest

        FetchQuest f = QuestManager.Instance.activeQuests[questNum] as FetchQuest;

        if(f != null) QuestManager.Instance.activeQuests[questNum].assignee = possibleFetchAssignees[Random.Range(0, possibleFetchAssignees.Count)];

        HuntQuest h = QuestManager.Instance.activeQuests[questNum] as HuntQuest;

        if(h != null) QuestManager.Instance.activeQuests[questNum].assignee = possibleHuntAssignees[Random.Range(0, possibleHuntAssignees.Count)];

        GrowQuest g = QuestManager.Instance.activeQuests[questNum] as GrowQuest;

        if(g != null) QuestManager.Instance.activeQuests[questNum].assignee = possibleGrowAssignees[Random.Range(0, possibleGrowAssignees.Count)];
    }

    bool CanGiveQuest()
    {
        int subQuestsActive = 0;
        foreach(Quest q in QuestManager.Instance.activeQuests)
        {
            if(q.isMajorQuest == false && q.alreadyCompleted == false) subQuestsActive++;
        }
        if(subQuestsActive < 3) return true;
        else return false;
    }


    public override void BeginWorking()
    {
    }

    public override void StopWorking()
    {
    }
}
