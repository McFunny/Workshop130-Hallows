using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crop Behavior", menuName = "Crop Behavior/Root Node")]
public class RootNodeBehavior : CropBehavior
{
    public List<CropData> siegeCrops;
    public override void OnCropDestroyed(FarmLand tile)
    {
        Collider[] hitStructures = Physics.OverlapSphere(tile.transform.position, 80f, 1 << 6);
        foreach(Collider collider in hitStructures)
        {
            FarmLand siegeFlowerTile = collider.gameObject.GetComponentInParent<FarmLand>();
            if(siegeFlowerTile && siegeFlowerTile.crop && siegeCrops.Contains(siegeFlowerTile.crop))
            {
                siegeFlowerTile.TakeStressDamage(1);
                return;
            }
        }
    }

    public override void OnHour(FarmLand tile)
    {
        tile.DrainNutrients(out bool gainedStress, false);
        if(gainedStress && tile.growthImpeded) tile.growthImpeded.Play();
    }
}
