using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crop Behavior", menuName = "Crop Behavior/Mimic Trap")]
public class MimicTrapBehavior : CropBehavior
{
    public List<CreatureObject> immuneCreatures;
    public GameObject particleEffect;
    public override void OnCropDestroyed(FarmLand tile)
    {
        Collider[] hitCreatures = Physics.OverlapSphere(tile.transform.position, 5f, 1 << 9);
        foreach(Collider collider in hitCreatures)
        {
            //apply effect
            var creature = collider.GetComponentInParent<CreatureBehaviorScript>();
            if (creature != null && creature.shovelVulnerable && !immuneCreatures.Contains(creature.creatureData))
            {
                creature.ApplyStatusEffect(StatusDatabase.Instance.GetStatus(StatusEffectName.MimicScent), 80);
                continue;
            }
        }
        Instantiate(particleEffect, tile.transform.position, Quaternion.identity);
    }
}
