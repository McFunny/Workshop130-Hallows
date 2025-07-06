using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crop Behavior", menuName = "Crop Behavior/Cactus")]
public class CactusBehavior : CropBehavior
{
    public AudioClip contactSFX;
    public override void CropBonusYield(FarmLand tile, out int cropBonus, out int secondaryCropBonus)
    {
        cropBonus = 0;
        if(tile.growthStage >= 5 && tile.growthStage <= 7) secondaryCropBonus = Random.Range(1,3);
        else secondaryCropBonus = 0;
    }

    public override void OnHour(FarmLand tile)
    {
        if(tile.GetCropStats().waterLevel < 5 && tile.growthStage == 6) //Retract spikes
        {
            tile.growthStage = 5;
            tile.SpriteChange();
            return;
        }

        if((tile.growthStage == 5 || tile.growthStage == 6))
        {
            if(Random.Range(0, 100) > 96)
            {
                tile.growthStage = 7;
                tile.SpriteChange();
            }
        }
        else if(!TimeManager.Instance.isDay)
        {
            tile.GetCropStats().waterLevel -= 5;
        }
    }

    public override bool DestroyOnHarvest(FarmLand tile)
    {
        if(tile.growthStage == 7) return false;
        return true;
    }

    public override void OnContact(FarmLand tile, GameObject contactedObject)
    {
        if(tile.growthStage != 6) return;

        CreatureBehaviorScript c = contactedObject.GetComponentInParent<CreatureBehaviorScript>();
        if(c && c.shovelVulnerable)
        {
            c.TakeDamage(10);
            AudioPoolManager.Instance.PlayClipAtPosition(contactSFX, tile.transform.position);
            ParticlePoolManager.Instance.MoveAndPlayParticle(tile.transform.position, ParticlePoolManager.Instance.dirtParticle);
        } 

        if(contactedObject.layer == 10)
        {
            PlayerInteraction.Instance.StaminaChange(-6);
            AudioPoolManager.Instance.PlayClipAtPosition(contactSFX, tile.transform.position);
            ParticlePoolManager.Instance.MoveAndPlayParticle(tile.transform.position, ParticlePoolManager.Instance.dirtParticle);
        }
    }

    public override void OnWatered(FarmLand tile)
    {
        if(tile.GetCropStats().waterLevel == 10 && tile.growthStage == 5)
        {
            tile.growthStage = 6;
            tile.SpriteChange();
            return;
        }
    }

    public override bool CanGrow(FarmLand tile)
    {
        if(tile.growthStage > 4) return false;
        return true;
    }
}
