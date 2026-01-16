using SaveLoadSystem;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class TrinketInventoryHandler : MonoBehaviour
{
    public static TrinketInventoryHandler Instance;
    public List<TrinketInventoryData> trinkets = new List<TrinketInventoryData>(); //This holds the trinkets

    public AudioClip equipSFX, removeSFX, breakSFX;


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
        //SaveLoad.OnLoadGame += OnLoad;
    }

    private void OnDisable()
    {
        SaveLoad.OnSaveGame -= OnSave;
        //SaveLoad.OnLoadGame -= OnLoad;
    }

    public void TrinketEntered(InventorySlot slot)
    {
        Debug.Log("Trinket entered slot: " + slot);
        Debug.Log("Trinket: " + slot.ItemData.displayName);
        TrinketItem trinket = slot.ItemData as TrinketItem;
        if(!trinket)
        {
            Debug.LogError("This is not a trinket and should not be here");
            return;
        }
        GetTrinketDataFromSlot(slot).maxDurability = trinket.maxDurability;
        GetTrinketDataFromSlot(slot).durability = trinket.maxDurability;

        trinket.OnEquip();

        DialogueController.Instance.source.PlayOneShot(equipSFX);
    }

    public void TrinketRemoved(InventorySlot slot, MouseItemData mouseItemData)
    {
        Debug.Log("Trinket left slot: " + slot);
        Debug.Log("Trinket: " + slot.ItemData.displayName);

        TrinketItem trinket = slot.ItemData as TrinketItem;
        if(!trinket)
        {
            Debug.LogError("This is not a trinket and should not be here");
            return;
        }
        trinket.OnRemove();

        if(GetTrinketDataFromSlot(slot).durability < UnityEngine.Random.Range(trinket.guaranteedBreakThreshold + 1, trinket.maxDurability))
        {
            GetTrinketDataFromSlot(slot).durability = 0f;
            BreakTrinket(mouseItemData);
            DialogueController.Instance.source.PlayOneShot(breakSFX);
        }
        else DialogueController.Instance.source.PlayOneShot(removeSFX);
    }

    public void BreakTrinket(MouseItemData mouseItemData)
    {
       mouseItemData.ClearSlot();
    }

    public void ApplyTrinketDamage(TrinketKey _key)
    {
        for(int i = 0; i < trinkets.Count; ++i)
        {
            InventoryItemData item = trinkets[i].slot.ItemData;
            if(!item) continue;
            TrinketItem t_item = item as TrinketItem;

            if(t_item && t_item.key == _key)
            {
                //Apply Damage
                return;
            }
        }
    }

    public bool CheckForTrinket(TrinketKey _key)
    {
        for(int i = 0; i < trinkets.Count; ++i)
        {
            InventoryItemData item = trinkets[i].slot.ItemData;
            if(!item) continue;
            TrinketItem t_item = item as TrinketItem;

            if(t_item && t_item.key == _key)
            {
                return true;
            }
        }

        return false;
    }

    private void OnSave()
    {
        List<float> durabilityList = trinkets.Select(t => t.durability).ToList();
        List<float> maxDurabilityList = trinkets.Select(t => t.maxDurability).ToList();

        SaveLoad.CurrentSaveData.playerTrinketDurabilityData = durabilityList;
        SaveLoad.CurrentSaveData.playerTrinketMaxDurabilityData = maxDurabilityList;
    }

    public void OnLoad(SaveData data)
    {
        if (data.playerTrinketDurabilityData != null)
        {
            for (int i = 0; i < data.playerTrinketDurabilityData.Count; i++)
            {
                trinkets[i].durability = data.playerTrinketDurabilityData[i];
            }
        }

        if (data.playerTrinketMaxDurabilityData != null)
        {
            for (int i = 0; i < data.playerTrinketMaxDurabilityData.Count; i++)
            {
                trinkets[i].maxDurability = data.playerTrinketMaxDurabilityData[i];
            }
        }
    }

    public void OnInventoryUpdate(InventorySlot_UI uiSlot, InventorySlot slot)
    {
        TrinketInventoryData trinketData = GetTrinketDataFromSlot(slot);

        float trinketDurability = trinketData != null ? trinketData.durability : 0f;
        float trinketMaxDurability = trinketData != null ? trinketData.maxDurability : 0f;
        ChangeTrinketDurability(slot, trinketDurability);

        Debug.Log("Trinket durability: " + trinketDurability);

        uiSlot.durabilitySlider.maxValue = trinketMaxDurability;
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

    public List<float> GetDurabilityList()
    {
        return trinkets.Select(t => t.durability).ToList();
    }

    public List<float> GetMaxDurabilityList()
    {
        return trinkets.Select(t => t.maxDurability).ToList();
    }

}

[Serializable]
public class TrinketInventoryData
{
    public InventorySlot slot;
    public float durability;
    public float maxDurability;
}