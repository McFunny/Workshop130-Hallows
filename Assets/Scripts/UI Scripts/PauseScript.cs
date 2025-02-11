using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


public class PauseScript : MonoBehaviour
{
    public static bool isPaused;
    bool isTransitioning = false;
    public GameObject settingsCanvas, pauseObject, defaultObject, settingsDefault;
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
        controlManager.pauseGame.action.started += PausePressed;
        //controlManager.closeCodex.action.started += UnPause;
    }
    private void OnDisable()
    {
        controlManager.pauseGame.action.started -= PausePressed;
        //controlManager.closeCodex.action.started -= UnPause;
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
        //ResumeGame();
    }

    public void PauseGame()
    {
        if(isTransitioning || (!isPaused && PlayerMovement.restrictMovementTokens > 0)) return;
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
            settingsCanvas.SetActive(false);
            //controlManager.playerInput.SwitchCurrentActionMap("UI");
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            //controlManager.playerInput.SwitchCurrentActionMap("Gameplay");
            pauseObject.SetActive(false);
            settingsCanvas.SetActive(false);
            //controlManager.playerInput.SwitchCurrentActionMap("Gameplay");
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public void ResumeGame()
    {
        print("Resume Game Pressed");
        if(settingsCanvas.activeSelf)
        {
            settingsCanvas.SetActive(false);
            EventSystem.current.SetSelectedGameObject(defaultObject);
            return;
        }

        PauseGame();
        controlManager.playerInput.SwitchCurrentActionMap("Gameplay");
    }

    public void GoToMainMenu()
    {
        if(isTransitioning) return;
        isTransitioning = true;
        StartCoroutine(MainMenuTransition());
        //SceneManager.LoadSceneAsync(0);
    }

    IEnumerator MainMenuTransition()
    {
        pauseObject.SetActive(false);
        FadeScreen.coverScreen = true;
        yield return new WaitForSecondsRealtime(1);
        SceneManager.LoadSceneAsync(0);
    }

    public void OpenSettingsScreen()
    {
        print("Settings Pressed");
        settingsCanvas.SetActive(true);
        EventSystem.current.SetSelectedGameObject(settingsDefault);
    }
    
}
