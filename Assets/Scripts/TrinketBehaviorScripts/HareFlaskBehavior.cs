using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Trinket Behavior/Hare Flask")]
public class HareFlaskBehavior : TrinketBehavior
{
    public GameObject waterExplosionPrefab;
    public override void OnEquip()
    {
        PlayerInteraction.Instance.maxWaterHeld += 5;
        var UIMeters = FindFirstObjectByType<UIMeters>();
        UIMeters.UpdateMeters();
    }

    public override void TriggerEffect(out float durabilityCost)
    {
        durabilityCost = 25;
    }

    public override void OnRemove()
    {
        PlayerInteraction.Instance.maxWaterHeld -= 5;
        if(PlayerInteraction.Instance.waterHeld > PlayerInteraction.Instance.maxWaterHeld)
        {
            //Play a splash
            PlayerInteraction.Instance.waterHeld = PlayerInteraction.Instance.maxWaterHeld;

            GameObject bigSplashEffect = Instantiate(waterExplosionPrefab, PlayerInteraction.Instance.transform.position, Quaternion.identity);

            StatusEffectManager.Instance.RemoveStatusOnPlayer(StatusEffectName.Fire);
            Vector3 origin = PlayerInteraction.Instance.playerFeet.position;
            Collider[] hitStructures = Physics.OverlapSphere(origin, 3f, 1 << 6);
            foreach(Collider collider in hitStructures)
            {
                StructureBehaviorScript structure = collider.gameObject.GetComponentInParent<StructureBehaviorScript>();
                if(structure)
                {
                    structure.HitWithWater();
                }
            }

            Collider[] hitEnemies = Physics.OverlapSphere(origin, 4.5f, 1 << 9);
            foreach(Collider collider in hitEnemies)
            {
                var creature = collider.GetComponentInParent<CreatureBehaviorScript>();
                if (creature != null)
                {
                    creature.HitWithWater();
                }
            }
        }

        var UIMeters = FindFirstObjectByType<UIMeters>();
        UIMeters.UpdateMeters();
    }
}
