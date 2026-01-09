using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SleepWithTorchLit", menuName = "AchievementObjects/Misc/SleepWithTorchLit", order = 1)]
public class SleepWithTorchLit : AchievementObject
{
   public override void SleepWithLitTorch()
    {
        AddProgress(1f);
    }
}
