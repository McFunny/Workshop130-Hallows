using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CreatureKilledByHoe", menuName = "AchievementObjects/Misc/CreatureKilledByHoe", order = 1)]
public class CreatureKilledByHoe : AchievementObject
{
    public override void OnCreatureKilledByHoe()
    {
        AddProgress(1f);
    }
}
