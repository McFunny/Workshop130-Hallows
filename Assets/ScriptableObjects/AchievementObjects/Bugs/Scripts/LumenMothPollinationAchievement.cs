using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LumenMothPollinationAchievement", menuName = "AchievementObjects/Bugs/LumenMothPollinationAchievement", order = 1)]
public class LumenMothPollinationAchievement : AchievementObject
{
    public override void OnCropPollinated()
    {
        AddProgress(1f);    
    }
}
