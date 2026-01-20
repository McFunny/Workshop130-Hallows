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

        //DialogueController.Instance.source.PlayOneShot(equipSFX);
        AudioPoolManager.Instance.PlayClip(equipSFX, 0.1f);
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
        }
        else AudioPoolManager.Instance.PlayClip(removeSFX, 0.9f);//DialogueController.Instance.source.PlayOneShot(removeSFX);
    }

    public void BreakTrinket(MouseItemData mouseItemData)
    {
       mouseItemData.ClearSlot();
       //DialogueController.Instance.source.PlayOneShot(breakSFX);
       AudioPoolManager.Instance.PlayClip(breakSFX, 0.1f);
    }

    public void BreakTrinket(InventorySlot slot)
    {
        TrinketItem trinket = slot.ItemData as TrinketItem;
        if(trinket) trinket.OnRemove();
        slot.ClearSlot();
        //DialogueController.Instance.source.PlayOneShot(breakSFX);
        AudioPoolManager.Instance.PlayClip(breakSFX, 0.4f);
    }

    public void ApplyTrinketDamage(TrinketKey _key, float damage = 1) //Reduced trinket durability
    {
        if(damage < 0) damage *= -1; //Make sure its not healing the trinkets

        for(int i = 0; i < trinkets.Count; ++i)
        {
            InventoryItemData item = trinkets[i].slot.ItemData;
            if(!item) continue;
            TrinketItem t_item = item as TrinketItem;

            if(t_item && t_item.key == _key)
            {
                //Apply Damage
                ChangeTrinketDurability(trinkets[i].slot, trinkets[i].durability - damage);
                return;
            }
        }
    }

    /*public void DamageArmorTrinkets(float damage) //Damage trinkets that can take damage from attacks
    {
        if(damage < 0) damage *= -1; //Make sure its not healing the trinkets

        for(int i = 0; i < trinkets.Count; ++i)
        {
            InventoryItemData item = trinkets[i].slot.ItemData;
            if(!item) continue;
            TrinketItem t_item = item as TrinketItem;

            if(t_item && t_item.damagedByAttacks)
            {
                //Apply Damage
                ChangeTrinketDurability(trinkets[i].slot, trinkets[i].durability - damage);
            }
        }
    }*/

    public float ApplyTrinketDamageModifiers(float damage) //Apply trinket armor
    {
        damage *= -1;
        for(int i = 0; i < trinkets.Count; ++i)
        {
            InventoryItemData item = trinkets[i].slot.ItemData;
            if(!item) continue;
            TrinketItem t_item = item as TrinketItem;

            if(t_item)
            {
                float amountReduced = damage - (damage * t_item.damageMultiplier);
                amountReduced = Mathf.CeilToInt(amountReduced);
                damage -= amountReduced;

                if(t_item.damagedByAttacks)
                {
                    if(t_item.damageMultiplier == 1) ChangeTrinketDurability(trinkets[i].slot, trinkets[i].durability - Mathf.Clamp(damage / 5, 1, 10));
                    else ChangeTrinketDurability(trinkets[i].slot, trinkets[i].durability - Mathf.Clamp(amountReduced, 1, 100));
                }
            }
        }

        return -damage;
    }

    public bool CheckForTrinket(TrinketKey _key) //Checking if specific trinket exists
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

    bool CheckForRepeatNonStackableTrinket(TrinketKey _key) //Checking if there is another non-stackable trinket of the same type
    {
        for(int i = 0; i < trinkets.Count; ++i)
        {
            InventoryItemData item = trinkets[i].slot.ItemData;
            if(!item) continue;
            TrinketItem t_item = item as TrinketItem;

            if(t_item && t_item.key == _key && t_item.stackable)
            {
                return true;
            }
        }

        return false;
    }

    public bool CanPlaceInTrinketSlot(InventoryItemData _item)
    {
        TrinketItem t_item = _item as TrinketItem;
        if(!t_item) return false;
        return !CheckForRepeatNonStackableTrinket(t_item.key);
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

        ReloadTrinkets();
    }

    void ReloadTrinkets()
    {
        for(int i = 0; i < trinkets.Count; ++i)
        {
            TrinketEntered(trinkets[i].slot);
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

            if(newDurability <= 0) BreakTrinket(slot);
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
    public float GetTrinketDurability(InventorySlot slot)
    {
        TrinketInventoryData trinketData = GetTrinketDataFromSlot(slot);
        if (trinketData != null)
        {
            return trinketData.durability;
        }
        return -1f; // Return -1 if the trinket data is not found
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