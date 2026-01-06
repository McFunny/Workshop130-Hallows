using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HogKillsPyrefly", menuName = "AchievementObjects/Creature/HogKillsPyrefly", order = 1)]

public class HogKillsPyrefly : AchievementObject
{
    public CreatureObject vileHogData;
    public CreatureObject pyreflyData;
    public override void OnCreatureKillByOtherCreature(CreatureObject creatureKilled, CreatureObject killer)
    {
        if (creatureKilled == pyreflyData && killer == vileHogData)
        {
            AddProgress(1f);
        }
    }

}
