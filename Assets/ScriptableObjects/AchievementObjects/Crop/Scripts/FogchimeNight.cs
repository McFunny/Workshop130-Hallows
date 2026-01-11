using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FogchimeNight", menuName = "AchievementObjects/Crop/FogchimeNight")]
public class FogchimeNight : AchievementObject
{
    public CropData fogchimeCrop;

    private int harvestedThisNight;

    public override void OnHourlyUpdate(int hour)
    {
        if (hour == 6) // 
        {
            harvestedThisNight = 0;
            ResetProgress();
        }
    }

    public override void OnCropHarvest(CropData harvestedCrop)
    {
        if (TimeManager.Instance.isDay) return;

        if (harvestedCrop == fogchimeCrop)
        {
            harvestedThisNight++;
            AddProgress(1f);
        }
    }
}
