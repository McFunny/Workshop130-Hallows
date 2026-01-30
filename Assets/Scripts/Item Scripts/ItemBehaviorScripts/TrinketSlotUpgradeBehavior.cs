using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item Behavior", menuName = "Item Behavior/TrinketUpgrade")]
public class TrinketSlotUpgradeBehavior : ItemBehavior
{
    public override void OnRecieve(InventoryItemData recievedItem)
    {
        Debug.Log("Increased inventory");
        PlayerInventoryHolder.Instance.IncreaseTrinketInventory(1);
    }
}
