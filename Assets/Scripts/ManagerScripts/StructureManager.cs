using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class StructureManager : MonoBehaviour
{
    public static StructureManager Instance;
    [Header("Tiles")]
    public Tilemap tileMap;
    public TileBase freeTile, occupiedTile;

    public List<StructureBehaviorScript> allStructs;

    public GameObject weedTile;

    //Game will compare the two to find out which tile position correlates with the nutrients associated with it.
    List<Vector3Int> allTiles = new List<Vector3Int>();
    List<NutrientStorage> storage = new List<NutrientStorage>();

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }
        //load in all the saved data, such as the nutrient storages and alltiles list
        PopulateWeeds(5, 20); //Only do this when a new game has started. Implement weeds spawning in overtime
    }

    public void HourUpdate()
    {
        foreach (StructureBehaviorScript structure in allStructs)
        {
            structure.HourPassed();
        }
        PopulateWeeds(-9, 3);
    }


    public Vector3 CheckTile(Vector3 pos)
    {
        Vector3Int gridPos = tileMap.WorldToCell(pos);

        TileBase currentTile = tileMap.GetTile(gridPos);

        if(currentTile != null && currentTile == freeTile)
        {
            //
            Vector3 spawnPos = tileMap.GetCellCenterWorld(gridPos);
            return spawnPos;
        } 
        else return new Vector3 (0,0,0);
    }

    public void SpawnStructure(GameObject obj, Vector3 pos)
    {
        Instantiate(obj, pos, Quaternion.identity);
        Vector3Int gridPos = tileMap.WorldToCell(pos);
        tileMap.SetTile(gridPos, occupiedTile);
    }

    public void SetTile(Vector3 pos)
    {
        Vector3Int gridPos = tileMap.WorldToCell(pos);
        tileMap.SetTile(gridPos, occupiedTile);
    }

    public void Set4Tile(Vector3 pos) //to set a cube of tiles for big structures, currently unimplemented
    {
        Vector3Int gridPos = tileMap.WorldToCell(pos);
        tileMap.SetTile(gridPos, occupiedTile);
    }

    public void ClearTile(Vector3 pos)
    {
        Vector3Int gridPos = tileMap.WorldToCell(pos);
        print(tileMap.GetTile(gridPos));
        tileMap.SetTile(gridPos, freeTile);
    }

    public NutrientStorage FetchNutrient(Vector3 pos)
    {
        Vector3Int gridPos = tileMap.WorldToCell(pos);
        for(int i = 0; i < allTiles.Count; i++)
        {
            if(allTiles[i] == gridPos)
            {
                if(storage[i] != null) return storage[i];
                else
                {
                    storage[i] = new NutrientStorage();
                    return storage[i];
                }
            } 
        }
        //if its not in the list
        allTiles.Add(gridPos);
        NutrientStorage newStorage = new NutrientStorage();
        storage.Add(newStorage);
        return newStorage;
    }

    public void UpdateStorage(Vector3 pos, NutrientStorage s)
    {
        Vector3Int gridPos = tileMap.WorldToCell(pos);
        for(int i = 0; i < allTiles.Count; i++)
        {
            if(allTiles[i] == gridPos) storage[i].LoadStorage(storage[i], s.ichorLevel, s.terraLevel, s.gloamLevel, s.waterLevel);
        }
    }

    void PopulateWeeds(int min, int max)
    {
        List<Vector3Int> spawnablePositions = new List<Vector3Int>();

        Vector3 spawnPos = new Vector3 (0,0,0);
        foreach (Vector3Int position in tileMap.cellBounds.allPositionsWithin)
        {
            spawnablePositions.Add(position);
        }

        int r = Random.Range(min,max);
        if (r <= 0) return;
        for(int i = 0; i < r; i++)
        {
            if(spawnablePositions.Count != 0)
            {
                int randomIndex = Random.Range(0, spawnablePositions.Count);
                spawnPos = tileMap.GetCellCenterWorld(spawnablePositions[randomIndex]);

                if(tileMap.GetTile(spawnablePositions[randomIndex]) != null)
                {
                    SpawnStructure(weedTile, spawnPos);
                }
            }
        }
    }

}

[System.Serializable]
public class NutrientStorage
{
    public float ichorLevel = 10; //max is 10
    public float terraLevel = 10; //max is 10
    public float gloamLevel = 10; //max is 10

    public float waterLevel = 5; //max is 5

    public NutrientStorage()
    {
        ichorLevel = 10; 
        terraLevel = 10; 
        gloamLevel = 10; 
        waterLevel = 5;
    }

    public void ResetStorage(NutrientStorage s)
    {
        s.ichorLevel = 10;
        s.terraLevel = 10;
        s.gloamLevel = 10;
        s.waterLevel = 5;
    }
    public void LoadStorage(NutrientStorage s, float i, float t, float g, float w)
    {
        s.ichorLevel = i;
        s.terraLevel = t;
        s.gloamLevel = g;
        s.waterLevel = w;
    }
}
