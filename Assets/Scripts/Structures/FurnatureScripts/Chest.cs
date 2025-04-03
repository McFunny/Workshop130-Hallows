using SaveLoadSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(UniqueID))]
public class Chest : FurnitureBehaviorScript
{
    [SerializeField] private int inventorySize;
    [SerializeField] protected InventorySystem primaryInventorySystem;

    public InventorySystem PrimaryInventorySystem => primaryInventorySystem;

    string chestID;

    void Awake()
    {
        base.Awake();
        primaryInventorySystem = new InventorySystem(inventorySize);
        SaveLoad.OnLoadGame += LoadInventory;
    }

    private void OnDisable()
    {
        SaveLoad.OnLoadGame -= LoadInventory;
    }

    private void Start()
    {

        if(chestID == null) chestID = GetComponent<UniqueID>().ID;

        if (SaveLoad.CurrentSaveData.chestDictionary.ContainsKey(chestID))
        {
            var chestSavedData = new ChestSaveData(primaryInventorySystem, transform.position, transform.rotation);
        }
        else
        {
            var chestSavedData = new ChestSaveData(primaryInventorySystem, transform.position, transform.rotation);
            SaveLoad.CurrentSaveData.chestDictionary.Add(chestID, chestSavedData);
        }
        base.Start();
        FurnitureStart();
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(type == ToolType.Shovel && PlayerInventoryHolder.Instance.IsInventoryFull() == false) //ADD A CHECK TO SEE IF CHEST INVENTORY HAS ITEMS
        {
            StartCoroutine(DugUp());
            success = true;
        }
    }

    IEnumerator DugUp()
    {
        yield return new WaitForSeconds(1);
        PlayerInventoryHolder.Instance.AddToInventory(recoveredItem, 1);

        Destroy(this.gameObject);
    }


    private void LoadInventory(SaveData data)
    {
        chestID = saveString1;
        if (data.chestDictionary.TryGetValue(chestID, out ChestSaveData chestData))
        {
            this.primaryInventorySystem = chestData.invSystem;
            this.transform.position = chestData.position;
            this.transform.rotation = chestData.rotation;
        }
    }

    public override void StructureInteraction()
    {
        InventoryHolder.OnDynamicInventoryDisplayRequested?.Invoke(primaryInventorySystem);
    }

    public override void SaveVariables()
    {
        //Save ID. Also should generate a new ID if it does not have one (When placed)
        saveString1 = chestID;
    }

    public override void LoadVariables()
    {
        //Load ID
        chestID = saveString1;
    }

 
}