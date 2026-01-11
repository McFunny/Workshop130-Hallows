using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HighCrowKill", menuName = "AchievementObjects/Creature/HighCrowKill", order = 1)]
public class HighCrowKill : AchievementObject
{
    public override void OnHighCrowKill()
    {
        AddProgress(1f);
    }
}
