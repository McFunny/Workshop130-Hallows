using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControlManager : MonoBehaviour
{
    public static bool isController;
    public InputActionReference useHeldItem, interactWithItem, interactWithoutItem, 
    movement, sprint, look, moreInfo, hotbarScroll, hotbarUp, hotbarDown, showGrid, openInventory, closeInventory,
    select, split;
    string currentDevice;
    public static bool isGamepad;

    // Update is called once per frame
    void Update()
    {
        currentDevice = GetComponent<PlayerInput>().currentControlScheme;
        //print(currentDevice);
        if(currentDevice == "Gamepad")
        {
            isGamepad = true;
            //isController = true;
        }
        else
        {
            isGamepad = false;
            //isController = false;
        }
    }
}
