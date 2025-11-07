using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUpgrades : MonoBehaviour
{
    PlayerInventoryHolder playerInventoryHolder;
    [Header("Disable all before build")]
    public bool gainedInventoryUpgrade;
    public bool gainedWaterStorage;
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

    public void GainWaterStorage()
    {
        PlayerInteraction.Instance.maxWaterHeld += 5;
        PlayerInteraction.Instance.waterHeld += 5;
        gainedWaterStorage = true;
    }

    public void GainWaterPackUpgrade()
    {
        gainedWaterPack = true;
    }
}
