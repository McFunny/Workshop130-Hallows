using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crop Behavior", menuName = "Crop Behavior/GeneralCrop")]
public class GeneralCropBehavior : CropBehavior
{
    public int stagesReverted = 3;
    public bool multipleHarvests = false;

    public override bool DestroyOnHarvest(FarmLand tile, out int stagesReduced)
    {
        stagesReduced = stagesReverted;
        return false;
    }
}
