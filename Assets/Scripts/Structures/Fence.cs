using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Fence : StructureBehaviorScript
{
    /*public GameObject single, plus;
    public List<GameObject> straight, half, t, l;
    public GameObject activeModel;*/

    public GameObject[] fenceVariants;

    private Dictionary<int, GameObject> lookup;

    GameObject spawnedCrow = null;
    public GameObject crowPrefab;
    public Transform crowSpawn;
    // Start is called before the first frame update
    void Start()
    {
        transform.rotation = Quaternion.Euler(new Vector3(0,0,0));
        base.Start();
        lookup = new Dictionary<int, GameObject>();
        for (int i = 0; i < fenceVariants.Length; i++)
        {
            if (fenceVariants[i] != null)
                lookup[i] = fenceVariants[i];
        }

        UpdateModel();

        StructureBehaviorScript.OnStructuresUpdated += UpdateModel;
        StartCoroutine(LateStart());
    }

    IEnumerator LateStart()
    {
        yield return new WaitForSeconds(1);
        UpdateModel();
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

    public override void HourPassed()
    {
        if(TownGate.Instance.location != PlayerLocation.InFarm && !spawnedCrow && Random.Range(0, 100) < 4)
        {
            spawnedCrow = Instantiate(crowPrefab, crowSpawn.position, Quaternion.identity);
            spawnedCrow.GetComponent<MutatedCrow>().isDecorCrow = true;
            spawnedCrow.transform.localEulerAngles = new Vector3(0, Random.Range(0,360), 0);
        }
    }

    void UpdateModel()
    {
        int mask = 0;

        Tilemap currentMap;
        if(!absentFromFarmGrid) currentMap = StructureManager.Instance.farmTileMap;
        else currentMap = StructureManager.Instance.barnTileMap;
        StructureBehaviorScript foundFence = null;
        StructureManager manager = StructureManager.Instance;

        Vector3Int gridPos = currentMap.WorldToCell(transform.position);

        foundFence = manager.GetStructureOnPosition(currentMap.GetCellCenterWorld(new Vector3Int(gridPos.x + 1, gridPos.y))); //right
        if(foundFence != null && foundFence.TryGetComponent(out Fence f1)) mask |= 2;

        foundFence = manager.GetStructureOnPosition(currentMap.GetCellCenterWorld(new Vector3Int(gridPos.x - 1, gridPos.y))); //left
        if(foundFence != null && foundFence.TryGetComponent(out Fence f2)) mask |= 8;

        foundFence = manager.GetStructureOnPosition(currentMap.GetCellCenterWorld(new Vector3Int(gridPos.x, gridPos.y + 1))); //up
        if(foundFence != null && foundFence.TryGetComponent(out Fence f3)) mask |= 1;

        foundFence = manager.GetStructureOnPosition(currentMap.GetCellCenterWorld(new Vector3Int(gridPos.x, gridPos.y - 1))); //down
        if(foundFence != null && foundFence.TryGetComponent(out Fence f4)) mask |= 4;

        // deactivate all
        foreach (var variant in fenceVariants)
            if (variant != null) variant.SetActive(false);

        // activate the right one
        if (lookup.TryGetValue(mask, out GameObject chosen))
            chosen.SetActive(true);
    }

    void OnDestroy()
    {
        base.OnDestroy();
        StructureBehaviorScript.OnStructuresUpdated -= UpdateModel;
        if(!gameObject.scene.isLoaded) return;
    }
}
