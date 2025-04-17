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

    public List<SpriteRenderer> itemSockets = new List<SpriteRenderer>();

    public Animator anim;

    void Awake()
    {
        base.Awake();
        primaryInventorySystem = new InventorySystem(inventorySize);
        SaveLoad.OnSaveGame += SaveInventory;
        SaveLoad.OnLateLoad += LoadInventory;
    }

    private void OnDisable()
    {
        SaveLoad.OnSaveGame -= SaveInventory;
        SaveLoad.OnLateLoad -= LoadInventory;
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

    private void SaveInventory()
    {
        SaveLoad.CurrentSaveData.chestDictionary[chestID] = new ChestSaveData(primaryInventorySystem, transform.position, transform.rotation);
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(type == ToolType.Shovel && PlayerInventoryHolder.Instance.IsInventoryFull() == false && !primaryInventorySystem.ContainsAnyItems()) //ADD A CHECK TO SEE IF CHEST INVENTORY HAS ITEMS
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
        Debug.Log("LoadingChest");
        //chestID = saveString1;
        if (data.chestDictionary.TryGetValue(chestID, out ChestSaveData chestData))
        {
            this.primaryInventorySystem = new InventorySystem(data.chestDictionary[chestID].invSystem.savedSlots.Count);
            this.primaryInventorySystem.LoadFromSaveData(data.chestDictionary[chestID].invSystem, Database.Instance);
            //this.transform.position = chestData.position;
            //this.transform.rotation = chestData.rotation;
        }
    }

    public override void StructureInteraction()
    {
        StartCoroutine(WaitForClose());
        InventoryHolder.OnDynamicInventoryDisplayRequested?.Invoke(primaryInventorySystem);
        PlayerInventoryHolder.Instance.UpdateOpenInventory();

        RefreshSockets();
    }

    void RefreshSockets()
    {
        for(int i = 0; i < itemSockets.Count; i++)
        {
            if(primaryInventorySystem.InventorySlots[i].ItemData == null) itemSockets[i].sprite = null;
            else itemSockets[i].sprite = primaryInventorySystem.InventorySlots[i].ItemData.icon;
        }
    }

    IEnumerator WaitForClose()
    {
        anim.SetBool("isOpen", true);
        yield return new WaitForSeconds(0.1f);
        yield return new WaitUntil(() => !PlayerMovement.accessingInventory);
        anim.SetBool("isOpen", false);
        RefreshSockets();
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

[System.Serializable]

public struct ChestSaveData
{
    public InventorySystemSaveData invSystem;
    public Vector3 position;
    public Quaternion rotation;

    public ChestSaveData(InventorySystem _invSystem, Vector3 _position, Quaternion _rotation)
    {
        invSystem = _invSystem.GetSaveData();
        position = _position;
        rotation = _rotation;
    }
}

