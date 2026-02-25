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
        List<ValidRecipe> recipeList = new List<ValidRecipe>();
        List<InventoryItemData> itemList = new List<InventoryItemData>();

        int n = 0; //iterations
        foreach(CookingRecipe c in _cookingDatabase)
        {
            if(n >= data.cookingStats.Length) return;
            c.amountMade = data.cookingStats[n].amountMade;
            //c.validRecipes = new List<ValidRecipe>(data.cookingStats[i].validRecipes);

            recipeList.Clear();
            for(int i = 0; i < data.cookingStats[n].saveableRecipes.Count; ++i)
            {
                itemList.Clear();
                for(int x = 0; x < data.cookingStats[n].saveableRecipes[i].ingredientIDs.Count; ++x)
                {
                    itemList.Add(Database.Instance.GetItem(data.cookingStats[n].saveableRecipes[i].ingredientIDs[x]));
                }
                ValidRecipe newRecipe = new ValidRecipe(itemList);
                recipeList.Add(newRecipe);
            }
            c.validRecipes = recipeList;

            n++;
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
    //public List<ValidRecipe> validRecipes = new List<ValidRecipe>();
    public List<SaveableRecipe> saveableRecipes = new List<SaveableRecipe>();

    public CookingPlayerStats(int _amountMade, List<ValidRecipe> _validRecipes)
    {
        amountMade = _amountMade;
        //validRecipes = _validRecipes;

        for(int i = 0; i < _validRecipes.Count; ++i)
        {
            saveableRecipes.Add(new SaveableRecipe(_validRecipes[i]));
        }
    }
}

[System.Serializable]
public class SaveableRecipe
{
    public List<int> ingredientIDs = new List<int>();

    public SaveableRecipe(ValidRecipe _validRecipe)
    {
        for(int i = 0; i < _validRecipe.usedItems.Count; ++i)
        {
            ingredientIDs.Add(_validRecipe.usedItems[i].ID);
        }
    }
}
