using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;


public class ControlsMenuScript : MonoBehaviour
{
   public GameObject previousMenuObject, kbmContainer, controllerContainer;
   public InputActionReference UICancel;

    void Update()
    {
        if(ControlManager.isController)
        {
            controllerContainer.SetActive(true);
            kbmContainer.SetActive(false);
        }
        else
        {
            controllerContainer.SetActive(false);
            kbmContainer.SetActive(true);
        }

        if(UICancel.action.WasPressedThisFrame())
        {
            EventSystem.current.SetSelectedGameObject(previousMenuObject);
            this.gameObject.SetActive(false);
            print("Controls hidden");
        }
    }
}