using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New Crop Behavior", menuName = "Crop Behavior/Dare")]
public class DareBehavior : CropBehavior
{
    public override void OnConsumed(CreatureBehaviorScript creature)
    {
        creature.ApplyStatusEffect(StatusDatabase.Instance.GetStatus(StatusEffectName.Dare), 30);
    }

    public override void OnConsumedBeforeMaturity(CreatureBehaviorScript creature)
    {
        creature.ApplyStatusEffect(StatusDatabase.Instance.GetStatus(StatusEffectName.Dare), 15);
    }
}
