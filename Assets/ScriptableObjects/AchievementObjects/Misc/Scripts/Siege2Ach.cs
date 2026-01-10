using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Siege2Ach", menuName = "AchievementObjects/Misc/Siege2Ach", order = 1)]
public class Siege2Ach : AchievementObject
{
    public override void OnSiegeComplete(int siegeIndex)
    {
        if (siegeIndex == 2)
        {
            AddProgress(1f);
        }
    }
}
