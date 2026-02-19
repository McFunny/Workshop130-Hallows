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
        if(c.health < 0) c.TakeDamage(creatureDamage/2);
        else c.TakeDamage(creatureDamage);
        c.PlayHitParticle(Vector3.zero);
    }

    public override void OnEffectApplied()
    {
        //What happens when the effect is applied
        if(TrinketInventoryHandler.Instance.CheckForTrinket(TrinketKey.Coolant))
        {
            if(StatusEffectManager.Instance.RemoveStatusOnPlayer(StatusEffectName.Fire))
            {
                TrinketInventoryHandler.Instance.ForceBreakTrinket(TrinketKey.Coolant);

                Collider[] hitStructures = Physics.OverlapSphere(PlayerInteraction.Instance.transform.position, 4.5f, 1 << 6);
                foreach(Collider collider in hitStructures)
                {
                    StructureBehaviorScript structure = collider.gameObject.GetComponentInParent<StructureBehaviorScript>();
                    if(structure && structure.onFire)
                    {
                        structure.Extinguish();
                    }
                }

                Collider[] hitEnemies = Physics.OverlapSphere(PlayerInteraction.Instance.transform.position, 4f, 1 << 9);
                foreach(Collider collider in hitEnemies)
                {
                    var creature = collider.GetComponentInParent<CreatureBehaviorScript>();
                    if (creature != null)
                    {
                        creature.HitWithWater();
                    }
                }

                PlayerInteraction.Instance.WaterChange(5);
            }
        }
    }
}
