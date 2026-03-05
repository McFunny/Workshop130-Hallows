using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item Behavior", menuName = "Item Behavior/WaterFood")]
public class WaterFoodBehavior : ItemBehavior
{
    public int waterRestored = 1;
    public override void UseItem(out bool consumeItem)
    {
        PlayerInteraction.Instance.WaterChange(waterRestored);

        consumeItem = true;
    }
}
