using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUpgrades : MonoBehaviour
{
    PlayerInventoryHolder playerInventoryHolder;
    [Header("Disable all before build")]
    public bool gainedInventoryUpgrade;

    void Awake()
    {
        playerInventoryHolder = GetComponent<PlayerInventoryHolder>();
    }

    public void LoadData(AllGameSaveData data)
    {
        gainedInventoryUpgrade = data.gainedInventoryUpgrade;
    }

    //[ContextMenu("TestInventoryIncrease")]
    public void GainInventoryUpgrade()
    {
        gainedInventoryUpgrade = true;
        playerInventoryHolder.IncreaseBackpackInventory();
    }
}
