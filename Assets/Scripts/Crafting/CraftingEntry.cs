using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CraftingEntry : ScriptableObject
{
    public int id = -1;
    [Tooltip("Overrides the name that appears on the recipe if that's something you need to do. Does NOT change item name.")]
    public string nameOverride;
    public CraftingCategory category = CraftingCategory.Misc;
    [Header("Unlock Requirements")]
    public int tier = -1; //-1 means unlocked through alternative means
    public bool isUnlocked = false;
    public bool isRecentlyUnlocked = true;
    public bool unlockedAtStart = false; //Starts unlocked
    public bool isTrinket; //If true, cannot be unlocked until after fanatic quest
    [Header("Output Data")]
    public InventoryItemData output;
    public int outputAmount = 1;
    public int craftTimeInSeconds;
    [Header("Requirement cap is 5. This includes mints.")]
    [Header("If you want a craft to have a mint cost, then do not have more than 4 other crafting requirements.")]
    public int mintCost = 0;
    public List<CraftingRequirement> craftingRequirements = new List<CraftingRequirement>();
}

[System.Serializable]
public class CraftingRequirement
{
    public InventoryItemData requiredItem;
    public int requiredAmount;
}

public enum CraftingCategory
{
    Seed,
    Structure,
    Furniture,
    Trinket,
    Misc
}