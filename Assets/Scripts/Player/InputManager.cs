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
    public Tilemap structGrid;
    public Color activeColor, hiddenColor;
    public bool gridIsActive;
    ControlManager controlManager;
    PauseScript pauseScript;

    public static bool isCharging = false;

    void Awake()
    {
        controlManager = FindFirstObjectByType<ControlManager>();
        pauseScript = FindFirstObjectByType<PauseScript>();
    }

    private void OnEnable()
    {
        controlManager.hotbarUp.action.started += HotbarUp;
        controlManager.hotbarDown.action.canceled += HotbarDown;  
        controlManager.showGrid.action.canceled += ShowGrid;
        controlManager.pauseGame.action.started += PauseGame;
        controlManager.waterGunCharge.action.performed += BeginCharge;
    }
    private void OnDisable()
    {
        controlManager.hotbarUp.action.started -= HotbarUp; 
        controlManager.hotbarDown.action.canceled -= HotbarDown;  
        controlManager.showGrid.action.canceled -= ShowGrid;
        controlManager.pauseGame.action.started -= PauseGame;
        controlManager.waterGunCharge.action.performed -= BeginCharge;
    }

    void Update()
    {
        CheckForScrollInput();
        CheckNumberInput();
        
        if (gridIsActive){ structGrid.color = activeColor;}
        else{ structGrid.color = hiddenColor;}

        //if(Input.GetKeyDown("t"))
        //{
        //    if(Time.timeScale == 1f) Time.timeScale = 4f;
        //    else Time.timeScale = 1f;
        //}
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
        if(!PlayerMovement.accessingInventory){gridIsActive = !gridIsActive;}
    }

    private void PauseGame(InputAction.CallbackContext obj)
    {
        if(PlayerMovement.isCodexOpen) return;
        if(PlayerMovement.restrictMovementTokens > 0 || DialogueController.Instance.IsTalking()) return;
        if(!PlayerMovement.accessingInventory) pauseScript.PauseGame();
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
        if(PlayerMovement.restrictMovementTokens > 0 || PauseScript.isPaused)
        {
            isCharging = false;
            return;
        }
        isCharging = !isCharging;
        //print(isCharging);
    }
}
