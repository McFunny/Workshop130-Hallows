using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crop Behavior", menuName = "Crop Behavior/MysteryCrop")]
public class MysteryCropBehavior : CropBehavior
{
    public CropData[] possibleCrops;
    public override void OnFullyGrown(FarmLand tile)
    {

    }
}
