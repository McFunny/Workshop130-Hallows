using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Trinket Behavior/Wing Tag")]
public class WingTagBehavior : TrinketBehavior
{
    public float speedMod = 1.1f;
    public string trinketID;
    public override void OnEquip()
    {
        PlayerMovement.Instance.ApplySpeedMod(new MovementSpeedModifiers(TrinketInventoryHandler.Instance.gameObject, speedMod, trinketID, true));
    }

    public override void OnRemove()
    {
        PlayerMovement.Instance.RemoveSpeedMod(trinketID);
    }
}
