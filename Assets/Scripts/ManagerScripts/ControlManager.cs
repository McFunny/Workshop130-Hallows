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
    string currentDevice;
    public static bool isGamepad;
    public PlayerInput playerInput;

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
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
}
