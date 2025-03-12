using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.IO;
using SaveLoadSystem;
using UnityEngine.UI;

public class MainMenuScript : MonoBehaviour
{
    public InputActionReference hideUI, UICancel;
    public GameObject menuObject, defaultObject, settingsDefault, settingsCanvas, controlsCanvas, controlsDefault;
    private SettingsValueManager settingsValueManager;
    ControlManager controlManager;
    public AudioSource source;
    public AudioClip hover, select;
    bool isTransitioning = false;
    public static bool loadingData = false;
    private OpenWebsite webObject;

    public Transform sunMoonPivot;
    float dayRotation; 
    float nightRotation;

    public Material skyMat;

    public Transform menuPos1, menuPos2;

    public GameObject camera;

    public GameObject dayLight, nightLight;
    public Button[] buttons;
    public Button[] nonNavigableButtons;
    public ConfirmationBox confirmationBox;

    // Start is called before the first frame update
    void Awake()
    {
        controlManager = FindFirstObjectByType<ControlManager>();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        webObject = FindFirstObjectByType<OpenWebsite>();
        settingsValueManager = settingsCanvas.GetComponent<SettingsValueManager>();
        //source.GetComponent<AudioSource>();
        controlManager.playerInput.SwitchCurrentActionMap("UI");
        int r = Random.Range(0,3);

        ChangeMenu(r);
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
            if(confirmationBox.gameObject.activeSelf)EventSystem.current.SetSelectedGameObject(confirmationBox.noButton.gameObject);
            else if(controlsCanvas.activeSelf)EventSystem.current.SetSelectedGameObject(controlsDefault);
            else if(settingsCanvas.activeSelf)EventSystem.current.SetSelectedGameObject(settingsDefault);
            else{EventSystem.current.SetSelectedGameObject(defaultObject);}
            print("Default Menu Object Selected");
        } 

        /*if(hideUI.action.WasPressedThisFrame())
        {
            if(!settingsCanvas.activeInHierarchy){HideUI();}
        }*/

        if(settingsCanvas.activeInHierarchy && UICancel.action.WasPressedThisFrame())
        {
            settingsValueManager.Back();
        }
        else if(confirmationBox.gameObject.activeInHierarchy && UICancel.action.WasPressedThisFrame())
        {
            confirmationBox.noButton.onClick.Invoke();
        }

        if(ControlManager.isGamepad)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            for(int i = 0; i < nonNavigableButtons.Length; i++)
            {
                if (EventSystem.current.currentSelectedGameObject == nonNavigableButtons[i])
                {
                    EventSystem.current.SetSelectedGameObject(defaultObject);
                }
            }       
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if(settingsCanvas.activeSelf || controlsCanvas.activeSelf || confirmationBox.gameObject.activeSelf) webObject.canOpen = false;
        else webObject.canOpen = true;
    }
    void HideUI()
    {
        menuObject.SetActive(!menuObject.activeInHierarchy);
    }

    public void TestPress()
    {
        print("Test");
    }
    public void OpenConfirmationBox(string message, Button b)
    {
        if(isTransitioning) return;
        if(ControlManager.isGamepad) EventSystem.current.SetSelectedGameObject(null);
        confirmationBox.messageText.text = message;
        confirmationBox.gameObject.SetActive(true);
        confirmationBox.calledBy = b;
        confirmationBox.yesButton.onClick.AddListener(YesPressed);
        confirmationBox.noButton.onClick.AddListener(NoPressed);
    }

    private void YesPressed()
    {
        if(confirmationBox.calledBy == buttons[0]) // New Game
        {
            if(isTransitioning) return;
            isTransitioning = true;
            loadingData = false;
            //DeleteSaveData();
            StartCoroutine(StartGame());
        }
        else if(confirmationBox.calledBy == buttons[4]) // Quit Game
        {
            if(isTransitioning) return;
            Application.Quit();
            print("Game Exited Successfully :)");
        }

        confirmationBox.gameObject.SetActive(false);
        confirmationBox.yesButton.onClick.RemoveListener(YesPressed);
        confirmationBox.yesButton.onClick.RemoveListener(NoPressed);
        EventSystem.current.SetSelectedGameObject(confirmationBox.calledBy.gameObject);
    }

    private void NoPressed()
    {
        print("No Pressed");
        confirmationBox.gameObject.SetActive(false);
        confirmationBox.yesButton.onClick.RemoveListener(YesPressed);
        confirmationBox.yesButton.onClick.RemoveListener(NoPressed);
        EventSystem.current.SetSelectedGameObject(confirmationBox.calledBy.gameObject);
    }

    public void ExitGame()
    {
        OpenConfirmationBox("Are you sure you want to quit?", buttons[4]);
        if(ControlManager.isGamepad) EventSystem.current.SetSelectedGameObject(confirmationBox.noButton.gameObject);
    }

    public void NewGame()
    {
        OpenConfirmationBox("Are you sure you want to start a new game?", buttons[0]);
        if(ControlManager.isGamepad) EventSystem.current.SetSelectedGameObject(confirmationBox.noButton.gameObject);
    }

    public void LoadGame()
    {
        string fullPath = Application.persistentDataPath + SaveLoad.SaveDirectory + SaveLoad.FileName;
        //SaveData tempData = new SaveData();

        if (!File.Exists(fullPath))
        {
            Debug.Log("No save data");
            return;
        }


        if(isTransitioning) return;
        isTransitioning = true;
        loadingData = true;
        StartCoroutine(StartGame());
    }

    IEnumerator StartGame()
    {
        FadeScreen.coverScreen = true;
        yield return new WaitForSecondsRealtime(2);
        SceneManager.LoadSceneAsync(1);
    }

    public void OpenSettingsScreen()
    {
        if(isTransitioning) return;
        settingsCanvas.SetActive(true);
        EventSystem.current.SetSelectedGameObject(settingsDefault);
    }

    public void OpenControlsScreen()
    {
        if(isTransitioning) return;
        controlsCanvas.SetActive(true);
        EventSystem.current.SetSelectedGameObject(controlsDefault);
        print("Controls Opened");
    }

    public void OnHover()
    {
        if(isTransitioning) return;
        source.PlayOneShot(hover);
    }

    public void OnSelect()
    {
        if(isTransitioning) return;
        source.PlayOneShot(select);
    }

    void ChangeMenu(int num)
    {
        if(num == 0 || num == 1)
        {
            SetToMenu1();
            return;
        }
        if(num == 2)
        {
            SetToMenu2();
        }
    }

    [ContextMenu("Set To Menu 1")]
    public void SetToMenu1()
    {
        camera.transform.position = menuPos1.position;
        skyMat.SetFloat("_BlendCubemaps", 1f);
        dayLight.SetActive(true);
        nightLight.SetActive(false);
    }

    [ContextMenu("Set To Menu 2")]
    public void SetToMenu2()
    {
        camera.transform.position = menuPos2.position;
        skyMat.SetFloat("_BlendCubemaps", 0f);
        dayLight.SetActive(false);
        nightLight.SetActive(true);
    }
    
}
