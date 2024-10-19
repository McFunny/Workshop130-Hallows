using SaveLoadSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(UniqueID))]

public class ChestInventory : InventoryHolder , IInteractable
{

    protected override void Awake()
    {
        base.Awake();
       SaveLoad.OnLoadGame += LoadInventory;
    }

    private void Start()
    {
        var chestSavedData = new ChestSaveData(primaryInventorySystem, transform.position, transform.rotation);

        SaveLoad.CurrentSaveData.chestDictionary.Add(GetComponent<UniqueID>().ID, chestSavedData);
    }


    private void LoadInventory(SaveData data)
    {
        if (data.chestDictionary.TryGetValue(GetComponent<UniqueID>().ID, out ChestSaveData chestData))
        {
            this.primaryInventorySystem = chestData.invSystem;
            this.transform.position = chestData.position;
            this.transform.rotation = chestData.rotation;
        }
    }

    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
       
        OnDynamicInventoryDisplayRequested?.Invoke(primaryInventorySystem);
        interactSuccessful = true;
    }

    public void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        interactSuccessful = false;
    }
    
    public void EndInteraction()
    {
       
    }

 
}

[System.Serializable]

public struct ChestSaveData
{
    public InventorySystem invSystem;
    public Vector3 position;
    public Quaternion rotation;

    public ChestSaveData(InventorySystem _invSystem, Vector3 _position, Quaternion _rotation)
    {
        invSystem = _invSystem;
        position = _position;
        rotation = _rotation;
    }

}
