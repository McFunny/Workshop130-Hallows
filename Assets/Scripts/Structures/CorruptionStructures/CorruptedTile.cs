using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CorruptedTile : StructureBehaviorScript
{
    public GameObject[] extraTiles;

    public StructureBehaviorScript containedStructure; //The structure that occupied this tile, such as a node

    [HideInInspector] public bool beingCleansed;
    // Start is called before the first frame update
    void Start()
    {
        transform.rotation = Quaternion.Euler(new Vector3(0,0,0));
        base.Start();

        UpdateModel();

        StructureBehaviorScript.OnStructuresUpdated += UpdateModel;
        StartCoroutine(LateStart());

        //StartCoroutine(TestSpread());
        StartCoroutine(IdleSounds());


        CorruptionManager.Instance.corruptedTiles++;
        
    }

    IEnumerator LateStart()
    {
        yield return new WaitForSeconds(1);
        UpdateModel();

        //Grab the top structure
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, 1, 1 << 6);
        foreach(Collider collider in nearbyColliders)
        {
            if(containedStructure) break;
            StructureBehaviorScript structure = collider.gameObject.GetComponentInParent<StructureBehaviorScript>();

            if(structure && structure != this)
            {
                if(structure.structData == structData)
                {
                    Destroy(gameObject); //Duplicate tile
                    yield break;
                }
                containedStructure = structure;
                containedStructure.clearTileOnDestroy = false;
            }
        }
    }

    IEnumerator TestSpread() // Debugging only
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
        /*
        if(type == ToolType.Shovel)
        {
            //StartCoroutine(DugUp());
            success = true;
        }
        else if(type == ToolType.WateringCan && !beingCleansed)
        {
            StartCoroutine(CleanseRoutine());
            success = true;
        }
        */
    }

    void UpdateModel()
    {
        Tilemap currentMap = StructureManager.Instance.farmTileMap;
        StructureBehaviorScript foundTile = null;
        StructureManager manager = StructureManager.Instance;

        Vector3Int gridPos = currentMap.WorldToCell(transform.position);

        foundTile = manager.GetStructureOnPosition(currentMap.GetCellCenterWorld(new Vector3Int(gridPos.x + 1, gridPos.y))); //right
        if(foundTile != null && (foundTile.TryGetComponent(out FarmLand t1) || foundTile.TryGetComponent(out CorruptedTile f1))) 
        {
            if(t1 != null && (t1.isWeed || !t1.crop)) t1.TakeDamage(1);
            extraTiles[0].SetActive(false);
        }
        else extraTiles[0].SetActive(true);

        foundTile = manager.GetStructureOnPosition(currentMap.GetCellCenterWorld(new Vector3Int(gridPos.x - 1, gridPos.y))); //left
        if(foundTile != null && (foundTile.TryGetComponent(out FarmLand t2) || foundTile.TryGetComponent(out CorruptedTile f2))) 
        {
            if(t2 != null && (t2.isWeed || !t2.crop)) t2.TakeDamage(1);
            extraTiles[1].SetActive(false);
        }
        else extraTiles[1].SetActive(true);

        foundTile = manager.GetStructureOnPosition(currentMap.GetCellCenterWorld(new Vector3Int(gridPos.x, gridPos.y + 1))); //up
        if(foundTile != null && (foundTile.TryGetComponent(out FarmLand t3) || foundTile.TryGetComponent(out CorruptedTile f3))) 
        {
            if(t3 != null && (t3.isWeed || !t3.crop)) t3.TakeDamage(1);
            extraTiles[2].SetActive(false);
        }
        else extraTiles[2].SetActive(true);

        foundTile = manager.GetStructureOnPosition(currentMap.GetCellCenterWorld(new Vector3Int(gridPos.x, gridPos.y - 1))); //down
        if(foundTile != null && (foundTile.TryGetComponent(out FarmLand t4) || foundTile.TryGetComponent(out CorruptedTile f4))) 
        {
            if(t4 != null && (t4.isWeed || !t4.crop)) t4.TakeDamage(1);
            extraTiles[3].SetActive(false);
        }
        else extraTiles[3].SetActive(true);
    }

    public IEnumerator CleanseRoutine()
    {
        beingCleansed = true;
        ParticlePoolManager.Instance.GrabCleanseParticle().transform.position = transform.position;
        yield return new WaitForSeconds(6);
        health = 0;
        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == 10)
        {
            PlayerMovement.Instance.ApplySpeedMod(new MovementSpeedModifiers(gameObject, 0.7f, "CorruptedTile", false));
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.gameObject.layer == 10)
        {
            PlayerMovement.Instance.RemoveSpeedMod(gameObject);
        }
    }

    void OnDestroy()
    {
        base.OnDestroy();
        StructureBehaviorScript.OnStructuresUpdated -= UpdateModel;
        if(!gameObject.scene.isLoaded) return;

        if(containedStructure)
        {
            Destroy(containedStructure.gameObject);
        }

        CorruptionManager.Instance.corruptedTiles--;
    }
}
