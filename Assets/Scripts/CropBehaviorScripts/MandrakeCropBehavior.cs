using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crop Behavior", menuName = "Crop Behavior/Mandrake")]
public class MandrakeCropBehavior : CropBehavior
{
    public GameObject mandrake;
    public override void OnHour(FarmLand tile)
    {
        //BUG WHERE IN BUILD, THE MANDRAKE LEAVES IMMEDIATLY WHEN PLANTED
        Debug.Log(TimeManager.Instance.isDay);
        if(TimeManager.Instance.isDay == false && tile.crop.growthStages == tile.growthStage)
        {
            float r = Random.Range(0, 4);
            float probability = -1;
            if(TimeManager.Instance.currentHour > 20)
            {
                probability = 1;
            }
            else if (TimeManager.Instance.currentHour != 20)
            {
                probability = TimeManager.Instance.currentHour + 1;
            }
            //if(r <= probability)
            //{
                tile.StartCoroutine(SpawnMandrake(tile));
            //}
        }
    }

    IEnumerator SpawnMandrake(FarmLand tile)
    {
        Debug.Log("Spawning");
        float r = Random.Range(0.1f, 15);
        yield return new WaitForSeconds(r);
        Instantiate(mandrake, tile.transform.position, Quaternion.identity);
        tile.CropDestroyed();
        //should limit how many can spawn: if there already is over 5 mandrakes, add code to stop more

    }
}
