using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crop Behavior", menuName = "Crop Behavior/Pinex")]
public class PinexBehavior : CropBehavior
{
    public override bool DestroyOnHarvest(FarmLand tile)
    {
        return false;
    }

    public override void CropBonusYield(FarmLand tile, out int cropBonus, out int secondaryCropBonus)
    {
        int r = Random.Range(0,10);
        if(r >= 7)
        {
            if(r >= 8)
            {
                cropBonus = -1;
                secondaryCropBonus = 1;
            }
            else
            {
                cropBonus = 1;
                secondaryCropBonus = 0;
            }
        }
        else
        {
            cropBonus = 0;
            secondaryCropBonus = 0;
        }
    }
}
