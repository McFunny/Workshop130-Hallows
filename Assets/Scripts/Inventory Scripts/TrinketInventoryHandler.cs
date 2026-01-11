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

    public void TrinketRemoved(InventorySlot slot, MouseItemData mouseItemData)
    {
        Debug.Log("Trinket left slot: " + slot);
        Debug.Log("Trinket: " + slot.ItemData.displayName);
        GetTrinketDataFromSlot(slot).durability = 0f;
        BreakTrinket(mouseItemData);
    }

    public void TrinketQuickSwitched(InventorySlot slot, InventorySlot slotToSwitchTo)
    {
        Debug.Log("Trinket left slot: " + slot);
        Debug.Log("Trinket: " + slot.ItemData.displayName);
        GetTrinketDataFromSlot(slot).durability = 0f;
        BreakTrinket(slotToSwitchTo);
    }

    public void BreakTrinket(InventorySlot slot)
    {
       slot.ClearSlot();
    }

    public void BreakTrinket(MouseItemData mouseItemData)
    {
       mouseItemData.ClearSlot();
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

    public void OnInventoryUpdate(InventorySlot_UI uiSlot, InventorySlot slot)
    {
        TrinketInventoryData trinketData = GetTrinketDataFromSlot(slot);

        float trinketDurability = trinketData != null ? trinketData.durability : 0f;
        ChangeTrinketDurability(slot, trinketDurability);

        uiSlot.durabilitySlider.value = trinketDurability;

        if(trinketDurability <= 0f)
        {
            uiSlot.durabilitySlider.gameObject.SetActive(false);
        }
        else 
        {
            uiSlot.durabilitySlider.gameObject.SetActive(true);
        }

    }

    private void ChangeTrinketDurability(InventorySlot slot, float newDurability)
    {
        TrinketInventoryData trinketData = GetTrinketDataFromSlot(slot);
        
        if (trinketData != null)
        {
            trinketData.durability = newDurability;
        }
    }

    private TrinketInventoryData GetTrinketDataFromSlot(InventorySlot slot)
    {
        return trinkets.Find(t => t.slot == slot);
    }

}

[Serializable]
public class TrinketInventoryData
{
    public InventorySlot slot;
    public float durability;
}