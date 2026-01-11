using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FinaleAch", menuName = "AchievementObjects/Misc/FinaleAch", order = 1)]
public class FinaleAch : AchievementObject
{
   public override void OnFinaleComplete()
   {
       AddProgress(1f);
    }
}
