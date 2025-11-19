using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crop Behavior", menuName = "Crop Behavior/Mimic Trap")]
public class MimicTrapBehavior : CropBehavior
{
    public override void OnCropDestroyed(FarmLand tile)
    {
        Collider[] hitCreatures = Physics.OverlapSphere(tile.transform.position, 5f, 1 << 9);
        foreach(Collider collider in hitCreatures)
        {
            //apply effect
        }
    }
}
