
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

public abstract class InventoryDisplay : MonoBehaviour
{
    [SerializeField] MouseItemData mouseInventoryItem;

    protected InventorySystem inventorySystem;
    protected Dictionary<InventorySlot_UI, InventorySlot> slotDictionary; // Pair up the UI slots with the system slots
    public InventorySystem InventorySystem => inventorySystem;
    public Dictionary<InventorySlot_UI, InventorySlot> SlotDictionary => slotDictionary;

    ControlManager controlManager;

    public abstract void AssignSlot(InventorySystem invToDisplay); // Implemented in child classes

    protected virtual void Start()
    {

    }

    protected virtual void UpdateSlot(InventorySlot updatedSlot)
    {
        foreach (var slot in slotDictionary)
        {
            //print(slot);
            if (slot.Value == updatedSlot) // Slot value - the "under the hood" inventory slot.
            {
                slot.Key.UpdateUISlot(updatedSlot); // slot key - the ui representation of the value/
            }
        }
    }

    public virtual void UpdateSlots()
    {
        foreach (var slot in slotDictionary)
        {
            slot.Key.UpdateUISlot(slot.Key.AssignedInventorySlot);
        }
    }

    void PrintSystem(InventorySystem i)
    {
        if(i == PlayerInventoryHolder.Instance.PrimaryInventorySystem) print("I clicked in the Primary");
        if(i == PlayerInventoryHolder.Instance.secondaryInventorySystem) print("I clicked in the Secondary");
        if(i == InventoryUIController.Instance.chestPanel.InventorySystem) print("I clicked in the Tertiary");
    }

    public void HandleSlotLeftClick(InventorySlot_UI clickedUISlot)
    {
        print(clickedUISlot);
        bool isShiftPress = Input.GetKey(KeyCode.LeftShift);
        PlayerInventoryHolder.OnPlayerInventoryChanged?.Invoke(inventorySystem);
        PrintSystem(inventorySystem);
        // Left-click logic:
        if (clickedUISlot.AssignedInventorySlot.ItemData != null && mouseInventoryItem.assignedInventorySlot.ItemData == null)
        {
            ///////////The player clicked on a slot without holding an item//////////////
            bool intoPrimary = false;
            if(inventorySystem != PlayerInventoryHolder.Instance.PrimaryInventorySystem) intoPrimary = true;

            ///////////Checking to see if a chest is opened//////////////
            if(isShiftPress && InventoryUIController.Instance.chestPanel.gameObject.activeSelf)
            {
                if(inventorySystem != InventoryUIController.Instance.chestPanel.InventorySystem)
                {
                    ///////////Checking to see if we can quick switch the item into a chest//////////////
                    if(PlayerInventoryHolder.Instance.CanQuickSwitchIntoChest(InventoryUIController.Instance.chestPanel.InventorySystem, clickedUISlot.AssignedInventorySlot.ItemData, 
                    clickedUISlot.AssignedInventorySlot.StackSize, out InventorySlot t_slot))
                    {
                        ///////////Moving item into the chest//////////////
                        clickedUISlot.ClearSlot();
                        //PlayerInventoryHolder.OnPlayerInventoryChanged?.Invoke(inventorySystem);
                        PlayerInventoryHolder.Instance.UpdateOpenInventory();
                        //if(t_slot != null) UpdateSlot(t_slot); 
                        return;
                    }
                    else
                    {
                        //unable to put object into chest due to being full
                    }
                }
                else
                {
                    ///////////Checking to see if we can quick switch the item out of a chest//////////////
                    if(PlayerInventoryHolder.Instance.CanQuickSwitchOutOfChest(clickedUISlot.AssignedInventorySlot.ItemData, clickedUISlot.AssignedInventorySlot.StackSize, out InventorySlot o_slot))
                    {
                        ///////////Moving item into the chest//////////////
                        clickedUISlot.ClearSlot();
                        //PlayerInventoryHolder.OnPlayerInventoryChanged?.Invoke(inventorySystem);
                        PlayerInventoryHolder.Instance.UpdateOpenInventory();
                        //if(o_slot != null) UpdateSlot(o_slot); 
                        return;
                    }
                }

                
            }

            ///////////Checking to see if we can quick switch the item into one of the player inventories//////////////
            else if (isShiftPress && PlayerInventoryHolder.Instance.CanQuickSwitch(intoPrimary, clickedUISlot.AssignedInventorySlot.ItemData, clickedUISlot.AssignedInventorySlot.StackSize, out InventorySlot slot))
            {
                //for quick swapping into one of the player's inventories
                clickedUISlot.ClearSlot();
                //PlayerInventoryHolder.OnPlayerInventoryChanged?.Invoke(inventorySystem);
                PlayerInventoryHolder.Instance.UpdateOpenInventory();
                //if(slot != null) UpdateSlot(slot); 
                return;
            }
            else
            {
                ///////////The player picked up an item from a slot//////////////
                mouseInventoryItem.UpdateMouseSlot(clickedUISlot.AssignedInventorySlot);
                clickedUISlot.ClearSlot();
                PlayerInventoryHolder.OnPlayerInventoryChanged?.Invoke(inventorySystem);
                return;
            }
        } 

        if (clickedUISlot.AssignedInventorySlot.ItemData == null && mouseInventoryItem.assignedInventorySlot.ItemData != null)
        {
            ///////////The player clicked on an empty slot while holding an item//////////////
            clickedUISlot.AssignedInventorySlot.AssignItem(mouseInventoryItem.assignedInventorySlot);
            clickedUISlot.UpdateUISlot();
            mouseInventoryItem.ClearSlot();
            PlayerInventoryHolder.OnPlayerInventoryChanged?.Invoke(inventorySystem);
            return;
        }

        if (clickedUISlot.AssignedInventorySlot.ItemData != null && mouseInventoryItem.assignedInventorySlot.ItemData != null)
        {
            ///////////The player clicked on a slot while holding an item//////////////
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
       // bool isShiftPress = Input.GetKey(KeyCode.LeftShift);
        PlayerInventoryHolder.OnPlayerInventoryChanged?.Invoke(inventorySystem);
        // Right-click on an empty slot to add one from the mouse stack
        if (clickedUISlot.AssignedInventorySlot.ItemData != null && mouseInventoryItem.assignedInventorySlot.ItemData == null)
        {
            if (clickedUISlot.AssignedInventorySlot.SplitStack(out InventorySlot halfStackSlot))
            {
                mouseInventoryItem.UpdateMouseSlot(halfStackSlot);
                clickedUISlot.UpdateUISlot();
                PlayerInventoryHolder.OnPlayerInventoryChanged?.Invoke(inventorySystem);
                return;
            }
            else
            {
                //mouseInventoryItem.UpdateMouseSlot(clickedUISlot.AssignedInventorySlot);
                //clickedUISlot.ClearSlot();
                //PlayerInventoryHolder.OnPlayerInventoryChanged?.Invoke(inventorySystem);
                return;
            }
        }

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

    public void HandleLeftBumper(InventorySlot_UI clickedUISlot) //quick stack
    {
        PlayerInventoryHolder.OnPlayerInventoryChanged?.Invoke(inventorySystem);
        if (clickedUISlot.AssignedInventorySlot.ItemData != null && mouseInventoryItem.assignedInventorySlot.ItemData == null)
        {
            bool intoPrimary = false;
            if(inventorySystem == PlayerInventoryHolder.Instance.secondaryInventorySystem) intoPrimary = true;
            /*if (PlayerInventoryHolder.Instance.CanQuickSwitch(intoPrimary, clickedUISlot.AssignedInventorySlot.ItemData, clickedUISlot.AssignedInventorySlot.StackSize, out InventorySlot slot))
            {
                //clickedUISlot.UpdateUISlot();
                clickedUISlot.ClearSlot();
                //PlayerInventoryHolder.OnPlayerInventoryChanged?.Invoke(inventorySystem);
                //PlayerInventoryHolder.Instance.UpdateOpenInventory(); //WHY WONT IT UPDATE THE BACKPACK
                var allDisplays = FindObjectsByType<InventoryDisplay>(FindObjectsSortMode.None); 

                for(int i = 0; i < allDisplays.Length; i++)
                {
                    allDisplays[i].UpdateSlots();
                }
                UpdateSlots(); 
                
                clickedUISlot.ParentDisplay.UpdateSlots();
                return;
            }*/


            ///////////Checking to see if a chest is opened//////////////
            if(InventoryUIController.Instance.chestPanel.gameObject.activeSelf)
            {
                if(inventorySystem != InventoryUIController.Instance.chestPanel.InventorySystem)
                {
                    ///////////Checking to see if we can quick switch the item into a chest//////////////
                    if(PlayerInventoryHolder.Instance.CanQuickSwitchIntoChest(InventoryUIController.Instance.chestPanel.InventorySystem, clickedUISlot.AssignedInventorySlot.ItemData, 
                    clickedUISlot.AssignedInventorySlot.StackSize, out InventorySlot t_slot))
                    {
                        ///////////Moving item into the chest//////////////
                        clickedUISlot.ClearSlot();
                        //PlayerInventoryHolder.OnPlayerInventoryChanged?.Invoke(inventorySystem);
                        PlayerInventoryHolder.Instance.UpdateOpenInventory();
                        //if(t_slot != null) UpdateSlot(t_slot); 
                        return;
                    }
                    else
                    {
                        //unable to put object into chest due to being full
                    }
                }
                else
                {
                    ///////////Checking to see if we can quick switch the item out of a chest//////////////
                    if(PlayerInventoryHolder.Instance.CanQuickSwitchOutOfChest(clickedUISlot.AssignedInventorySlot.ItemData, clickedUISlot.AssignedInventorySlot.StackSize, out InventorySlot o_slot))
                    {
                        ///////////Moving item into the chest//////////////
                        clickedUISlot.ClearSlot();
                        //PlayerInventoryHolder.OnPlayerInventoryChanged?.Invoke(inventorySystem);
                        PlayerInventoryHolder.Instance.UpdateOpenInventory();
                        //if(o_slot != null) UpdateSlot(o_slot); 
                        return;
                    }
                }

                
            }

            ///////////Checking to see if we can quick switch the item into one of the player inventories//////////////
            else if (PlayerInventoryHolder.Instance.CanQuickSwitch(intoPrimary, clickedUISlot.AssignedInventorySlot.ItemData, clickedUISlot.AssignedInventorySlot.StackSize, out InventorySlot slot))
            {
                clickedUISlot.ClearSlot();
                var allDisplays = FindObjectsByType<InventoryDisplay>(FindObjectsSortMode.None); 

                for(int i = 0; i < allDisplays.Length; i++)
                {
                    allDisplays[i].UpdateSlots();
                }
                //UpdateSlots(); 
                
                //clickedUISlot.ParentDisplay.UpdateSlots();
                return;
            }
        } 
    }

    public void HandleRightBumper(InventorySlot_UI clickedUISlot) //half stack
    {
        PlayerInventoryHolder.OnPlayerInventoryChanged?.Invoke(inventorySystem);
        // Right-click on an empty slot to add one from the mouse stack
        if (clickedUISlot.AssignedInventorySlot.ItemData != null && mouseInventoryItem.assignedInventorySlot.ItemData == null)
        {
            if (clickedUISlot.AssignedInventorySlot.SplitStack(out InventorySlot halfStackSlot))
            {
                mouseInventoryItem.UpdateMouseSlot(halfStackSlot);
                clickedUISlot.UpdateUISlot();
                PlayerInventoryHolder.OnPlayerInventoryChanged?.Invoke(inventorySystem);
                return;
            }
            else
            {
                //mouseInventoryItem.UpdateMouseSlot(clickedUISlot.AssignedInventorySlot);
                //clickedUISlot.ClearSlot();
                //PlayerInventoryHolder.OnPlayerInventoryChanged?.Invoke(inventorySystem);
                return;
            }
        }

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
