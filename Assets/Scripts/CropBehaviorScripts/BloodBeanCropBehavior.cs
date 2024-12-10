using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crop Behavior", menuName = "Crop Behavior/BloodBean")]
public class BloodBeanCropBehavior : CropBehavior
{
    public override bool DestroyOnHarvest()
    {
        return false;
    }
}
