using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmTree : StructureBehaviorScript
{
    TreeType type;

    public InventoryItemData treePapers;

    public StructureObject leafPile;

    public bool taggedForCutting = false;

    public GameObject papers, papersPine;
    public GameObject logPile;

    public Transform itemDrop;
    public ParticleSystem leafBurst, leafBurstPine;

    public GameObject mothHivePrefab, acornPrefab;
    GameObject currentHangingObject = null;
    TreeObject treeObject;

    public bool forceHiveSpawn;

    public Transform[] hiveSpawns, acornSpawns;
    //public Transform[] hiveSpawnsPine;

    public GameObject[] treeModels;

    bool newTree = true;
    void Awake()
    {
        base.Awake();

        StartCoroutine(DelayedStart());
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(1);
        if(Random.Range(0,10) < 1)
        {
            type = TreeType.Evergreen;
            UpdateModel();
        }
    }

    void Start()
    {
        base.Start();
        if(taggedForCutting)
        {
            TogglePapers(true);
        }
        OnDamage += TreeHit;

        if(forceHiveSpawn) SpawnHive();
    }

    void TogglePapers(bool enable)
    {
        if(enable)
        {
            papers.SetActive(true);
            papersPine.SetActive(true);
        }
        else
        {
            papers.SetActive(false);
            papersPine.SetActive(false);
        }
    }

    public override void StructureInteraction()
    {
        if(taggedForCutting)
        {
            taggedForCutting = false;
            TogglePapers(false);

            GameObject droppedItem = ItemPoolManager.Instance.GrabItem(treePapers);
            droppedItem.transform.position = itemDrop.position;
        }
    }

    public override void ItemInteraction(InventoryItemData item)
    {
        if(item == treePapers && !taggedForCutting)
        {
            taggedForCutting = true;
            TogglePapers(true);
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            PlayerInventoryHolder.Instance.UpdateInventory();
        }
    }

    public override void HourPassed()
    {
        if(taggedForCutting && TimeManager.Instance.currentHour == 8)
        {
            Instantiate(logPile, StructureManager.Instance.GetTileCenter(transform.position), Quaternion.identity);
            Destroy(this.gameObject);
            return;
        }

        if(Random.Range(0, 500) >= 499 && TimeManager.Instance.dayNum > 3 && TimeManager.Instance.isDay) forceHiveSpawn = true;

        bool playerNearby = true;
        if(Vector3.Distance(PlayerInteraction.Instance.transform.position, transform.position) > 80) playerNearby = false;

        if(playerNearby) return;

        if(forceHiveSpawn && TimeManager.Instance.isDay) SpawnHive();

        if(Random.Range(0, 100) >= 93 || (TimeManager.Instance.currentHour == 8 && Random.Range(0, 10) > 8)) StartCoroutine(SpawnLeafPile());

        if(Random.Range(0, 100) >= 96 && !currentHangingObject && type == TreeType.Orange) SpawnAcorn();
    }

    void SpawnHive()
    {
        forceHiveSpawn = false;
        if(treeObject == TreeObject.Hive) return;
        if(currentHangingObject) Destroy(currentHangingObject);
        treeObject = TreeObject.Hive;
        
        currentHangingObject = Instantiate(mothHivePrefab, hiveSpawns[Random.Range(0, hiveSpawns.Length)].position, Quaternion.identity);
        //else currentHangingObject = Instantiate(mothHivePrefab, hiveSpawnsPine[Random.Range(0, hiveSpawnsPine.Length)].position, Quaternion.identity);

        Vector3 directionAway = currentHangingObject.transform.position - transform.position;
        directionAway.y = 0;
        currentHangingObject.transform.rotation = Quaternion.LookRotation(directionAway);
    }

    void SpawnAcorn()
    {
        treeObject = TreeObject.Acorn;
        currentHangingObject = Instantiate(acornPrefab, acornSpawns[Random.Range(0, acornSpawns.Length)].position, Quaternion.identity);
    }

    public IEnumerator SpawnLeafPile(bool forceSpiders = false)
    {
        if(type == TreeType.Evergreen) yield break;
        yield return new WaitForSeconds(Random.Range(1.5f, 4f));

        List<Vector3> availableTiles = StructureManager.Instance.GetNearbyClearTiles(transform.position, 7);

        if(availableTiles.Count == 0) yield break;

        GameObject pile = Instantiate(leafPile.objectPrefab, availableTiles[Random.Range(0, availableTiles.Count)], Quaternion.identity);
        pile.transform.localEulerAngles = new Vector3(0, Random.Range(0,360), 0);

        if(forceSpiders) pile.GetComponent<LeafPile>().holdSpiders = true;
    }

    void OnDestroy()
    {
        OnDamage -= TreeHit;
        base.OnDestroy();
    }

    void TreeHit()
    {
        if(type == TreeType.Orange) leafBurst.Play();
        else leafBurstPine.Play();

        if(currentHangingObject)
        {
            FyllaraNut nutScript = currentHangingObject.GetComponent<FyllaraNut>();
            if(nutScript) nutScript.TreeNutDrop();
        }
    }

    void UpdateModel()
    {
        if(type == TreeType.Evergreen)
        {
            treeModels[0].SetActive(false);
            treeModels[1].SetActive(true);
        }
        else 
        {
            treeModels[0].SetActive(true);
            treeModels[1].SetActive(false);
        }
    }

    public override void LoadVariables()
    {
        if(saveInt1 == 1) SpawnHive();
        if(saveString2 == "Hive")
        {
            SpawnHive();
            treeObject = TreeObject.Hive;
        }
        else if(saveString2 == "Acorn")
        {
            SpawnAcorn();
            treeObject = TreeObject.Acorn;
        }

        if(saveString1 == "Evergreen") type = TreeType.Evergreen;

        newTree = saveBool1;

        UpdateModel();
    }

    public override void SaveVariables()
    {
        //if(currentHangingObject) saveInt1 = 1;
        //else saveInt1 = 0;

        saveString1 = type.ToString();
        saveString2 = treeObject.ToString();

        saveBool1 = newTree;
    }

    public TreeType GetType()
    {
        return type;
    }

    /*public override object GetSaveData()
    {
        return new FarmTreeSaveData(this);
    }*/
}

public enum TreeType
{
    Orange,
    Evergreen
}

public enum TreeObject
{
    Null,
    Hive,
    Acorn
}
