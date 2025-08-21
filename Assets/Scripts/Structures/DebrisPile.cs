using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebrisPile : StructureBehaviorScript
{
    bool isDigging = false;
    public StructureObject repairedStruct;

    bool containsItems = false;

    public GameObject wood_debris, hay_debris, metal_debris, default_debris;

    public int repairsLeft = 1;
    public int missesLeft = 1;
    private RepairMinigame repairMinigame;
    private DebrisUI debrisUI;

    public PopupScript popup;

    public InventoryItemData repairKit;


    //Do we prevent these being repaired at night? Or make it so u have to hold an interaction on them
    //Use popup to tell player if resources are insufficient and if they cant repair at night

    void Awake()
    {
        base.Awake();
        repairMinigame = FindObjectOfType<RepairMinigame>();
        debrisUI = GetComponent<DebrisUI>();
    }
    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        //grab the repaired struct via saved int1 and the database
        if(!repairedStruct) LoadVariables();
        EnablePile();
    }
    
    public void InsertStructure(StructureObject newStructure)
    {
        repairedStruct = newStructure;
        repairsLeft = repairedStruct.requiredRepairs;
        missesLeft = repairedStruct.maxMisses;
    }

    public override void StructureInteraction()
    {
        if(repairedStruct.mintRepairCost == 0 && repairedStruct.repairItems.Count == 0)
        {
            Destroy(gameObject);
            return;
        }

        if(containsItems)
        {
            repairMinigame.StartMinigame(this);
            //RepairStructure();
            return;
        }

        if (CanRepair())
        {
            PlayerInteraction.Instance.currentMoney -= repairedStruct.mintRepairCost;
            /*for(int i = 0; i < repairedStruct.repairItems.Count; i++)
            {
                inventory.RemoveItemsFromInventory(repairedStruct.repairItems[i].item, repairedStruct.repairItems[i].amount);
            }*/
            PlayerInventoryHolder.Instance.RemoveItemsFromBothInventories(repairedStruct.repairItems);
            PlayerInventoryHolder.Instance.UpdateInventory();
            containsItems = true; // Is ready to start the minigame
            debrisUI.ShowRepairUI();
            //RepairStructure();
        }
        else
        {
            PopupHandler.Instance.AddToQueue(popup);
        }
    }

    public override void ItemInteraction(InventoryItemData item)
    {
        if(item == repairKit)
        {
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            PlayerInventoryHolder.Instance.UpdateInventory();
            RepairStructure();
            return;
        }
        StructureInteraction();

    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(type == ToolType.Shovel && !containsItems)
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
            case StructureType.Stone:
                default_debris.SetActive(true);
                break;
        }
    }

    public void RepairStructure()
    {
        clearTileOnDestroy = false;
        GameObject s = StructureManager.Instance.SpawnStructureWithInstance(repairedStruct.objectPrefab, transform.position);
        if(repairedStruct.gridSize == GridSize.TwoByTwo) StructureManager.Instance.SetLargeTile(transform.position);
        if(repairedStruct.gridSize == GridSize.OneByTwo) StructureManager.Instance.SetOneByTwoTile(transform.position);
        s.transform.rotation = transform.rotation;

        Destroy(gameObject);
    }

    public void DestroyStructure()
    {
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
        if (!gameObject.scene.isLoaded) return;
        ParticlePoolManager.Instance.GrabPoofParticle().transform.position = transform.position;
        ParticlePoolManager.Instance.GrabDirtPixelParticle().transform.position = transform.position;
    }

    public override void SaveVariables()
    {
        saveInt1 = repairedStruct.id;
        saveInt2 = repairsLeft;
        saveInt3 = missesLeft;
        saveBool1 = containsItems; 
    }

    public override void LoadVariables()
    {
        repairedStruct = StructureDatabase.Instance.GetStructure(saveInt1);
        containsItems = saveBool1;
        repairsLeft = saveInt2;
        missesLeft = saveInt3;
    }
}
