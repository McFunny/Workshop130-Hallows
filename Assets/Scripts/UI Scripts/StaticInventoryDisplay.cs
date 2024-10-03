using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticInventoryDisplay : InventoryDisplay
{
    [SerializeField] private InventoryHolder inventoryHolder;
    [SerializeField] protected InventorySlot_UI[] slots;

    protected virtual void OnEnable()
    {
        PlayerInventoryHolder.OnPlayerInventoryChanged += RefreshStaticDisplay;
    }

    protected virtual void OnDisable()
    {
        PlayerInventoryHolder.OnPlayerInventoryChanged -= RefreshStaticDisplay;
    } //TODO: Incorperate hotbar

    protected override void Start()
    {
        base.Start();

        
    }

    public void RefreshStaticDisplay(InventorySystem invToDisplay)
    {
        if (inventoryHolder != null)
        {
            inventorySystem = inventoryHolder.PrimaryInventorySystem;
            inventorySystem.OnInventorySlotChanged += UpdateSlot;
        }
        else Debug.LogWarning($"No inventory assigned to {this.gameObject}");

        AssignSlot(inventorySystem);
    }
    public override void AssignSlot(InventorySystem invToDisplay)
    {
        // Clear the dictionary to avoid duplicate entries if the method is called again
        slotDictionary = new Dictionary<InventorySlot_UI, InventorySlot>();

        // Check if the length of slots and the inventory size are in sync
        if (slots.Length != invToDisplay.InventorySize)
        {
            Debug.LogError($"Inventory slots out of sync on {this.gameObject}. Slots length: {slots.Length}, Inventory size: {invToDisplay.InventorySize}");
            return; // Exit early if they're out of sync
        }

        // Loop through the inventory size and assign slots
        for (int i = 0; i < invToDisplay.InventorySize; i++)
        {
            if (!slotDictionary.ContainsKey(slots[i])) // Check for duplicate keys
            {
                slotDictionary.Add(slots[i], invToDisplay.InventorySlots[i]);
                slots[i].Init(invToDisplay.InventorySlots[i]);
            }
            else
            {
                Debug.LogError($"Duplicate slot found at index {i}: {slots[i]}");
            }
        }
    }

}
