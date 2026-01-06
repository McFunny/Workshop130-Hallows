using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
