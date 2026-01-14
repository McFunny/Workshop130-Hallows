using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Trinket Behavior/Hare Flask")]
public class HareFlaskBehavior : TrinketBehavior
{
    public override void OnEquip()
    {
        PlayerInteraction.Instance.maxWaterHeld += 5;
    }

    public override void TriggerEffect(out float durabilityCost)
    {
        durabilityCost = 25;
    }

    public override void OnRemove()
    {
        PlayerInteraction.Instance.maxWaterHeld -= 5;
    }
}
