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

    public bool CheckForQuest(Quest q) //Finds if the current quest is already in the active quests list
    {
        for(int i = 0; i < activeQuests.Count; i++)
        {
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

    public int FindSameQuest(Quest q) //Finds if the current quest is already in the active quests list and returns the index
    {
        for(int i = 0; i < activeQuests.Count; i++)
        {
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

    public void AddQuestProgress(int amount)
    {

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

    public void SaveQuestData(out Quest[] s_activeQuests, out FetchQuest[] s_activeFetchQuests, out HuntQuest[] s_activeHuntQuests, out GrowQuest[] s_activeGrowQuests)
    {
        List<Quest> aQuestList = new List<Quest>();
        List<FetchQuest> fQuestList = new List<FetchQuest>();
        List<HuntQuest> hQuestList = new List<HuntQuest>();
        List<GrowQuest> gQuestList = new List<GrowQuest>();

        int i = 0;
        foreach(Quest q in activeQuests)
        {
            q.savedRewardIDs.Clear();
            for(int x = 0; x < q.itemRewards.Count; x++) //Saved item id's
            {
                q.savedRewardIDs.Add(q.itemRewards[x].ID);
            }
            //var type = q.GetType();
            FetchQuest fQ = q as FetchQuest;
            HuntQuest hQ = q as HuntQuest;
            GrowQuest gQ = q as GrowQuest;

            if(fQ != null && !fQ.alreadyCompleted)
            {
                fQ.objectID = fQ.desiredItem.ID;
                fQ.orderIndex = i;
                fQuestList.Add(fQ);
            }
            else if(hQ != null && !hQ.alreadyCompleted)
            {
                hQ.objectID = hQ.targetCreature.id;
                hQ.orderIndex = i;
                hQuestList.Add(hQ);
            }
            else if(gQ != null && !gQ.alreadyCompleted)
            {
                gQ.objectID = gQ.desiredCrop.id;
                gQ.objectID2 = gQ.desiredItem.ID;
                gQ.orderIndex = i;
                gQuestList.Add(gQ);
            }
            else if(!q.alreadyCompleted || q.isMajorQuest)
            {
                q.orderIndex = i;
                aQuestList.Add(q);
            }

            i++;
        }
        //go thru completed quests too

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
                    for(int x = 0; x < q.savedRewardIDs.Count; x++) //Saved item id's
                    {
                        q.itemRewards.Add(Database.Instance.GetItem(q.savedRewardIDs[x]));
                    }

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
    //public int townFavorReward;

    public int progress = 0;
    public int maxProgress; //Just because its at max progress does NOT mean a quest is completed. You still need to check in with the assignee if there is one
    public int daysLeft = -1; //if -1, there is no time limit. Will need to setup this with the new day function to tick these down by 1 and then remove them later. Unimplemented

    public Character assignee; //use an enum to keep track of NPCs, and fill that in here

    public bool displayProgress = false; //if set to false, should hide the progess bar in the codex// BY DEFAULT, IF MAX PROGRESS IS 0, THE BAR SHOULD BE HIDDEN

    [HideInInspector] public int orderIndex; //What order is this quest on the codex?

    [HideInInspector] public int objectID = -1; //The ID of the saved creature, item, crop, ect
    [HideInInspector] public int objectID2 = -1; //The ID of another saved creature, item, crop, ect
    [HideInInspector] public List<int> savedRewardIDs = new List<int>(); //The ID of the item rewards
    public int questID = -1; //The ID of this quest in the database. Used only by main quests

    public Quest(){}

    public Quest(Quest q) //Initialize a new quest based on a reference
    {
        name = q.name;
        description = q.description;
        //type = q.type;
        isMajorQuest = q.isMajorQuest;
        mintReward = q.mintReward;
        itemRewards = q.itemRewards;
        maxProgress = q.maxProgress;
        daysLeft = q.daysLeft;
        assignee = q.assignee;
        displayProgress = q.displayProgress;
        questID = q.questID;
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
}

/*[System.Serializable]
public class MiscQuest: Quest //For odd things like delivering an item or paying money to an npc
{
    public CreatureObject targetCreature;
    public int amount;

    public HuntQuest(CreatureObject _targetCreature, int _amount)
    {
        targetCreature = _targetCreature;
        amount = _amount;
    }
}*/

/*public enum QuestType
{
    Main,
    Collect,
    Tinkerer,
    Hunt
}*/
