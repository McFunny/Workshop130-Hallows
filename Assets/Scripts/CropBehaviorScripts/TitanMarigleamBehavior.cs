using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crop Behavior", menuName = "Crop Behavior/Titan Marigleam")]
public class TitanMarigleamBehavior : CropBehavior
{
    public override void OnHour(FarmLand tile)
    {
        if(TimeManager.Instance.isDay == true && TimeManager.Instance.currentHour != 6)
        {
            tile.CropDied();
        }
    }

    public override void OnPlanted(FarmLand tile)
    {
        if(SiegeManager.Instance) SiegeManager.Instance.siegeCropOnFarm = true;
        GameSaveData.Instance.siegeCropInHand = false;
    }

    public override void OnCropDestroyed(FarmLand tile)
    {
        if(SiegeManager.Instance) SiegeManager.Instance.siegeCropOnFarm = false;
    }

    public override void OnHarvest(FarmLand tile, bool usedShovel, bool usedScythe)
    {
        SiegeManager.Instance.siegeCropOnFarm = false;
        GameSaveData.Instance.siege1Cleared = true;
        QuestManager.Instance.ForceCompleteQuest(QuestDatabase.Instance.MainQuests[9]);
    }
}
