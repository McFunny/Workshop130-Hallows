using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorruptionManager : MonoBehaviour
{
    public static CorruptionManager Instance;

    public int corruptedTiles = 0;
    public int maxCorruption = 200;

    public GameObject corruptedTile, nodePrefab, farmTile;

    public StructureObject nodeData;

    int cropSpawnCooldown = 0;
    public CropData twistedFiberCrop;


    // Start is called before the first frame update
    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            print("Destroyed Copy");
            return;
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        TimeManager.OnHourlyUpdate += HourUpdate;

        //StartCoroutine(TestSpread()); // Debugging only
    }

    IEnumerator TestSpread() // Debugging only
    {
        while(Instance)
        {
            yield return new WaitForSeconds(Random.Range(6, 12));
            TrySpawnNode();
        }
    }

    public void HourUpdate()
    {
        if(TimeManager.Instance.currentHour == 8 && corruptedTiles > 10)
        {
            //StartCoroutine(StructureManager.Instance.PopulateStructure(-3, 5, weedTile, false, StructureManager.Instance.farmTileMap));
        }
        if(!TimeManager.Instance.isDay)
        {
            if(Random.Range(0,10) > 3) TrySpawnNode();
            if(cropSpawnCooldown > 0) --cropSpawnCooldown;
            else if(Random.Range(0, 10) > 7) TrySpawnCorruptWeeds();
        }
    }

    void TrySpawnNode()
    {
        ReturnFreeTileList(out List<CorruptedTile> cTiles);

        if(cTiles.Count == 0) return;
        int currentNodes = StructureManager.Instance.TallyStructure(nodeData);
        int maxNodes = (corruptedTiles/10) + 1; //How many nodes can be present on the farm
        if(currentNodes >= maxNodes) return;

        //Every hour try to spawn up to 10 nodes, making sure they spawn not too close to existing nodes
        int x = 0; //attempts
        int s = 0; //successful attempts
        int maxS = Random.Range(1, 4); //max successful attempts
        while(x < 10 && s < maxS && currentNodes < maxNodes)
        {
            bool spotTooClose = false;
            int index = Random.Range(0, cTiles.Count);
            Collider[] nearbyNodes = Physics.OverlapSphere(cTiles[index].transform.position, 7, 1 << 6);
            for(int i = 0; i < nearbyNodes.Length; i++)
            {
                CorruptionNode node = nearbyNodes[i].gameObject.GetComponentInParent<CorruptionNode>();
                if(node)
                {
                    spotTooClose = true;
                    break;
                }
            }

            if(spotTooClose)
            {
                x++; 
                continue;
            }

            //spawn node
            cTiles[index].containedStructure = Instantiate(nodePrefab, cTiles[index].transform.position, Quaternion.identity).GetComponent<StructureBehaviorScript>();

            cTiles.RemoveAt(index);
            x++;
            s++;
            currentNodes++;
        }
    }

    [ContextMenu("Test Weed Spawn")]
    void TrySpawnCorruptWeeds()
    {
        ReturnFreeTileList(out List<CorruptedTile> cTiles);

        if(cTiles.Count == 0) return;

        cropSpawnCooldown = Random.Range(4, 12);
        int fibersToSpawn = Random.Range(3, 7);

        Collider[] nearbyTiles = Physics.OverlapSphere(cTiles[Random.Range(0, cTiles.Count)].transform.position, 4.5f, 1 << 6);
        for(int i = 0; i < nearbyTiles.Length; i++)
        {
            if(Random.Range(0, 10) > 1)
            {
                CorruptedTile cTile = nearbyTiles[i].GetComponent<CorruptedTile>();
                cTile.containedStructure = Instantiate(farmTile, cTile.transform.position, Quaternion.identity).GetComponent<StructureBehaviorScript>();
                FarmLand tile = cTile.containedStructure as FarmLand;
                tile.ApplyNewUpgrade(FarmLand.FarmTileUpgrade.Corrupt);
                tile.InsertCrop(twistedFiberCrop);
                cTile.containedStructure.clearTileOnDestroy = false;
            }
        }
    }

    public void ReturnFreeTileList(out List<CorruptedTile> cTiles) //Returns an open corruption Tile
    {
        //List<Vector3> freeTiles = new List<Vector3>();
        cTiles = new List<CorruptedTile>();

        for(int i = 0; i < StructureManager.Instance.allStructs.Count; i++) //Collects all available corrupted tiles that are empty
        {
            CorruptedTile cTile = StructureManager.Instance.allStructs[i] as CorruptedTile;
            if(cTile && cTile.containedStructure == null)
            {
                //freeTiles.Add(cTile.transform.position);
                cTiles.Add(cTile);
            }
        }
    }

    public void CorruptionExplosion(Vector3 pos, float range)
    {
        Collider[] nearbyFarmTiles = Physics.OverlapSphere(pos, range, 1 << 6);

        for(int i = 0; i < nearbyFarmTiles.Length; i++) //Clear empty tiles
        {
            FarmLand farmLand = nearbyFarmTiles[i].gameObject.GetComponentInParent<FarmLand>();
            if(farmLand && (!farmLand.crop || Random.Range(0,10) > 5))
            {
                farmLand.TakeDamage(99);
            }
        }

        List<Vector3> tileSpots = StructureManager.Instance.GetNearbyClearTiles(pos, range); 
        if(tileSpots.Count == 0)
        {
            print("No free tiles found");
            return;
        } 

        foreach(Vector3 tilePos in tileSpots)
        {
            if(Random.Range(0f,100f) <= 90)
            {
                if(StructureManager.Instance.CheckTile(tilePos) == Vector3.zero) continue; //This once free tile is now occupied
                StructureManager.Instance.SpawnStructure(corruptedTile, tilePos);

            }
        }
    }

    public IEnumerator FinaleComplete()
    {
        for(int i = 0; i < StructureManager.Instance.allStructs.Count; ++i)
        {
            if(StructureManager.Instance.allStructs[i] == null) continue;
            CorruptedTile c = StructureManager.Instance.allStructs[i] as CorruptedTile;

            if(c)
            {
                yield return new WaitForSeconds(Random.Range(0.2f, 0.8f));
                c.health = 0;
                Destroy(c.gameObject);
            }
        }
    }

    public float CorruptedSpawnMod()
    {
        return 5 + (corruptedTiles/maxCorruption) * 70;
    }

    void OnDestroy()
    {
        TimeManager.OnHourlyUpdate -= HourUpdate;
        if(Instance != null && Instance == this) Instance = null;
    }
}
