using SaveLoadSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerInventoryHolder : InventoryHolder
{
    public static PlayerInventoryHolder Instance;

    [SerializeField] protected int secondaryInventorySize;
    [SerializeField] public InventorySystem secondaryInventorySystem;
    [SerializeField] private Database _database;

    public static UnityAction<InventorySystem> OnPlayerHotbarDisplayRequested;
    public static UnityAction<InventorySystem> OnPlayerBackpackDisplayRequested;
    public static UnityAction<InventorySystem> OnPlayerInventoryChanged;

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
    }

    protected override void Awake()
    {
        base.Awake();
        secondaryInventorySystem = new InventorySystem(secondaryInventorySize);

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
        EquipStartingItems();
    }

    private void EquipStartingItems()
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

    public bool AddToInventory(InventoryItemData data, int amount)
    {
        if (primaryInventorySystem.ContainsItem(data, out List<InventorySlot> primarySlots))
        {
            foreach (var slot in primarySlots)
            {
                if (slot.EnoughRoomLeftInStack(amount))
                {
                    slot.AddToStack(amount);
                    OnPlayerHotbarDisplayRequested?.Invoke(primaryInventorySystem);
                    OnPlayerInventoryChanged?.Invoke(primaryInventorySystem);
                    return true;
                }
            }
        }

        if (secondaryInventorySystem.ContainsItem(data, out List<InventorySlot> secondarySlots))
        {
            foreach (var slot in secondarySlots)
            {
                if (slot.EnoughRoomLeftInStack(amount))
                {
                    slot.AddToStack(amount);
                    OnPlayerInventoryChanged?.Invoke(secondaryInventorySystem);
                    return true;
                }
            }
        }

        if (primaryInventorySystem.HasFreeSlot(out InventorySlot freePrimarySlot))
        {
            if (freePrimarySlot.EnoughRoomLeftInStack(amount))
            {
                freePrimarySlot.UpdateInventorySlot(data, amount);
                OnPlayerHotbarDisplayRequested?.Invoke(primaryInventorySystem);
                OnPlayerInventoryChanged?.Invoke(primaryInventorySystem);
                return true;
            }
        }

        if (secondaryInventorySystem.HasFreeSlot(out InventorySlot freeSecondarySlot))
        {
            if (freeSecondarySlot.EnoughRoomLeftInStack(amount))
            {
                freeSecondarySlot.UpdateInventorySlot(data, amount);
                OnPlayerInventoryChanged?.Invoke(secondaryInventorySystem);
                return true;
            }
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
