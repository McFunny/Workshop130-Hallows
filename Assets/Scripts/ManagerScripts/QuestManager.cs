using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    public List<Quest> activeQuests = new List<Quest>();

    //public List<Quest> completedQuests = new List<Quest>();

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else Instance = this;

    }

    void OnEnable()
    {
        StructureBehaviorScript.OnStructureDestroyed += StructureDestroyedEvent;
        TimeManager.OnHourlyUpdate += HourUpdate;
    }

    void OnDisable()
    {
        StructureBehaviorScript.OnStructureDestroyed -= StructureDestroyedEvent;
        TimeManager.OnHourlyUpdate -= HourUpdate;
    }

    public void AddQuest(Quest q)
    {
        if(!CheckForQuest(q))
        {
            activeQuests.Add(q);
            PopupHandler.Instance.AddToQueue(PopupHandler.Instance.newQuestPopup);
        }
    }

    public void ForceCompleteQuest(Quest q) //Compares ID's to see if the quest given is an active quest. If so, mark it as done
    {
        int questFoundID = FindSameQuest(q);
        if(questFoundID > -1)
        {
            activeQuests[questFoundID].progress = activeQuests[questFoundID].maxProgress;
            activeQuests[questFoundID].alreadyCompleted = true;
            print("Quest was successfully completed");
        }
        else print("Quest Not Found");

        /*for(int i = 0; i < activeQuests.Count; i++)
        {
            if(activeQuests[i].name == q.name || (activeQuests[i].questID == q.questID && activeQuests[i].questID != -1))
            {
                activeQuests[i].progress = activeQuests[i].maxProgress;
                activeQuests[i].alreadyCompleted = true;
                print("Quest was successfully completed");
                return;
            }
        }*/
    }

    public void ForceRemoveQuest(Quest q)
    {
        int questFoundID = FindSameQuest(q);
        if(questFoundID > -1)
        {
            activeQuests.Remove(activeQuests[questFoundID]);
            print("Quest was successfully removed");
        }
        else print("Quest Not Found");
        /*for(int i = 0; i < activeQuests.Count; i++)
        {
            if(activeQuests[i].name == q.name || (activeQuests[i].questID == q.questID && activeQuests[i].questID != -1))
            {
                activeQuests.Remove(activeQuests[i]);
                print("Quest was successfully removed");
                return;
            }
        }*/
    }

    public bool CheckForQuest(Quest q) //Finds if the current quest is already in the active quests list //SAME AS FINDSAMEQUEST
    {
        for(int i = 0; i < activeQuests.Count; i++)
        {
            if(activeQuests[i].assignee != q.assignee) continue;

            FetchQuest fQ = activeQuests[i] as FetchQuest;
            HuntQuest hQ = activeQuests[i] as HuntQuest;
            GrowQuest gQ = activeQuests[i] as GrowQuest;

            if(fQ != null && (q as FetchQuest) != null && fQ.desiredItem == (q as FetchQuest).desiredItem)
            {
                return true;
            }
            else if(hQ != null && (q as HuntQuest) != null && hQ.targetCreature == (q as HuntQuest).targetCreature)
            {
                return true;
            }
            else if(gQ != null && (q as GrowQuest) != null && gQ.desiredCrop == (q as GrowQuest).desiredCrop)
            {
                return true;
            }

            if(q.isMajorQuest && (activeQuests[i].name == q.name || (activeQuests[i].questID == q.questID && activeQuests[i].questID != -1))) //If not major quest, then dont check ID's
            {
                return true;
            }
        }
        return false;
    }

    public int FindSameQuest(Quest q) //Finds if the current quest is already in the active quests list and returns the index //SAME AS CHECKFORQUEST
    {
        for(int i = 0; i < activeQuests.Count; i++)
        {
            if(activeQuests[i].assignee != q.assignee) continue;

            FetchQuest fQ = activeQuests[i] as FetchQuest;
            HuntQuest hQ = activeQuests[i] as HuntQuest;
            GrowQuest gQ = activeQuests[i] as GrowQuest;

            if(fQ != null && (q as FetchQuest) != null && fQ.desiredItem == (q as FetchQuest).desiredItem)
            {
                return i;
            }
            else if(hQ != null && (q as HuntQuest) != null && hQ.targetCreature == (q as HuntQuest).targetCreature)
            {
                return i;
            }
            else if(gQ != null && (q as GrowQuest) != null && gQ.desiredCrop == (q as GrowQuest).desiredCrop)
            {
                return i;
            }

            if(q.isMajorQuest && (activeQuests[i].name == q.name || (activeQuests[i].questID == q.questID && activeQuests[i].questID != -1))) //If not major quest, then dont check ID's
            {
                return i;
            }
        }
        return -1;
    }

    public bool CompareQuests(Quest q1, Quest q2) //Finds if the 2 quests are the same
    {
        if(q1.assignee != q2.assignee) return false;

        FetchQuest fQ1 = q1 as FetchQuest;
        HuntQuest hQ1 = q1 as HuntQuest;
        GrowQuest gQ1 = q1 as GrowQuest;

        if(fQ1 != null)
        {
            FetchQuest fQ2 = q2 as FetchQuest;
            if(fQ2 != null && fQ1.desiredItem == fQ2.desiredItem) return true;
        }
        else if(hQ1 != null)
        {
            HuntQuest hQ2 = q2 as HuntQuest;
            if(hQ2 != null && hQ1.targetCreature == hQ2.targetCreature) return true;
        }
        else if(gQ1 != null)
        {
            GrowQuest gQ2 = q2 as GrowQuest;
            if(gQ2 != null && gQ1.desiredCrop == gQ2.desiredCrop) return true;
        }

        if(q1.isMajorQuest && q2.isMajorQuest)
        {
            if((q1.name == q2.name || (q1.questID == q2.questID && q1.questID != -1))) return true;
        }
        return false;
    }

    public bool DuplicateAssignees(Character name)
    {
        for(int i = 0; i < activeQuests.Count; i++)
        {
            if(activeQuests[i].assignee == name && !activeQuests[i].alreadyCompleted) return true;
        }
        return false;
    }

    public bool CheckForFinishedNPCQuest(Character name) //Checks if the player completed a quest and needs to return it to the person
    {
        for(int i = 0; i < activeQuests.Count; i++)
        {
            if(activeQuests[i].assignee == name && !activeQuests[i].alreadyCompleted && activeQuests[i].progress == activeQuests[i].maxProgress && activeQuests[i].progress != 0) return true;
        }
        return false;
    }

    public void AddQuestProgress(int amount, Quest q)
    {
        int questFoundID = FindSameQuest(q);
        if(questFoundID > -1 && activeQuests[questFoundID].progress < activeQuests[questFoundID].maxProgress)
        {
            activeQuests[questFoundID].progress += amount;
            if(activeQuests[questFoundID].progress > activeQuests[questFoundID].maxProgress) activeQuests[questFoundID].progress = activeQuests[questFoundID].maxProgress;
            if(activeQuests[questFoundID].progress == activeQuests[questFoundID].maxProgress)
            {
                PopupHandler.Instance.AddToQueue(PopupHandler.Instance.questCompletePopup);
            }
        }
    }

    //This is probably bad practice, and should be changed into using Unity Events instead
    public void CreatureDeath(CreatureObject c)
    {
        for(int i = 0; i < activeQuests.Count; i++)
        {
            HuntQuest hQuest = activeQuests[i] as HuntQuest;
            if(hQuest == null) continue;
            
            if(hQuest.targetCreature == c && hQuest.progress != hQuest.maxProgress)
            {
                hQuest.progress++;
                if(hQuest.progress == hQuest.maxProgress)
                {
                    PopupHandler.Instance.AddToQueue(PopupHandler.Instance.questCompletePopup);
                }
                return;
            }
        }
    }

    public void CropHarvested(CropData c)
    {
        for(int i = 0; i < activeQuests.Count; i++)
        {
            GrowQuest gQuest = activeQuests[i] as GrowQuest;
            if(gQuest == null) continue;
            
            if(gQuest.desiredCrop == c && gQuest.progress != gQuest.maxProgress)
            {
                gQuest.progress++;
                if(gQuest.progress >= gQuest.maxProgress)
                {
                    PopupHandler.Instance.AddToQueue(PopupHandler.Instance.questCompletePopup);
                }
                return;
            }
        }
    }

    public void HourUpdate()
    {
        for(int i = 0; i < activeQuests.Count; i++)
        {
            Quest q = activeQuests[i];
            if(q.questBehavior && !q.alreadyCompleted && q.progress != q.maxProgress) q.questBehavior.HourUpdate(q);
        }
    }

    public void StructureDestroyedEvent(StructureObject structData, Vector3 pos)
    {
        //Tigger virtual functions in active quests
        for(int i = 0; i < activeQuests.Count; i++)
        {
            Quest q = activeQuests[i];
            if(q.questBehavior && !q.alreadyCompleted && q.progress != q.maxProgress) q.questBehavior.StructureDestroyedEvent(structData, q);
        }
    }

    public void SaveQuestData(out Quest[] s_activeQuests, out FetchQuest[] s_activeFetchQuests, out HuntQuest[] s_activeHuntQuests, out GrowQuest[] s_activeGrowQuests)
    {
        List<Quest> aQuestList = new List<Quest>();
        List<FetchQuest> fQuestList = new List<FetchQuest>();
        List<HuntQuest> hQuestList = new List<HuntQuest>();
        List<GrowQuest> gQuestList = new List<GrowQuest>();

        int i = 0;
        foreach(Quest q in activeQuests)
        {
            if(q.alreadyCompleted && !q.isMajorQuest) //Removes all non main quests that are completed
            {
                i++;
                continue;
            }

            q.savedRewardIDs.Clear();
            for(int x = 0; x < q.itemRewards.Count; x++) //Saved item id's
            {
                q.savedRewardIDs.Add(q.itemRewards[x].ID);
            }

            if(q.questBehavior) q.behaviorID = q.questBehavior.id;
            else q.behaviorID = -1;

            FetchQuest fQ = q as FetchQuest;
            HuntQuest hQ = q as HuntQuest;
            GrowQuest gQ = q as GrowQuest;

            if(fQ != null)
            {
                fQ.objectID = fQ.desiredItem.ID;
                fQ.orderIndex = i;
                fQuestList.Add(fQ);
            }
            else if(hQ != null)
            {
                hQ.objectID = hQ.targetCreature.id;
                hQ.orderIndex = i;
                hQuestList.Add(hQ);
            }
            else if(gQ != null)
            {
                gQ.objectID = gQ.desiredCrop.id;
                gQ.objectID2 = gQ.desiredItem.ID;
                gQ.orderIndex = i;
                gQuestList.Add(gQ);
            }
            else
            {
                q.orderIndex = i;
                aQuestList.Add(q);
            }

            i++;
        }

        s_activeQuests = aQuestList.ToArray();
        s_activeFetchQuests = fQuestList.ToArray();
        s_activeHuntQuests = hQuestList.ToArray();
        s_activeGrowQuests = gQuestList.ToArray();
    }

    public void LoadData(AllGameSaveData data)
    {
        activeQuests.Clear();
        //completedQuests.Clear();

        List<Quest> tempList = new List<Quest>();

        tempList.AddRange(data.activeQuests);
        tempList.AddRange(data.activeFetchQuests);
        tempList.AddRange(data.activeHuntQuests);
        tempList.AddRange(data.activeGrowQuests);

        int i = 0;
        while(i < tempList.Count)
        {
            foreach(Quest q in tempList)
            {
                if(q.orderIndex == i)
                {
                    q.itemRewards.Clear(); //To remove the leftover scriptable object data
                    for(int x = 0; x < q.savedRewardIDs.Count; x++) //Saved item id's
                    {
                        if(q.savedRewardIDs[x] != -1) q.itemRewards.Add(Database.Instance.GetItem(q.savedRewardIDs[x]));
                    }

                    q.questBehavior = QuestDatabase.Instance.GetQuestBehavior(q.behaviorID);

                    FetchQuest fQ = q as FetchQuest;
                    HuntQuest hQ = q as HuntQuest;
                    GrowQuest gQ = q as GrowQuest;

                    if(fQ != null && fQ.objectID != -1)
                    {
                        fQ.desiredItem = Database.Instance.GetItem(fQ.objectID);
                        activeQuests.Add(fQ);
                    }
                    else if(hQ != null && hQ.objectID != -1)
                    {
                        hQ.targetCreature = CreatureDatabase.Instance.GetCreature(hQ.objectID);
                        activeQuests.Add(hQ);
                    }
                    else if(gQ != null && gQ.objectID != -1)
                    {
                        gQ.desiredCrop = CropDatabase.Instance.GetCrop(gQ.objectID);
                        gQ.desiredItem = Database.Instance.GetItem(gQ.objectID2);
                        activeQuests.Add(gQ);
                    }
                    else activeQuests.Add(q);
                }
            }
            i++;
        }

        //
    }
}

[System.Serializable]
public class Quest
{
    //A quest is completed by either achieving its goal (main quests) or telling an npc it is done (sub quests)
    //If its a subquest, it will use the progress variables to determine if it is done. Main quest stuff is on a case by case basis currently
    //Later I should add a thing to randomize stuff, such as random crop type, random amount, and a multiplier for the money earned
    public string name; //NEVER CHANGE THE NAME OF THIS FOR MAIN QUESTS, OR ELSE IT WILL MAKE SAVE FILES CORRUPT
    [TextArea(5,10)]
    public string description; //Use the same method I used in the dialogue controller to parse the code in the strings
    //public QuestType type; //Dont worry about this, currently unnused
    public bool isMajorQuest = false;
    public bool alreadyCompleted = false; //if you want to store completed quests, or just store completed main quests.
    public int mintReward;
    public List<InventoryItemData> itemRewards = new List<InventoryItemData>();
    public int townFavorReward;

    public int progress = 0;
    public int maxProgress; //Just because its at max progress does NOT mean a quest is completed. You still need to check in with the assignee if there is one
    public int daysLeft = -1; //if -1, there is no time limit. Will need to setup this with the new day function to tick these down by 1 and then remove them later. Unimplemented

    public Character assignee; //use an enum to keep track of NPCs, and fill that in here

    public bool displayProgress = false; //if set to false, should hide the progess bar in the codex// BY DEFAULT, IF MAX PROGRESS IS 0, THE BAR SHOULD BE HIDDEN

    [HideInInspector] public int orderIndex; //What order is this quest on the codex?

    [HideInInspector] public int objectID = -1; //The ID of the saved creature, item, crop, ect
    [HideInInspector] public int objectID2 = -1; //The ID of another saved creature, item, crop, ect
    [HideInInspector] public List<int> savedRewardIDs = new List<int>(); //The ID of the item rewards
    [HideInInspector] public int behaviorID = -1; //The ID of the behavior associated with the quest
    [HideInInspector] public QuestBehavior questBehavior; //The Behavior Object of the quest to handle special interactions
    public int questID = -1; //The ID of this quest in the database. Used only by main quests

    public Quest()
    {
        daysLeft = -1;
        questID = -1;
        behaviorID = -1;
    }

    public Quest(Quest q) //Initialize a new quest based on a reference
    {
        name = q.name;
        description = q.description;
        isMajorQuest = q.isMajorQuest;
        mintReward = q.mintReward;
        itemRewards = q.itemRewards;
        maxProgress = q.maxProgress;
        daysLeft = q.daysLeft;
        assignee = q.assignee;
        displayProgress = q.displayProgress;
        questID = q.questID;

        if(q.questBehavior) questBehavior = q.questBehavior;
    }

    public Quest(QuestTemplate q) //Initialize a new quest based on a reference
    {
        name = q.name;
        description = q.description;
        if(q.itemRewards.Count == 0) mintReward = (int)q.mintMultiplier; //Money Reward
        else
        {
            int i = Random.Range(0, q.itemRewards.Count);
            if(!q.itemRewards[i].item || q.itemRewards[i].amount <= 0) mintReward = (int)q.mintMultiplier; //Money Reward
            else for(int x = 0; x < q.itemRewards[i].amount; x++) itemRewards.Add(q.itemRewards[i].item); //Item Reward
        }
        maxProgress = Random.Range(q.minObject, q.maxObject); //dictates how much progress is needed
        if(maxProgress <= 0) maxProgress = 1;
        if(q.daysLeftMin <= 0) daysLeft = -1;
        else daysLeft = Random.Range(q.daysLeftMin, q.daysLeftMax);
        assignee = q.assignee;
        displayProgress = q.displayProgress;

        if(q.questBehavior) questBehavior = q.questBehavior;
    }

}
[System.Serializable]
public class FetchQuest: Quest //Should hide progress, and max progress should be 0
{
    public InventoryItemData desiredItem; //MUST SAVE ID
    public int amount;

    public FetchQuest(InventoryItemData _desiredItem, int _amount)
    {
        desiredItem = _desiredItem;
        amount = _amount;
    }

    public FetchQuest(QuestTemplate q) //Initialize a new quest based on a reference
    {
        name = q.name;
        description = q.description;
        //isMajorQuest = q.isMajorQuest;
        desiredItem = q.item;
        amount = Random.Range(q.minObject, q.maxObject);
        if(q.itemRewards.Count == 0) mintReward = Mathf.RoundToInt(q.item.value * q.mintMultiplier * q.item.sellValueMultiplier * amount); //Money Reward
        else
        {
            int i = Random.Range(0, q.itemRewards.Count);
            if(!q.itemRewards[i].item || q.itemRewards[i].amount <= 0) mintReward = Mathf.RoundToInt(q.item.value * q.mintMultiplier * q.item.sellValueMultiplier * amount); //Money Reward
            else for(int x = 0; x < q.itemRewards[i].amount; x++) itemRewards.Add(q.itemRewards[i].item); //Item Reward
        }
        maxProgress = amount;
        if(q.daysLeftMin <= 0) daysLeft = -1;
        else daysLeft = Random.Range(q.daysLeftMin, q.daysLeftMax);
        assignee = q.assignee;
        displayProgress = q.displayProgress;

        if(q.questBehavior) questBehavior = q.questBehavior;
        else maxProgress = 0; //Remember that fetch quests dont track progress, so only have max progress be tracked if there is a behavior modifying that progress
    }
}

[System.Serializable]
public class HuntQuest: Quest //max progress should be amount
{
    public CreatureObject targetCreature; //MUST SAVE ID
    public int amount;

    public HuntQuest(CreatureObject _targetCreature, int _amount)
    {
        targetCreature = _targetCreature;
        amount = _amount;
    }

    public HuntQuest(QuestTemplate q) //Initialize a new quest based on a reference
    {
        name = q.name;
        description = q.description;
        //isMajorQuest = q.isMajorQuest;
        targetCreature = q.creature;
        amount = Random.Range(q.minObject, q.maxObject);
        if(q.itemRewards.Count == 0) mintReward = Mathf.RoundToInt(q.creature.mintWorth * q.mintMultiplier * amount); //Money Reward
        else
        {
            int i = Random.Range(0, q.itemRewards.Count);
            if(!q.itemRewards[i].item || q.itemRewards[i].amount <= 0) mintReward = Mathf.RoundToInt(q.creature.mintWorth * q.mintMultiplier * amount); //Money Reward
            else for(int x = 0; x < q.itemRewards[i].amount; x++) itemRewards.Add(q.itemRewards[i].item); //Item Reward
        }
        maxProgress = amount;
        if(q.daysLeftMin <= 0) daysLeft = -1;
        else daysLeft = Random.Range(q.daysLeftMin, q.daysLeftMax);
        assignee = q.assignee;
        displayProgress = q.displayProgress;
        
        if(q.questBehavior) questBehavior = q.questBehavior;
    }
}

[System.Serializable]
public class GrowQuest: Quest //max progress should be amount
{
    public CropData desiredCrop; //MUST SAVE ID
    public InventoryItemData desiredItem;
    public int amount;

    public GrowQuest(InventoryItemData _desiredItem, int _amount, CropData _desiredCrop)
    {
        desiredItem = _desiredItem;
        amount = _amount;
        desiredCrop = _desiredCrop;
    }

    public GrowQuest(QuestTemplate q) //Initialize a new quest based on a reference
    {
        name = q.name;
        description = q.description;
        //isMajorQuest = q.isMajorQuest;
        desiredCrop = q.crop;
        desiredItem = q.crop.cropYield;
        amount = Random.Range(q.minObject, q.maxObject);
        if(q.itemRewards.Count == 0) mintReward = Mathf.RoundToInt(desiredItem.value * q.mintMultiplier * desiredItem.sellValueMultiplier * amount); //Money Reward
        else
        {
            int i = Random.Range(0, q.itemRewards.Count);
            if(!q.itemRewards[i].item || q.itemRewards[i].amount <= 0) mintReward = Mathf.RoundToInt(desiredItem.value * q.mintMultiplier * desiredItem.sellValueMultiplier * amount); //Money Reward
            else for(int x = 0; x < q.itemRewards[i].amount; x++) itemRewards.Add(q.itemRewards[i].item); //Item Reward
        }
        maxProgress = amount;
        if(q.daysLeftMin <= 0) daysLeft = -1;
        else daysLeft = Random.Range(q.daysLeftMin, q.daysLeftMax);
        assignee = q.assignee;
        displayProgress = q.displayProgress;

        if(q.questBehavior) questBehavior = q.questBehavior;
    }
}

//(Should probably make a new quest archetype for break structure quest)

/*public enum QuestType
{
    Main,
    Collect,
    Tinkerer,
    Hunt
}*/
