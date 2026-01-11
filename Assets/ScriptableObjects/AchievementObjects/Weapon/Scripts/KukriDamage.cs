using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "KukriDamage", menuName = "AchievementObjects/Weapon/KukriDamage", order = 1)]
public class KukriDamage : AchievementObject
{
    public override void On150KukriKill()
    {
        AddProgress(1f);
    }
}
