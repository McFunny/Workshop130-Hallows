using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crop Behavior", menuName = "Crop Behavior/Mandrake")]
public class MandrakeCropBehavior : CropBehavior
{
    public GameObject mandrake;
    public override void OnHour(FarmLand tile)
    {
        if(TimeManager.Instance.isDay == false && tile.crop.growthStages == tile.growthStage)
        {
            float r = Random.Range(0, 4);
            if(r <= 3)
            {
                tile.StartCoroutine(SpawnMandrake(tile));
            }
        }
    }

    IEnumerator SpawnMandrake(FarmLand tile)
    {
        Debug.Log("Spawning");
        float r = Random.Range(0.1f, 15);
        yield return new WaitForSeconds(r);
        if(tile.crop && tile.ShouldIgnoreNextGrowth() == false) 
        {
            Instantiate(mandrake, tile.transform.position, Quaternion.identity);
            tile.CropDestroyed();
        }
        //should limit how many can spawn: if there already is over 5 mandrakes, add code to stop more

    }
}
