using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class PauseScript : MonoBehaviour
{
    public static bool isPaused;
    bool isTransitioning = false;
    public GameObject settingsCanvas, controlsObject, pauseObject, defaultObject, settingsDefault, controlsDefault, codexObject, codexDefault, resolutionBox;
    private SettingsValueManager settingsValueManager;
    public Button[] buttons;
    ControlManager controlManager;
    PlayerEffectsHandler pEffectsHandler;
    public OpenWebsite openWebsite;
    public ConfirmationBox confirmationBox;
    public GameObject loadingScreen;
    
    //private CodexRework codex;
    private Codex3 codex3;
    private FadeScreen fadeScreen;
    private bool isScreenBlack;
    // Start is called before the first frame update
    void Awake()
    {
        isPaused = false;
        controlManager = FindFirstObjectByType<ControlManager>();
        settingsValueManager = settingsCanvas.GetComponent<SettingsValueManager>();
        codex3 = FindFirstObjectByType<Codex3>();
        fadeScreen = FindFirstObjectByType<FadeScreen>();
    }

    private void OnEnable()
    {
        codex3.onCodexClosed += CodexClosed;
        //controlManager.closeCodex.action.started += UnPause;
    }
    private void OnDisable()
    {
        codex3.onCodexClosed -= CodexClosed;
        //controlManager.closeCodex.action.started -= UnPause;
    }

    // Update is called once per frame
    void Update()
    {
        if (pauseObject.activeSelf)
        {
            if (ControlManager.isGamepad)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if (settingsCanvas.activeSelf || controlsObject.activeSelf || confirmationBox.gameObject.activeSelf)
            {
                openWebsite.canOpen = false;
            }
            else openWebsite.canOpen = true;

            Time.timeScale = 0;
        }

        if (ControlManager.isController && isPaused && Gamepad.current.buttonEast.wasPressedThisFrame)
        {
            ResumeGame();
            //StartCoroutine(CodexCheck());
        }



        if (EventSystem.current.currentSelectedGameObject == null && ControlManager.isGamepad && isPaused && !PlayerMovement.isCodexOpen)
        {
            if (confirmationBox.gameObject.activeSelf) EventSystem.current.SetSelectedGameObject(confirmationBox.noButton.gameObject);
            else if (controlsObject.activeSelf) EventSystem.current.SetSelectedGameObject(controlsDefault);
            else if (resolutionBox.activeSelf) EventSystem.current.SetSelectedGameObject(settingsValueManager.resolutionDefault);
            else if (settingsCanvas.activeSelf) EventSystem.current.SetSelectedGameObject(settingsDefault);
            else { EventSystem.current.SetSelectedGameObject(defaultObject); }
            print("Default Menu Object Selected");
        } 
        
        if (FadeScreen.coverScreen == true)
        {
            isScreenBlack = fadeScreen.imageColor.a >= 1.0f;
            //print("Is screen black? " + isScreenBlack + ", image alpha: " + fadeScreen.imageColor.a);
        }
    }

    IEnumerator CodexCheck()
    {
        print("HELP!!!");
        if(!codexObject.activeSelf)
        {
            ResumeGame();
            yield return new WaitForSeconds(.5f);
            StopCoroutine(CodexCheck());
        }
        /*else
        {
            PlayerMovement.isCodexOpen = false;
            codex.OpenCloseCodex();
            EventSystem.current.SetSelectedGameObject(buttons[4].gameObject);
        }*/
    }

    public void PauseGame()
    {
        if(PlayerMovement.isCodexOpen) return;
        if (isTransitioning || (!isPaused && PlayerMovement.restrictMovementTokens > 0)) return;
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
            controlsObject.SetActive(false);
            //controlManager.playerInput.SwitchCurrentActionMap("UI");
        }
        else
        {
            //controlManager.playerInput.SwitchCurrentActionMap("Gameplay");
            pauseObject.SetActive(false);
            settingsCanvas.SetActive(false);
            controlsObject.SetActive(false);
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
        //if(codexObject.activeSelf) return;

        if (confirmationBox.gameObject.activeSelf)
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
            controlsObject.SetActive(false);
            return;
        }
        /*if(codexObject.activeSelf)
        {
            if(ControlManager.isController) EventSystem.current.SetSelectedGameObject(buttons[4].gameObject);
            return;
        }*/
        PauseGame();
        controlManager.playerInput.SwitchCurrentActionMap("Gameplay");
    }

    private void CodexClosed()
    {
        if(ControlManager.isController) EventSystem.current.SetSelectedGameObject(buttons[4].gameObject);
    }

    public void GoToMainMenu()
    {
        OpenConfirmationBox("Are you sure? All progress since last save will be lost.", buttons[3]);
    }

    IEnumerator MainMenuTransition()
    {
        pauseObject.SetActive(false);
        FadeScreen.coverScreen = true;
        yield return new WaitUntil(() => isScreenBlack); // Waits until the bool is true!!! AWESOME!!!
        yield return new WaitForSecondsRealtime(1f);

        AsyncOperation operation;

        operation = SceneManager.LoadSceneAsync(0); //Main Menu
        loadingScreen.SetActive(true);
        var loadText = loadingScreen.GetComponentInChildren<TextMeshProUGUI>();
        var loadAnims = FindFirstObjectByType<EnableRandomObject>();
        if (loadAnims != null) loadAnims.camera.enabled = true;

        var load1 = "Loading";
        var load2 = "Loading.";
        var load3 = "Loading..";
        var load4 = "Loading...";

        while (!operation.isDone)
        {
            loadText.text = load1;
            yield return new WaitForSecondsRealtime(.2f);
            loadText.text = load2;
            yield return new WaitForSecondsRealtime(.2f);
            loadText.text = load3;
            yield return new WaitForSecondsRealtime(.2f);
            loadText.text = load4;
            yield return new WaitForSecondsRealtime(.2f);
        }
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

    public void OpenResolutionScreen()
    {
        resolutionBox.SetActive(true);
        if(ControlManager.isController) EventSystem.current.SetSelectedGameObject(settingsValueManager.resolutionDefault);
    }

    public void OpenPauseCodex()
    {
        codex3.OpenCodex();
        //PlayerMovement.isCodexOpen = true;
        //if(ControlManager.isController) EventSystem.current.SetSelectedGameObject(codexDefault);
    }
    
}
