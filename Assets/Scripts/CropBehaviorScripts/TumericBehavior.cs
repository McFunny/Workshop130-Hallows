using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crop Behavior", menuName = "Crop Behavior/Tumeric")]
public class TumericBehavior : CropBehavior
{
    float range = 1.5f;
    public override void OnWatered(FarmLand tile)
    {
        Collider[] hitColliders = Physics.OverlapSphere(tile.transform.position, range);
        foreach(Collider collider in hitColliders)
        {
            FarmLand foundTile = collider.gameObject.GetComponentInParent<FarmLand>();
            if(foundTile && foundTile.crop)
            {
                foundTile.WaterCrops();
            }
        }
    }

    public override void OnCropDestroyed(FarmLand tile)
    {
        Collider[] hitColliders = Physics.OverlapSphere(tile.transform.position, range);
        foreach(Collider collider in hitColliders)
        {
            FarmLand foundTile = collider.gameObject.GetComponentInParent<FarmLand>();
            if(foundTile && foundTile.crop)
            {
                if(foundTile.harvestable) foundTile.StructureInteraction();
                else foundTile.TakeStressDamage(5);
            }
        }
    }

    public override void OnHarvest(FarmLand tile, bool usedShovel, bool usedScythe)
    {
        Collider[] hitColliders = Physics.OverlapSphere(tile.transform.position, range);
        foreach(Collider collider in hitColliders)
        {
            FarmLand foundTile = collider.gameObject.GetComponentInParent<FarmLand>();
            if(foundTile && foundTile.crop)
            {
                if(foundTile.harvestable) foundTile.StructureInteraction();
                else foundTile.TakeStressDamage(5);
            }
        }
    }

    public override void OnPollinate(FarmLand tile)
    {
        Collider[] hitColliders = Physics.OverlapSphere(tile.transform.position, range);
        foreach(Collider collider in hitColliders)
        {
            FarmLand foundTile = collider.gameObject.GetComponentInParent<FarmLand>();
            if(foundTile && foundTile.crop && !foundTile.isPollinated)
            {
                foundTile.Pollinate();
            }
        }
    }
}
