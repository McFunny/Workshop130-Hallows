using SaveLoadSystem;
using UnityEngine;
using UnityEngine.Events;

public class PlayerInventoryHolder : InventoryHolder
{
    public static PlayerInventoryHolder Instance;

    [SerializeField] protected int secondaryInventorySize;
    [SerializeField] public InventorySystem secondaryInventorySystem; // Only manage secondary inventory here

    // Unity Actions to handle the UI updates for the primary and secondary inventories
    public static UnityAction<InventorySystem> OnPlayerHotbarDisplayRequested;   // For the hotbar (primary)
    public static UnityAction<InventorySystem> OnPlayerBackpackDisplayRequested; // For the backpack (secondary)

    // New: Unity Action for notifying any inventory changes
    public static UnityAction<InventorySystem> OnPlayerInventoryChanged;

    protected override void Awake()
    {
        base.Awake();
        secondaryInventorySystem = new InventorySystem(secondaryInventorySize); // Initialize secondary inventory

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        var inventoryData = new PlayerInventorySaveData(primaryInventorySystem, secondaryInventorySystem);
        SaveLoad.CurrentSaveData.playerInventoryData = inventoryData;
    }

    // Method to add items to the correct inventory system (primary or secondary)
    public bool AddToInventory(InventoryItemData data, int amount)
    {
        bool addedToPrimary = primaryInventorySystem.AddToInventory(data, amount);
        bool addedToSecondary = false;

        if (addedToPrimary)
        {
            OnPlayerHotbarDisplayRequested?.Invoke(primaryInventorySystem); // Update only hotbar
            OnPlayerInventoryChanged?.Invoke(primaryInventorySystem);       // Notify general inventory change
            return true;
        }
        else
        {
            addedToSecondary = secondaryInventorySystem.AddToInventory(data, amount);
            if (addedToSecondary)
            {
                OnPlayerBackpackDisplayRequested?.Invoke(secondaryInventorySystem); // Update only backpack
                OnPlayerInventoryChanged?.Invoke(secondaryInventorySystem);         // Notify general inventory change
                return true;
            }
        }

        return false;
    }

    // Explicitly refresh both inventories (hotbar and backpack)
    public void UpdateInventory()
    {
        //OnPlayerHotbarDisplayRequested?.Invoke(primaryInventorySystem);   // Update primary (hotbar)
        //OnPlayerBackpackDisplayRequested?.Invoke(secondaryInventorySystem); // Update secondary (backpack)

        // Notify listeners about inventory changes
        OnPlayerInventoryChanged?.Invoke(primaryInventorySystem);   // Notify general inventory change for hotbar
        OnPlayerInventoryChanged?.Invoke(secondaryInventorySystem); // Notify general inventory change for backpack
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