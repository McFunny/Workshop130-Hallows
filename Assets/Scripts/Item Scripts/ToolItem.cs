using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory System/Tool Item")]
public class ToolItem : InventoryItemData
{
    public float durability;
    public ToolBehavior behavior;
    public ToolType tool;

    public void PrimaryUse(Transform player)
    {
        behavior.PrimaryUse(player, tool);
    }

    public void SecondaryUse(Transform player)
    {
        behavior.SecondaryUse(player, tool);
    }
}

public enum ToolType
{
    Null,
    Hoe,
    Shovel,
    WateringCan,
    ShotGun
}
