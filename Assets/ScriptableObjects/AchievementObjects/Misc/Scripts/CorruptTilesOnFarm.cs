using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CorruptTilesOnFarm", menuName = "AchievementObjects/Misc/CorruptTilesOnFarm", order = 1)]
public class CorruptTilesOnFarm : AchievementObject
{
    public override void CheckProgress(float number = 1)
    {
        if (CorruptionManager.Instance != null)
        {
            if (CorruptionManager.Instance.corruptedTiles == 150f)
            {
                AddProgress(1f);
            }
            else
            {
                return;
            }

        }
    }
}