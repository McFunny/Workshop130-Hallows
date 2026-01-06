using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CatchAllBugs", menuName = "AchievementObjects/Bugs/CatchAllBugs", order = 1)]

public class CatchAllBugs : AchievementObject
{
    CodexEntries[] BugEntries;

    public override void OnBugCatch(BugObject caughtBug)
    {
        BugEntries = Resources.LoadAll<CodexEntries>("Codex/Bugs");
        maxProgress = BugEntries.Length;
        ResetProgress();
        foreach (CodexEntries entry in BugEntries)
        {
            if (entry.unlocked)
            {
                AddProgress(1f);
            }
        }
    }


}
