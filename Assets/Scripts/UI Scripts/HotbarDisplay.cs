using System;
using UnityEngine;

public class HotbarDisplay : MonoBehaviour
{
    public InputManager inputManager;  // Reference to the InputManager
    public InventorySlot_UI[] hotbarSlots;   // Array of hotbar slots (InventorySlot_UI)
    public static InventorySlot_UI currentSlot;
    private int currentIndex;
    TooltipControlsScript tooltipControls; //Handles hovering over structure with item

    public InventoryItemData torch, pyrefly;


    private void Start()
    {
        currentIndex = 0;
        currentSlot = hotbarSlots[currentIndex];
        tooltipControls = FindObjectOfType<TooltipControlsScript>();
        currentSlot.ToggleHighlight(); // Highlight the initial slot
        SelectHotbarSlot(currentIndex);
    }

    private void OnEnable()
    {
        if (inputManager != null)
        {
            inputManager.OnNumberPressed += HandleNumberPressed;
            inputManager.OnScrollInput += HandleScrollInput;
        }

        PlayerInventoryHolder.OnPlayerInventoryChanged += UpdateHandItem;
        InventoryUIController.OnInventoryOpened += EnableDisableNavigation;
    }

    private void OnDisable()
    {
        if (inputManager != null)
        {
            inputManager.OnNumberPressed -= HandleNumberPressed;
            inputManager.OnScrollInput -= HandleScrollInput;
        }

        PlayerInventoryHolder.OnPlayerInventoryChanged -= UpdateHandItem;
        InventoryUIController.OnInventoryOpened -= EnableDisableNavigation;
    }

    private void HandleScrollInput(int direction)
    {
        if (PlayerMovement.restrictMovementTokens > 0 || PlayerInteraction.Instance.toolCooldown || PauseScript.isPaused) return; //to solve the issue where there is a skip in the hotbar

        if (PlayerMovement.isCodexOpen) return;
        currentIndex += direction;

        if (currentIndex > (hotbarSlots.Length - 1)) currentIndex = 0;
        if (currentIndex < 0) currentIndex = hotbarSlots.Length - 1;

        SelectHotbarSlot(currentIndex);
    }

    private void HandleNumberPressed(int number)
    {
        if (PauseScript.isPaused) return;
        if (PlayerMovement.isCodexOpen) return;
        if (number > 0 && number <= hotbarSlots.Length)
        {
            SelectHotbarSlot(number - 1);  // Hotbar slots are 0-indexed
        }
    }

    public int FindItemInHotbar(InventoryItemData item)
    {
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            InventorySlot_UI s = hotbarSlots[i];
            if (s.AssignedInventorySlot != null && s.AssignedInventorySlot.ItemData == item) return i;
        }
        return -1;
    }

    public void SelectHotbarSlot(int slotIndex)
    {
        if (PlayerMovement.restrictMovementTokens > 0 || PlayerInteraction.Instance.toolCooldown || InputManager.isCharging) return;
        if (PlayerMovement.isCodexOpen || PlayerMovement.accessingInventory) return;

        // Turn off highlight on the current slot
        if (currentSlot != null)
        {
            currentSlot.ToggleHighlight();
        }

        PlaceableItem p_item = currentSlot.AssignedInventorySlot.ItemData as PlaceableItem;
        if (p_item) p_item.DisableHologram();

        ToolItem current_t_item = currentSlot.AssignedInventorySlot.ItemData as ToolItem;
        if(current_t_item) current_t_item.behavior.OnHolster();

        //if(currentIndex == slotIndex)
        //{
        //    return;
        //}

        // Set the new slot
        currentIndex = slotIndex;
        currentSlot = hotbarSlots[slotIndex];

        // Turn on highlight for the newly selected slot
        currentSlot.ToggleHighlight();
        tooltipControls.SelectedItem();

        // Optionally, use the item in the selected slot
        if (currentSlot.AssignedInventorySlot != null && currentSlot.AssignedInventorySlot.ItemData != null)
        {
            ToolItem t_item = currentSlot.AssignedInventorySlot.ItemData as ToolItem;
            if (t_item)
            {
                HandItemManager.Instance.SwapHandModel(t_item.tool, t_item.isUpgrade);
                t_item.behavior.OnEquip();
            }
            else
            {
                HandItemManager.Instance.SwapHandModel(ToolType.Null, false);
                HandItemManager.Instance.ShowSpriteInHand(currentSlot.AssignedInventorySlot.ItemData);
            }
        }
        else
        {
            //Debug.Log($"No item in hotbar slot {slotIndex + 1}");
            HandItemManager.Instance.ClearHandModel();
        }

        if (PlayerInventoryHolder.Instance.FindItemInBothInventories(torch)) HandItemManager.Instance.TorchFlameToggle(false);
        if (PlayerInventoryHolder.Instance.FindItemInBothInventories(pyrefly)) HandItemManager.Instance.PyreflyFlameToggle(false);
    }

    private void UpdateHandItem(InventorySystem inv)
    {
        if (currentSlot.AssignedInventorySlot != null && currentSlot.AssignedInventorySlot.ItemData != null)
        {
            currentSlot.AssignedInventorySlot.ItemData.UseItem(); //currently just reports what item is in the slot in the debugger

            ToolItem t_item = currentSlot.AssignedInventorySlot.ItemData as ToolItem;
            if (t_item)
            {
                HandItemManager.Instance.SwapHandModel(t_item.tool, t_item.isUpgrade);
            }
            else
            {
                //Debug.Log("Running this");
                HandItemManager.Instance.SwapHandModel(ToolType.Null, false);
                HandItemManager.Instance.ShowSpriteInHand(currentSlot.AssignedInventorySlot.ItemData);
            }
        }
        else
        {
            //Debug.Log($"No item in hotbar slot {slotIndex + 1}");
            HandItemManager.Instance.ClearHandModel();
        }
    }
    
    private void EnableDisableNavigation(bool val)
    {
        foreach (InventorySlot_UI slot in hotbarSlots)
        {
            slot.GetComponent<UnityEngine.UI.Button>().interactable = val;
        }
    }

}
