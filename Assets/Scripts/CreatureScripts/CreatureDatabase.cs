using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(menuName = "Creature Database")]
public class CreatureDatabase : ScriptableObject
{
    private static CreatureDatabase _instance;

    public static CreatureDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                // Load the instance of the Database if not already set
                _instance = Resources.Load<CreatureDatabase>("CreatureDatabase");
            }
            return _instance;
        }
    }

    [SerializeField] private List<CreatureObject> _creatureDatabase; //DONT ALTER ORDER

    [ContextMenu("Update ID's")]
    public void UpdateID()
    {
        for(int i = 0; i < _creatureDatabase.Count; i++)
        {
            _creatureDatabase[i].id = i;
            #if UNITY_EDITOR

            if (_creatureDatabase[i]) EditorUtility.SetDirty(_creatureDatabase[i]);

            #endif
        }
        #if UNITY_EDITOR       
            AssetDatabase.SaveAssets();
        #endif
    }

    public CreatureObject GetCreature(int id) //USE THIS FOR GRABBING CREATURES WHEN SAVING AND LOADING
    {
        return _creatureDatabase.Find(i => i.id == id);
    }

    public void ResetStats()
    {
        for(int i = 0; i < _creatureDatabase.Count; i++)
        {
            _creatureDatabase[i].hasSpawned = false;
            _creatureDatabase[i].amountKilled = 0;
        }
    }

    public void SaveStats(out CreaturePlayerStats[] creatureStats)
    {
        List<CreaturePlayerStats> temp = new List<CreaturePlayerStats>();

        foreach(CreatureObject c in _creatureDatabase)
        {
            temp.Add(new CreaturePlayerStats(c.hasSpawned, c.amountKilled));
        }
        creatureStats = temp.ToArray();
    }

    public void LoadStats(AllGameSaveData data)
    {
        int i = 0;
        foreach(CreatureObject c in _creatureDatabase)
        {
            if(i >= data.creatureStats.Length) return;
            c.hasSpawned = data.creatureStats[i].hasSpawned;
            c.amountKilled = data.creatureStats[i].amountKilled;
            i++;
        }
    }

}

[System.Serializable]
public class CreaturePlayerStats
{
    public bool hasSpawned = false;
    public int amountKilled = 0;

    public CreaturePlayerStats(bool _spawned, int _killed)
    {
        hasSpawned = _spawned;
        amountKilled = _killed;
    }
}
