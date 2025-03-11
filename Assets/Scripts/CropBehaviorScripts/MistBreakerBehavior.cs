using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crop Behavior", menuName = "Crop Behavior/MistBreaker")]
public class MistBreaker : CropBehavior
{
    public override void OnCropDestroyed(FarmLand tile)
    {
        Debug.Log("Finale Turned Off");
        NightSpawningManager.Instance.DeactivateFinale();
    }

    public override void OnHour(FarmLand tile)
    {
        //Call Creatures to this
    }
}
