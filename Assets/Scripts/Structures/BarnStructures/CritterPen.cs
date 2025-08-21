using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CritterPen : StructureBehaviorScript
{
    //Should probably have text to show which creatures reside here

    public PenType type;
    //public int currentOccupents = 0;
    public List<CreatureBehaviorScript> housedCritters;
    public int maxOccupency = 2;

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(type == ToolType.Shovel)
        {
            success = true;
        }
    }

    void OnDestroy()
    {
        base.OnDestroy();
        if (!gameObject.scene.isLoaded) return; 
        //drop items
        GameObject droppedItem;
        foreach(ItemWithAmount repairItem in structData.repairItems)
        {
            for(int i = 0; i < repairItem.amount; i++)
            {
                droppedItem = ItemPoolManager.Instance.GrabItem(repairItem.item);
                droppedItem.transform.position = transform.position;
            }
        }
    }
}

public enum PenType
{
    Pen,
    Coop,
    Hive
}
