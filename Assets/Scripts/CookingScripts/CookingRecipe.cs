using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CookingRecipe : ScriptableObject
{
    public int id = -1;
    //public int amountMade = 0; //How many has the player made?

    [Tooltip("Higher priority recipes will be made over lower ones")]
    public int priority = 0; 

    [Header("Requirements")]
    [Tooltip("These items MUST be included to make the recipe")]
    public List<InventoryItemData> requiredItems = new List<InventoryItemData>(); 
    [Tooltip("These items MUST NOT be included to make the recipe")]
    public List<InventoryItemData> barredItems = new List<InventoryItemData>(); 

    [Tooltip("If the value is less than 0, that means it CANNOT have it in the recipe")]
    public float veggieValue, fruitValue, meatValue, sweetValue, bugValue, eggValue, fillerValue;

    [Header("Output Data")]
    public InventoryItemData output;
    public int cookTimeInSeconds;

}

[System.Serializable]
public class CookingStats //For items
{
    public IngredientType type;
    public float value;
}

public enum IngredientType
{
    Veggie,
    Fruit,
    Meat,
    Sweetener,
    Bug,
    Egg,
    Filler

}
