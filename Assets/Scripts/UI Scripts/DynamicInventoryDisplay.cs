using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;

public class DynamicInventoryDisplay : InventoryDisplay
{
    [SerializeField] protected InventorySlot_UI slotPrefab;

    protected override void Start()
    {

        base.Start();


    }

    private void OnEnable()
    {
        PlayerInventoryHolder.OnPlayerInventoryChanged += RefreshDynamicInventory;
    }


    private void OnDisable()
    {
        PlayerInventoryHolder.OnPlayerInventoryChanged -= RefreshDynamicInventory;
        if (inventorySystem != null) inventorySystem.OnInventorySlotChanged -= UpdateSlot;
    }


    public void RefreshDynamicInventory(InventorySystem invToDisplay)
    {
        ClearSlots();
        inventorySystem = invToDisplay;
        if (inventorySystem != null) inventorySystem.OnInventorySlotChanged += UpdateSlot;
        AssignSlot(invToDisplay);
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
            Destroy(item.gameObject); //TODO: Look into object pooling 
        }

        if (slotDictionary != null) slotDictionary.Clear();
    }

}
