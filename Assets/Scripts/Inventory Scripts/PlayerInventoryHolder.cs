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
        InventoryItemData hoe = _database.GetItem(0);
        InventoryItemData shovel = _database.GetItem(1);
        InventoryItemData waterCan = _database.GetItem(2);
        InventoryItemData shotgun = _database.GetItem(3);
        InventoryItemData ammo = _database.GetItem(4);
        AddToInventory(hoe, 1);
        AddToInventory(shovel, 1);
        AddToInventory(waterCan, 1);
        AddToInventory(shotgun, 1);
        AddToInventory(ammo, 2);
        AddToInventory(ammo, 2);
        AddToInventory(ammo, 2);
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