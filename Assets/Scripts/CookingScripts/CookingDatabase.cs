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
            _cookingDatabase[i].unlocked = false;
            _cookingDatabase[i].validRecipes.Clear();
        }
    }

    public void SaveStats(out CookingPlayerStats[] cookingStats)
    {
        List<CookingPlayerStats> temp = new List<CookingPlayerStats>();

        foreach(CookingRecipe c in _cookingDatabase)
        {
            temp.Add(new CookingPlayerStats(c.amountMade, c.validRecipes, c.unlocked));
        }
        cookingStats = temp.ToArray();
    }

    public void LoadStats(AllGameSaveData data)
    {
        int n = 0;
        foreach (CookingRecipe c in _cookingDatabase)
        {
            if (n >= data.cookingStats.Length) return;

            CookingPlayerStats savedRecipe = data.cookingStats[n];

            c.amountMade = savedRecipe.amountMade;
            c.unlocked = savedRecipe.unlocked;

            c.validRecipes = new List<ValidRecipe>();  // Fresh list per recipe
            for (int i = 0; i < savedRecipe.saveableRecipes.Count; ++i)
            {
                List<InventoryItemData> itemList = new List<InventoryItemData>();  // Fresh list per valid recipe
                for (int x = 0; x < savedRecipe.saveableRecipes[i].ingredientIDs.Count; ++x)
                {
                    itemList.Add(Database.Instance.GetItem(savedRecipe.saveableRecipes[i].ingredientIDs[x]));
                }
                c.validRecipes.Add(new ValidRecipe(itemList));
            }

            n++;
        }
    }

    public List<CookingRecipe> GetCraftingDatabase()
    {
        return new List<CookingRecipe>(_cookingDatabase); 
    }

    public bool AllRecipesUnlocked() //For checking if cul can sell recipes
    {
        foreach(CookingRecipe c in _cookingDatabase)
        {
            if(!c.unlocked) return false;
        }
        return true;
    }

    public void UnlockRandomRecipe()
    {
        List<CookingRecipe> lockedRecipes = new List<CookingRecipe>();

        foreach(CookingRecipe c in _cookingDatabase)
        {
            if(!c.unlocked) lockedRecipes.Add(c);
        }

        if(lockedRecipes.Count == 0) return;

        int r = Random.Range(0, lockedRecipes.Count);

        lockedRecipes[r].unlocked = true;
        lockedRecipes[r].AddNewRecipe(lockedRecipes[r].exampleRecipe.usedItems);
    }
}


[System.Serializable]
public class CookingPlayerStats
{
    public int amountMade = 0;
    public bool unlocked = false;
    //public List<ValidRecipe> validRecipes = new List<ValidRecipe>();
    public List<SaveableRecipe> saveableRecipes = new List<SaveableRecipe>();

    public CookingPlayerStats(int _amountMade, List<ValidRecipe> _validRecipes, bool _unlocked)
    {
        amountMade = _amountMade;
        unlocked = _unlocked;
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
