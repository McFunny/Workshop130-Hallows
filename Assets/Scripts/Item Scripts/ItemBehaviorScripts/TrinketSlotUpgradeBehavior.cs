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

        GameSaveData.Instance.trinketSlotsGiven++;

        if(GameSaveData.Instance.trinketSlotsGiven == 3) AchievementManager.Instance.CompleteProgressWithEnum(ACHKey.Packed_Pockets);
    }
}
