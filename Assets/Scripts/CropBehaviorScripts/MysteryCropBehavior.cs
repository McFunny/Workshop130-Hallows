using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crop Behavior", menuName = "Crop Behavior/MysteryCrop")]
public class MysteryCropBehavior : CropBehavior
{
    public CropWithProbability[] possibleCrops;
    public override void OnFullyGrown(FarmLand tile)
    {
        tile.crop.amountHarvested++;
        int x = 0;
        CropData crop = null;
        while(crop == null)
        {
            int r = Random.Range(0, possibleCrops.Length);
            if(Random.Range(0,100) < possibleCrops[r]._probability) crop = possibleCrops[r]._crop;

            if(x > 20) crop = possibleCrops[0]._crop;
        }
        tile.crop = crop;
        tile.growthStage = 3;
        tile.harvestable = false;
        tile.SpriteChange();
    }
}
