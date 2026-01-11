using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Siege3Ach", menuName = "AchievementObjects/Misc/Siege3Ach", order = 1)]
public class Siege3Ach : AchievementObject
{

    public override void OnSiegeComplete(int siegeIndex)
    {
        if (siegeIndex == 3)
        {
            AddProgress(1f);
        }
    }
}

