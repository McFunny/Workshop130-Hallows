using System;
using UnityEngine;

public class HotbarDisplay : MonoBehaviour
{
    public InputManager inputManager;  // Reference to the InputManager
    public InventorySlot_UI[] hotbarSlots;   // Array of hotbar slots (InventorySlot_UI)
    public static InventorySlot_UI currentSlot;
    private int currentIndex;

    private void Start()
    {
        currentIndex = 0;
        currentSlot = hotbarSlots[currentIndex];
        currentSlot.ToggleHighlight(); // Highlight the initial slot
    }

    private void OnEnable()
    {
        if (inputManager != null)
        {
            inputManager.OnNumberPressed += HandleNumberPressed;
            inputManager.OnScrollInput += HandleScrollInput;
        }
    }

    private void OnDisable()
    {
        if (inputManager != null)
        {
            inputManager.OnNumberPressed -= HandleNumberPressed;
            inputManager.OnScrollInput -= HandleScrollInput;
        }
    }

    private void HandleScrollInput(int direction)
    {
        currentIndex += direction;

        if (currentIndex > (hotbarSlots.Length - 1)) currentIndex = 0;
        if (currentIndex < 0) currentIndex = hotbarSlots.Length - 1;

        SelectHotbarSlot(currentIndex);
    }

    private void HandleNumberPressed(int number)
    {
        if (number > 0 && number <= hotbarSlots.Length)
        {
            SelectHotbarSlot(number - 1);  // Hotbar slots are 0-indexed
        }
    }

    private void SelectHotbarSlot(int slotIndex) //if possible, call this again when picking up an item to refresh hand item, or find a workaround (preferred)
    {
        // Turn off highlight on the current slot
        if (currentSlot != null)
        {
            currentSlot.ToggleHighlight();
        }

        // Set the new slot
        currentIndex = slotIndex;
        currentSlot = hotbarSlots[slotIndex];

        // Turn on highlight for the newly selected slot
        currentSlot.ToggleHighlight();

        // Optionally, use the item in the selected slot
        if (currentSlot.AssignedInventorySlot != null && currentSlot.AssignedInventorySlot.ItemData != null)
        {
            currentSlot.AssignedInventorySlot.ItemData.UseItem();
            ToolItem t_item = currentSlot.AssignedInventorySlot.ItemData as ToolItem;
            if(t_item)
            {
                HandItemManager.Instance.SwapHandModel(t_item.tool);
            }
            else HandItemManager.Instance.SwapHandModel(ToolType.Null);
        }
        else
        {
            Debug.Log($"No item in hotbar slot {slotIndex + 1}");
            HandItemManager.Instance.ClearHandModel();
        }
    }
}
