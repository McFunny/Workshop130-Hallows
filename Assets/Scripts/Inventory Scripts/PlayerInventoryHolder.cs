using SaveLoadSystem;
using UnityEngine;
using UnityEngine.Events;

public class PlayerInventoryHolder : InventoryHolder
{
    [SerializeField] protected int secondaryInventorySize;
    [SerializeField] public InventorySystem secondaryInventorySystem;


    public InventorySystem InventorySystem => secondaryInventorySystem;
    public static UnityAction<InventorySystem> OnPlayerBackpackDisplayRequested;
    public static UnityAction<InventorySystem> OnPlayerInventoryChanged;

    protected override void Awake()
    {
        base.Awake();
        secondaryInventorySystem = new InventorySystem(secondaryInventorySize);
    }

    private void Start()
    {
        var inventoryData = new PlayerInventorySaveData(primaryInventorySystem, secondaryInventorySystem);

        // Directly save to playerInventoryData in SaveData
        SaveLoad.CurrentSaveData.playerInventoryData = inventoryData;
    }

    private void LoadInventory(SaveData data)
    {
        if (data.playerInventoryData.primaryInvSystem != null)
        {
            this.primaryInventorySystem = data.playerInventoryData.primaryInvSystem;
            this.secondaryInventorySystem = data.playerInventoryData.secondaryInvSystem;
            OnPlayerInventoryChanged?.Invoke(this.primaryInventorySystem);
            OnPlayerInventoryChanged?.Invoke(this.secondaryInventorySystem);
        }
    }

    public bool AddToInventory(InventoryItemData data, int amount)
    {
        if (primaryInventorySystem.AddToInventory(data, amount))
        {
            OnPlayerInventoryChanged?.Invoke(primaryInventorySystem);
            return true;
        }
        else if (secondaryInventorySystem.AddToInventory(data, amount))
        {
            OnPlayerInventoryChanged?.Invoke(secondaryInventorySystem);
            return true;
        }

        return false;
    }

    public void UpdateInventory()
    {
        OnPlayerInventoryChanged?.Invoke(primaryInventorySystem);
        OnPlayerInventoryChanged?.Invoke(secondaryInventorySystem);
    }
}

[System.Serializable]
public struct PlayerInventorySaveData
{
    public InventorySystem primaryInvSystem;
    public InventorySystem secondaryInvSystem;

    public PlayerInventorySaveData(InventorySystem _primaryInvSystem, InventorySystem _secondaryInvSystem)
    {
        primaryInvSystem = _primaryInvSystem;
        secondaryInvSystem = _secondaryInvSystem;
    }
}
