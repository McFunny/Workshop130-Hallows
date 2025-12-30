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

    public void ResetStats()
    {
        for(int i = 0; i < _cookingDatabase.Count; i++)
        {
            _cookingDatabase[i].amountMade = 0;
            _cookingDatabase[i].validRecipes.Clear();
        }
    }

    public void SaveStats(out CookingPlayerStats[] cookingStats)
    {
        List<CookingPlayerStats> temp = new List<CookingPlayerStats>();

        foreach(CookingRecipe c in _cookingDatabase)
        {
            temp.Add(new CookingPlayerStats(c.amountMade, c.validRecipes));
        }
        cookingStats = temp.ToArray();
    }

    public void LoadStats(AllGameSaveData data)
    {
        int i = 0;
        foreach(CookingRecipe c in _cookingDatabase)
        {
            if(i >= data.cookingStats.Length) return;
            c.amountMade = data.cookingStats[i].amountMade;
            c.validRecipes = new List<ValidRecipe>(data.cookingStats[i].validRecipes);
            i++;
        }
    }

    public List<CookingRecipe> GetCraftingDatabase()
    {
        return new List<CookingRecipe>(_cookingDatabase); 
    }
}


[System.Serializable]
public class CookingPlayerStats
{
    public int amountMade = 0;
    public List<ValidRecipe> validRecipes = new List<ValidRecipe>();

    public CookingPlayerStats(int _amountMade, List<ValidRecipe> _validRecipes)
    {
        amountMade = _amountMade;
        validRecipes = _validRecipes;
    }
}
