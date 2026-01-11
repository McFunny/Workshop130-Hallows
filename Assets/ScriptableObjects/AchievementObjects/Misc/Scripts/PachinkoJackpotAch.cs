using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PachinkoJackpotAch", menuName = "AchievementObjects/Misc/PachinkoJackpot")]
public class PachinkoJackpotAch : AchievementObject
{
    public override void OnPachinkoJackpot()
    {
        AddProgress(1f);
    }
}
