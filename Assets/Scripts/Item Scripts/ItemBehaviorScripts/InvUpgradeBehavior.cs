using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item Behavior", menuName = "Item Behavior/InvUpgrade")]
public class InvUpgradeBehavior : ItemBehavior
{
    public override void OnRecieve(InventoryItemData recievedItem)
    {
        PlayerInteraction.Instance.playerUpgrades.GainInventoryUpgrade();
    }
}
