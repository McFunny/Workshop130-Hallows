using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ControlsMenuScript : MonoBehaviour
{
    public InputActionReference scrollInput, UICancel;
    public Scrollbar scrollbar;
    public GameObject previousMenuObject, controlsContainer;
    public TextMeshProUGUI currentScheme;
    ControlManager controlManager;
    [SerializeField] TextMeshProUGUI[] controls;
    [SerializeField] String[] controllerText, kbmText;
    bool isControllerSelected = false;

    void Awake()
    {
        controlManager = FindFirstObjectByType<ControlManager>();
        scrollbar.value = 1.0f;
    }

    // Start is called before the first frame update
    void Start()
    {
        controls = controlsContainer.GetComponentsInChildren<TextMeshProUGUI>();
        for(int i = 0; i < controls.Length; i++)
        {
            if(ControlManager.isGamepad){controls[i].text = controllerText[i]; isControllerSelected = true;}
            else{controls[i].text = kbmText[i]; isControllerSelected = false;}
        }
        if(isControllerSelected){currentScheme.text = "Controller";}
        else{currentScheme.text = "Keyboard";}
    }

    void OnEnable()
    {
        //controlManager.playerInput.SwitchCurrentActionMap("UI");
        for(int i = 0; i < controls.Length; i++)
        {
            if(ControlManager.isGamepad){controls[i].text = controllerText[i]; isControllerSelected = true;}
            else{controls[i].text = kbmText[i]; isControllerSelected = false;}
        }
        if(isControllerSelected){currentScheme.text = "Controller";}
        else{currentScheme.text = "Keyboard";}
        scrollbar.value = 1.0f;
    }

    void OnDisable()
    {
        //controlManager.playerInput.SwitchCurrentActionMap("Gameplay");
    }

    // Update is called once per frame
    void Update()
    {

        if(scrollInput != null)
        {
            //print(scrollInput.action.ReadValue<float>());
            if(scrollInput.action.ReadValue<float>() > 0)
            {
                if(ControlManager.isGamepad){scrollbar.value += 0.005f;}
                else{scrollbar.value += 0.1f;}
            }
            else if(scrollInput.action.ReadValue<float>() < 0)
            {
                if(ControlManager.isGamepad){scrollbar.value -= 0.0051f;}
                else{scrollbar.value -= 0.1f;}
            }
        }
            

        if(UICancel.action.WasPressedThisFrame())
        {
            Back();
        }
    }

    public void SwapInputs()
    {
        isControllerSelected = !isControllerSelected;

        for(int i = 0; i < controls.Length; i++)
        {
            if(isControllerSelected){controls[i].text = controllerText[i];}
            else{controls[i].text = kbmText[i];}
        }

        if(isControllerSelected){currentScheme.text = "Controller";}
        else{currentScheme.text = "Keyboard";}
    }

    public void Back()
    {
        EventSystem.current.SetSelectedGameObject(previousMenuObject);
        //controlManager.playerInput.SwitchCurrentActionMap("UI");
        this.gameObject.SetActive(false);
    }
}
