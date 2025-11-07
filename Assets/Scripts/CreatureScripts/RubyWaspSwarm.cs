using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RubyWaspSwarm : CreatureBehaviorScript
{
    public List<GameObject> wasps = new List<GameObject>();

    public int waspMin, waspMax;
    public GameObject waspPrefab;

    // Start is called before the first frame update
    void Start()
    {
        base.Start();

        int waspsToSpawn = Random.Range(waspMin, waspMax + 1);

        for(int i = 0; i < waspsToSpawn; i++)
        {
            RubyWasp wasp = Instantiate(waspPrefab, transform.position, Quaternion.identity).GetComponentInParent<RubyWasp>();
            wasp.homeSwarm = this;
            wasps.Add(wasp.gameObject);
        }

        StartCoroutine(MoveAround());
    }

    IEnumerator MoveAround()
    {
        while(wasps.Count > 0)
        {
            yield return new WaitForSeconds(Random.Range(10, 20));
            if(MoveToSpecialTarget() == false)transform.position = StructureManager.Instance.GetRandomNearbyTile(GridType.Farm, 40, transform.position);
        }
        Destroy(gameObject);
    }

    bool MoveToSpecialTarget()
    {
        if(Random.Range(0,10) > 6) return false;

        List<Vector3> flowerPos = new List<Vector3>();
        foreach(StructureBehaviorScript s in StructureManager.Instance.allStructs)
        {
            FarmLand tile = s as FarmLand;
            if(tile && tile.crop && tile.crop.id == 20)
            {
                flowerPos.Add(tile.transform.position);
                continue;
            }

            CandleCluster candle = s as CandleCluster;
            if(candle && candle.type == CandleType.Aroma && candle.burning)
            {
                flowerPos.Add(tile.transform.position);
                continue;
            }
        }

        if(flowerPos.Count > 0)
        {
            transform.position = flowerPos[Random.Range(0, flowerPos.Count)];
            return true;
        }
        else return false;
    }
}
