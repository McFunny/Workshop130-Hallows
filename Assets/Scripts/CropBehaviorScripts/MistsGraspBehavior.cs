using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crop Behavior", menuName = "Crop Behavior/MistsGrasp")]
public class MistsGraspBehavior : CropBehavior
{
    public override void OnIchorRefill(FarmLand tile)
    {
        tile.hoursSpent = tile.crop.hoursPerStage + 1;
        tile.HourPassed();
    }

    public override void CropBonusYield(FarmLand tile, out int cropBonus)
    {
        cropBonus = (tile.growthStage - 1);
    }
}
