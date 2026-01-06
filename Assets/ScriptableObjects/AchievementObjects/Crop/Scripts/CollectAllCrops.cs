using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "CollectAllCrops", menuName = "AchievementObjects/Crops/CollectAllCrops", order = 1)]
public class CollectAllCrops : AchievementObject
{

    private CodexEntries[] cropEntries;

    public override void OnCropHarvest(CropData harvestedCrop)
    {
        cropEntries = Resources.LoadAll<CodexEntries>("Codex/Plants/");

        maxProgress = cropEntries.Length;

        ResetProgress();

        foreach (CodexEntries entry in cropEntries)
        {
            if (entry.unlocked)
            {
                AddProgress(1f);
            }
        }
    }
}