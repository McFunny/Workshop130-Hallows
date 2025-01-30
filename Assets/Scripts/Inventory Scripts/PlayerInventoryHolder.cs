using SaveLoadSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerInventoryHolder : InventoryHolder
{
    public static PlayerInventoryHolder Instance;
 

    [SerializeField] protected int secondaryInventorySize;
    [SerializeField] public InventorySystem secondaryInventorySystem; // Only manage secondary inventory here
    [SerializeField] private Database _database;
    // Unity Actions to handle the UI updates for the primary and secondary inventories
    public static UnityAction<InventorySystem> OnPlayerHotbarDisplayRequested;   // For the hotbar (primary)
    public static UnityAction<InventorySystem> OnPlayerBackpackDisplayRequested; // For the backpack (secondary)

    // New: Unity Action for notifying any inventory changes
    public static UnityAction<InventorySystem> OnPlayerInventoryChanged;

    public bool useDebugItems;

    [System.Serializable]
    public class Item
    {
        public string name;
        public InventoryItemData itemData;
        public int amount;
    }

    [Header("Starting Items")]
    [SerializeField] private List<Item> startingItems;

    [Header("Debug Items")]
    [SerializeField] private List<Item> debugItems;

    [ContextMenu("Name Items")]
    public void NameItems()
    {
        for(int i = 0; i < startingItems.Count; i++)
        {
            startingItems[i].name = startingItems[i].itemData.displayName;
        }

        for(int i = 0; i < debugItems.Count; i++)
        {
            debugItems[i].name = debugItems[i].itemData.displayName;
        }
    }

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
        StartCoroutine(DelayedStart());
       
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(0.5f);
        EquipTools();
    }

    private void EquipTools()
    {
        foreach (var startingItem in startingItems)
        {
            if (startingItem.itemData != null)
            {
                bool addedSuccessfully = AddToInventory(startingItem.itemData, startingItem.amount);
                if (!addedSuccessfully)
                {
                    Debug.LogWarning($"Failed to add {startingItem.amount} of {startingItem.itemData.name} to inventory.");
                }
            }
            else
            {
                Debug.LogWarning("Starting item data is null.");
            }
        }
        if (useDebugItems)
        {
            foreach (var debugItem in debugItems)
            {
                if (debugItem.itemData != null)
                {
                    bool addedSuccessfully = AddToInventory(debugItem.itemData, debugItem.amount);
                    if (!addedSuccessfully)
                    {
                        Debug.LogWarning($"Failed to add {debugItem.amount} of {debugItem.itemData.name} to inventory.");
                    }
                }
                else
                {
                    Debug.LogWarning("Debug item data is null.");
                }
            }
        }
    }

    // Method to add items to the correct inventory system (primary or secondary)
    public bool AddToInventory(InventoryItemData data, int amount)
    {
        // Check for existing stack in the primary inventory
        if (primaryInventorySystem.ContainsItem(data, out List<InventorySlot> primarySlots))
        {
            foreach (var slot in primarySlots)
            {
                if (slot.EnoughRoomLeftInStack(amount))
                {
                    slot.AddToStack(amount);
                    OnPlayerHotbarDisplayRequested?.Invoke(primaryInventorySystem); // Update only hotbar
                    OnPlayerInventoryChanged?.Invoke(primaryInventorySystem);       // Notify general inventory change
                    return true;
                }
            }
        }

        // Check for existing stack in the secondary inventory
        if (secondaryInventorySystem.ContainsItem(data, out List<InventorySlot> secondarySlots))
        {
            foreach (var slot in secondarySlots)
            {
                if (slot.EnoughRoomLeftInStack(amount))
                {
                    slot.AddToStack(amount);
                    OnPlayerInventoryChanged?.Invoke(secondaryInventorySystem); // Notify general inventory change
                    return true;
                }
            }
        }

        // No stack found; look for a free slot in the primary inventory
        if (primaryInventorySystem.HasFreeSlot(out InventorySlot freePrimarySlot))
        {
            if (freePrimarySlot.EnoughRoomLeftInStack(amount))
            {
                freePrimarySlot.UpdateInventorySlot(data, amount);
                OnPlayerHotbarDisplayRequested?.Invoke(primaryInventorySystem); // Update only hotbar
                OnPlayerInventoryChanged?.Invoke(primaryInventorySystem);       // Notify general inventory change
                return true;
            }
        }

        // No free slot in primary; look for a free slot in the secondary inventory
        if (secondaryInventorySystem.HasFreeSlot(out InventorySlot freeSecondarySlot))
        {
            if (freeSecondarySlot.EnoughRoomLeftInStack(amount))
            {
                freeSecondarySlot.UpdateInventorySlot(data, amount);
                OnPlayerInventoryChanged?.Invoke(secondaryInventorySystem); // Notify general inventory change
                return true;
            }
        }

        // If no stack or free slot found, return false
        return false;
    }

    public bool IsInventoryFull()
    {
        if (primaryInventorySystem.HasFreeSlot(out InventorySlot freePrimarySlot))
        {
            return false;
        }

        if (secondaryInventorySystem.HasFreeSlot(out InventorySlot freeSecondarySlot))
        {
            return false;
        }

        return true;
    }

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