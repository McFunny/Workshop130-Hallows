using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmTree : StructureBehaviorScript
{
    public InventoryItemData treePapers;

    public bool taggedForCutting = false;

    public GameObject papers;
    public GameObject logPile;

    public Transform itemDrop;
    public ParticleSystem leafBurst;

    public GameObject mothHivePrefab;
    public GameObject currentHive;

    public bool forceHiveSpawn;

    public Transform[] hiveSpawns;
    void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        base.Start();
        if(taggedForCutting) papers.SetActive(true);
        OnDamage += TreeHit;

        if(forceHiveSpawn) SpawnHive();
    }

    public override void StructureInteraction()
    {
        if(taggedForCutting)
        {
            taggedForCutting = false;
            papers.SetActive(false);

            GameObject droppedItem = ItemPoolManager.Instance.GrabItem(treePapers);
            droppedItem.transform.position = itemDrop.position;
        }
    }

    public override void ItemInteraction(InventoryItemData item)
    {
        if(item == treePapers && !taggedForCutting)
        {
            taggedForCutting = true;
            papers.SetActive(true);
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
        }

        if(Random.Range(0, 500) >= 499 && TimeManager.Instance.dayNum > 3) forceHiveSpawn = true;

        if(forceHiveSpawn && Vector3.Distance(PlayerInteraction.Instance.transform.position, transform.position) > 80) SpawnHive();
    }

    void SpawnHive()
    {
        forceHiveSpawn = false;
        currentHive = Instantiate(mothHivePrefab, hiveSpawns[Random.Range(0, hiveSpawns.Length)].position, Quaternion.identity);

        Vector3 directionAway = currentHive.transform.position - transform.position;
        directionAway.y = 0;
        currentHive.transform.rotation = Quaternion.LookRotation(directionAway);
    }

    void OnDestroy()
    {
        OnDamage -= TreeHit;
        base.OnDestroy();
    }

    void TreeHit()
    {
        leafBurst.Play();
    }

    public override void LoadVariables()
    {
        if(saveInt1 == 1) SpawnHive();
    }

    public override void SaveVariables()
    {
        if(currentHive) saveInt1 = 1;
        else saveInt1 = 0;
    }

    /*public override object GetSaveData()
    {
        return new FarmTreeSaveData(this);
    }*/
}
