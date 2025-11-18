using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item Behavior", menuName = "Item Behavior/Tool Pickup")]
public class ToolPickupBehavior : ItemBehavior
{
    public override void OnRecieve(InventoryItemData receivedItem)
    {
        ToolItem tItem = receivedItem as ToolItem;
        if(!tItem) return;
        switch(tItem.tool)
        {
            default:
            break;
            case ToolType.Kukri:
            GameSaveData.Instance.kukriObtained = true;
            break;
            case ToolType.NutTester:
            GameSaveData.Instance.testerObtained = true;
            break;
            case ToolType.Flintlock:
            GameSaveData.Instance.pistolObtained = true;
            break;
        }
    }
}
