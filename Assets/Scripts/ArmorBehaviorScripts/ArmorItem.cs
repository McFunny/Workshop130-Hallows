using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory System/Armor Item")]
public class ArmorItem : InventoryItemData
{
    public ArmorBehavior behavior;
    public ArmorType armorType;

}

public enum ArmorType
{
    Null,
    Head,
    Chest,
    Legs,
    Boots
}
