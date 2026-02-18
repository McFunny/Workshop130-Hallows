using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Trinket Behavior/WeedArmor Trinket")]
public class WeedArmorBehavior : TrinketBehavior
{
    public bool weedOnly;
    public override bool ArmorEnabled()
    {
        Collider[] hitStructures = Physics.OverlapSphere(PlayerInteraction.Instance.playerFeet.position, 1, 1 << 6);
        foreach(Collider collider in hitStructures)
        {
            var tile = collider.GetComponentInParent<FarmLand>();
            if(tile)
            {
                if(tile.isWeed) return true;
                else if(weedOnly || tile.crop == null) return false;
                else return true;
            }
        }
        return false;
    }
}
