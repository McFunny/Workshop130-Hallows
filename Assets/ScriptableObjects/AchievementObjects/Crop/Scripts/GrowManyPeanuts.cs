using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GrowManyPeanuts", menuName = "AchievementObjects/Crop/GrowManyPeanuts")]
public class GrowManyPeanuts : AchievementObject
{
    public override void OnGrowHellaNuts()
    {
        AddProgress(1);
    }
}
