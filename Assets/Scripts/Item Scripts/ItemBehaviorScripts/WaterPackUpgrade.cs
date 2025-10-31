using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item Behavior", menuName = "Item Behavior/WaterPack Upgrade")]
public class WaterPackUpgrade : ItemBehavior
{
    public override void OnRecieve(InventoryItemData recievedItem)
    {
        PlayerInteraction.Instance.playerUpgrades.GainWaterPackUpgrade();
    }
}
