using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crop Behavior", menuName = "Crop Behavior/GloamCabbage")]
public class GloamCabbageBehavior : CropBehavior
{
    public int extraHealth = 15;
    public override void OnPlanted(FarmLand tile)
    {
        tile.maxHealth += extraHealth;
        tile.health = tile.maxHealth;
    }

    /*public override void OnHour(FarmLand tile)
    {
        tile.maxHealth = extraHealth;
        tile.health = tile.maxHealth;
    }*/

    public override void OnCropDestroyed(FarmLand tile)
    {
        tile.maxHealth -= extraHealth;
        tile.health = tile.maxHealth;
    }

    /*public override void OnGrowth(FarmLand tile)
    {
        tile.health += 5;
        if(tile.health > tile.maxHealth) tile.health = tile.maxHealth;
    }*/
}
