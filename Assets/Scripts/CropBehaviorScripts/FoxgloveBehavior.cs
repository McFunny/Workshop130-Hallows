using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crop Behavior", menuName = "Crop Behavior/Foxglove")]
public class FoxgloveBehavior : CropBehavior
{
    public override void OnConsumed(CreatureBehaviorScript creature)
    {
        creature.TakeDamage(100);
    }

    public override void CropBonusYield(FarmLand tile, out int cropBonus, out int secondaryCropBonus)
    {
        cropBonus = 0;
        secondaryCropBonus = 1;
    }
}
