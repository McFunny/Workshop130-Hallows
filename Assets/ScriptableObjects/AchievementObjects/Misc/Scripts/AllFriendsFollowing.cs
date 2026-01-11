using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AllFriendsFollowing", menuName = "AchievementObjects/Misc/AllFriendsFollowing", order = 1)]
public class AllFriendsFollowing : AchievementObject
{
    public override void OnAllFriendsAch()
    {
        AddProgress(1f);
    }
}
