using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(menuName = "Cooking Database")]
public class CookingDatabase : ScriptableObject
{
    private static CookingDatabase _instance;

    public static CookingDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                // Load the instance of the Database if not already set
                _instance = Resources.Load<CookingDatabase>("Cooking Database");
            }
            return _instance;
        }
    }

    [SerializeField] private List<CookingRecipe> _cookingDatabase; //DONT ALTER ORDER

    [ContextMenu("Update ID's")]
    public void UpdateID()
    {
        for(int i = 0; i < _cookingDatabase.Count; i++)
        {
            _cookingDatabase[i].id = i;
            #if UNITY_EDITOR

            if (_cookingDatabase[i]) EditorUtility.SetDirty(_cookingDatabase[i]);

            #endif
        }
        #if UNITY_EDITOR       
            AssetDatabase.SaveAssets();
        #endif
    }

    public CookingRecipe GetRecipe(int id)
    {
        return _cookingDatabase.Find(i => i.id == id);
    }

    /*public void ResetStats()
    {
        for(int i = 0; i < _cookingDatabase.Count; i++)
        {
            _cookingDatabase[i].isUnlocked = false;
            _cookingDatabase[i].isRecentlyUnlocked = false;

            if(forceUnlockAll) _cookingDatabase[i].isUnlocked = true;
        }
    }*/

    /*public void SaveStats(out CraftingPlayerStats[] craftingStats)
    {
        List<CraftingPlayerStats> temp = new List<CraftingPlayerStats>();

        foreach(CookingRecipe c in _cookingDatabase)
        {
            temp.Add(new CraftingPlayerStats(c.isUnlocked, c.isRecentlyUnlocked));
        }
        craftingStats = temp.ToArray();
    }

    public void LoadStats(AllGameSaveData data)
    {
        int i = 0;
        foreach(CookingRecipe c in _cookingDatabase)
        {
            if(i >= data.craftingStats.Length) return;
            c.isUnlocked = data.craftingStats[i].isUnlocked;
            c.isRecentlyUnlocked = data.craftingStats[i].isRecentlyUnlocked;
            i++;
        }
    }*/

    public List<CookingRecipe> GetCraftingDatabase()
    {
        return _cookingDatabase; 
    }
}
