using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using UnityEngine.UIElements;

[System.Serializable]

public class InventorySystem
{
    [SerializeField] private List<InventorySlot> inventorySlots; 

    public List<InventorySlot> InventorySlots => inventorySlots;
    public int InventorySize => InventorySlots.Count;

    public UnityAction<InventorySlot> OnInventorySlotChanged;

    public InventorySystem(int size) // Constructor that sets the amount of slots
    {
        inventorySlots = new List<InventorySlot>(size);

        for (int i = 0; i < size; i++)
        {
            InventorySlots.Add(new InventorySlot());
        }
    }

    public bool AddToInventory(InventoryItemData itemToAdd, int amountToAdd)
    {
        if (ContainsItem(itemToAdd, out List<InventorySlot> invSlot)) //check if item already exists in inventory
        {
            foreach (var slot in invSlot)
            {
                if(slot.EnoughRoomLeftInStack(amountToAdd))
                {
                    slot.AddToStack(amountToAdd);
                    OnInventorySlotChanged?.Invoke(slot);
                    return true;
                }
            }
           
        }

        if (HasFreeSlot(out InventorySlot freeSlot)) //gets the first available slot
        {
            if (freeSlot.EnoughRoomLeftInStack(amountToAdd))
            { 
                freeSlot.UpdateInventorySlot(itemToAdd, amountToAdd);
                OnInventorySlotChanged?.Invoke(freeSlot);
                return true;
            }
            // Add implementation to only take what can fill the stack, check for another free slot to put the remainder in
        }

        return false;
    }

    public bool CanAddToInventory(InventoryItemData itemToAdd, int amountToAdd)
    {
        if (ContainsItem(itemToAdd, out List<InventorySlot> invSlot)) //check if item already exists in inventory
        {
            foreach (var slot in invSlot)
            {
                if(slot.EnoughRoomLeftInStack(amountToAdd))
                {
                    return true;
                }
            }
           
        }

        return false;
    }

    public bool ContainsItem(InventoryItemData itemToAdd, out List<InventorySlot> invSlot) //Do any of our slots have the item to add in them?
    {
       invSlot = InventorySlots.Where(i => i.ItemData == itemToAdd).ToList(); // If they do get a list of all of them

        return invSlot == null || invSlot.Count == 0 ? false : true; // If they do return true, if not return false
    }

    public bool ContainsItems(List<InventoryItemData> itemsToAdd, out List<InventorySlot> invSlot) //Do any of our slots have the item to add in them?
    {
       invSlot = InventorySlots.Where(i => itemsToAdd.Contains(i.ItemData)).ToList(); // If they do get a list of all of them

        return invSlot == null || invSlot.Count == 0 ? false : true; // If they do return true, if not return false
    }

    public int ReturnItemCount(InventoryItemData itemToFind)
    {
        int i = 0;
        List<InventorySlot> invSlot = InventorySlots.Where(i => i.ItemData == itemToFind).ToList(); // If they do get a list of all of them

        foreach(InventorySlot s in invSlot)
        {
            i += s.StackSize;
        }

        return i;
    }

    public bool HasFreeSlot(out InventorySlot freeSlot)
    {
      freeSlot = InventorySlots.FirstOrDefault(i => i.ItemData == null); //Get the first free slot
        return freeSlot == null ? false : true;
    }

    public void RemoveItemsFromInventory(InventoryItemData data, int amount)
    {
        int itemsRemoved = 0;
        if (ContainsItem(data, out List<InventorySlot> invSlot))
        {
            
            foreach (var slot in invSlot)
            {
                var stackSize = slot.StackSize;

                if (stackSize > amount)
                {
                    slot.RemoveFromStack(amount);
                    itemsRemoved = amount;
                }
                else
                {
                    itemsRemoved += stackSize;
                    slot.RemoveFromStack(stackSize);
                    //amount -= stackSize; 
                }

                OnInventorySlotChanged?.Invoke(slot);
                if(itemsRemoved >= amount) break;
            }
        }
    }

    public InventorySystemSaveData GetSaveData()
    {
        List<InventorySlotSaveData> slotSaves = new List<InventorySlotSaveData>();
        foreach (var slot in InventorySlots)
        {
            if (slot.ItemData != null)
            {
                slotSaves.Add(new InventorySlotSaveData(slot.ItemData.ID, slot.StackSize));
            }
            else
            {
                slotSaves.Add(new InventorySlotSaveData(-1, -1)); //This creates an empty slot
            }
        }
        return new InventorySystemSaveData(slotSaves);
    }

    public void LoadFromSaveData(InventorySystemSaveData saveData, Database database) //Also call this for when we dynamically change inventory size
    {
        inventorySlots.Clear();
        foreach (var slotData in saveData.savedSlots)
        {
            if (slotData.itemID != -1)
            {
                InventoryItemData data = database.GetItem(slotData.itemID);
                inventorySlots.Add(new InventorySlot(data, slotData.stackSize));
            }
            else
            {
                inventorySlots.Add(new InventorySlot()); // This also creates an empty slot
            }
        }
    }

    public bool ContainsAnyItems()
    {
        foreach (var slot in inventorySlots)
        {
            if (slot.StackSize != -1) return true;
        }
        return false;
    }

}

[System.Serializable]
public struct InventorySystemSaveData
{
    public List<InventorySlotSaveData> savedSlots;

    public InventorySystemSaveData(List<InventorySlotSaveData> slots)
    {
        savedSlots = slots;
    }
}
