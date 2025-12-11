using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUpgrades : MonoBehaviour
{
    PlayerInventoryHolder playerInventoryHolder;
    [Header("Disable all before build")]
    public bool gainedInventoryUpgrade;
    public bool gainedWaterStorage; //Specifically from watercan upgrade
    public bool gainedWaterPack;

    void Awake()
    {
        playerInventoryHolder = GetComponent<PlayerInventoryHolder>();
    }

    public void LoadData(AllGameSaveData data)
    {
        gainedInventoryUpgrade = data.gainedInventoryUpgrade;
        gainedWaterStorage = data.gainedWaterStorage;
        if(gainedWaterStorage) PlayerInteraction.Instance.maxWaterHeld += 5;
        gainedWaterPack = data.gainedWaterPack;
    }

    //[ContextMenu("TestInventoryIncrease")]
    public void GainInventoryUpgrade()
    {
        gainedInventoryUpgrade = true;
        playerInventoryHolder.IncreaseBackpackInventory();
    }
    //[ContextMenu("Test Water Increase")]
    public void GainWaterStorage()
    {
        if(gainedWaterStorage) return;
        
        PlayerInteraction.Instance.maxWaterHeld += 5;
        PlayerInteraction.Instance.waterHeld += 5;
        gainedWaterStorage = true;
        var UIMeters = FindFirstObjectByType<UIMeters>();
        UIMeters.UpdateMeters();
    }

    public void GainWaterPackUpgrade()
    {
        gainedWaterPack = true;
    }
}
