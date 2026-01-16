using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Trinket Behavior/Basic Trinket")]
public class TrinketBehavior : ScriptableObject
{
    public virtual void OnEquip()
    {

    }

    public virtual void TriggerEffect(out float durabilityCost)
    {
        durabilityCost = 0;
    }

    public virtual void OnRemove()
    {

    }


}
