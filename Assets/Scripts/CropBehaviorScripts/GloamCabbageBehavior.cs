using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crop Behavior", menuName = "Crop Behavior/GloamCabbage")]
public class GloamCabbageBehavior : CropBehavior
{
    public int newMaxHealth = 25;
    public override void OnPlanted(FarmLand tile)
    {
        tile.maxHealth = newMaxHealth;
        tile.health = tile.maxHealth;
    }

    public override void OnHour(FarmLand tile)
    {
        tile.maxHealth = newMaxHealth;
        tile.health = tile.maxHealth;
    }
}
