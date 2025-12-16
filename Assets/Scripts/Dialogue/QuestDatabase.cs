using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestDatabase", menuName = "Databases/QuestDatabase")]
public class QuestDatabase : ScriptableObject
{
    [Header("ALWAYS UPDATE ID'S AND NEVER REORDER")]
    public Quest[] MainQuests; 
    public Quest[] TutorialQuests; //id's are shifted by 300 to distinguish from main quests. If by some impossible metric we reach over 300 main quests, we got an issue

    public GrowQuest[] UniqueGrowQuests;

    public FetchQuest[] UniqueFetchQuests;

    public List<NPCQuestObject> npcQuestObjects = new List<NPCQuestObject>();

    //List of the behaviors
    public List<QuestBehavior> questBehaviors = new List<QuestBehavior>();

    [ContextMenu("Update ID's")]
    public void UpdateID()
    {
        for(int i = 0; i < MainQuests.Length; i++)
        {
            MainQuests[i].questID = i;
        }

        for(int i = 0; i < TutorialQuests.Length; i++)
        {
            TutorialQuests[i].questID = i + 300;
        }

        for(int i = 0; i < UniqueGrowQuests.Length; i++)
        {
            UniqueGrowQuests[i].questID = i;
        }

        for(int i = 0; i < UniqueFetchQuests.Length; i++)
        {
            UniqueFetchQuests[i].questID = i;
        }

        //Function to order the behaviors
        for(int i = 0; i < questBehaviors.Count; i++)
        {
            questBehaviors[i].id = i;
        }
    }


    private static QuestDatabase _instance;

    public static QuestDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                // Load the instance of the Database if not already set
                _instance = Resources.Load<QuestDatabase>("QuestDatabase");
            }
            return _instance;
        }
    }

    public int GetQuestPath(Character name)
    {
        foreach(NPCQuestObject questPool in npcQuestObjects)
        {
            if(questPool.character == name && questPool.currentDailyQuestPath >= 0)
            {
                return questPool.currentDailyQuestPath;
            }
        }
        return 0;
    }

    public Quest GetMainQuest(int id)
    {
        return new Quest(MainQuests[id]);
    }

    public Quest GetTutorialQuest(int id)
    {
        return new Quest(TutorialQuests[id - 300]);
    }

    public Quest GetDailyQuest(Character npcName)
    {
        if(npcName == Character.Null) return null;

        Quest chosenQuest = null;

        Debug.Log ("Accessing Quest Pool");

        foreach(NPCQuestObject questPool in npcQuestObjects)
        {
            if(questPool.character == npcName)
            {
                chosenQuest = questPool.GrabQuest();
                if(chosenQuest as GrowQuest != null) Debug.Log ("Its a grow quest");
                if(chosenQuest as HuntQuest != null) Debug.Log ("Its a hunt quest");
                if(chosenQuest as FetchQuest != null) Debug.Log ("Its a fetch quest");

                return chosenQuest;
            }
        }

        return null;
    }

    public QuestBehavior GetQuestBehavior(int id)
    {
        if(id == -1) return null;
        return questBehaviors[id];
    }

    [ContextMenu("Test Quest Get")]
    public void GetDailyQuestTest() //IT ACTUALLY WORKS!!!!!
    {
        Quest chosenQuest = npcQuestObjects[0].GrabSampleQuest();
        if(chosenQuest as GrowQuest != null) Debug.Log ("Its a grow quest");
        if(chosenQuest as HuntQuest != null) Debug.Log ("Its a hunt quest");
        if(chosenQuest as FetchQuest != null) Debug.Log ("Its a fetch quest");
    }

}
