using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MistwalkerKills", menuName = "AchievementObjects/Creature/MistwalkerKills", order = 1)]
public class MistwalkerKills : AchievementObject
{
    public CreatureObject mistwalkerData;
    public override void OnCreatureKill(CreatureObject killedCreature)
    {
        if (killedCreature == mistwalkerData)
        {
            AddProgress(1f);
        }
    }
}

