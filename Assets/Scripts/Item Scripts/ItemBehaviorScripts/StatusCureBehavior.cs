using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item Behavior", menuName = "Item Behavior/StatusCure")]
public class StatusCureBehavior : ItemBehavior
{
    public StatusEffectName statusCured;
    public override void UseItem(out bool consumeItem)
    {
        if(StatusEffectManager.Instance.RemoveStatusOnPlayer(statusCured));

        consumeItem = true;
    }
}
