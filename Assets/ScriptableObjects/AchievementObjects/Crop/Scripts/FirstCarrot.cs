using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FirstCarrot", menuName = "AchievementObjects/Crop/FirstCarrot", order = 1)]
public class FirstCarrot : AchievementObject
{
    public CropData carrotData;

    public override void OnCropHarvest(CropData harvestedCrop)
    {
        if(harvestedCrop == carrotData)
        {
            AddProgress(1f);
        }
    }
}
