using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DynamicInventoryDisplay : InventoryDisplay
{
    [SerializeField] protected InventorySlot_UI slotPrefab;

    protected override void Start()
    {
        base.Start();
    }

    private void OnEnable()
    {
        // Listen for the correct inventory change based on what this display is assigned to
        if (gameObject.name == "Player Hotbar") // Example name for the hotbar UI
        {
            PlayerInventoryHolder.OnPlayerHotbarDisplayRequested += RefreshDynamicInventory;
        }
        else if (gameObject.name == "PlayerBackPack") // Example name for the backpack UI
        {
            PlayerInventoryHolder.OnPlayerBackpackDisplayRequested += RefreshDynamicInventory;
        }
    }

    private void OnDisable()
    {
        if (gameObject.name == "Player Hotbar")
        {
            PlayerInventoryHolder.OnPlayerHotbarDisplayRequested -= RefreshDynamicInventory;
        }
        else if (gameObject.name == "PlayerBackPack")
        {
            PlayerInventoryHolder.OnPlayerBackpackDisplayRequested -= RefreshDynamicInventory;
        }

        if (inventorySystem != null) inventorySystem.OnInventorySlotChanged -= UpdateSlot;
    }

    public void RefreshDynamicInventory(InventorySystem invToDisplay)
    {
        ClearSlots();

        // Unsubscribe from the previous inventory system to prevent double updates
        if (inventorySystem != null)
        {
            inventorySystem.OnInventorySlotChanged -= UpdateSlot;
        }

        // Assign the correct inventory system to the display
        inventorySystem = invToDisplay;

        if (inventorySystem != null)
        {
            inventorySystem.OnInventorySlotChanged += UpdateSlot;
            AssignSlot(inventorySystem);
        }

        Debug.Log($"Displaying {inventorySystem} in UI: {gameObject.name}"); // Log to verify correct inventory is shown
    }

    public override void AssignSlot(InventorySystem invToDisplay)
    {
        slotDictionary = new Dictionary<InventorySlot_UI, InventorySlot>();

        if (invToDisplay == null) return;

        for (int i = 0; i < invToDisplay.InventorySize; i++)
        {
            var uiSlot = Instantiate(slotPrefab, transform);
            slotDictionary.Add(uiSlot, invToDisplay.InventorySlots[i]);
            uiSlot.Init(invToDisplay.InventorySlots[i]);
            uiSlot.UpdateUISlot();
        }
    }

    private void ClearSlots()
    {
        foreach (var item in transform.Cast<Transform>())
        {
            Destroy(item.gameObject); // TODO: Consider object pooling for performance
        }

        if (slotDictionary != null) slotDictionary.Clear();
    }
}
