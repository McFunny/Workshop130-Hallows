using SaveLoadSystem;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class TrinketInventoryHandler : MonoBehaviour
{
    public static TrinketInventoryHandler Instance;
    public List<TrinketInventoryData> trinkets = new List<TrinketInventoryData>();

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }

        SaveLoad.OnSaveGame += OnSave;
        SaveLoad.OnLoadGame += OnLoad;
    }

    private void OnDisable()
    {
        SaveLoad.OnSaveGame -= OnSave;
        SaveLoad.OnLoadGame -= OnLoad;
    }

    public void TrinketEntered(InventorySlot slot)
    {
        Debug.Log("Trinket entered slot: " + slot);
        Debug.Log("Trinket: " + slot.ItemData.displayName);
    }

    public void TrinketRemoved(InventorySlot slot)
    {
        Debug.Log("Trinket left slot: " + slot);
        Debug.Log("Trinket: " + slot.ItemData.displayName);
    }

    private void OnSave()
    {
        List<float> durabilityList = trinkets.Select(t => t.durability).ToList();

        SaveLoad.CurrentSaveData.playerTrinketDurabilityData = durabilityList;
    }

    private void OnLoad(SaveData data)
    {
        if (data.playerTrinketDurabilityData != null)
        {
            for (int i = 0; i < data.playerTrinketDurabilityData.Capacity; i++)
            {
                trinkets[i].durability = data.playerTrinketDurabilityData[i];
            }
        }
    }

}

[Serializable]
public class TrinketInventoryData
{
    public InventorySlot slot;
    public float durability;
}