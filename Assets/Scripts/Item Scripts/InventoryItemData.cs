using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This is a scriptable object, that defines what an item isin our game
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
    public int maxStackSize = 1;
   
    public void UseItem()
    {
        Debug.Log($"Using {this.displayName}");
    }

    public virtual void PrimaryUse(){}
}
