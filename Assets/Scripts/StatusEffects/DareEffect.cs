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
}
