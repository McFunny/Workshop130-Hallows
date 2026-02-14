using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "KickTheBucket", menuName = "AchievementObjects/Creature/KickTheBucket", order = 1)]
public class KickTheBucket : AchievementObject
{
    public override void OnKickedBucket()
    {
        AddProgress(1f);
    }
}
