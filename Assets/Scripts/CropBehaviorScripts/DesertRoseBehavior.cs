using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crop Behavior", menuName = "Crop Behavior/DesertRose")]
public class DesertRoseBehavior : CropBehavior
{
    public override void OnWatered(FarmLand tile)
    {
        tile.TakeStressDamage(5);
    }
}
