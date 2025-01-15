using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIMenuSlider : MonoBehaviour
{
    private GameObject textObject;
    TextMeshProUGUI text;
    Color c_deselected, c_selected, c_disabled;
    bool isSelected;
    ControlManager controlManager;
    Slider slider;
    public bool isPauseButton = true;
    public bool isDisabled = false;

    void Awake()
    {
        controlManager = FindFirstObjectByType<ControlManager>();
    }
    void Start()
    {
        isSelected = false;
        textObject = transform.GetChild (4).gameObject;
        text = textObject.GetComponent<TextMeshProUGUI>();
        slider = GetComponent<Slider>();
        c_selected = new Color(1f, 0.8870801f, 0.2877358f, 1.0f);
        c_deselected = new Color(0.8509804f, 0.7490196f, 0.2078431f, 1.0f);
        c_disabled = new Color(0.5660378f, 0.5029674f, 0.1682093f, 1.0f);
    }
    
    void Update()
    {
        if(isPauseButton)
        {
            if(PauseScript.isPaused){slider.enabled = true;}
            else{slider.enabled = false;}
        }

        if(isDisabled)
        {
            text.color = c_disabled;
        }
            
        if(!isDisabled)
        {
            if(EventSystem.current.currentSelectedGameObject == this.gameObject)
            {
                text.color = c_selected;
                isSelected = true;
            }
            else
            {
                text.color = c_deselected;
                isSelected = false;
            }
        }

    }

    public void PointerEnter()
    {
        if(!isDisabled) EventSystem.current.SetSelectedGameObject(this.gameObject);
    }

    public void PointerExit()
    {
        if(!isDisabled) EventSystem.current.SetSelectedGameObject(null);
    }

}
