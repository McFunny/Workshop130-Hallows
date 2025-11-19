using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item Behavior", menuName = "Item Behavior/WagonRepair")]
public class WagonRepairKitBehavior : ItemBehavior
{
    public override void OnRecieve(InventoryItemData recievedItem)
    {
        GameSaveData.Instance.playerWagonUnlocked = true;
    }
}
