using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIMenuButton : MonoBehaviour
{
    TextMeshProUGUI text;
    Color c_deselected, c_selected, c_disabled, c_interactable, c_noninteractable, c_invisible;
    bool isSelected;
    ControlManager controlManager;
    Button button;
    private Image arrowImage;
    public bool isPauseButton = true;
    public bool isDisabled = false;

    void Awake()
    {
        controlManager = FindFirstObjectByType<ControlManager>();
    }
    void Start()
    {
        isSelected = false;
        
        arrowImage = GetComponentInChildren<Image>();
        text = GetComponentInChildren<TextMeshProUGUI>();
        button = GetComponentInChildren<Button>();
        c_selected = new Color(1f, 0.8870801f, 0.2877358f, 1.0f);
        c_deselected = new Color(0.8509804f, 0.7490196f, 0.2078431f, 1.0f);
        c_disabled = new Color(0.5660378f, 0.5029674f, 0.1682093f, 1.0f);
        c_interactable = new Color(1f, 1f, 1f, 1);
        c_noninteractable = new Color(0.5f, 0.5f, 0.5f, 1);
        c_invisible = new Color(0f,0f,0f,0f);
    }

    void OnEnable()
    {
        controlManager.select.action.started += Select;
    }
    void OnDisable()
    {
        controlManager.select.action.started -= Select;
    }
    
    void Update()
    {
        if(button.interactable == false) isDisabled = true;
        else isDisabled = false;


        if(isPauseButton)
        {
            if(PauseScript.isPaused){button.enabled = true;}
            else{button.enabled = false;}
        }

        if(isDisabled)
        {
            text.color = c_disabled;
            if(EventSystem.current.currentSelectedGameObject == this.gameObject) arrowImage.color = c_noninteractable;
            else arrowImage.color = c_invisible;
        }
            
        if(!isDisabled)
        {
            if(EventSystem.current.currentSelectedGameObject == this.gameObject)
            {
                text.color = c_selected;
                arrowImage.color = c_interactable;
                isSelected = true;
            }
            else
            {
                text.color = c_deselected;
                arrowImage.color = c_invisible;
                isSelected = false;
            }
        }

    }

    public void PointerEnter()
    {
        if(!isDisabled) EventSystem.current.SetSelectedGameObject(this.gameObject);
        arrowImage.enabled = true;
    }

    public void PointerExit()
    {
        if(!isDisabled) EventSystem.current.SetSelectedGameObject(null);
        arrowImage.enabled = false;
    }

    void Select(InputAction.CallbackContext obj)
    {
        if(isSelected) button.onClick.Invoke();
    }

}
