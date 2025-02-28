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
    public ToolTipScript toolTip;
    MouseItemData mouseData;

    AudioSource source;
    public AudioClip openInventory;

    private void Awake()
    {
        readyToPress = true;
        chestPanel.gameObject.SetActive(false);
        playerBackpackPanel.gameObject.SetActive(false);

        inventoryHolder = FindObjectOfType<PlayerInventoryHolder>();
        
        controlManager = FindFirstObjectByType<ControlManager>();
        toolTip = FindFirstObjectByType<ToolTipScript>();
        mouseData = FindFirstObjectByType<MouseItemData>();
        source = GetComponent<AudioSource>();
    }

    void Start()
    {
        PlayerInventoryHolder.OnPlayerBackpackDisplayRequested?.Invoke(inventoryHolder.secondaryInventorySystem);
        StartCoroutine(CloseBackpack());
        readyToPress = true;
        eventSystem = EventSystem.current;
    }

    private void OnEnable()
    {
        InventoryHolder.OnDynamicInventoryDisplayRequested += DisplayInventory;
        PlayerInventoryHolder.OnPlayerBackpackDisplayRequested += DisplayPlayerBackpack;
        controlManager.openInventory.action.started += OpenInventory;
        controlManager.closeInventory.action.started += CloseInput;
    }

    private void OnDisable()
    {
        InventoryHolder.OnDynamicInventoryDisplayRequested -= DisplayInventory;
        PlayerInventoryHolder.OnPlayerBackpackDisplayRequested -= DisplayPlayerBackpack;
        controlManager.openInventory.action.started -= OpenInventory;
        controlManager.closeInventory.action.started -= CloseInput;
    }

    void Update()
    {
        //if(EventSystem.current.currentSelectedGameObject == null){toolTip.panel.SetActive(false);}
        //print(eventSystem.currentSelectedGameObject);
        if(!PlayerMovement.accessingInventory)
        {
            toolTip.panel.SetActive(false);
        }

        if (PlayerMovement.accessingInventory && ControlManager.isController && eventSystem.currentSelectedGameObject == null)
        {
            eventSystem.SetSelectedGameObject(HotbarDisplay.currentSlot.gameObject);
        }
    }

    private void OpenInventory(InputAction.CallbackContext obj)
    {
        //print("Pressed");
        if(mouseData && mouseData.IsHoldingItem()) return;
        if(PlayerMovement.isCodexOpen) return;

        if(DialogueController.Instance && DialogueController.Instance.IsTalking()) return;

        if(PlayerMovement.restrictMovementTokens > 0 || PlayerInteraction.Instance.toolCooldown || PauseScript.isPaused || PlayerMovement.isCodexOpen) return;

        if(!PlayerMovement.accessingInventory)
        {
            if(ControlManager.isGamepad) eventSystem.SetSelectedGameObject(HotbarDisplay.currentSlot.gameObject);
            PlayerInventoryHolder.OnPlayerBackpackDisplayRequested?.Invoke(inventoryHolder.secondaryInventorySystem);
            HotbarDisplay.currentSlot.slotHighlight.SetActive(false);
            source.PlayOneShot(openInventory);
            return;
        }
        
        if (chestPanel.gameObject.activeInHierarchy)
        {
            if(eventSystem.currentSelectedGameObject != null)
            {
                eventSystem.currentSelectedGameObject.GetComponent<InventorySlot_UI>().slotHighlight.SetActive(false);
            }
            eventSystem.SetSelectedGameObject(null);
            StartCoroutine(CloseInventory());
            HotbarDisplay.currentSlot.slotHighlight.SetActive(true);
        }
        else if (isBackpackOpen)
        {
            if(eventSystem.currentSelectedGameObject != null) eventSystem.currentSelectedGameObject.GetComponent<InventorySlot_UI>().slotHighlight.SetActive(false);
            eventSystem.SetSelectedGameObject(null);
            StartCoroutine(CloseBackpack());
            print("Closing backpack");
            HotbarDisplay.currentSlot.slotHighlight.SetActive(true);
        }
    }

    private void CloseInput(InputAction.CallbackContext obj)
    {
        
        if(mouseData && mouseData.IsHoldingItem()) return;
        print("Close Attempted");
        if(DialogueController.Instance && DialogueController.Instance.IsTalking()) return;

        if(PlayerMovement.restrictMovementTokens > 0 || PlayerInteraction.Instance.toolCooldown || PlayerMovement.isCodexOpen) return;
        
        if (chestPanel.gameObject.activeInHierarchy)
        {
            if(eventSystem.currentSelectedGameObject != null)
            {
                eventSystem.currentSelectedGameObject.GetComponent<InventorySlot_UI>().slotHighlight.SetActive(false);
            }
            eventSystem.SetSelectedGameObject(null);
            StartCoroutine(CloseInventory());
            HotbarDisplay.currentSlot.slotHighlight.SetActive(true);
        }
        else if (isBackpackOpen)
        {
            if(eventSystem.currentSelectedGameObject != null)
            {
                eventSystem.currentSelectedGameObject.GetComponent<InventorySlot_UI>().slotHighlight.SetActive(false);
            }
            eventSystem.SetSelectedGameObject(null);
            StartCoroutine(CloseBackpack());
            HotbarDisplay.currentSlot.slotHighlight.SetActive(true);
            print("Closing backpack");
        }
        EventSystem.current.SetSelectedGameObject(null);
    }

    void DisplayInventory(InventorySystem invToDisplay)
    {
        //Chest Inventory
        eventSystem.SetSelectedGameObject(HotbarDisplay.currentSlot.gameObject);
        PlayerMovement.accessingInventory = true;
        chestPanel.gameObject.SetActive(true);
        playerBackpackPanel.gameObject.SetActive(true);
        chestPanel.RefreshDynamicInventory(invToDisplay);
       
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
            isBackpackOpen = true; 
            readyToPress = false;
        }
    }

    IEnumerator CloseInventory()
    {
        //Close Chest
        yield return new WaitForEndOfFrame();
        chestPanel.gameObject.SetActive(false);
        playerBackpackPanel.gameObject.SetActive(false);
        PlayerMovement.accessingInventory = false;
        isBackpackOpen = false; 
    }

    IEnumerator CloseBackpack()
    {
        yield return new WaitForEndOfFrame();
        //print("Closing");
        //HandItemManager.Instance.CheckSlotForTool();
        playerBackpackPanel.gameObject.SetActive(false);
        PlayerMovement.accessingInventory = false;
        isBackpackOpen = false; 
    }
}
