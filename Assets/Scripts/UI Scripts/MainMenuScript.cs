using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;


public class MainMenuScript : MonoBehaviour
{
    public GameObject menuObject, defaultObject;
    ControlManager controlManager;
    // Start is called before the first frame update
    void Awake()
    {
        controlManager = FindFirstObjectByType<ControlManager>();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnEnable()
    {
        controlManager.moreInfo.action.started += HideUI;
    }

    private void OnDisable()
    {
        controlManager.moreInfo.action.started -= HideUI;
    }

    void HideUI(InputAction.CallbackContext obj)
    {
        menuObject.SetActive(!menuObject.activeInHierarchy);
    }

    // Update is called once per frame
    void Update()
    {
        //print(isPaused);
        //print(EventSystem.current.currentSelectedGameObject);
        if(EventSystem.current.currentSelectedGameObject == null && ControlManager.isGamepad)
        {
            print("Default Menu Object Selected");
            EventSystem.current.SetSelectedGameObject(defaultObject);
        } 
    }

    public void TestPress()
    {
        print("Test");
    }
    public void ExitGame()
    {
        Application.Quit();
        print("Game Exited Successfully :)");
    }
    
}
