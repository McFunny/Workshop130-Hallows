using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIToggleButton : MonoBehaviour
{
    private GameObject textObject;
    public TextMeshProUGUI text;
    Color c_deselected, c_selected, c_disabled;
    bool isSelected;
    ControlManager controlManager;
    Toggle toggle;
    public bool isPauseButton = true;
    public bool isDisabled = false;
    [SerializeField] private AudioClip selectSound, hoverSound;
    [SerializeField] private float selectVolume, hoverVolume;
    private bool canPlay = false;

    void Awake()
    {
        controlManager = FindFirstObjectByType<ControlManager>();
    }
    void Start()
    {
        isSelected = false;
        //textObject = transform.GetChild (4).gameObject;
        //text = textObject.GetComponent<TextMeshProUGUI>();
        toggle = GetComponent<Toggle>();
        c_selected = new Color(1f, 0.8870801f, 0.2877358f, 1.0f);
        c_deselected = new Color(0.8509804f, 0.7490196f, 0.2078431f, 1.0f);
        c_disabled = new Color(0.5660378f, 0.5029674f, 0.1682093f, 1.0f);
    }

    private void OnDisable()
    {
        canPlay = false;
    }

    void Update()
    {
        if(isPauseButton)
        {
            if(PauseScript.isPaused){toggle.enabled = true;}
            else{toggle.enabled = false;}
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

        if(isSelected && ControlManager.isController && Gamepad.current.buttonSouth.wasPressedThisFrame)
        {
            toggle.isOn = !toggle.isOn;
            //PlaySelectSound();
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

    public void PlaySelectSound()
    {
        if(canPlay == false) return;
        if(selectSound != null && AudioPoolManager.Instance != null) AudioPoolManager.Instance.PlayClip(selectSound, selectVolume);
    }

    public void PlayHoverSound()
    {
        if(hoverSound != null && AudioPoolManager.Instance != null) AudioPoolManager.Instance.PlayClip(hoverSound, hoverVolume);
    }

    public void EnablePlay() //When settings are initially loaded, this prevents sfx from playing when toggle values are set to match current settings.
    {
        canPlay = true;
    }

}

