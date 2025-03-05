using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;


public class ControlsMenuScript : MonoBehaviour
{
   public GameObject previousMenuObject, kbmContainer, controllerContainer, defaultMenuObject;
   public GameObject[] kbmObjects, controllerObjects;
   public InputActionReference UICancel;

    void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(defaultMenuObject);
        for(int i = 0; i < kbmObjects.Length; i++)
        {
            kbmObjects[i].SetActive(false);
        }
        for(int i = 0; i < controllerObjects.Length; i++)
        {
            controllerObjects[i].SetActive(false);
        }
        
        kbmObjects[0].SetActive(true);
        controllerObjects[0].SetActive(true);
        
    }

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