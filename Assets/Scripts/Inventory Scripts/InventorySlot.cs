using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    //DONT SAVE INVENTORYITEMDATA. SAVE THE ID. MAKE SAVE AND LOAD FUNCTIONS FOR THE INVENTORY TO POPULATE THE DATA FROM THE DATABASE, NOT BY SAVING THE TEMP REFERENCE TO THE INSTANCEID/OBJECT
    [SerializeField] private InventoryItemData itemData; // Reference to the data
    [SerializeField] private int stackSize; // Current stack size - how many of the data do we have?

    public InventoryItemData ItemData => itemData;
    public int StackSize => stackSize;
    [Flags]
    public enum AcceptedItemType
    {
        None = 0,
        Misc = 1 << 0,
        Consumable = 1 << 1,
        Tool = 1 << 2,
        Structure = 1 << 3,
        BarnStructure = 1 << 4,
        CabinDecor = 1 << 5,
        Seed  = 1 << 6,
        Ammo = 1 << 7,
        Creature = 1 << 8,
        Bug = 1 << 9,
        Throwable = 1 << 10,
        Trinket = 1 << 11,

        Everything = ~0
    }

    public AcceptedItemType acceptedItemType = AcceptedItemType.Everything;

    public InventorySlot(InventoryItemData source, int amount) // Constructor to make a occupied inventory slot
    {
        itemData = source;
        stackSize = amount;
    }

    public InventorySlot() // Constructor to make an empty inventory slot
    {
        ClearSlot();
    }

    public void ClearSlot() // Clears the slot
    {
        itemData = null;
        stackSize = -1;
    }

    public void AssignItem(InventorySlot invSlot)
    {
        if (itemData == invSlot.itemData) // Same item, add to stack
        {
            AddToStack(invSlot.StackSize);
        }
        else // New item, overwrite
        {
            itemData = invSlot.itemData;
            stackSize = invSlot.stackSize; // Correctly set the stack size
        }
    }


    public void RemoveFromStack(int amount)
    {
        stackSize -= amount;
        if (stackSize <= 0)
        {
            ClearSlot(); // Clears the slot if stack is zero or less
        }
    }


    public void UpdateInventorySlot(InventoryItemData data, int amount) // Updates slot directly
    {
        itemData = data;
        stackSize = amount;
    }

    public bool RoomLeftInStack(int amountToAdd, out int amountRemaining) // Would there be enough room in stack for what we are trying to add
    {
        amountRemaining = itemData.maxStackSize - stackSize;

        return EnoughRoomLeftInStack(amountToAdd);
    }

    public bool EnoughRoomLeftInStack(int amountToAdd)
    {
        if(itemData == null || itemData != null && stackSize + amountToAdd <= itemData.maxStackSize) return true;
        else return false;
    }

    public void AddToStack(int amount)
    {
        stackSize += amount;
    }

    public bool SplitStack(out InventorySlot splitStack)
    {
        if (stackSize <= 1) // Is there enough to actually split?
        {
            splitStack = null;
            return false;
        }

        int halfStack = Mathf.RoundToInt(stackSize / 2); // Get half the stack
        RemoveFromStack(halfStack);

        splitStack = new InventorySlot(ItemData, halfStack); //creates a copy of this slot with 1/2 the stack size
        return true;
    }
}

[System.Serializable]
public struct InventorySlotSaveData
{
    public int itemID;
    public int stackSize;

    public InventorySlotSaveData(int id, int stack)
    {
        itemID = id;
        stackSize = stack;
    }
}
