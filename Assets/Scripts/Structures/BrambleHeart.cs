using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BrambleHeart : StructureBehaviorScript
{
    public StructureObject woodBarricade;

    float range = 25;

    public Transform heart;

    void Start()
    {
        base.Start();
        StartCoroutine(HeartBeat());
    }

    public override void HourPassed()
    {
        //if(TimeManager.Instance.isDay) return;

        Collider[] nearbyWeeds = Physics.OverlapSphere(transform.position, range, 1 << 6);
        foreach(Collider collider in nearbyWeeds)
        {
            FarmLand tile = collider.gameObject.GetComponentInParent<FarmLand>();

            if(tile && tile.isWeed && tile.growthStage != 7 && Random.Range(0f, 10f) >= 9.4f)
            {
                if(Random.Range(0,10) == 9)
                {
                    //kill the weed and spawn the barrier
                    tile.clearTileOnDestroy = false;
                    Vector3 spawnPos = tile.transform.position;
                    Destroy(tile.gameObject);
                    GameObject newObject = Instantiate(woodBarricade.objectPrefab, spawnPos, Quaternion.identity);
                    newObject.transform.localEulerAngles = new Vector3(0, Random.Range(0,360), 0);
                }
                else
                {
                    //convert the weed
                    tile.growthStage = 7;
                    tile.SpriteChange();
                }
            }
        }
    }

    IEnumerator HeartBeat()
    {
        //Vector3 originalScale = heart.localScale;
        Vector3 newScale = new Vector3(0.4f,0.4f,0.4f);
        while(health > 0)
        {
            heart.DOPunchScale(newScale, 1f, 0, 0.5f);
            audioHandler.PlaySound(audioHandler.interactSound);
            yield return new WaitForSeconds(1.2f);

            //heart.localScale = originalScale;
        }
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(type == ToolType.Shovel)
        {
            //StartCoroutine(DugUp());
            success = true;
        }
    }

    void OnDestroy()
    {
        base.OnDestroy();
        if(!gameObject.scene.isLoaded) return;

        Collider[] nearbyWeeds = Physics.OverlapSphere(transform.position, range, 1 << 6);
        foreach(Collider collider in nearbyWeeds)
        {
            FarmLand tile = collider.gameObject.GetComponentInParent<FarmLand>();

            if(tile && tile.isWeed && tile.growthStage == 7)
            {
                tile.growthStage = 5;
                tile.SpriteChange();
            }
        }
    }
}
