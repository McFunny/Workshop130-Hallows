using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "MultipleStuckWasps", menuName = "AchievementObjects/Creature/MultipleStuckWasps", order = 1)]
public class MultipleStuckWasps : AchievementObject
{
    public override void OnWaspsStuck()
    {
        AddProgress(1f);
    }
}
