using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebrisPile : StructureBehaviorScript
{
    bool isDigging = false;
    public StructureObject repairedStruct;

    public GameObject wood_debris, hay_debris, metal_debris, default_debris;


    //Do we prevent these being repaired at night? Or make it so u have to hold an interaction on them
    //Use popup to tell player if resources are insufficient and if they cant repair at night

    void Awake()
    {
        base.Awake();
    }
    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        //grab the repaired struct via saved int1 and the database
        if(!repairedStruct) LoadVariables();
        EnablePile();
    }

    public override void StructureInteraction()
    {
        if(repairedStruct.mintRepairCost == 0 && repairedStruct.repairItems.Count == 0)
        {
            Destroy(gameObject);
            return;
        }

        if(CanRepair())
        {
            PlayerInteraction.Instance.currentMoney -= repairedStruct.mintRepairCost;
            /*for(int i = 0; i < repairedStruct.repairItems.Count; i++)
            {
                inventory.RemoveItemsFromInventory(repairedStruct.repairItems[i].item, repairedStruct.repairItems[i].amount);
            }*/
            PlayerInventoryHolder.Instance.RemoveItemsFromBothInventories(repairedStruct.repairItems);
            PlayerInventoryHolder.Instance.UpdateInventory();
            RepairStructure();
        }
    }

    public override void ItemInteraction(InventoryItemData item)
    {
        StructureInteraction();

    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(type == ToolType.Shovel)
        {
            //StartCoroutine(Dig());
            success = true;
        }
    }

    public override void DigAction()
    {
        audioHandler.PlaySoundAtPoint(audioHandler.interactSound, transform.position);
        ParticlePoolManager.Instance.GrabPoofParticle().transform.position = transform.position;
        ParticlePoolManager.Instance.GrabDirtPixelParticle().transform.position = transform.position;
        Destroy(this.gameObject);
    }

    void EnablePile()
    {
        if(!repairedStruct) return;
        wood_debris.SetActive(false);
        metal_debris.SetActive(false);
        hay_debris.SetActive(false);
        default_debris.SetActive(false);

        switch(repairedStruct.structureType)
        {
            case StructureType.Null:
                default_debris.SetActive(true);
                break;
            case StructureType.Wood:
                wood_debris.SetActive(true);
                break;
            case StructureType.Metal:
                metal_debris.SetActive(true);
                break;
            case StructureType.Hay:
                hay_debris.SetActive(true);
                break;
        }
    }

    void RepairStructure()
    {
        clearTileOnDestroy = false;
        GameObject s = StructureManager.Instance.SpawnStructureWithInstance(repairedStruct.objectPrefab, transform.position);
        if(repairedStruct.gridSize == GridSize.TwoByTwo) StructureManager.Instance.SetLargeTile(transform.position);
        if(repairedStruct.gridSize == GridSize.OneByTwo) StructureManager.Instance.SetOneByTwoTile(transform.position);
        s.transform.rotation = transform.rotation;
        ParticlePoolManager.Instance.GrabPoofParticle().transform.position = transform.position;
        ParticlePoolManager.Instance.GrabDirtPixelParticle().transform.position = transform.position;
        Destroy(gameObject);
    }

    bool CanRepair()
    {
        if(PlayerInteraction.Instance.currentMoney < repairedStruct.mintRepairCost) return false;
        for(int i = 0; i < repairedStruct.repairItems.Count; i++)
        {
            int amountToFind = repairedStruct.repairItems[i].amount;
            if(PlayerInventoryHolder.Instance.ReturnItemCountInPlayerInventory(repairedStruct.repairItems[i].item) < amountToFind) return false;
        }
        return true;
    }

    void OnDestroy()
    {
        base.OnDestroy();
    }

    public override void SaveVariables()
    {
        saveInt1 = repairedStruct.id;
    }

    public override void LoadVariables()
    {
        repairedStruct = StructureDatabase.Instance.GetStructure(saveInt1);
    }
}
