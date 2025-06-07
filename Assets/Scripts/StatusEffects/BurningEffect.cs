using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Status Effect", menuName = "Status Effects/Burning")]
public class BurningEffect : StatusEffectObject
{
    public float playerDamage, creatureDamage;
    public override void TimedEffect()
    {
        //What happens every second for player
        PlayerInteraction.Instance.StaminaChange(-playerDamage);
    }

    public override void TimedEffect(CreatureBehaviorScript c)
    {
        //What happens every second for creature
        c.TakeDamage(creatureDamage);
        c.PlayHitParticle(Vector3.zero);
    }
}
