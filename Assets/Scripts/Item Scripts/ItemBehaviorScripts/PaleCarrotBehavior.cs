using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item Behavior", menuName = "Item Behavior/PaleCarrot")]
public class PaleCarrotBehavior : ItemBehavior
{
    public override void UseItem(out bool consumeItem)
    {
        StatusEffectManager.Instance.RemoveStatusOnPlayer(StatusEffectName.Blindness);
        consumeItem = true;


        Collider[] hitNPCs = Physics.OverlapSphere(PlayerInteraction.Instance.playerFeet.position, 15f, 1 << 6);
        foreach(Collider collider in hitNPCs)
        {
            RascalNPC rascal = collider.GetComponentInParent<RascalNPC>();
            if (rascal != null)
            {
                rascal.TauntedWithCarrot();
            }
        }
    }
}
