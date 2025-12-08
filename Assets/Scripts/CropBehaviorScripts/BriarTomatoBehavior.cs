using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crop Behavior", menuName = "Crop Behavior/BriarTomato")]
public class BriarTomatoBehavior : CropBehavior
{
    public override void CropBonusYield(FarmLand tile, out int cropBonus, out int secondaryCropBonus)
    {
        cropBonus = 0;
        if(tile.growthStage == 7) secondaryCropBonus = 1;
        else secondaryCropBonus = 0;
    }
}
