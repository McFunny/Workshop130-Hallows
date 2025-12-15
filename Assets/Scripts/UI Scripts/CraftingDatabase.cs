using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(menuName = "Crafting Database")]
public class CraftingDatabase : ScriptableObject
{
    public PopupScript recipeUnlockedP;

    public bool forceUnlockAll = false;

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

            if(forceUnlockAll) _craftingDatabase[i].isUnlocked = true;
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

    public void UnlockRecipiePopup()
    {
        PopupHandler.Instance.AddToQueue(recipeUnlockedP);
    }

    public void UnlockRecipe(int id)
    {
        if(_craftingDatabase.Count < id || _craftingDatabase[id].isUnlocked) return;
        _craftingDatabase[id].isUnlocked = true;
        PopupHandler.Instance.AddToQueue(recipeUnlockedP);


        return;
        foreach(CraftingEntry c in _craftingDatabase)
        {
            if(c.isUnlocked == false && c.id == id) 
            {
                c.isUnlocked = true;
                PopupHandler.Instance.AddToQueue(recipeUnlockedP);
            }
        }
    }

    public void UnlockRandomLockedRecipeInTier() //Call when using machine
    {
        for(int tier = 0; tier < 10; ++tier)
        {
            List<CraftingEntry> recipesInTier = new List<CraftingEntry>();
            foreach(CraftingEntry c in _craftingDatabase)
            {
                if(c.isUnlocked == false && c.tier == tier) recipesInTier.Add(c);
            }
            if(recipesInTier.Count > 0)
            {
                recipesInTier[Random.Range(0, recipesInTier.Count)].isUnlocked = true;
                UnlockRecipiePopup();
                break;
            }
            
        }
    }

    public int CurrentTier() //Tracks what the current tier of the next unlock will be
    {
        int heldTickets = GameSaveData.Instance.tTicketsHeld;
        for(int tier = 0; tier < 10; ++tier)
        {
            List<CraftingEntry> recipesInTier = new List<CraftingEntry>();
            foreach(CraftingEntry c in _craftingDatabase)
            {
                if(c.isUnlocked == false && c.tier == tier) recipesInTier.Add(c);
            }
            if(recipesInTier.Count > heldTickets)
            {
                return tier;
            }
            else heldTickets -= recipesInTier.Count;
            
        }

        return -1;
    }

    /*public List<CraftingEntry> GetLockedRecipesInTier(out int currentTier, out List<CraftingEntry> recipesInTier)
    {
        for(int tier = 0; tier < 10; ++tier)
        {
            recipesInTier.Clear();
            recipesInTier = new List<CraftingEntry>();
            foreach(CraftingEntry c in _craftingDatabase)
            {
                if(c.isUnlocked == false && c.tier == tier) recipesInTier.Add(c.isUnlocked);
            }
            if(recipesInTier.Count > 0)
            {
                currentTier = tier;
                return;
            }
            
        }

        currentTier = -1;
        recipesInTier = new List<CraftingEntry>();
    }*/

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
