using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Trinket Behavior/Backstep Pendant")]
public class BackstepPendantBehavior : TrinketBehavior
{
    public override void OnEquip()
    {
        PlayerMovement.Instance.backstepPendantEquipped = true;
    }

    public override void OnRemove()
    {
        PlayerMovement.Instance.backstepPendantEquipped = false;
    }
}
