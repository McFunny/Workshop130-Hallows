using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory System/Trinket Item")]
public class TrinketItem : InventoryItemData
{
    public TrinketBehavior behavior;

    public TrinketKey key;

    public bool stackable = false;

    public void OnEquip()
    {
        behavior.OnEquip();
    }

    public void OnRemove()
    {
        behavior.OnRemove();
    }

    public void TriggerEffect(out float durabilityCost)
    {
        behavior.TriggerEffect(out durabilityCost);
    }
}

public enum TrinketKey
{
    Basic,
    WaterFlask
}
