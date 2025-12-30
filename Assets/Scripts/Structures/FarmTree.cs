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

    public GameObject mothHivePrefab;
    public GameObject currentHive;

    public bool forceHiveSpawn;

    public Transform[] hiveSpawns;
    //public Transform[] hiveSpawnsPine;

    public GameObject[] treeModels;
    void Awake()
    {
        base.Awake();

        if(Random.Range(0,10) < 2)
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

        if(Random.Range(0, 400) >= 399 && TimeManager.Instance.dayNum > 3) forceHiveSpawn = true;

        bool playerNearby = true;
        if(Vector3.Distance(PlayerInteraction.Instance.transform.position, transform.position) > 80) playerNearby = false;

        if(forceHiveSpawn && !playerNearby) SpawnHive();

        if(Random.Range(0, 100) >= 92 && !playerNearby || (TimeManager.Instance.currentHour == 8 && Random.Range(0, 10) > 8)) StartCoroutine(SpawnLeafPile());
    }

    void SpawnHive()
    {
        forceHiveSpawn = false;
        currentHive = Instantiate(mothHivePrefab, hiveSpawns[Random.Range(0, hiveSpawns.Length)].position, Quaternion.identity);
        //else currentHive = Instantiate(mothHivePrefab, hiveSpawnsPine[Random.Range(0, hiveSpawnsPine.Length)].position, Quaternion.identity);

        Vector3 directionAway = currentHive.transform.position - transform.position;
        directionAway.y = 0;
        currentHive.transform.rotation = Quaternion.LookRotation(directionAway);
    }

    IEnumerator SpawnLeafPile()
    {
        if(type == TreeType.Evergreen) yield break;
        yield return new WaitForSeconds(Random.Range(0.5f, 3f));

        List<Vector3> availableTiles = StructureManager.Instance.GetNearbyClearTiles(transform.position, 5);

        if(availableTiles.Count == 0) yield break;

        GameObject pile = Instantiate(leafPile.objectPrefab, availableTiles[Random.Range(0, availableTiles.Count)], Quaternion.identity);
        pile.transform.localEulerAngles = new Vector3(0, Random.Range(0,360), 0);
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

        if(saveString1 == "Evergreen") type = TreeType.Evergreen;

        UpdateModel();
    }

    public override void SaveVariables()
    {
        if(currentHive) saveInt1 = 1;
        else saveInt1 = 0;

        saveString1 = type.ToString();
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
