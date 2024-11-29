using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;


public class PauseScript : MonoBehaviour
{
    public static bool isPaused;
    public GameObject pauseObject, defaultObject;
    ControlManager controlManager;
    // Start is called before the first frame update
    void Awake()
    {
        isPaused = false;
        controlManager = FindFirstObjectByType<ControlManager>();
    }

    private void OnEnable()
    {
        controlManager.uiPause.action.started += PausePressed;
    }
    private void OnDisable()
    {
        controlManager.uiPause.action.started -= PausePressed;
    }

    // Update is called once per frame
    void Update()
    {
        //print(isPaused);
        if(isPaused)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }

         if(EventSystem.current.currentSelectedGameObject == null && ControlManager.isGamepad && isPaused)
        {
            print("Default Pause Object Selected");
            EventSystem.current.SetSelectedGameObject(defaultObject);
        } 
    }

    private void PausePressed(InputAction.CallbackContext obj)
    {

        if(isPaused) PauseGame(); 
    }

    public void PauseGame()
    {
        isPaused = !isPaused;

        if(isPaused)
        {
            //controlManager.playerInput.SwitchCurrentActionMap("UI");
            pauseObject.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            //controlManager.playerInput.SwitchCurrentActionMap("Gameplay");
            pauseObject.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void ResumeGame()
    {
        PauseGame();
    }

    public void GoToMainMenu()
    {

    }
    
}
