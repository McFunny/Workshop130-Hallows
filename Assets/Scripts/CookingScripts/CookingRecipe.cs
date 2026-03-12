using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CookingRecipe : ScriptableObject
{
    public int id = -1;
    public int amountMade = 0; //How many has the player made?
    public bool unlocked = false;

    [Tooltip("Higher priority recipes will be made over lower ones. Max is 5, Min is 0")]
    public int priority = 0; 

    [Header("Requirements")]
    [Tooltip("These items MUST be included to make the recipe")]
    public List<InventoryItemData> requiredItems = new List<InventoryItemData>(); 
    [Tooltip("These items MUST NOT be included to make the recipe")]
    public List<InventoryItemData> barredItems = new List<InventoryItemData>(); 

    [Tooltip("If the value is less than 0, that means it CANNOT have it in the recipe")]
    //public float veggieValue, fruitValue, meatValue, sweetValue, bugValue, eggValue, fillerValue, weedValue;
    public List<CookingStats> recipeStats = new List<CookingStats>();

    [Header("Output Data")]
    public InventoryItemData output;
    public int cookTimeInSeconds;

    [Header("Past Recipes")]
    public List<ValidRecipe> validRecipes = new List<ValidRecipe>();

    [Header("Template Recipe")]
    public ValidRecipe exampleRecipe;


    public bool EligibleRecipe(List<InventoryItemData> ingredients, List<CookingStats> currentStats)
    {
        List<InventoryItemData> neededItems = new List<InventoryItemData>(requiredItems);
        foreach(InventoryItemData item in ingredients)
        {
            if(barredItems.Contains(item))
            {
                return false; //Recipe Contains a banned item
            }

            if(neededItems.Contains(item))
            {
                neededItems.Remove(item);
            }
        }
        if(neededItems.Count > 0) return false; //Does not have the required items to make

        foreach(CookingStats stats in recipeStats) //Checks if the recipe stats are reached
        {
            bool statsMet = false;
            for(int i = 0; i < currentStats.Count; ++i)
            {
                if(currentStats[i].type == stats.type)
                {
                    if(stats.value == 0 || (stats.value < 0 && currentStats[i].value == 0) || (stats.value > 0 && currentStats[i].value >= stats.value)) statsMet = true;
                    break;
                }
            }
            if(!statsMet) return false; //Requirements are not met
        }

        return true;

    }

    public void AddNewRecipe(List<InventoryItemData> ingredients)
    {
        ValidRecipe newRecipe = new ValidRecipe(ingredients);

        if(validRecipes.Count == 0) validRecipes.Add(newRecipe);
        else validRecipes.Insert(0, newRecipe);

        if(validRecipes.Count >= 5) validRecipes.RemoveAt(4);
    }

    [ContextMenu ("Increase Priority")]
    public void IncreasePriority()
    {
        priority++;
    }

    [ContextMenu ("Decrease Priority")]
    public void DecreasePriority()
    {
        priority--;
    }

}

[System.Serializable]
public class CookingStats //For items
{
    public IngredientType type;
    public float value;

    public CookingStats(IngredientType _type, float  _value)
    {
        type = _type;
        value = _value;
    }
}

[System.Serializable]
public class ValidRecipe
{
    public List<InventoryItemData> usedItems;

    public ValidRecipe(List<InventoryItemData> ingredients)
    {
        usedItems = ingredients;
    }
}

//Dont forget to update the initialize stats function in the crockpot script if adding or removing any types
public enum IngredientType
{
    Veggie,
    Fruit,
    Meat,
    Sweetener,
    Bug,
    Egg,
    Filler,
    Weeds, //Plant fiber, reedtail, yarrow, ect
    Tuber,
    Nut

}
