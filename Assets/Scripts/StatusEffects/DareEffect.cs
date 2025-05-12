using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Status Effect", menuName = "Status Effects/Dare")]
public class DareEffect : StatusEffectObject
{
    public override void TimedEffect()
    {
        //What happens every second for player
    }

    public override void TimedEffect(CreatureBehaviorScript c)
    {
        //What happens every second for creature
    }

    public override void OnEffectApplied()
    {
        //What happens when the effect is applied
    }

    public override void OnEffectApplied(CreatureBehaviorScript c)
    {
        //What happens when the effect is applied
        c.damageToStructure = (int)(c.damageToStructure * 1.5f);
        c.damageToPlayer = (int)(c.damageToPlayer * 1.5f);
    }

    public override void OnEffectRemoved()
    {
        //What happens when the effect is removed
    }

    public override void OnEffectRemoved(CreatureBehaviorScript c)
    {
        //What happens when the effect is removed
        c.damageToStructure = (int)(c.damageToStructure * 0.5f);
        c.damageToPlayer = (int)(c.damageToPlayer * 0.5f);
    }
}
