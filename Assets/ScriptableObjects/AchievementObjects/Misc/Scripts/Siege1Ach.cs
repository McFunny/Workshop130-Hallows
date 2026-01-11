using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Siege1Ach", menuName = "AchievementObjects/Misc/Siege1Ach", order = 1)]
public class Siege1Ach : AchievementObject
{

    public override void OnSiegeComplete(int siegeIndex)
    {
        if(siegeIndex == 1)
        {
            AddProgress(1f);
        }
    }
}

