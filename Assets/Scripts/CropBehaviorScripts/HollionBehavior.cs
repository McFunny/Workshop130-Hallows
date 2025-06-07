using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crop Behavior", menuName = "Crop Behavior/Hollion")]
public class HollionBehavior : CropBehavior
{
    public GameObject hollionBullet;

    public override void OnFullyGrown(FarmLand tile)
    {
        Debug.Log("I fully grown");
        tile.ForceChangeGrowthStage(3);

        int berriesToSpawn = Random.Range(3, 8);
        for(int i = 0; i < berriesToSpawn; i++)
        {
            Vector3 spawnPos = new Vector3(tile.transform.position.x, tile.transform.position.y + Random.Range(1.2f, 2.2f), tile.transform.position.z);
            Rigidbody rb = Instantiate(hollionBullet, spawnPos, Quaternion.identity).GetComponent<Rigidbody>();

            Vector3 dir;// = new Vector3(rb.transform.position.x + Random.Range(-5f, 5f), 0, rb.transform.position.z + Random.Range(-5f, 5f)).normalized;
            dir = Random.onUnitSphere;
            dir.y = 0;

            rb.AddForce(dir * Random.Range(5, 10), ForceMode.Impulse);
        }
    }

    public override bool DestroyOnHarvest()
    {
        return false;
    }

}
