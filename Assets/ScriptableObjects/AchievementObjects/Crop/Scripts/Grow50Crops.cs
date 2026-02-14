using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Grow50Crops", menuName = "AchievementObjects/Crop/Grow50Crops", order = 1)]
public class Grow50Crops : AchievementObject
{
    public override void OnCropHarvest(CropData harvestedCrop)
    {
        AddProgress(1f);
    }
}
