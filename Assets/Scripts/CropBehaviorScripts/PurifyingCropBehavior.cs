using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crop Behavior", menuName = "Crop Behavior/Purifying Crop")]
public class PurifyingCropBehavior : CropBehavior
{
    public override void OnGrowth(FarmLand tile)
    {
        CorruptionManager.Instance.StartCoroutine(CleanCorruption(tile));
    }

    IEnumerator CleanCorruption(FarmLand tile)
    {
        float range = 4;
        float maxTiles = 1;
        float currentTiles = 0;

        if(tile.growthStage < 3);
        else if(tile.growthStage < 5)
        {
            range += 2;
            maxTiles += 1;
        }
        else
        {
            range += 2;
            maxTiles += 3;
        }

        Collider[] nearbyTiles = Physics.OverlapSphere(tile.transform.position, range, 1 << 6);
        for(int i = 0; i < nearbyTiles.Length; i++)
        {
            CorruptedTile c_tile = nearbyTiles[i].gameObject.GetComponentInParent<CorruptedTile>();
            if(c_tile && !c_tile.beingCleansed)
            {
                yield return new WaitForSeconds(Random.Range(0.3f, 2f));
                c_tile.StartCoroutine(c_tile.CleanseRoutine());
                currentTiles++;
                if(currentTiles >= maxTiles) yield break;
            }
        }
    }
}
