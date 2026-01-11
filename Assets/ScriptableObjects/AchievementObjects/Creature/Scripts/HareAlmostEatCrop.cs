using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HareAlmostEatCrop", menuName = "AchievementObjects/Creature/HareAlmostEatCrop", order = 1)]
public class HareAlmostEatCrop : AchievementObject
{
    public override void OnHareAlmostDoneEatingDeath()
    {
        AddProgress(1);
    }
}
