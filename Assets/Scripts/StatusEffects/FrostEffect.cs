using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Status Effect", menuName = "Status Effects/Frost")]
public class FrostEffect : StatusEffectObject
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
        c.actionSpeedMod -= 0.3f;
    }

    public override void OnEffectRemoved()
    {
        //What happens when the effect is removed
    }

    public override void OnEffectRemoved(CreatureBehaviorScript c)
    {
        //What happens when the effect is removed
        c.actionSpeedMod += 0.3f;
    }
}
