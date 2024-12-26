using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crop Behavior", menuName = "Crop Behavior/Fog Chime")]
public class FogChimeBehavior : CropBehavior
{
    public override void OnHour(FarmLand tile)
    {
        if(TimeManager.Instance.isDay == true && TimeManager.Instance.currentHour != 6)
        {
            tile.CropDied();
        }
    }
}
