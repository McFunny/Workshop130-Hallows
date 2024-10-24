using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crop Behavior", menuName = "Crop Behavior/Mandrake")]
public class MandrakeCropBehavior : CropBehavior
{
    public GameObject mandrake;
    public override void OnHour(FarmLand tile)
    {
        Debug.Log("OnHour");
        if(TimeManager.isDay == false)
        {
            float r = Random.Range(0, 4);
            float probability = -1;
            if(TimeManager.currentHour > 20)
            {
                probability = 1;
            }
            else if (TimeManager.currentHour != 20)
            {
                probability = TimeManager.currentHour + 1;
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

    }
}
