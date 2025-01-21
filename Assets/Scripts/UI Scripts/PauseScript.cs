using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


public class PauseScript : MonoBehaviour
{
    public static bool isPaused;
    bool isTransitioning = false;
    public GameObject controlsCanvas, pauseObject, defaultObject, controlsDefault;
    ControlManager controlManager;
    PlayerEffectsHandler pEffectsHandler;
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
        //print(controlManager.playerInput.currentActionMap);
        //print(isPaused);

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
        if(isTransitioning) return;
        isPaused = !isPaused;
        if(isPaused)
        {
            if(!pEffectsHandler) pEffectsHandler = PlayerInteraction.Instance.GetComponent<PlayerEffectsHandler>();
            pEffectsHandler.footStepSource.enabled = false;
            Time.timeScale = 0;
            PlayerMovement.restrictMovementTokens++;
            pEffectsHandler.StartCoroutine(pEffectsHandler.Focus());
        }
        else
        {
            Time.timeScale = 1;
            pEffectsHandler.footStepSource.enabled = true;
            PlayerMovement.restrictMovementTokens--;
        }
        
        if(isPaused)
        {
            //controlManager.playerInput.SwitchCurrentActionMap("UI");
            pauseObject.SetActive(true);
            controlsCanvas.SetActive(false);
            //controlManager.playerInput.SwitchCurrentActionMap("UI");
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            //controlManager.playerInput.SwitchCurrentActionMap("Gameplay");
            pauseObject.SetActive(false);
            controlsCanvas.SetActive(false);
            //controlManager.playerInput.SwitchCurrentActionMap("Gameplay");
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public void ResumeGame()
    {
        print("Resume Game Pressed");
        PauseGame();
        controlManager.playerInput.SwitchCurrentActionMap("Gameplay");
    }

    public void GoToMainMenu()
    {
        if(isTransitioning) return;
        isTransitioning = true;
        StartCoroutine(MainMenuTransition());
        //SceneManager.LoadSceneAsync("MainMenu");
    }

    IEnumerator MainMenuTransition()
    {
        pauseObject.SetActive(false);
        FadeScreen.coverScreen = true;
        yield return new WaitForSecondsRealtime(1);
        SceneManager.LoadSceneAsync("MainMenu");
    }

    public void OpenControlsScreen()
    {
        print("Controls Pressed");
        controlsCanvas.SetActive(true);
        EventSystem.current.SetSelectedGameObject(controlsDefault);
    }
    
}
