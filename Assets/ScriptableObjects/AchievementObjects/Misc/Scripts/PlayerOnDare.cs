using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerOnDare", menuName = "AchievementObjects/Misc/PlayerOnDare", order = 1)]
public class PlayerOnDare : AchievementObject
{
    public override void OnDareConsumed()
    {
        AddProgress(1f);
    }
}
