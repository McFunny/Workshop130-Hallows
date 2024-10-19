using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    [SerializeField] private InventoryItemData itemData; // Reference to the data
    [SerializeField] private int stackSize; // Current stack size - how many of the data do we have?

    public InventoryItemData ItemData => itemData;
    public int StackSize => stackSize;



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
