using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Status Effect", menuName = "Status Effects/Frost")]
public class FrostEffect : StatusEffectObject
{
    public override void TimedEffect()
    {
        //What happens every second for player
        if(StatusEffectManager.Instance.FindStatusOnPlayer(StatusEffectName.Fire)) StatusEffectManager.Instance.RemoveStatusOnPlayer(StatusEffectName.Frost);
    }

    public override void TimedEffect(CreatureBehaviorScript c)
    {
        //What happens every second for creature
        if(StatusEffectManager.Instance.FindStatusOnCreature(StatusEffectName.Fire, c)) StatusEffectManager.Instance.RemoveStatusOnCreature(StatusEffectName.Frost, c);
    }

    public override void OnEffectApplied()
    {
        if(TrinketInventoryHandler.Instance.CheckForTrinket(TrinketKey.Coolant))
        {
            StatusEffectManager.Instance.RemoveStatusOnPlayer(StatusEffectName.Frost);
        }
    }

    public override void OnEffectApplied(CreatureBehaviorScript c)
    {
        //What happens when the effect is applied
        c.actionSpeedMod -= 0.3f;
    }

    public override void OnEffectRemoved()
    {
        //What happens when the effect is removed
        ParticlePoolManager.Instance.GrabThawParticle().transform.position = PlayerInteraction.Instance.transform.position;
    }

    public override void OnEffectRemoved(CreatureBehaviorScript c)
    {
        //What happens when the effect is removed
        c.actionSpeedMod += 0.3f;
        ParticlePoolManager.Instance.GrabThawParticle().transform.position = c.transform.position;
    }
}
