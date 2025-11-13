using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(menuName = "Crafting Database")]
public class CraftingDatabase : ScriptableObject
{
    private static CraftingDatabase _instance;

    public static CraftingDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                // Load the instance of the Database if not already set
                _instance = Resources.Load<CraftingDatabase>("Crafting Database");
            }
            return _instance;
        }
    }

    [SerializeField] private List<CraftingEntry> _craftingDatabase; //DONT ALTER ORDER

    [ContextMenu("Update ID's")]
    public void UpdateID()
    {
        for(int i = 0; i < _craftingDatabase.Count; i++)
        {
            _craftingDatabase[i].id = i;
            #if UNITY_EDITOR

            if (_craftingDatabase[i]) EditorUtility.SetDirty(_craftingDatabase[i]);

            #endif
        }
        #if UNITY_EDITOR       
            AssetDatabase.SaveAssets();
        #endif
    }

    public CraftingEntry GetCraft(int id) //USE THIS FOR GRABBING CRAFTS WHEN SAVING AND LOADING
    {
        return _craftingDatabase.Find(i => i.id == id);
    }

    public void ResetStats()
    {
        for(int i = 0; i < _craftingDatabase.Count; i++)
        {
            _craftingDatabase[i].isUnlocked = false;
            _craftingDatabase[i].isRecentlyUnlocked = false;
        }
    }

    public void SaveStats(out CraftingPlayerStats[] craftingStats)
    {
        List<CraftingPlayerStats> temp = new List<CraftingPlayerStats>();

        foreach(CraftingEntry c in _craftingDatabase)
        {
            temp.Add(new CraftingPlayerStats(c.isUnlocked, c.isRecentlyUnlocked));
        }
        craftingStats = temp.ToArray();
    }

    public void LoadStats(AllGameSaveData data)
    {
        int i = 0;
        foreach(CraftingEntry c in _craftingDatabase)
        {
            if(i >= data.craftingStats.Length) return;
            c.isUnlocked = data.craftingStats[i].isUnlocked;
            c.isRecentlyUnlocked = data.craftingStats[i].isRecentlyUnlocked;
            i++;
        }
    }

    public List<CraftingEntry> GetCraftingDatabase()
    {
        return _craftingDatabase; 
    }

}

[System.Serializable]
public class CraftingPlayerStats
{
    public bool isUnlocked = false;
    public bool isRecentlyUnlocked = false;

    public CraftingPlayerStats(bool _isUnlocked, bool _isRecentlyUnlocked)
    {
        isUnlocked = _isUnlocked;
        isRecentlyUnlocked = _isRecentlyUnlocked;
    }
}
