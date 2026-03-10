using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControlManager : MonoBehaviour
{
    public static bool isController;
    public InputActionReference useHeldItem, interactWithItem, interactWithoutItem, 
    movement, sprint, look, moreInfo, pauseGame, backCodex, closeCodex, codexPageUp, codexPageDown, uiPause, uiPageTurn, hotbarScroll, hotbarUp, hotbarDown, showGrid, rotateStructure, openInventory, closeInventory,
    select, split, waterGunCharge, dropItem, holdInteraction, minigamePress, minigameExit, hotbarSwitch, codexOpen, deleteQuest, codexSelect, waterJet, secondaryCharge, uiScroll;
    public InputActionReference waterGunCharge_C, secondaryCharge_C; //Specifically handling these inputs for controller
    string currentDevice;
    public static bool isGamepad;
    public PlayerInput playerInput;
    private PauseScript pauseScript;

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        pauseScript = FindObjectOfType<PauseScript>();
    }

    private void OnEnable()
    {
        InputSystem.onDeviceChange += OnDeviceChange;
    }
    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace)) print(playerInput.currentActionMap);
        //print(playerInput.currentActionMap);
        currentDevice = GetComponent<PlayerInput>().currentControlScheme;
        //print(currentDevice);
        if(currentDevice == "Gamepad")
        {
            isGamepad = true;
            isController = true;
        }
        else
        {
            isGamepad = false;
            isController = false;
        }
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if(pauseScript == null) return;

        if (change == InputDeviceChange.Removed)
        {
            // Remove from list of devices.
            if(PauseScript.isPaused != true) pauseScript.PauseGame();
            //Debug.Log("Device removed: " + device);
        }
        else if (change == InputDeviceChange.Disconnected)
        {
            // Device got unplugged.
            if(PauseScript.isPaused != true) pauseScript.PauseGame();
            //Debug.Log("Device disconnected: " + device);
        }
    }
}
