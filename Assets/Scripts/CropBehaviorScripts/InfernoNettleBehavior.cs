using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crop Behavior", menuName = "Crop Behavior/InfernoNettles")]
public class InfernoNettleBehavior : CropBehavior
{
    public AudioClip contactSFX;
    public override void OnWatered(FarmLand tile)
    {
        tile.TakeStressDamage(5);
    }

    public override bool IsFlammable()
    {
        return false;
    }

    public override void OnContact(FarmLand tile, GameObject contactedObject)
    {
        if(tile.rotted) return;
        int burnDuration;
        switch(tile.growthStage)
        {
            case 1:
            return;
            case 2:
            burnDuration = 6;
            break;
            case 3:
            burnDuration = 12;
            break;
            case 4:
            burnDuration = 24;
            break;
            default:
            burnDuration = 0;
            break;
        }

        CreatureBehaviorScript c = contactedObject.GetComponentInParent<CreatureBehaviorScript>();
        if(c && c.fireVulnerable && (c as ICritter) == null)
        {
            c.ApplyStatusEffect(StatusDatabase.Instance.GetStatus(StatusEffectName.Fire), burnDuration);
            tile.growthStage = 1;
            tile.SpriteChange();
            tile.harvestable = false;
            AudioPoolManager.Instance.PlayClipAtPosition(contactSFX, tile.transform.position);
            ParticlePoolManager.Instance.MoveAndPlayParticle(tile.transform.position, ParticlePoolManager.Instance.dirtParticle);
        } 

        if(contactedObject.layer == 10)
        {
            PlayerInteraction.Instance.ApplyStatusEffect(StatusDatabase.Instance.GetStatus(StatusEffectName.Fire), burnDuration/2);
            tile.growthStage = 1;
            tile.SpriteChange();
            tile.harvestable = false;
            AudioPoolManager.Instance.PlayClipAtPosition(contactSFX, tile.transform.position);
            ParticlePoolManager.Instance.MoveAndPlayParticle(tile.transform.position, ParticlePoolManager.Instance.dirtParticle);
        }
    }

    public override void OnHarvest(FarmLand tile, bool usedShovel, bool usedScythe)
    {
        if(!usedShovel && !usedScythe) PlayerInteraction.Instance.ApplyStatusEffect(StatusDatabase.Instance.GetStatus(StatusEffectName.Fire), 4);
    }
}
