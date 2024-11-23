
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class InventoryDisplay : MonoBehaviour
{
    [SerializeField] MouseItemData mouseInventoryItem;

    protected InventorySystem inventorySystem;
    protected Dictionary<InventorySlot_UI, InventorySlot> slotDictionary; // Pair up the UI slots with the system slots
    public InventorySystem InventorySystem => inventorySystem;
    public Dictionary<InventorySlot_UI, InventorySlot> SlotDictionary => slotDictionary;

    public abstract void AssignSlot(InventorySystem invToDisplay); // Implemented in child classes


    protected virtual void Start()
    {

    }

    protected virtual void UpdateSlot(InventorySlot updatedSlot)
    {
        foreach (var slot in slotDictionary)
        {
            if (slot.Value == updatedSlot) // Slot value - the "under the hood" inventory slot.
            {
                slot.Key.UpdateUISlot(updatedSlot); // slot key - the ui representation of the value/
            }
        }
    }

    public void HandleSlotLeftClick(InventorySlot_UI clickedUISlot)
    {
        print(clickedUISlot);
        bool isShiftPress = Input.GetKey(KeyCode.LeftShift);
        PlayerInventoryHolder.OnPlayerInventoryChanged?.Invoke(inventorySystem);
        // Left-click logic:
        if (clickedUISlot.AssignedInventorySlot.ItemData != null && mouseInventoryItem.assignedInventorySlot.ItemData == null)
        {
            if (isShiftPress && clickedUISlot.AssignedInventorySlot.SplitStack(out InventorySlot halfStackSlot))
            {
                mouseInventoryItem.UpdateMouseSlot(halfStackSlot);
                clickedUISlot.UpdateUISlot();
                PlayerInventoryHolder.OnPlayerInventoryChanged?.Invoke(inventorySystem);
                return;
            }
            else
            {
                mouseInventoryItem.UpdateMouseSlot(clickedUISlot.AssignedInventorySlot);
                clickedUISlot.ClearSlot();
                PlayerInventoryHolder.OnPlayerInventoryChanged?.Invoke(inventorySystem);
                return;
            }
        }

        if (clickedUISlot.AssignedInventorySlot.ItemData == null && mouseInventoryItem.assignedInventorySlot.ItemData != null)
        {
            clickedUISlot.AssignedInventorySlot.AssignItem(mouseInventoryItem.assignedInventorySlot);
            clickedUISlot.UpdateUISlot();
            mouseInventoryItem.ClearSlot();
            PlayerInventoryHolder.OnPlayerInventoryChanged?.Invoke(inventorySystem);
            return;
        }

        if (clickedUISlot.AssignedInventorySlot.ItemData != null && mouseInventoryItem.assignedInventorySlot.ItemData != null)
        {
            bool isSameItem = clickedUISlot.AssignedInventorySlot.ItemData == mouseInventoryItem.assignedInventorySlot.ItemData;

            if (isSameItem && clickedUISlot.AssignedInventorySlot.EnoughRoomLeftInStack(mouseInventoryItem.assignedInventorySlot.StackSize))
            {
                clickedUISlot.AssignedInventorySlot.AssignItem(mouseInventoryItem.assignedInventorySlot);
                clickedUISlot.UpdateUISlot();
                mouseInventoryItem.ClearSlot();
                PlayerInventoryHolder.OnPlayerInventoryChanged?.Invoke(inventorySystem);
                return;
            }
            else if (isSameItem && !clickedUISlot.AssignedInventorySlot.RoomLeftInStack(mouseInventoryItem.assignedInventorySlot.StackSize, out int leftInStack))
            {
                int remainingOnMouse = mouseInventoryItem.assignedInventorySlot.StackSize - leftInStack;
                clickedUISlot.AssignedInventorySlot.AddToStack(leftInStack);
                clickedUISlot.UpdateUISlot();

                var newItem = new InventorySlot(mouseInventoryItem.assignedInventorySlot.ItemData, remainingOnMouse);
                mouseInventoryItem.ClearSlot();
                mouseInventoryItem.UpdateMouseSlot(newItem);
                PlayerInventoryHolder.OnPlayerInventoryChanged?.Invoke(inventorySystem);
                return;
            }
            else if (!isSameItem)
            {
                SwapSlots(clickedUISlot);
                PlayerInventoryHolder.OnPlayerInventoryChanged?.Invoke(inventorySystem);
                return;
            }
        }
    }

    public void HandleSlotRightClick(InventorySlot_UI clickedUISlot)
    {
        // Right-click on an empty slot to add one from the mouse stack
        if (clickedUISlot.AssignedInventorySlot.ItemData == null && mouseInventoryItem.assignedInventorySlot.ItemData != null)
        {
            // Add one item from the mouse inventory to the clicked slot
            clickedUISlot.AssignedInventorySlot.AssignItem(new InventorySlot(mouseInventoryItem.assignedInventorySlot.ItemData, 1));
            mouseInventoryItem.assignedInventorySlot.RemoveFromStack(1); // Remove one from the mouse

            // Update the clicked slot UI
            clickedUISlot.UpdateUISlot();

            // Check if the mouse slot stack is empty after the removal
            if (mouseInventoryItem.assignedInventorySlot.StackSize <= 0)
            {
                mouseInventoryItem.ClearSlot(); // Clear the mouse if stack is empty
            }
            else
            {
                // Create a new item representing the remaining stack on the mouse
                var newItem = new InventorySlot(mouseInventoryItem.assignedInventorySlot.ItemData, mouseInventoryItem.assignedInventorySlot.StackSize);
                mouseInventoryItem.ClearSlot();
                mouseInventoryItem.UpdateMouseSlot(newItem); // Update the mouse UI with the new stack
            }
            PlayerInventoryHolder.OnPlayerInventoryChanged?.Invoke(inventorySystem);
            return;
        }

        // Right-click on the same item to add one to the stack
        if (clickedUISlot.AssignedInventorySlot.ItemData != null && mouseInventoryItem.assignedInventorySlot.ItemData != null)
        {
            bool isSameItem = clickedUISlot.AssignedInventorySlot.ItemData == mouseInventoryItem.assignedInventorySlot.ItemData;

            if (isSameItem && clickedUISlot.AssignedInventorySlot.EnoughRoomLeftInStack(1))
            {
                // Add one to the clicked slot
                clickedUISlot.AssignedInventorySlot.AddToStack(1);
                clickedUISlot.UpdateUISlot();

                // Remove one from the mouse inventory
                mouseInventoryItem.assignedInventorySlot.RemoveFromStack(1);

                // Check if the mouse inventory stack is empty after removal
                if (mouseInventoryItem.assignedInventorySlot.StackSize <= 0)
                {
                    mouseInventoryItem.ClearSlot(); // Clear the mouse if stack is empty
                }
                else
                {
                    // Create a new item for the remaining stack and update the mouse UI
                    var newItem = new InventorySlot(mouseInventoryItem.assignedInventorySlot.ItemData, mouseInventoryItem.assignedInventorySlot.StackSize);
                    mouseInventoryItem.ClearSlot();
                    mouseInventoryItem.UpdateMouseSlot(newItem); // Update the mouse UI with the remaining stack
                }
                PlayerInventoryHolder.OnPlayerInventoryChanged?.Invoke(inventorySystem);
                return;
            }

            // Right-click on a different item does nothing
            PlayerInventoryHolder.OnPlayerInventoryChanged?.Invoke(inventorySystem);
            return;
        }
    }







    private void SwapSlots(InventorySlot_UI clickedUISlot)
    {
        var clonedSlot = new InventorySlot(mouseInventoryItem.assignedInventorySlot.ItemData, mouseInventoryItem.assignedInventorySlot.StackSize);
        mouseInventoryItem.ClearSlot();

        mouseInventoryItem.UpdateMouseSlot(clickedUISlot.AssignedInventorySlot);

        clickedUISlot.ClearSlot();
        clickedUISlot.AssignedInventorySlot.AssignItem(clonedSlot);
        clickedUISlot.UpdateUISlot();


    }
}
