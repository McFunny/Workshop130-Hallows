using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Siege4Ach", menuName = "AchievementObjects/Misc/Siege4Ach", order = 1)]
public class Siege4Ach : AchievementObject
{

    public override void OnSiegeComplete(int siegeIndex)
    {
        if (siegeIndex == 4)
        {
            AddProgress(1f);
        }
    }
}

