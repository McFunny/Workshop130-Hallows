using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public UnityAction<int> OnNumberPressed;
    public UnityAction<int> OnScrollInput;

    Tilemap structGrid, cabinGrid;
    public Color activeColor, activeNightColor, hiddenColor;
    Color cabinColor;
    public bool gridIsActive;
    ControlManager controlManager;
    PauseScript pauseScript;

    public static bool isCharging = false;
    public static bool isChargingSecondary = false;
    public static bool isHoldingInteract = false;

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
        if (cabinGrid) cabinColor = cabinGrid.color;
    }

    private void OnEnable()
    {
        controlManager.waterGunCharge.action.actionMap.Enable();
        controlManager.hotbarUp.action.started += HotbarUp;
        controlManager.hotbarDown.action.started += HotbarDown;
        controlManager.showGrid.action.canceled += ShowGrid;
        controlManager.pauseGame.action.started += PauseGame;
    }

    private void OnDisable()
    {
        controlManager.hotbarUp.action.started -= HotbarUp;
        controlManager.hotbarDown.action.started -= HotbarDown;
        controlManager.showGrid.action.canceled -= ShowGrid;
        controlManager.pauseGame.action.started -= PauseGame;

        // Always clear state on disable to prevent stuck inputs
        isCharging = false;
        isChargingSecondary = false;
        isHoldingInteract = false;
    }

    void Update()
    {
        CheckForScrollInput();
        CheckNumberInput();
        UpdateChargeState();
        UpdateSecondaryChargeState();
        UpdateHoldInteractState();

        if (gridIsActive && !NightSpawningManager.Instance.finaleWon)
        {
            structGrid.color = TimeManager.Instance.isDay ? activeColor : activeNightColor;
        }
        else
        {
            structGrid.color = hiddenColor;
        }
    }

    private void UpdateChargeState()
    {
        if (PauseScript.isPaused || PlayerMovement.restrictMovementTokens > 0)
        {
            isCharging = false;
            return;
        }

        // Read raw input values — ground truth, never gets stuck
        float primaryHeld   = controlManager.waterGunCharge.action.ReadValue<float>();
        float controllerHeld = controlManager.waterGunCharge_C.action.ReadValue<float>();
        bool buttonDown = primaryHeld > 0.1f || controllerHeld > 0.1f;

        // Secondary charge takes priority over primary
        if (isChargingSecondary)
        {
            isCharging = false;
            return;
        }

        if (!buttonDown)
        {
            isCharging = false;
            return;
        }

        InventoryItemData heldItem = HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData;
        bool validItem = heldItem != null && (
            heldItem == waterGun ||
            heldItem.ID == 0 ||
            heldItem.ID == 1 ||
            heldItem.ID == 2 ||
            heldItem.ID == 234 ||
            heldItem.ID == 270 ||
            heldItem.ID == 272 ||
            heldItem.ID == 274
        );

        isCharging = validItem;
    }

    private void UpdateSecondaryChargeState()
    {
        if (PauseScript.isPaused || PlayerMovement.restrictMovementTokens > 0)
        {
            isChargingSecondary = false;
            isCharging = false;
            return;
        }

        float primaryHeld    = controlManager.secondaryCharge.action.ReadValue<float>();
        float controllerHeld = controlManager.secondaryCharge_C.action.ReadValue<float>();
        bool buttonDown = primaryHeld > 0.1f || controllerHeld > 0.1f;

        if (!buttonDown)
        {
            isChargingSecondary = false;
            return;
        }

        // Primary charge takes priority over secondary
        if (isCharging)
        {
            isChargingSecondary = false;
            return;
        }

        InventoryItemData heldItem = HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData;
        bool validItem = heldItem != null && heldItem.ID == 228;

        isChargingSecondary = validItem;
        // Keep isCharging in sync so other systems don't need to check both flags
        isCharging = isChargingSecondary;
    }

    private void UpdateHoldInteractState()
    {
        if (PauseScript.isPaused || PlayerMovement.restrictMovementTokens > 0)
        {
            isHoldingInteract = false;
            return;
        }

        float held = controlManager.holdInteraction.action.ReadValue<float>();
        isHoldingInteract = held > 0.1f;
    }

    private void HotbarUp(InputAction.CallbackContext obj)
    {
        if (PauseScript.isPaused) return;
        if (PlayerMovement.isCodexOpen) return;
        if (!PlayerMovement.accessingInventory) OnScrollInput?.Invoke(-1);
    }

    private void HotbarDown(InputAction.CallbackContext obj)
    {
        if (PauseScript.isPaused) return;
        if (PlayerMovement.isCodexOpen) return;
        if (!PlayerMovement.accessingInventory) OnScrollInput?.Invoke(1);
    }

    private void ShowGrid(InputAction.CallbackContext obj)
    {
        if (PauseScript.isPaused) return;
        if (PlayerMovement.isCodexOpen) return;
        if (CraftingSystem.isCraftingMenuOpen) return;
        if (CookingRecipeBook.recipeBookOpen) return;
        if (!PlayerMovement.accessingInventory)
        {
            gridIsActive = !gridIsActive;
            if (cabinGrid)
            {
                cabinGrid.color = gridIsActive ? cabinColor : hiddenColor;
            }
        }
    }

    private void PauseGame(InputAction.CallbackContext obj)
    {
        if (PauseScript.isPaused) { pauseScript.ResumeGame(); return; }
        if (PlayerMovement.restrictMovementTokens > 0 || DialogueController.Instance.IsTalking()) return;
        if (!PlayerMovement.accessingInventory)
        {
            if (!repairMinigame.IsMinigameActive() && !PlayerMovement.isCodexOpen)
            {
                isCharging = false;
                isChargingSecondary = false;
                isHoldingInteract = false;
                pauseScript.PauseGame();
            }
        }
    }

    private void CheckForScrollInput()
    {
        if (PlayerMovement.restrictMovementTokens > 0 || PlayerMovement.accessingInventory) return;

        float scrollInput = controlManager.hotbarScroll.action.ReadValue<float>();

        if (scrollInput > 0f)
        {
            OnScrollInput?.Invoke(-1);
        }
        else if (scrollInput < 0f)
        {
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
                OnNumberPressed?.Invoke(i);
            }
        }
    }
}