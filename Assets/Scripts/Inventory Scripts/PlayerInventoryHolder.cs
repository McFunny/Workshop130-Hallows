 using SaveLoadSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerInventoryHolder : InventoryHolder
{
    public static PlayerInventoryHolder Instance;

   //[HideInInspector] public InventorySystem PrimaryInventorySystem => primaryInventorySystem;

    [SerializeField] protected int secondaryInventorySize;
    [SerializeField] public InventorySystem secondaryInventorySystem;
    [SerializeField] private Database _database;

    public static UnityAction<InventorySystem> OnPlayerHotbarDisplayRequested;
    public static UnityAction<InventorySystem> OnPlayerBackpackDisplayRequested;
    public static UnityAction<InventorySystem> OnPlayerInventoryChanged;

    public bool useDebugItems;

    ControlManager controlManager;

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

    private void OnEnable()
    {
        if(!controlManager) controlManager = FindFirstObjectByType<ControlManager>();
        controlManager.hotbarSwitch.action.started += SwitchHotBars;
        controlManager.hotbarSwitch.action.canceled += SwitchHotBars;
    }

    private void OnDisable()
    {
        SaveLoad.OnSaveGame -= SaveInventory;
        SaveLoad.OnLoadGame -= LoadInventory;

        controlManager.hotbarSwitch.action.started -= SwitchHotBars;
        controlManager.hotbarSwitch.action.canceled -= SwitchHotBars;
    }

    protected override void Awake()
    {
        base.Awake();
        secondaryInventorySystem = new InventorySystem(secondaryInventorySize);
        SaveLoad.OnSaveGame += SaveInventory;
        SaveLoad.OnLoadGame += LoadInventory;

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

    private void LoadInventory(SaveData data)
    {
        if (data.playerInventoryData.primaryInvSystemSave.savedSlots != null && data.playerInventoryData.secondaryInvSystemSave.savedSlots != null)
        {
            this.primaryInventorySystem = new InventorySystem(data.playerInventoryData.primaryInvSystemSave.savedSlots.Count);
            this.primaryInventorySystem.LoadFromSaveData(data.playerInventoryData.primaryInvSystemSave, _database);

            this.secondaryInventorySize = data.playerInventoryData.secondaryInventorySizeSave;
            this.secondaryInventorySystem = new InventorySystem(secondaryInventorySize);
            this.secondaryInventorySystem.LoadFromSaveData(data.playerInventoryData.secondaryInvSystemSave, _database);

            UpdateInventory();
        }
        else
        {
            Debug.Log("Missing saved inventory slot data.");
        }

    }

    public void IncreaseBackpackInventory() //For changing the size at runtime
    {
        this.secondaryInventorySize += 9;
        //store temp ref of current inventory
        InventorySystemSaveData tempData = this.secondaryInventorySystem.GetSaveData();
        for(int i = 0; i < 9; i++)
        {
            tempData.savedSlots.Add(new InventorySlotSaveData(-1, 0));
        }

        this.secondaryInventorySystem = new InventorySystem(secondaryInventorySize);
        this.secondaryInventorySystem.LoadFromSaveData(tempData, _database);

        UpdateInventory();
    }


    private void Start()
    {
        StartCoroutine(DelayedStart());
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(0.5f);
        if(!MainMenuScript.loadingData) EquipStartingItems();
    }

    private void SaveInventory()
    {
        SaveLoad.CurrentSaveData.playerInventoryData = new PlayerInventorySaveData(primaryInventorySystem, secondaryInventorySystem, secondaryInventorySize);
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

    public bool IsInventoryFull(InventoryItemData itemToAdd, int amountToAdd)
    {
        if (primaryInventorySystem.HasFreeSlot(out InventorySlot freePrimarySlot) || primaryInventorySystem.CanAddToInventory(itemToAdd, amountToAdd))
        {
            return false;
        }

        if (secondaryInventorySystem.HasFreeSlot(out InventorySlot freeSecondarySlot) || secondaryInventorySystem.CanAddToInventory(itemToAdd, amountToAdd))
        {
            return false;
        }

        return true;
    }

    public int ReturnItemCountInPlayerInventory(InventoryItemData itemToFind)
    {
        return primaryInventorySystem.ReturnItemCount(itemToFind) + secondaryInventorySystem.ReturnItemCount(itemToFind);
    }

    public int ReturnFreeSlots()
    {
        int freeSlots = 0;
        for(int i = 0; i < primaryInventorySystem.InventorySlots.Count; i++)
        {
            if(primaryInventorySystem.InventorySlots[i].ItemData == null) freeSlots++;
        }

        for(int i = 0; i < secondaryInventorySystem.InventorySlots.Count; i++)
        {
            if(secondaryInventorySystem.InventorySlots[i].ItemData == null) freeSlots++;
        }


        return freeSlots;
    }

    public bool CanQuickSwitch(bool intoPrimary, InventoryItemData itemToAdd, int amountToAdd, out InventorySlot _slot) //Between primary and secondary inventories
    {
        _slot = null; //returns the slot that is being swap to
        //if intoPrimary, you are trying to move a slot into primary, else vice versa
        if(intoPrimary)
        {
            if (primaryInventorySystem.HasFreeSlot(out InventorySlot freePrimarySlot) || primaryInventorySystem.CanAddToInventory(itemToAdd, amountToAdd))
            {
                if (primaryInventorySystem.ContainsItem(itemToAdd, out List<InventorySlot> primarySlots))
                {
                    foreach (var slot in primarySlots)
                    {
                        if (slot.EnoughRoomLeftInStack(amountToAdd))
                        {
                            slot.AddToStack(amountToAdd);
                            OnPlayerHotbarDisplayRequested?.Invoke(primaryInventorySystem);
                            OnPlayerInventoryChanged?.Invoke(primaryInventorySystem);
                            _slot = slot;
                            return true;
                        }
                    }
                }

                if (freePrimarySlot != null)
                {
                    if (freePrimarySlot.EnoughRoomLeftInStack(amountToAdd))
                    {
                        freePrimarySlot.UpdateInventorySlot(itemToAdd, amountToAdd);
                        OnPlayerHotbarDisplayRequested?.Invoke(primaryInventorySystem);
                        OnPlayerInventoryChanged?.Invoke(primaryInventorySystem);
                        _slot = freePrimarySlot;
                        return true;
                    }
                }
                return false;
            }
            return false;
        }
        else
        {
            if (secondaryInventorySystem.HasFreeSlot(out InventorySlot freeSecondarySlot) || secondaryInventorySystem.CanAddToInventory(itemToAdd, amountToAdd))
            {
                if (secondaryInventorySystem.ContainsItem(itemToAdd, out List<InventorySlot> primarySlots))
                {
                    foreach (var slot in primarySlots)
                    {
                        if (slot.EnoughRoomLeftInStack(amountToAdd))
                        {
                            slot.AddToStack(amountToAdd);
                            OnPlayerHotbarDisplayRequested?.Invoke(secondaryInventorySystem);
                            OnPlayerInventoryChanged?.Invoke(secondaryInventorySystem);
                            _slot = slot;
                            return true;
                        }
                    }
                }

                if (freeSecondarySlot != null)
                {
                    if (freeSecondarySlot.EnoughRoomLeftInStack(amountToAdd))
                    {
                        freeSecondarySlot.UpdateInventorySlot(itemToAdd, amountToAdd);
                        OnPlayerHotbarDisplayRequested?.Invoke(secondaryInventorySystem);
                        OnPlayerInventoryChanged?.Invoke(secondaryInventorySystem);
                        _slot = freeSecondarySlot;
                        return true;
                    }
                }
                return false;
            }
            return false;
        }
    }

    public bool CanQuickSwitchIntoChest(InventorySystem tertiarySystem, InventoryItemData itemToAdd, int amountToAdd, out InventorySlot _slot)//Between Tertiary and either primary or secondary inventories
    {
        _slot = null; //returns the slot that is being swap to
        if (tertiarySystem.HasFreeSlot(out InventorySlot freeSecondarySlot) || tertiarySystem.CanAddToInventory(itemToAdd, amountToAdd))
        {
            if (tertiarySystem.ContainsItem(itemToAdd, out List<InventorySlot> primarySlots))
            {
                foreach (var slot in primarySlots)
                {
                    if (slot.EnoughRoomLeftInStack(amountToAdd))
                    {
                        slot.AddToStack(amountToAdd);
                        OnPlayerHotbarDisplayRequested?.Invoke(tertiarySystem);
                        OnPlayerInventoryChanged?.Invoke(tertiarySystem);
                        _slot = slot;
                        return true;
                    }
                }
            }

            if (freeSecondarySlot != null)
            {
                if (freeSecondarySlot.EnoughRoomLeftInStack(amountToAdd))
                {
                    freeSecondarySlot.UpdateInventorySlot(itemToAdd, amountToAdd);
                    OnPlayerHotbarDisplayRequested?.Invoke(tertiarySystem);
                    OnPlayerInventoryChanged?.Invoke(tertiarySystem);
                    _slot = freeSecondarySlot;
                    return true;
                }
            }
            return false;
        }
        return false;
    }

    public bool CanQuickSwitchOutOfChest(InventoryItemData itemToAdd, int amountToAdd, out InventorySlot _slot)//Between Tertiary and either primary or secondary inventories
    {
        _slot = null; //returns the slot that is being swap to
        if (primaryInventorySystem.HasFreeSlot(out InventorySlot freePrimarySlot) || primaryInventorySystem.CanAddToInventory(itemToAdd, amountToAdd))
        {
            if (primaryInventorySystem.ContainsItem(itemToAdd, out List<InventorySlot> primarySlots))
            {
                foreach (var slot in primarySlots)
                {
                    if (slot.EnoughRoomLeftInStack(amountToAdd))
                    {
                        slot.AddToStack(amountToAdd);
                        OnPlayerHotbarDisplayRequested?.Invoke(primaryInventorySystem);
                        OnPlayerInventoryChanged?.Invoke(primaryInventorySystem);
                        _slot = slot;
                        return true;
                    }
                }
            }

            if (freePrimarySlot != null)
            {
                if (freePrimarySlot.EnoughRoomLeftInStack(amountToAdd))
                {
                    freePrimarySlot.UpdateInventorySlot(itemToAdd, amountToAdd);
                    OnPlayerHotbarDisplayRequested?.Invoke(primaryInventorySystem);
                    OnPlayerInventoryChanged?.Invoke(primaryInventorySystem);
                    _slot = freePrimarySlot;
                    return true;
                }
            }
            return false;
        }

        if (secondaryInventorySystem.HasFreeSlot(out InventorySlot freeSecondarySlot) || secondaryInventorySystem.CanAddToInventory(itemToAdd, amountToAdd))
        {
            if (secondaryInventorySystem.ContainsItem(itemToAdd, out List<InventorySlot> secondarySlots))
            {
                foreach (var slot in secondarySlots)
                {
                    if (slot.EnoughRoomLeftInStack(amountToAdd))
                    {
                        slot.AddToStack(amountToAdd);
                        OnPlayerHotbarDisplayRequested?.Invoke(secondaryInventorySystem);
                        OnPlayerInventoryChanged?.Invoke(secondaryInventorySystem);
                        _slot = slot;
                        return true;
                    }
                }
            }

            if (freeSecondarySlot != null)
            {
                if (freeSecondarySlot.EnoughRoomLeftInStack(amountToAdd))
                {
                    freeSecondarySlot.UpdateInventorySlot(itemToAdd, amountToAdd);
                    OnPlayerHotbarDisplayRequested?.Invoke(primaryInventorySystem);
                    OnPlayerInventoryChanged?.Invoke(primaryInventorySystem);
                    _slot = freeSecondarySlot;
                    return true;
                }
            }
            return false;
        }
        return false;
    }

    public bool FindItemInBothInventories(InventoryItemData item)
    {
        if (!PrimaryInventorySystem.ContainsItem(item, out List<InventorySlot> invSlot))
        {
            if (!secondaryInventorySystem.ContainsItem(item, out List<InventorySlot> invSlot2))
            {
                return false;
            }
            else return true;
        }
        else return true;
    }

    public void RemoveItemsFromBothInventories(List<ItemWithAmount> list)
    {
        for(int i = 0; i < list.Count; i++)
        {
            int amountToRemove = list[i].amount;

            amountToRemove -= primaryInventorySystem.ReturnItemCount(list[i].item);
            primaryInventorySystem.RemoveItemsFromInventory(list[i].item, list[i].amount);

            if(amountToRemove > 0)
            {
                secondaryInventorySystem.RemoveItemsFromInventory(list[i].item, amountToRemove);
            }
        }
    }

    public void SwitchHotBars(InputAction.CallbackContext obj)
    {
        if(PlayerMovement.restrictMovementTokens > 0 || InputManager.isCharging || PauseScript.isPaused) return;

        List<InventorySlot> currentPInventory = new List<InventorySlot>();
        List<InventorySlot> currentSInventoryRow1 = new List<InventorySlot>();
        List<InventorySlot> currentSInventoryRow2 = new List<InventorySlot>();

        List<InventorySlot> newSInventory = new List<InventorySlot>();

        for(int i = 0; i < 9; i++)
        {
            currentPInventory.Add(new InventorySlot(primaryInventorySystem.InventorySlots[i].ItemData, primaryInventorySystem.InventorySlots[i].StackSize));
        }

        for(int i = 0; i < secondaryInventorySystem.InventorySize; i++)
        {
            if(i < 9) currentSInventoryRow1.Add(new InventorySlot(secondaryInventorySystem.InventorySlots[i].ItemData, secondaryInventorySystem.InventorySlots[i].StackSize));
            else currentSInventoryRow2.Add(new InventorySlot(secondaryInventorySystem.InventorySlots[i].ItemData, secondaryInventorySystem.InventorySlots[i].StackSize));
        }

        newSInventory.AddRange(currentSInventoryRow2);
        newSInventory.AddRange(currentPInventory); //Reverse the order. This is now the secondary inventory

        primaryInventorySystem.ForcePopulateInventory(currentSInventoryRow1);
        secondaryInventorySystem.ForcePopulateInventory(newSInventory);
        UpdateInventory();
        
    }

    public void UpdateInventory()
    {
        OnPlayerInventoryChanged?.Invoke(primaryInventorySystem);
        OnPlayerInventoryChanged?.Invoke(secondaryInventorySystem);
       
    }

    public void UpdateOpenInventory()
    {
        OnPlayerInventoryChanged?.Invoke(primaryInventorySystem);
        OnPlayerInventoryChanged?.Invoke(secondaryInventorySystem);
        OnPlayerBackpackDisplayRequested?.Invoke(secondaryInventorySystem);
        if(InventoryUIController.Instance.chestPanel.gameObject.activeSelf) InventoryUIController.Instance.chestPanel.UpdateSlots();
    }
   

}

[System.Serializable]
public struct PlayerInventorySaveData
{
    public InventorySystemSaveData primaryInvSystemSave;
    public InventorySystemSaveData secondaryInvSystemSave;
    public int secondaryInventorySizeSave;

    public PlayerInventorySaveData(InventorySystem primary, InventorySystem secondary, int secondarySize)
    {
        primaryInvSystemSave = primary.GetSaveData();
        secondaryInvSystemSave = secondary.GetSaveData();
        secondaryInventorySizeSave = secondarySize;
    }

}
