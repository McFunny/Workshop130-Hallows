using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CorruptedTile : StructureBehaviorScript
{
    public GameObject[] extraTiles;
    // Start is called before the first frame update
    void Start()
    {
        transform.rotation = Quaternion.Euler(new Vector3(0,0,0));
        base.Start();

        UpdateModel();

        StructureBehaviorScript.OnStructuresUpdated += UpdateModel;
        StartCoroutine(LateStart());

        StartCoroutine(TestSpread());
        StartCoroutine(IdleSounds());
        
    }

    IEnumerator LateStart()
    {
        yield return new WaitForSeconds(1);
        UpdateModel();
    }

    IEnumerator TestSpread()
    {
        while(health > 0)
        {
            yield return new WaitForSeconds(Random.Range(6, 12));
            SpreadTiles();
        }
    }

    IEnumerator IdleSounds()
    {
        while(health > 0)
        {
            yield return new WaitForSeconds(Random.Range(5, 12));
            audioHandler.GetSource().enabled = true;
            audioHandler.PlayRandomSound(audioHandler.miscSounds1);
            yield return new WaitForSeconds(4);
            audioHandler.GetSource().enabled = false;
        }
    }

    void SpreadTiles()
    {
        List<Vector3> tileSpots = StructureManager.Instance.GetAdjacentClearTiles(transform.position); 
        if(tileSpots.Count == 0)
        {
            return;
        } 

        foreach(Vector3 tilePos in tileSpots)
        {
            if(Random.Range(0f,10f) > 6f)
            {
                StructureManager.Instance.SpawnStructure(structData.objectPrefab, tilePos);
                break;
            }
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

    void UpdateModel()
    {
        Tilemap currentMap = StructureManager.Instance.farmTileMap;
        StructureBehaviorScript foundTile = null;
        StructureManager manager = StructureManager.Instance;

        Vector3Int gridPos = currentMap.WorldToCell(transform.position);

        foundTile = manager.GetStructureOnPosition(currentMap.GetCellCenterWorld(new Vector3Int(gridPos.x + 1, gridPos.y))); //right
        if(foundTile != null && foundTile.TryGetComponent(out CorruptedTile f1)) extraTiles[0].SetActive(false);
        else extraTiles[0].SetActive(true);

        foundTile = manager.GetStructureOnPosition(currentMap.GetCellCenterWorld(new Vector3Int(gridPos.x - 1, gridPos.y))); //left
        if(foundTile != null && foundTile.TryGetComponent(out CorruptedTile f2)) extraTiles[1].SetActive(false);
        else extraTiles[1].SetActive(true);

        foundTile = manager.GetStructureOnPosition(currentMap.GetCellCenterWorld(new Vector3Int(gridPos.x, gridPos.y + 1))); //up
        if(foundTile != null && foundTile.TryGetComponent(out CorruptedTile f3)) extraTiles[2].SetActive(false);
        else extraTiles[2].SetActive(true);

        foundTile = manager.GetStructureOnPosition(currentMap.GetCellCenterWorld(new Vector3Int(gridPos.x, gridPos.y - 1))); //down
        if(foundTile != null && foundTile.TryGetComponent(out CorruptedTile f4)) extraTiles[3].SetActive(false);
        else extraTiles[3].SetActive(true);
    }

    void OnDestroy()
    {
        base.OnDestroy();
        StructureBehaviorScript.OnStructuresUpdated -= UpdateModel;
        if(!gameObject.scene.isLoaded) return;
    }
}
