using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crop Behavior", menuName = "Crop Behavior/Flak Kale")]
public class FlakKaleBehavior : CropBehavior
{
    public float abilityCost = 2;
    public override void CropBonusYield(FarmLand tile, out int cropBonus, out int secondaryCropBonus)
    {
        cropBonus = 0;
        if(tile.growthStage == 6) cropBonus = 1;
        if(tile.growthStage == 7) cropBonus = 2;
        secondaryCropBonus = 0;
    }

    public override bool WasFullyEaten(FarmLand tile, CreatureBehaviorScript creature)
    { 
        if(tile.growthStage > 2 && tile.GetCropStats().ichorLevel >= abilityCost)
        {
            --tile.growthStage;
            tile.SpriteChange();
            tile.GetCropStats().ichorLevel -= abilityCost;
            return false;
        }
        return true;
    }
}
