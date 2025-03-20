using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class PauseScript : MonoBehaviour
{
    public static bool isPaused;
    bool isTransitioning = false;
    public GameObject settingsCanvas, controlsObject, pauseObject, defaultObject, settingsDefault, controlsDefault, codexObject, codexDefault;
    private SettingsValueManager settingsValueManager;
    public Button[] buttons;
    ControlManager controlManager;
    PlayerEffectsHandler pEffectsHandler;
    public OpenWebsite openWebsite;
    public ConfirmationBox confirmationBox;
    
    private CodexRework codex;
    // Start is called before the first frame update
    void Awake()
    {
        isPaused = false;
        controlManager = FindFirstObjectByType<ControlManager>();
        settingsValueManager = settingsCanvas.GetComponent<SettingsValueManager>();
        codex = FindFirstObjectByType<CodexRework>();
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
        if(pauseObject.activeSelf)
        {
            if(ControlManager.isGamepad)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if(settingsCanvas.activeSelf || controlsObject.activeSelf || confirmationBox.gameObject.activeSelf)
            {
                openWebsite.canOpen = false;
            }
            else openWebsite.canOpen = true;
        }

        if (ControlManager.isController && isPaused && !PlayerMovement.isCodexOpen && Gamepad.current.buttonEast.wasPressedThisFrame)
        {
            ResumeGame();
        }
        

        if(EventSystem.current.currentSelectedGameObject == null && ControlManager.isGamepad && isPaused)
        {
            if(confirmationBox.gameObject.activeSelf)EventSystem.current.SetSelectedGameObject(confirmationBox.noButton.gameObject);
            else if(controlsObject.activeSelf)EventSystem.current.SetSelectedGameObject(controlsDefault);
            else if(settingsCanvas.activeSelf)EventSystem.current.SetSelectedGameObject(settingsDefault);
            else if(codexObject.activeSelf)EventSystem.current.SetSelectedGameObject(codexDefault);
            else{EventSystem.current.SetSelectedGameObject(defaultObject);}
            print("Default Menu Object Selected");
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
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(defaultObject);
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
        }
        else
        {
            //controlManager.playerInput.SwitchCurrentActionMap("Gameplay");
            pauseObject.SetActive(false);
            settingsCanvas.SetActive(false);
            //controlManager.playerInput.SwitchCurrentActionMap("Gameplay");
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public void OpenConfirmationBox(string message, Button b)
    {
        confirmationBox.messageText.text = message;
        if(ControlManager.isGamepad) EventSystem.current.SetSelectedGameObject(confirmationBox.noButton.gameObject);
        confirmationBox.gameObject.SetActive(true);
        confirmationBox.calledBy = b;
        confirmationBox.yesButton.onClick.AddListener(YesPressed);
        confirmationBox.noButton.onClick.AddListener(NoPressed);
    }

    private void YesPressed()
    {
        if(confirmationBox.calledBy == buttons[3]) //Main Menu
        {
            if(isTransitioning) return;
            isTransitioning = true;
            StartCoroutine(MainMenuTransition());
            //SceneManager.LoadSceneAsync(0);
        }

        confirmationBox.gameObject.SetActive(false);
        confirmationBox.yesButton.onClick.RemoveListener(YesPressed);
        confirmationBox.yesButton.onClick.RemoveListener(NoPressed);
        if(ControlManager.isController) EventSystem.current.SetSelectedGameObject(confirmationBox.calledBy.gameObject);
    }

    private void NoPressed()
    {
        confirmationBox.gameObject.SetActive(false);
        confirmationBox.yesButton.onClick.RemoveListener(YesPressed);
        confirmationBox.yesButton.onClick.RemoveListener(NoPressed);
        if(ControlManager.isController) EventSystem.current.SetSelectedGameObject(confirmationBox.calledBy.gameObject);
    }

    public void ResumeGame()
    {
        print("Resume Game Pressed");
        if(confirmationBox.gameObject.activeSelf)
        {
            NoPressed();
            return;
        }
        if(settingsCanvas.activeSelf)
        {
            settingsValueManager.Back();
            return;
        }
        if(controlsObject.activeSelf)
        {
            if(ControlManager.isController) EventSystem.current.SetSelectedGameObject(buttons[2].gameObject);
            return;
        }
        if(codexObject.activeSelf)
        {
            if(ControlManager.isController) EventSystem.current.SetSelectedGameObject(buttons[4].gameObject);
            PlayerMovement.isCodexOpen = false;
            return;
        }

        PauseGame();
        controlManager.playerInput.SwitchCurrentActionMap("Gameplay");
    }

    public void GoToMainMenu()
    {
        OpenConfirmationBox("Are you sure? All progress since last daybreak will be lost.", buttons[3]);
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
        if(ControlManager.isController) EventSystem.current.SetSelectedGameObject(settingsDefault);
    }

    public void OpenControlsScreen()
    {
        print("Controls Pressed");
        controlsObject.SetActive(true);
        if(ControlManager.isController) EventSystem.current.SetSelectedGameObject(controlsDefault);
    }

    public void OpenPauseCodex()
    {
        codex.OpenCloseCodex();
        PlayerMovement.isCodexOpen = true;
        if(ControlManager.isController) EventSystem.current.SetSelectedGameObject(codexDefault);
    }
    
}
