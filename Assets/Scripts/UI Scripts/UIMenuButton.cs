using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIMenuButton : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;
    Color c_deselected, c_selected, c_disabled, c_interactable, c_noninteractable, c_invisible;
    private bool isSelected;
    ControlManager controlManager;
    [SerializeField] Button button;
    [SerializeField] private Image arrowImage;
    [SerializeField] private KeepSelectionOnScreen keepSelectionOnScreen;
    [SerializeField] private ResolutionButtonID r;
    RectTransform rectTransform;
    public bool isPauseButton = true;
    public bool isDisabled = false;
    public bool ignoreColor = false;
    public bool isWithinScrollRect = false;
    public bool doNotUpdate = false;
    bool hasSnapped = false;

    void Awake()
    {
        controlManager = FindFirstObjectByType<ControlManager>();
    }
    void Start()
    {
        isSelected = false;
        
        if(arrowImage == null) arrowImage = GetComponentInChildren<Image>();
        if(text == null) text = GetComponentInChildren<TextMeshProUGUI>();
        if(button == null) button = GetComponentInChildren<Button>();
        c_selected = new Color(1f, 0.8870801f, 0.2877358f, 1.0f);
        c_deselected = new Color(0.8509804f, 0.7490196f, 0.2078431f, 1.0f);
        c_disabled = new Color(0.5660378f, 0.5029674f, 0.1682093f, 1.0f);
        c_interactable = new Color(1f, 1f, 1f, 1);
        c_noninteractable = new Color(0.5f, 0.5f, 0.5f, 1);
        c_invisible = new Color(0f,0f,0f,0f);

        rectTransform = this.gameObject.GetComponent<RectTransform>();
        if(isWithinScrollRect && keepSelectionOnScreen == null)
        {
            keepSelectionOnScreen = GetComponentInParent<KeepSelectionOnScreen>();
        }
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
        if (doNotUpdate)
        {
            if(EventSystem.current.currentSelectedGameObject == this.gameObject)
            {
                isSelected = true;
            }
            else
            {
                isSelected = false;
            }
            return;
        }    


        if (r != null)
        {
            if (EventSystem.current.currentSelectedGameObject == this.gameObject) isSelected = true;
            else isSelected = false;
            return;
        } 

        //print(controlManager.select.action.ReadValue<float>());
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
                isSelected = true;
                arrowImage.color = c_interactable;
                //arrowImage.enabled = true;
                if(ignoreColor) return;

                text.color = c_selected;
            }
            else
            {
                isSelected = false;
                arrowImage.color = c_invisible;
                //arrowImage.enabled = false;
                if(ignoreColor) return;
                
                text.color = c_deselected;
            }
        }

    }

    public void PointerEnter()
    {
        EventSystem.current.SetSelectedGameObject(this.gameObject);
        //arrowImage.enabled = true;
    }

    public void PointerExit()
    {
        //print("ButtonExited");
        EventSystem.current.SetSelectedGameObject(null);
        //arrowImage.enabled = false;
    }

    void Select(InputAction.CallbackContext obj)
    {
        if(isSelected && controlManager.select.action.ReadValue<float>() == 0) 
        {
            print("Onclick Attempted");
            if(button.interactable == false) return;
            button.onClick.Invoke();
            if(!ControlManager.isController) EventSystem.current.SetSelectedGameObject(null);
        }
        else if(isSelected && ControlManager.isController)
        {
            print("Onclick Attempted");
            if (button.interactable == false) return;
            button.onClick.Invoke();
            if(!ControlManager.isController) EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public bool GetSelected()
    {
        return isSelected;
    }
}
