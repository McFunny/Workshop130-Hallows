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
    }


    void Start()
    {
        
    }

    public void HourUpdate()
    {
        foreach (StructureBehaviorScript structure in allStructs)
        {
            structure.HourPassed();
        }
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
}
