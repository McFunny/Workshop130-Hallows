using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CraftingEntry : ScriptableObject
{
    [Tooltip("Overrides the name that appears on the recipe if that's something you need to do. Does NOT change item name.")]
    public string nameOverride;
    public int levelRequirement = -1;
    [Header("Output Data")]
    public InventoryItemData output;
    public int outputAmount = 1;
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