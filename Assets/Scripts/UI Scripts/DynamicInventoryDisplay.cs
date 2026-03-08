using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class DynamicInventoryDisplay : InventoryDisplay
{
    [SerializeField] protected InventorySlot_UI slotPrefab;

    private List<InventorySlot_UI> slotPool = new List<InventorySlot_UI>();

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
        else if (gameObject.name == "PlayerTrinkets")
        {
            PlayerInventoryHolder.OnPlayerTrinketDisplayRequested += RefreshDynamicInventory;
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
        else if (gameObject.name == "PlayerTrinkets")
        {
            PlayerInventoryHolder.OnPlayerTrinketDisplayRequested -= RefreshDynamicInventory;
        }

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

        if(gameObject.name == "PlayerTrinkets")
        {
            if(inventorySystem.InventorySize == 0)
            {
                GetComponent<Image>().enabled = false;
                gameObject.transform.parent.GetChild(1).gameObject.SetActive(false);
            }
            else 
            {
                GetComponent<Image>().enabled = true;
                gameObject.transform.parent.GetChild(1).gameObject.SetActive(true);
            }
        }

        Debug.Log($"Displaying {inventorySystem} in UI: {gameObject.name}"); // Log to verify correct inventory is shown
    }

    public override void AssignSlot(InventorySystem invToDisplay)
    {
        print(invToDisplay);
        slotDictionary = new Dictionary<InventorySlot_UI, InventorySlot>();

        if (invToDisplay == null) return;

        for (int i = 0; i < invToDisplay.InventorySize; i++)
        {
            var uiSlot = GetPooledSlot();
            slotDictionary.Add(uiSlot, invToDisplay.InventorySlots[i]);
            uiSlot.Init(invToDisplay.InventorySlots[i]);
            uiSlot.UpdateUISlot();
        }
    }

    private InventorySlot_UI GetPooledSlot()
    {
        // Look for an inactive slot in the pool
        for (int i = 0; i < slotPool.Count; i++)
        {
            if (!slotPool[i].gameObject.activeSelf)
            {
                slotPool[i].gameObject.SetActive(true);
                return slotPool[i];
            }
        }

        // No available slot in pool, instantiate a new one and add it
        var newSlot = Instantiate(slotPrefab, transform);
        slotPool.Add(newSlot);
        return newSlot;
    }

    private void ClearSlots()
    {
        // Deactivate all pooled slots instead of destroying them
        for (int i = 0; i < slotPool.Count; i++)
        {
            if (slotPool[i] != null && slotPool[i].gameObject.activeSelf)
            {
                slotPool[i].ResetSlotVisuals(); // Only reset visuals, don't clear inventory data
                slotPool[i].gameObject.SetActive(false);
            }
        }

        if (slotDictionary != null) slotDictionary.Clear();
    }
}
