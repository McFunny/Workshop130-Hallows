using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestDatabase", menuName = "Databases/QuestDatabase")]
public class QuestDatabase : ScriptableObject
{

    /////////////////CAMS STUFF///////////////////
    [Header("ALWAYS UPDATE ID'S AND NEVER REORDER")]
    public Quest[] MainQuests; 
    public Quest[] TutorialQuests; //id's are shifted by 300 to distinguish from main quests. If by some impossible metric we reach over 300 main quests, we got an issue

    public GrowQuest[] UniqueGrowQuests;
    //can make other lists for specific quest types later
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
    }
    //////////////////////////////////////////////

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

    public Quest GetMainQuest(int id)
    {
        return new Quest(MainQuests[id]);
    }

    public Quest GetTutorialQuest(int id)
    {
        return new Quest(TutorialQuests[id - 300]);
    }
}
