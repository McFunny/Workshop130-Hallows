using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;
public class InputManager : MonoBehaviour
{
    // Unity Actions for number key and scroll input
    public UnityAction<int> OnNumberPressed;
    public UnityAction<int> OnScrollInput;  // New UnityAction for scroll input

    // FOR TOGGLING THE GRID
    Tilemap structGrid, cabinGrid;
    public Color activeColor, activeNightColor, hiddenColor;
    Color cabinColor;
    public bool gridIsActive;
    ControlManager controlManager;
    PauseScript pauseScript;

    public static bool isCharging = false;
    bool chargeButtonHeld = false;

    public static bool isHoldingInteract = false;
    bool interactButtonHeld = false;

    public InventoryItemData waterGun;
    private RepairMinigame repairMinigame;

    void Awake()
    {
        controlManager = FindFirstObjectByType<ControlManager>();
        pauseScript = FindFirstObjectByType<PauseScript>();
        repairMinigame = FindFirstObjectByType<RepairMinigame>();
    }

    void Start()
    {
        structGrid = StructureManager.Instance.farmTileMap;
        cabinGrid = StructureManager.Instance.cabinTileMap;
        if(cabinGrid) cabinColor = cabinGrid.color;
    }

    private void OnEnable()
    {
        controlManager.hotbarUp.action.started += HotbarUp;
        controlManager.hotbarDown.action.started += HotbarDown;  
        controlManager.showGrid.action.canceled += ShowGrid;
        controlManager.pauseGame.action.started += PauseGame;
        //controlManager.waterGunCharge.action.performed += BeginCharge;
        controlManager.waterGunCharge.action.started += BeginCharge;
        controlManager.waterGunCharge.action.canceled += BeginCharge; 
        controlManager.holdInteraction.action.started += BeginHoldInteraction;
        controlManager.holdInteraction.action.canceled += BeginHoldInteraction;
    }
    private void OnDisable()
    {
        controlManager.hotbarUp.action.started -= HotbarUp; 
        controlManager.hotbarDown.action.started -= HotbarDown;  
        controlManager.showGrid.action.canceled -= ShowGrid;
        controlManager.pauseGame.action.started -= PauseGame;
        //controlManager.waterGunCharge.action.performed -= BeginCharge;
        controlManager.waterGunCharge.action.started -= BeginCharge;
        controlManager.waterGunCharge.action.canceled -= BeginCharge;
        controlManager.holdInteraction.action.started -= BeginHoldInteraction;
        controlManager.holdInteraction.action.canceled -= BeginHoldInteraction;
    }

    void Update()
    {
        CheckForScrollInput();
        CheckNumberInput();
        
        if (gridIsActive)
        { 
            if(TimeManager.Instance.isDay) structGrid.color = activeColor;
            else structGrid.color = activeNightColor;
        }
        else{ structGrid.color = hiddenColor;}
    }
    private void HotbarUp(InputAction.CallbackContext obj)
    {
        if(PauseScript.isPaused) return;
        if(PlayerMovement.isCodexOpen) return;
        if(!PlayerMovement.accessingInventory){OnScrollInput?.Invoke(-1);}
    }
    private void HotbarDown(InputAction.CallbackContext obj)
    {
        if(PauseScript.isPaused) return;
        if(PlayerMovement.isCodexOpen) return;
        if(!PlayerMovement.accessingInventory){OnScrollInput?.Invoke(1);}
    }
    private void ShowGrid(InputAction.CallbackContext obj)
    {
        if(PauseScript.isPaused) return;
        if(PlayerMovement.isCodexOpen) return;
        if(!PlayerMovement.accessingInventory)
        {
            gridIsActive = !gridIsActive;
            if(cabinGrid)
            {
                if(gridIsActive) cabinGrid.color = cabinColor;
                else cabinGrid.color = hiddenColor;
            }
        }
    }

    private void PauseGame(InputAction.CallbackContext obj)
    {
        if(PauseScript.isPaused) { pauseScript.ResumeGame(); return; }

        if(PlayerMovement.isCodexOpen) return;
        if(PlayerMovement.restrictMovementTokens > 0 || DialogueController.Instance.IsTalking()) return;
        if(!PlayerMovement.accessingInventory)
        {
            if(!PauseScript.isPaused && !repairMinigame.IsMinigameActive())
            {
                isCharging = false;
                chargeButtonHeld = false;
                pauseScript.PauseGame();

                isHoldingInteract = false;
                interactButtonHeld = false;
            }          
        } 
    }

    private void CheckForScrollInput()
    {
        if(PlayerMovement.restrictMovementTokens > 0 || PlayerMovement.accessingInventory) return; //could cause issues with ui that use scrolling
        float scrollInput = controlManager.hotbarScroll.action.ReadValue<float>();

        if (scrollInput > 0f)
        {
            // Scroll up
            OnScrollInput?.Invoke(-1); 
        }

        if (scrollInput < 0f)
        {
            // Scroll down
            OnScrollInput?.Invoke(1); 
        }
    }

    void CheckNumberInput()
    {
        for (int i = 1; i <= 9; i++)
        {
            KeyCode key = (KeyCode)System.Enum.Parse(typeof(KeyCode), "Alpha" + i);
            if (Input.GetKeyDown(key))
            {
                // Invoke the action and pass the number that was pressed
                OnNumberPressed?.Invoke(i);
            }
        }
    }

    private void BeginCharge(InputAction.CallbackContext obj)
    {
        if(PauseScript.isPaused) return;

        chargeButtonHeld = !chargeButtonHeld;
        //print("Is button held? " + chargeButtonHeld);

        if(chargeButtonHeld == false || PlayerMovement.restrictMovementTokens > 0 || HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData != waterGun)
        {
            isCharging = false;
            //return;
        }
        else isCharging = !isCharging;
        //print("Is the gun charging? " + isCharging);
    }

    private void BeginHoldInteraction(InputAction.CallbackContext obj)
    {
        if(PauseScript.isPaused) return;

        interactButtonHeld = !interactButtonHeld;
        //print("Is button held? " + interactButtonHeld);

        if(interactButtonHeld == false || PlayerMovement.restrictMovementTokens > 0)
        {
            isHoldingInteract = false;
            //return;
        }
        else isHoldingInteract = !isHoldingInteract;
        //print("Is the gun charging? " + isCharging);
    }
}
