using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SeeAllCreatures", menuName = "AchievementObjects/Creature/SeeAllCreatures", order = 1)]
public class SeeAllCreatures : AchievementObject
{
    private CodexEntries[] CreatureEntries;
    public override void OnCreatureKill(CreatureObject killedCreature)
    {

        CreatureEntries = Resources.LoadAll<CodexEntries>("Codex/Creatures/");
        maxProgress = CreatureEntries.Length;
        ResetProgress();
        foreach (CodexEntries entry in CreatureEntries)
        {
            if (entry.unlocked)
            {
                AddProgress(1f);
            }
        }

    }

}
