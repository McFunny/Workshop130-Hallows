using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ArmorInventoryDisplay : InventoryDisplay
{
   
    [SerializeField] public InventorySlot_UI slotPrefab;

    protected override void Start()
    {
        base.Start();
    }

    private void OnEnable()
    {
        PlayerInventoryHolder.OnPlayerArmorDisplayRequested += RefreshDynamicInventory;

        // Bind immediately (covers quick-move paths that fire before this panel got an event)
        var holder = PlayerInventoryHolder.Instance;
        if (holder && holder.armorInventory != null)
            RefreshDynamicInventory(holder.armorInventory);
    }

    private void OnDisable()
    {
        PlayerInventoryHolder.OnPlayerArmorDisplayRequested -= RefreshDynamicInventory;
        if (inventorySystem != null) inventorySystem.OnInventorySlotChanged -= UpdateSlot;
    }

    public void RefreshDynamicInventory(InventorySystem invToDisplay)
    {
        //print(invToDisplay);
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

        //Debug.Log($"Displaying {inventorySystem} in UI: {gameObject.name}"); // Log to verify correct inventory is shown
    }

    public override void AssignSlot(InventorySystem invToDisplay)
    {
        print(invToDisplay);
        slotDictionary = new Dictionary<InventorySlot_UI, InventorySlot>();

        if (invToDisplay == null) return;

        for (int i = 0; i < invToDisplay.InventorySize; i++)
        {
            var uiSlot = Instantiate(slotPrefab, transform);
           
            slotDictionary.Add(uiSlot, invToDisplay.InventorySlots[i]);
            uiSlot.Init(invToDisplay.InventorySlots[i]);
            uiSlot.UpdateUISlot();
            uiSlot.SetArmorSlot();

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
