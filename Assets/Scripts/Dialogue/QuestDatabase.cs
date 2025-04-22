using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestDatabase", menuName = "Databases/QuestDatabase")]
public class QuestDatabase : ScriptableObject
{

    /////////////////CAMS STUFF///////////////////
    [Header("ALWAYS UPDATE ID'S AND NEVER REORDER")]
    public Quest[] MainQuests; 
    //can make other lists for specific quest types later
    [ContextMenu("Update ID's")]
    public void UpdateID()
    {
        for(int i = 0; i < MainQuests.Length; i++)
        {
            MainQuests[i].questID = i;
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

    /*public void RefreshQuests() //Bad temp solution. Ideally, quests given from here should be a new instance of a quest
    {
        for(int i = 0; i < MainQuests.Length; i++)
        {
            MainQuests[i].progress = 0;
            MainQuests[i].alreadyCompleted = false;
        }
    }*/

    public Quest GetMainQuest(int id)
    {
        return new Quest(MainQuests[id]);
    }
}
