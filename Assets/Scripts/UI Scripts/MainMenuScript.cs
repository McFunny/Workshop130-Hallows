using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.IO;
using SaveLoadSystem;
using UnityEngine.UI;
using TMPro;

public class MainMenuScript : MonoBehaviour
{
    public InputActionReference hideUI, UICancel;
    public GameObject menuObject, defaultObject, settingsDefault, settingsCanvas, controlsCanvas, controlsDefault, loadCanvas, loadDefault, resolutionBox;
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
    public Button[] loadButtons;
    public Button[] nonNavigableButtons;
    public ConfirmationBox confirmationBox;

    public List<FileData> fileDatas = new List<FileData>();
    public static int currentSaveSlot = -1;//-1 means nothing is selected
    public bool isNewGame;
    
    public GameObject loadingScreen;

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

    void Start()
    {
        LoadSaveFileInfo();
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
            else if(resolutionBox.activeSelf)EventSystem.current.SetSelectedGameObject(settingsValueManager.resolutionDefault);
            else if(settingsCanvas.activeSelf)EventSystem.current.SetSelectedGameObject(settingsDefault);
            else if(loadCanvas.activeSelf)EventSystem.current.SetSelectedGameObject(loadDefault);
            else{EventSystem.current.SetSelectedGameObject(defaultObject);}
            print("Default Menu Object Selected");
        } 

        /*if(hideUI.action.WasPressedThisFrame())
        {
            if(!settingsCanvas.activeInHierarchy){HideUI();}
        }*/

        if(settingsCanvas.activeSelf && UICancel.action.WasPressedThisFrame())
        {
            settingsValueManager.Back();
        }
        else if(confirmationBox.gameObject.activeSelf && UICancel.action.WasPressedThisFrame())
        {
            confirmationBox.noButton.onClick.Invoke();
        }
        else if(loadCanvas.activeSelf && UICancel.action.WasPressedThisFrame())
        {
            loadCanvas.SetActive(false);
            EventSystem.current.SetSelectedGameObject(buttons[1].gameObject);
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

        if(settingsCanvas.activeSelf || controlsCanvas.activeSelf || confirmationBox.gameObject.activeSelf || loadCanvas.activeSelf) webObject.canOpen = false;
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
        
        for(int i = 0; i < loadButtons.Length; i++)
        {
            if(confirmationBox.calledBy == loadButtons[i]) // Load Game
            {
                string fullPath = Application.persistentDataPath + SaveLoad.SaveDirectory + i + SaveLoad.FileName;
                //SaveData tempData = new SaveData();

                if (!File.Exists(fullPath) && !isNewGame)
                {
                    Debug.Log("No save data");
                    return;
                }
                if(isNewGame)
                {
                    currentSaveSlot = i;
                    StartCoroutine(StartGame());
                    loadCanvas.SetActive(false);
                    break;
                }

                if(isTransitioning) return;
                isTransitioning = true;
                loadingData = true;
                currentSaveSlot = i;
                StartCoroutine(StartGame());
                loadCanvas.SetActive(false);
                break;
            }
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

    public void NewGame() // USELESS!!!!! DIE!!!
    {
        OpenConfirmationBox("Are you sure you want to start a new game?", buttons[0]);
        if(ControlManager.isGamepad) EventSystem.current.SetSelectedGameObject(confirmationBox.noButton.gameObject);
    }

    public void LoadGame(Button loadSlot)
    {
        if(isNewGame)
        {
            OpenConfirmationBox("Are you sure you want to start a new game in this slot?", loadSlot);
            if(ControlManager.isGamepad) EventSystem.current.SetSelectedGameObject(confirmationBox.noButton.gameObject);
            return;
        } 
        if(loadSlot.interactable)
        {
            OpenConfirmationBox("Are you sure you want to load this save?", loadSlot);
            if(ControlManager.isGamepad) EventSystem.current.SetSelectedGameObject(confirmationBox.noButton.gameObject);
            return;
        }   
    }

    IEnumerator StartGame()
    {
        FadeScreen.coverScreen = true;
        yield return new WaitForSecondsRealtime(2);

        AsyncOperation operation = SceneManager.LoadSceneAsync(1);
        loadingScreen.SetActive(true);
        var loadText = loadingScreen.GetComponentInChildren<TextMeshProUGUI>();

        var load1 = "Loading";
        var load2 = "Loading.";
        var load3 = "Loading..";
        var load4 = "Loading...";

        while(!operation.isDone)
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
        //SceneManager.LoadSceneAsync(1);
    }

    public void Credits()
    {
        if(isTransitioning) return;
        StartCoroutine(GoToCredits());
    }

    IEnumerator GoToCredits()
    {
        FadeScreen.coverScreen = true;
        yield return new WaitForSecondsRealtime(2);

        SceneManager.LoadSceneAsync(2);
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

    public void OpenLoadScreen(bool n)
    {
        if(n)
        {
            for(int i = 0; i < loadButtons.Length; i++)
            {
                loadButtons[i].interactable = true;
            }
        }
        else
        {
            LoadSaveFileInfo();
        }
        if(isTransitioning) return;
        isNewGame = n;
        loadCanvas.SetActive(true);
        EventSystem.current.SetSelectedGameObject(loadDefault);
    }
    public void OpenResolutionScreen()
    {
        resolutionBox.SetActive(true);
        if(ControlManager.isController) EventSystem.current.SetSelectedGameObject(settingsValueManager.resolutionDefault);
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

    void LoadSaveFileInfo()
    {
        var saveCount = 0;
        //for each filedata in fileDatas, load the info. if there is a save file, populate text, else say no file
        for(int i = 0; i < fileDatas.Count; i++)
        {
            string fullPath = Application.persistentDataPath + SaveLoad.SaveDirectory + i + SaveLoad.FileName;
            SaveData tempData = new SaveData();
                //SaveData tempData = new SaveData();

            if (!File.Exists(fullPath))
            {
                Debug.Log("No save data");
                //Have the text say no data
                fileDatas[i].saveDataPresent = false;
                fileDatas[i].dayNumText.gameObject.SetActive(false);
                fileDatas[i].mintsCurrentText.gameObject.SetActive(false);
                fileDatas[i].mintsTotalText.gameObject.SetActive(false);
                fileDatas[i].emptySlot.gameObject.SetActive(true);
                loadButtons[i].interactable = false;
                continue;
            }
            else
            {
                string json = File.ReadAllText(fullPath);
                tempData = JsonUtility.FromJson<SaveData>(json);
                fileDatas[i].saveDataPresent = true;
                fileDatas[i].dayNum = tempData.allGameSaveData.pDayNumber;
                fileDatas[i].mintsCurrent = tempData.allGameSaveData.pCurrentMoney;
                fileDatas[i].mintsTotal = tempData.allGameSaveData.pTotalMoneyEarned;
                //populate the text variables
                fileDatas[i].dayNumText.text = "Day: " + fileDatas[i].dayNum;
                fileDatas[i].mintsCurrentText.text ="Current Mints: " + fileDatas[i].mintsCurrent;
                fileDatas[i].mintsTotalText.text = "Total Mints: " + fileDatas[i].mintsTotal;

                fileDatas[i].dayNumText.gameObject.SetActive(true);
                fileDatas[i].mintsCurrentText.gameObject.SetActive(true);
                fileDatas[i].mintsTotalText.gameObject.SetActive(true);
                fileDatas[i].emptySlot.gameObject.SetActive(false);
                saveCount++;

                //Enable/Disable uhh the thing idk I forgot
                loadButtons[i].interactable = true;
            }
        }

        if(saveCount == 0)
        {
            buttons[1].interactable = false;
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

[System.Serializable]
public class FileData
{
    public TextMeshProUGUI dayNumText, mintsCurrentText, mintsTotalText, emptySlot;
    public bool saveDataPresent = false;

    public int dayNum;
    public int mintsCurrent;
    public int mintsTotal;
}
