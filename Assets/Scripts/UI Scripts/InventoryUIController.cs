using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InventoryUIController : MonoBehaviour
{
    public DynamicInventoryDisplay chestPanel;
    public DynamicInventoryDisplay playerBackpackPanel;

    PlayerInventoryHolder inventoryHolder;

    private bool isBackpackOpen = false;  
    public bool readyToPress;
    [SerializeField] private GameObject firstObject;
    ControlManager controlManager;
    EventSystem eventSystem;
    private void Awake()
    {
        readyToPress = true;
        chestPanel.gameObject.SetActive(false);
        playerBackpackPanel.gameObject.SetActive(false);

        inventoryHolder = FindObjectOfType<PlayerInventoryHolder>();
        
        controlManager = FindFirstObjectByType<ControlManager>();
    }

    void Start()
    {
        PlayerInventoryHolder.OnPlayerBackpackDisplayRequested?.Invoke(inventoryHolder.secondaryInventorySystem);
        CloseBackpack();
        readyToPress = true;
        eventSystem = EventSystem.current;
    }

    private void OnEnable()
    {
        InventoryHolder.OnDynamicInventoryDisplayRequested += DisplayInventory;
        PlayerInventoryHolder.OnPlayerBackpackDisplayRequested += DisplayPlayerBackpack;
        controlManager.openInventory.action.started += OpenInventory;
        controlManager.UIOpenInventory.action.started += UIOpenInventory;
        controlManager.closeInventory.action.started += CloseInput;
    }

    private void OnDisable()
    {
        InventoryHolder.OnDynamicInventoryDisplayRequested -= DisplayInventory;
        PlayerInventoryHolder.OnPlayerBackpackDisplayRequested -= DisplayPlayerBackpack;
        controlManager.openInventory.action.started -= OpenInventory;
        controlManager.UIOpenInventory.action.started -= UIOpenInventory;
        controlManager.closeInventory.action.started -= CloseInput;
    }

    void Update()
    {
        
    }

    private void OpenInventory(InputAction.CallbackContext obj)
    {
        print("Pressed");
        if(!PlayerMovement.accessingInventory)
        {
            eventSystem.SetSelectedGameObject(firstObject);
            PlayerInventoryHolder.OnPlayerBackpackDisplayRequested?.Invoke(inventoryHolder.secondaryInventorySystem);
            HotbarDisplay.currentSlot.slotHighlight.SetActive(false);
            controlManager.playerInput.SwitchCurrentActionMap("UI");
            return;
        }
        
        if (chestPanel.gameObject.activeInHierarchy)
        {
            eventSystem.currentSelectedGameObject.GetComponent<InventorySlot_UI>().slotHighlight.SetActive(false);
            eventSystem.SetSelectedGameObject(null);
            CloseInventory();
            HotbarDisplay.currentSlot.slotHighlight.SetActive(true);
            controlManager.playerInput.SwitchCurrentActionMap("Gameplay");
        }
        else if (isBackpackOpen)
        {
            eventSystem.currentSelectedGameObject.GetComponent<InventorySlot_UI>().slotHighlight.SetActive(false);
            eventSystem.SetSelectedGameObject(null);
            CloseBackpack();
            print("Closing backpack");
            HotbarDisplay.currentSlot.slotHighlight.SetActive(true);
            controlManager.playerInput.SwitchCurrentActionMap("Gameplay");
        }
    }

    private void CloseInput(InputAction.CallbackContext obj)
    {
        if (chestPanel.gameObject.activeInHierarchy)
        {
            eventSystem.currentSelectedGameObject.GetComponent<InventorySlot_UI>().slotHighlight.SetActive(false);
            eventSystem.SetSelectedGameObject(null);
            CloseInventory();
            HotbarDisplay.currentSlot.slotHighlight.SetActive(true);
            controlManager.playerInput.SwitchCurrentActionMap("Gameplay");
        }
        else if (isBackpackOpen)
        {
            eventSystem.currentSelectedGameObject.GetComponent<InventorySlot_UI>().slotHighlight.SetActive(false);
            eventSystem.SetSelectedGameObject(null);
            CloseBackpack();
            HotbarDisplay.currentSlot.slotHighlight.SetActive(true);
            controlManager.playerInput.SwitchCurrentActionMap("Gameplay");
            print("Closing backpack");
        }
    }

    private void UIOpenInventory(InputAction.CallbackContext obj)
    {
        if (chestPanel.gameObject.activeInHierarchy)
        {
            eventSystem.currentSelectedGameObject.GetComponent<InventorySlot_UI>().slotHighlight.SetActive(false);
            eventSystem.SetSelectedGameObject(null);
            CloseInventory();
            HotbarDisplay.currentSlot.slotHighlight.SetActive(true);
            controlManager.playerInput.SwitchCurrentActionMap("Gameplay");
        }
        else if (isBackpackOpen)
        {
            eventSystem.currentSelectedGameObject.GetComponent<InventorySlot_UI>().slotHighlight.SetActive(false);
            eventSystem.SetSelectedGameObject(null);
            CloseBackpack();
            HotbarDisplay.currentSlot.slotHighlight.SetActive(true);
            controlManager.playerInput.SwitchCurrentActionMap("Gameplay");
            print("Closing backpack");
        }
    }

    void DisplayInventory(InventorySystem invToDisplay)
    {
        //Chest Inventory
        eventSystem.SetSelectedGameObject(firstObject);
        PlayerMovement.accessingInventory = true;
        chestPanel.gameObject.SetActive(true);
        playerBackpackPanel.gameObject.SetActive(true);
        chestPanel.RefreshDynamicInventory(invToDisplay);
        controlManager.playerInput.SwitchCurrentActionMap("UI");
        isBackpackOpen = true;

    }

    void DisplayPlayerBackpack(InventorySystem invToDisplay)
    {
        if (!isBackpackOpen)
        {
            //print("Opening");
            PlayerMovement.accessingInventory = true;
            playerBackpackPanel.gameObject.SetActive(true);
            playerBackpackPanel.RefreshDynamicInventory(invToDisplay);
            controlManager.playerInput.SwitchCurrentActionMap("UI");
            isBackpackOpen = true; 
            readyToPress = false;
        }
    }

    void CloseInventory()
    {
        //Close Chest
        chestPanel.gameObject.SetActive(false);
        playerBackpackPanel.gameObject.SetActive(false);
        PlayerMovement.accessingInventory = false;
        controlManager.playerInput.SwitchCurrentActionMap("Gameplay");
        isBackpackOpen = false; 
    }

    void CloseBackpack()
    {
        //print("Closing");
        //HandItemManager.Instance.CheckSlotForTool();
        playerBackpackPanel.gameObject.SetActive(false);
        PlayerMovement.accessingInventory = false;
        controlManager.playerInput.SwitchCurrentActionMap("Gameplay");
        isBackpackOpen = false; 
    }
}
