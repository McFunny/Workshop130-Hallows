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

    public bool ContainsItem(InventoryItemData itemToAdd, out List<InventorySlot> invSlot) //Do any of our slots have the item to add in them?
    {
       invSlot = InventorySlots.Where(i => i.ItemData == itemToAdd).ToList(); // If they do get a list of all of them

        return invSlot == null || invSlot.Count == 0 ? false : true; // If they do return true, if not return false
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
                    amount -= stackSize; 
                }

                OnInventorySlotChanged?.Invoke(slot);
                if(itemsRemoved >= amount) break;
            }
        }
    }
}
