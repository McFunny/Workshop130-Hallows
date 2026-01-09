using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WhenTheCrowSings", menuName = "AchievementObjects/Creature/WhenTheCrowSings", order = 1)]
public class WhenTheCrowSings : AchievementObject
{
    public CreatureObject crowData;

    public override void OnCreatureKill(CreatureObject killedCreature)
    {
        if(killedCreature == crowData)
        {
            AddProgress(1f);
        }
    }
}
