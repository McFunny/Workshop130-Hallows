using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorruptionNode : StructureBehaviorScript
{
    public GameObject corruptedTile;

    public float radius = 10;
    public int maxTilesPerHour = 3;
    public int maxTilesPerHourCozy = 2;
    float tileSpawnChance = 35; // out of 100

    public ParticleSystem activatedParticles;

    // Start is called before the first frame update
    void Start()
    {
        TimeManager.OnHourlyUpdate += HourUpdate;

        //StartCoroutine(TestSpread()); // Debugging only
        StartCoroutine(LateStart()); 

        base.Start();
    }

    public void HourUpdate()
    {
        if(!TimeManager.Instance.isDay && CorruptionManager.Instance.corruptedTiles < CorruptionManager.Instance.maxCorruption)
        {
            StartCoroutine(TrySpawnTile());
        }
    }

    IEnumerator LateStart()
    {
        yield return new WaitForSeconds(2f);
        CheckTile();
    }

    void CheckTile() //make sure there is an infected tile nearby as well as the tile does not currently have another structure occupying it
    {
        Collider[] nearbyTiles = Physics.OverlapSphere(transform.position, 1, 1 << 6);
        for(int i = 0; i < nearbyTiles.Length; i++)
        {
            CorruptedTile tile = nearbyTiles[i].gameObject.GetComponentInParent<CorruptedTile>();
            if(tile)
            {
                if(tile.containedStructure && tile.containedStructure != this) Destroy(gameObject);
                return;
            }
        }
        StructureManager.Instance.SpawnStructure(corruptedTile, transform.position);
    }

    IEnumerator TestSpread() // Debugging only
    {
        while(health > 0)
        {
            yield return new WaitForSeconds(Random.Range(10, 12));
            StartCoroutine(TrySpawnTile());
        }
    }

    IEnumerator TrySpawnTile()
    {
        int currentMaxTilesPerHour = maxTilesPerHour;
        if(MainMenuScript.currentFileMode == FileMode.Cozy) currentMaxTilesPerHour = maxTilesPerHourCozy;
        //spawn tiles in a nearby radius
        List<Vector3> tileSpots = StructureManager.Instance.GetNearbyClearTiles(transform.position, radius); 
        if(tileSpots.Count == 0)
        {
            print("No free tiles found");
            yield break;
        } 
        int tilesSpawned = 0;
        foreach(Vector3 tilePos in tileSpots)
        {
            yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));
            if(Random.Range(0f,100f) <= tileSpawnChance)
            {
                Collider[] nearbyTiles = Physics.OverlapSphere(tilePos, 4, 1 << 6); //To ensure new tiles are made near other ones
                foreach(Collider collider in nearbyTiles)
                {
                    CorruptedTile tile = collider.gameObject.GetComponentInParent<CorruptedTile>();
                    if(tile)
                    {
                        if(StructureManager.Instance.CheckTile(tilePos) == Vector3.zero) break; //This once free tile is now occupied
                        StructureManager.Instance.SpawnStructure(corruptedTile, tilePos);
                        tilesSpawned++;
                        activatedParticles.Play();
                        if(tilesSpawned >= currentMaxTilesPerHour) yield break; //Placed enough tiles. Done
                        break;
                    }
                }

            }
        }
    }

    void OnDestroy()
    {
        TimeManager.OnHourlyUpdate -= HourUpdate;
        base.OnDestroy();
    }
}
