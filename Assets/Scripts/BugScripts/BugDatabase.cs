using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(menuName = "Bug Database")]
public class BugDatabase : ScriptableObject
{
    private static BugDatabase _instance;

    public static BugDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                // Load the instance of the Database if not already set
                _instance = Resources.Load<BugDatabase>("BugDatabase");
            }
            return _instance;
        }
    }

    [SerializeField] private List<BugObject> _bugDatabase; //DONT ALTER ORDER

    [ContextMenu("Update ID's")]
    public void UpdateID()
    {
        for(int i = 0; i < _bugDatabase.Count; i++)
        {
            _bugDatabase[i].id = i;
            #if UNITY_EDITOR

            if (_bugDatabase[i]) EditorUtility.SetDirty(_bugDatabase[i]);

            #endif
        }
        #if UNITY_EDITOR       
            AssetDatabase.SaveAssets();
        #endif
    }

    public BugObject GetBug(int id) //USE THIS FOR GRABBING CREATURES WHEN SAVING AND LOADING
    {
        return _bugDatabase.Find(i => i.id == id);
    }

    public void ResetStats()
    {
        for(int i = 0; i < _bugDatabase.Count; i++)
        {
            _bugDatabase[i].amountCaught = 0;
        }
    }

    /*public void SaveStats(out int[] bugStats) //NEEDS SAVING FUNCTIONALITY ADDED
    {
        List<int> temp = new List<int>();

        foreach(BugObject c in _bugDatabase)
        {
            temp.Add(c.amountCaught);
        }
        bugStats = temp.ToArray();
    }

    public void LoadStats(AllGameSaveData data)
    {
        int i = 0;
        foreach(BugObject c in _bugDatabase)
        {
            if(i >= data.bugStats.Count) return;
            c.amountCaught = data.bugStats[i];
            i++;
        }
    }*/

}
