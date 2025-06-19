using System.Collections;
using System.Collections.Generic;
//using UnityEditor.EditorTools;
using UnityEngine;


/// <summary>
/// This is a scriptable object, that defines what an item is in our game
/// It could be inherited from to have branched version of items, for example potions and equipment
/// </summary>

[CreateAssetMenu(menuName = "Inventory System/Inventory Item")]
public class InventoryItemData : ScriptableObject
{
    public int ID = -1;
    public string displayName;
    [TextArea(4,4)]
    public string description;
    public Sprite icon;
    public int maxStackSize = 20; //used also for the mint item pickup for determining value (sorry cameron)
    public float value = 0;
    public float sellValueMultiplier = 1; //if value or sellValueMultiplier == 0, cannot be sold
    public bool isKeyItem = false; //if true, should not be sold or be able to be thrown away.
    public bool hasModel;

    public float staminaValue = 0; //if higher than 0, restores stamina when eaten, and is therefore consumable
    public float bonusCompostValue = 0;
    public InventoryItemData pickledForm;

    [Tooltip("What can be done with this item? EX: 'LMB - Till Ground' or 'RMB - Plant Seed'")]
    public List<string> itemInputsKBM;
    public List<string> itemInputsController;

    [Tooltip("What Status Effects Will Be Gained Upon Consumption")]
    public List<StatusEffect> gainedEffects = new List<StatusEffect>();

    public ItemBehavior itemBehavior;

    public AudioClip useSound;

    public bool cannotEnterInventory = false; //This is for things that should never enter the inventory. Their behavior will be used if it has any instead of entering the inventory
   
    public void UseItem()
    {
        //Debug.Log($"Using {this.displayName}");
    }

    //public virtual void PrimaryUse(){}

    [ContextMenu("CalculatePickledValues")]
    public void CalculatePickledValues()
    {
        if(pickledForm)
        {
            pickledForm.value = value * 2f;
            pickledForm.staminaValue = staminaValue * 1.25f;
        }
    }
    [ContextMenu("PrintSellValue")]
    public void PrintSellValue()
    {
        Debug.Log("Sell Value is " + value * sellValueMultiplier);
    }
}
