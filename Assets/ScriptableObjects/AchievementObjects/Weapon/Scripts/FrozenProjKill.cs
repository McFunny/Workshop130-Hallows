using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FrozenProjKill", menuName = "AchievementObjects/Weapon/FrozenProjKill", order = 1)]
public class FrozenProjKill : AchievementObject
{
    public override void OnFrozenProjectileKill()
    {
        AddProgress(1f);
    }
}
