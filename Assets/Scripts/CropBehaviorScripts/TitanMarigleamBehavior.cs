using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crop Behavior", menuName = "Crop Behavior/Titan Marigleam")]
public class TitanMarigleamBehavior : CropBehavior
{
    public override void OnHour(FarmLand tile)
    {
        if(TimeManager.Instance.isDay == true && TimeManager.Instance.currentHour != 6 && tile.growthStage != 1) //Dies at morning, but not when its just planted
        {
            tile.CropDied();
        }
    }

    public override void OnPlanted(FarmLand tile)
    {
        GameSaveData.Instance.siegeCropInHand = false;
        if(!TimeManager.Instance.isDay)
        {
            tile.CropDied();
            return;
        }
        if(SiegeManager.Instance) SiegeManager.Instance.siegeCropOnFarm = true;
    }

    public override void OnCropDestroyed(FarmLand tile)
    {
        if(SiegeManager.Instance) SiegeManager.Instance.siegeCropOnFarm = false;
        //Maybe redrop the seed if it wasnt harvested?
    }

    public override void OnHarvest(FarmLand tile, bool usedShovel, bool usedScythe)
    {
        SiegeManager.Instance.siegeCropOnFarm = false;
        GameSaveData.Instance.siegesCleared++;
        switch(GameSaveData.Instance.siegesCleared)
        {
            case 1:
            QuestManager.Instance.ForceCompleteQuest(QuestDatabase.Instance.MainQuests[9]);
            QuestManager.Instance.AddQuest(QuestDatabase.Instance.MainQuests[10]);
            break;
            case 2:
            QuestManager.Instance.ForceCompleteQuest(QuestDatabase.Instance.MainQuests[10]);
            QuestManager.Instance.AddQuest(QuestDatabase.Instance.MainQuests[11]);
            break;
            case 3:
            QuestManager.Instance.ForceCompleteQuest(QuestDatabase.Instance.MainQuests[11]);
            QuestManager.Instance.AddQuest(QuestDatabase.Instance.MainQuests[12]);
            break;
            case 4:
            QuestManager.Instance.ForceCompleteQuest(QuestDatabase.Instance.MainQuests[12]);
            break;
            default:
            break;
        }
    }
}
