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
    public DynamicInventoryDisplay trinketPanel;
    public GameObject trinketVisuals;

    public static InventoryUIController Instance;

    PlayerInventoryHolder inventoryHolder;

    private bool isBackpackOpen = false;  
    public bool showTrinkets = false;
    public bool readyToPress;
    [SerializeField] private GameObject firstObject;
    ControlManager controlManager;
    EventSystem eventSystem;
    public ToolTipScript toolTip;
    MouseItemData mouseData;

    AudioSource source;
    public AudioClip openInventory;
    private TooltipControlsScript tooltipControlsScript;
    private RepairMinigame repairMinigame;
    public delegate void InventoryOpened(bool val);
    public static event InventoryOpened OnInventoryOpened;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else Instance = this;

        readyToPress = true;
        chestPanel.gameObject.SetActive(false);
        playerBackpackPanel.gameObject.SetActive(false);
        trinketPanel.gameObject.SetActive(false);
        trinketVisuals.SetActive(false);

        inventoryHolder = FindObjectOfType<PlayerInventoryHolder>();

        controlManager = FindFirstObjectByType<ControlManager>();
        toolTip = GameObject.Find("InventoryItemDescriptions").GetComponent<ToolTipScript>();
        mouseData = FindFirstObjectByType<MouseItemData>();
        source = GetComponent<AudioSource>();
        repairMinigame = FindFirstObjectByType<RepairMinigame>();
    }

    void Start()
    {
        PlayerInventoryHolder.OnPlayerBackpackDisplayRequested?.Invoke(inventoryHolder.secondaryInventorySystem);
        PlayerInventoryHolder.OnPlayerTrinketDisplayRequested?.Invoke(inventoryHolder.trinketInventorySystem);
        StartCoroutine(CloseBackpack());
        readyToPress = true;
        eventSystem = EventSystem.current;
        tooltipControlsScript = FindFirstObjectByType<TooltipControlsScript>();
    }

    private void OnEnable()
    {
        InventoryHolder.OnDynamicInventoryDisplayRequested += DisplayInventory;
        PlayerInventoryHolder.OnPlayerBackpackDisplayRequested += DisplayPlayerBackpack;
        PlayerInventoryHolder.OnPlayerTrinketDisplayRequested += DisplayPlayerBackpack;
        controlManager.openInventory.action.started += OpenInventory;
        controlManager.closeInventory.action.started += CloseInput;
    }

    private void OnDisable()
    {
        InventoryHolder.OnDynamicInventoryDisplayRequested -= DisplayInventory;
        PlayerInventoryHolder.OnPlayerBackpackDisplayRequested -= DisplayPlayerBackpack;
        PlayerInventoryHolder.OnPlayerTrinketDisplayRequested -= DisplayPlayerBackpack;
        controlManager.openInventory.action.started -= OpenInventory;
        controlManager.closeInventory.action.started -= CloseInput;
    }

    void Update()
    {
        //if(EventSystem.current.currentSelectedGameObject == null){toolTip.panel.SetActive(false);}
        //print(eventSystem.currentSelectedGameObject);
        if(showTrinkets == false) 
        {
            trinketPanel.gameObject.SetActive(false);
            trinketVisuals.SetActive(false);
        }

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
        if(repairMinigame.IsMinigameActive()) return;

        if (DialogueController.Instance && DialogueController.Instance.IsTalking()) return;

        if(PlayerMovement.restrictMovementTokens > 0 || PlayerInteraction.Instance.toolCooldown || PauseScript.isPaused || PlayerMovement.isCodexOpen) return;

        if(!PlayerMovement.accessingInventory)
        {
            if(ControlManager.isGamepad) eventSystem.SetSelectedGameObject(HotbarDisplay.currentSlot.gameObject);
            PlayerInventoryHolder.OnPlayerBackpackDisplayRequested?.Invoke(inventoryHolder.secondaryInventorySystem);
            PlayerInventoryHolder.OnPlayerTrinketDisplayRequested?.Invoke(inventoryHolder.trinketInventorySystem);
            HotbarDisplay.currentSlot.slotHighlight.SetActive(false);
            OnInventoryOpened?.Invoke(true);
            source.PlayOneShot(openInventory);
            tooltipControlsScript.ShowInventoryControls();
            return;
        }
        
        if (chestPanel.gameObject.activeInHierarchy)
        {
            if(eventSystem.currentSelectedGameObject != null)
            {
                eventSystem.currentSelectedGameObject.GetComponent<InventorySlot_UI>().slotHighlight.SetActive(false);
            }
            eventSystem.SetSelectedGameObject(null);
            OnInventoryOpened?.Invoke(false);
            StartCoroutine(CloseInventory());
            HotbarDisplay.currentSlot.slotHighlight.SetActive(true);
        }
        else if (isBackpackOpen)
        {
            if(eventSystem.currentSelectedGameObject != null) eventSystem.currentSelectedGameObject.GetComponent<InventorySlot_UI>().slotHighlight.SetActive(false);
            eventSystem.SetSelectedGameObject(null);
            OnInventoryOpened?.Invoke(false);
            StartCoroutine(CloseBackpack());
            print("Closing backpack");
            HotbarDisplay.currentSlot.slotHighlight.SetActive(true);
        }
    }

    private void CloseInput(InputAction.CallbackContext obj)
    {
        
        if(mouseData && mouseData.IsHoldingItem()) return;
        //print("Close Attempted");
        if(DialogueController.Instance && DialogueController.Instance.IsTalking()) return;

        if(PlayerMovement.restrictMovementTokens > 0 || PlayerInteraction.Instance.toolCooldown || PlayerMovement.isCodexOpen) return;
        
        if (chestPanel.gameObject.activeInHierarchy)
        {
            if(eventSystem.currentSelectedGameObject != null)
            {
                eventSystem.currentSelectedGameObject.GetComponent<InventorySlot_UI>().slotHighlight.SetActive(false);
            }
            eventSystem.SetSelectedGameObject(null);
            OnInventoryOpened?.Invoke(false);
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
            OnInventoryOpened?.Invoke(false);
            StartCoroutine(CloseBackpack());
            HotbarDisplay.currentSlot.slotHighlight.SetActive(true);
            print("Closing backpack");
        }
        EventSystem.current.SetSelectedGameObject(null);
    }

    void DisplayInventory(InventorySystem invToDisplay)
    {
        //Chest Inventory
        if(ControlManager.isController) 
        {
            eventSystem.SetSelectedGameObject(HotbarDisplay.currentSlot.gameObject);
            HotbarDisplay.currentSlot.slotHighlight.SetActive(true);
        }
        else
        {
            HotbarDisplay.currentSlot.slotHighlight.SetActive(false);
        }
        
        PlayerMovement.accessingInventory = true;
        chestPanel.gameObject.SetActive(true);
        playerBackpackPanel.gameObject.SetActive(true);
        trinketPanel.gameObject.SetActive(true);
        trinketVisuals.SetActive(true);
        trinketPanel.RefreshDynamicInventory(invToDisplay);
        chestPanel.RefreshDynamicInventory(invToDisplay);
        OnInventoryOpened?.Invoke(true);
        isBackpackOpen = true;

    }

    void DisplayPlayerBackpack(InventorySystem invToDisplay)
    {
        if (!isBackpackOpen)
        {
            //print("Opening");
            PlayerMovement.accessingInventory = true;
            playerBackpackPanel.gameObject.SetActive(true);
            trinketPanel.gameObject.SetActive(true);
            trinketVisuals.SetActive(true);
            trinketPanel.RefreshDynamicInventory(invToDisplay);
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
        trinketPanel.gameObject.SetActive(false);
        trinketVisuals.SetActive(false);
        PlayerMovement.accessingInventory = false;
        isBackpackOpen = false;
        tooltipControlsScript.ShowDefaultControls(); 
    }

    IEnumerator CloseBackpack()
    {
        yield return new WaitForEndOfFrame();
        //print("Closing");
        //HandItemManager.Instance.CheckSlotForTool();
        playerBackpackPanel.gameObject.SetActive(false);
        trinketPanel.gameObject.SetActive(false);
        trinketVisuals.SetActive(false);
        PlayerMovement.accessingInventory = false;
        isBackpackOpen = false; 
        tooltipControlsScript.ShowDefaultControls(); 
    }
}
