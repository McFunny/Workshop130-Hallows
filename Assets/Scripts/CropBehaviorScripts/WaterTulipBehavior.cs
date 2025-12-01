using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crop Behavior", menuName = "Crop Behavior/Water Lily")]
public class WaterTulipBehavior : CropBehavior
{
    public GameObject waterLily;

    /*public override bool IsFlammable()
    {
        return false;
    }*/

    public override void OnCropDestroyed(FarmLand tile)
    {
        if(tile.growthStage < 3) return;
        Instantiate(waterLily, new Vector3(tile.transform.position.x, tile.transform.position.y + 0.25f, tile.transform.position.z), Quaternion.identity);
    }

    public override void OnFrost(FarmLand tile)
    {
        tile.TakeStressDamage(5);
    }
}
