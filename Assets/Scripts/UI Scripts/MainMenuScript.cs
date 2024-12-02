using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


public class MainMenuScript : MonoBehaviour
{
    public InputActionReference hideUI;
    public GameObject menuObject, defaultObject, controlsDefault, controlsCanvas;
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
        //hideUI.action.started += HideUI;
    }

    private void OnDisable()
    {
        //controlManager.moreInfo.action.started -= HideUI;
    }

    // Update is called once per frame
    void Update()
    {
        //print(isPaused);
        //print(EventSystem.current.currentSelectedGameObject);
        //print(controlManager.playerInput.currentActionMap);
        if(EventSystem.current.currentSelectedGameObject == null && ControlManager.isGamepad)
        {
            if(controlsDefault.activeInHierarchy)EventSystem.current.SetSelectedGameObject(controlsDefault);
            else{EventSystem.current.SetSelectedGameObject(defaultObject);}
            print("Default Menu Object Selected");
        } 

        if(hideUI.action.WasPressedThisFrame())
        {
            if(!controlsCanvas.activeInHierarchy){HideUI();}
        }
    }
    void HideUI()
    {
        menuObject.SetActive(!menuObject.activeInHierarchy);
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

    public void NewGame()
    {
        SceneManager.LoadSceneAsync(1);
    }

    public void OpenControlsScreen()
    {
        controlsCanvas.SetActive(true);
        EventSystem.current.SetSelectedGameObject(controlsDefault);
    }
    
}
