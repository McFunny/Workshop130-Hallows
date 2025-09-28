using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item Behavior", menuName = "Item Behavior/Aloe")]
public class AloeBehavior : ItemBehavior
{
    public override void UseItem(out bool consumeItem)
    {
        if(StatusEffectManager.Instance.RemoveStatusOnPlayer(StatusEffectName.Fire)) PlayerInteraction.Instance.StaminaChange(20);

        consumeItem = true;
    }
}
