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
    [SerializeField] private bool forceEnableArcade = false;
    [SerializeField] private int saveFilesToCreate = 3;
    [SerializeField] private CanvasGroup mainCanvasGroup;
    public InputActionReference hideUI, UICancel;
    public GameObject menuObject, defaultObject, settingsDefault, settingsCanvas, controlsCanvas, controlsDefault, loadCanvas, loadSlotObject, loadDefault, difficultyOptions, difficultyDefault, resolutionBox, loadButtonContainer;
    public GameObject loadButtonPrefab;
    private SettingsValueManager settingsValueManager;
    ControlManager controlManager;
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
    public List<Button> loadButtons = new List<Button>();
    public List<Button> deleteButtons = new List<Button>();
    public Button[] nonNavigableButtons;
    public TextMeshProUGUI[] loadText;
    public ConfirmationBox confirmationBox;

    public List<GameObject> loadOptionsObjects = new List<GameObject>();
    GameObject selectedLoadSlot;

    public List<FileDatas> fileDatas = new List<FileDatas>();
    public static int currentSaveSlot = -1; //-1 means nothing is selecte
    public static FileMode currentFileMode;
    public bool isNewGame;
    public GameObject loadingScreen;
    //public string cozyDesc, normalDesc;
    public Button[] fileModeButtons;
    private int tempPathNum;
    private bool isScreenBlack;
    private FadeScreen fadeScreen;

    // Start is called before the first frame update
    void Awake()
    {
        loadingData = false;
        controlManager = FindFirstObjectByType<ControlManager>();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        webObject = FindFirstObjectByType<OpenWebsite>();
        settingsValueManager = settingsCanvas.GetComponent<SettingsValueManager>();
        fadeScreen = FindFirstObjectByType<FadeScreen>();
        //source.GetComponent<AudioSource>();
        controlManager.playerInput.SwitchCurrentActionMap("UI");
        int r = Random.Range(0, 3);

        foreach (Transform child in loadButtonContainer.transform)
        {
            Destroy(child.gameObject);
        }
        loadButtons.Clear();
        loadOptionsObjects.Clear();
        deleteButtons.Clear();
        // This script is a mess and I really really really don't feel like rewriting it
        for (int i = 0; i < saveFilesToCreate; i++)
        {
            var loadButton = Instantiate(loadButtonPrefab, loadButtonContainer.transform);
            var filedata = loadButton.GetComponent<FileDatas>();
            deleteButtons.Add(filedata.deleteSaveButton);
            filedata.fileNameText.text = "Save File " + (i + 1);
            fileDatas.Add(filedata);
        }

        for (int i = 0; i < saveFilesToCreate; i++)
        {
            loadButtons.Add(fileDatas[i].loadGameButton);
            loadOptionsObjects.Add(fileDatas[i].slotData);
        }

        for (int i = 0; i < saveFilesToCreate; i++)
        {
            loadButtons.Add(fileDatas[i].newGameButton);
            loadOptionsObjects.Add(fileDatas[i].slotButtons);
        }

        loadDefault = fileDatas[0].gameObject;
        
        ChangeMenu(r);
    }

    void Start()
    {
        LoadSaveFileInfo();
        UpdateNavigation();
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
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            if (!EventSystem.current.currentSelectedGameObject.gameObject.activeInHierarchy && ControlManager.isGamepad)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }


        if (EventSystem.current.currentSelectedGameObject == null && ControlManager.isGamepad)
        {
            if (confirmationBox.gameObject.activeSelf) EventSystem.current.SetSelectedGameObject(confirmationBox.noButton.gameObject);
            else if (controlsCanvas.activeSelf) EventSystem.current.SetSelectedGameObject(controlsDefault);
            else if (resolutionBox.activeSelf) EventSystem.current.SetSelectedGameObject(settingsValueManager.resolutionDefault);
            else if (settingsCanvas.activeSelf) EventSystem.current.SetSelectedGameObject(settingsValueManager.defaultMenuObject);
            else if (difficultyOptions.activeSelf) EventSystem.current.SetSelectedGameObject(difficultyDefault);
            else if (loadCanvas.activeSelf) EventSystem.current.SetSelectedGameObject(loadDefault);
            else if (menuObject.activeSelf) EventSystem.current.SetSelectedGameObject(defaultObject);
            else { EventSystem.current.SetSelectedGameObject(defaultObject); }
            print("Default Menu Object Selected");

            for (int i = 0; i < loadOptionsObjects.Count; i++)
            {
                if (i < loadOptionsObjects.Count / 2)
                {
                    loadOptionsObjects[i].SetActive(true);
                }
                else
                {
                    loadOptionsObjects[i].SetActive(false);
                }
            }
        }

        /*for(int i = 0; i < loadText.Length; i++)
            {
                if(isNewGame) loadText[i].text = "New Game";
                else loadText[i].text = "Load Game";
            }*/

        /*if(hideUI.action.WasPressedThisFrame())
        {
            if(!settingsCanvas.activeInHierarchy){HideUI();}
        }*/

        if (settingsCanvas.activeSelf && UICancel.action.WasPressedThisFrame())
        {
            settingsValueManager.Back();
        }
        else if (confirmationBox.gameObject.activeSelf && UICancel.action.WasPressedThisFrame())
        {
            confirmationBox.noButton.onClick.Invoke();
        }
        else if (loadCanvas.activeSelf && UICancel.action.WasPressedThisFrame())
        {
            LoadBack();
        }

        if (ControlManager.isGamepad)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            for (int i = 0; i < nonNavigableButtons.Length; i++)
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

        /*if (settingsCanvas.activeSelf || controlsCanvas.activeSelf || confirmationBox.gameObject.activeSelf || loadCanvas.activeSelf) webObject.canOpen = false;
        else webObject.canOpen = true;*/

        if (FadeScreen.coverScreen == true)
        {
            isScreenBlack = fadeScreen.imageColor.a >= 1.0f;
            //print("Is screen black? " + isScreenBlack + ", image alpha: " + fadeScreen.imageColor.a);
        }
    }
    void HideUI()
    {
        menuObject.SetActive(!menuObject.activeInHierarchy);
    }

    public void LoadBack()
    {
        bool optionsOpen = false;

        if (difficultyOptions.activeSelf)
        {
            difficultyOptions.SetActive(false);
            loadSlotObject.SetActive(true);
            if (ControlManager.isController) EventSystem.current.SetSelectedGameObject(loadDefault);
            return;
        }

        for (int i = 0; i < loadOptionsObjects.Count; i++)
        {
            if (i < loadOptionsObjects.Count / 2)
            {
                if (!loadOptionsObjects[i].activeSelf) optionsOpen = true;
                loadOptionsObjects[i].SetActive(true);
            }
            else
            {
                loadOptionsObjects[i].SetActive(false);
            }
        }
        //Debug.Log("We made it here");
        //Debug.Log(selectedLoadSlot);
        //Debug.Log(buttons[1].gameObject);
        if (optionsOpen)
        {
            if (ControlManager.isController) EventSystem.current.SetSelectedGameObject(selectedLoadSlot);
        }
        else
        {
            loadCanvas.SetActive(false);
            mainCanvasGroup.interactable = true;
            if (ControlManager.isController) EventSystem.current.SetSelectedGameObject(buttons[1].gameObject);
        }
    }

    public void TestPress()
    {
        print("Test");
    }
    public void OpenConfirmationBox(string message, Button b)
    {
        if (isTransitioning) return;
        if (ControlManager.isGamepad) EventSystem.current.SetSelectedGameObject(null);
        confirmationBox.messageText.text = message;
        confirmationBox.gameObject.SetActive(true);
        confirmationBox.calledBy = b;
        confirmationBox.yesButton.onClick.AddListener(YesPressed);
        confirmationBox.noButton.onClick.AddListener(NoPressed);
    }

    private void YesPressed()
    {
        if (confirmationBox.calledBy == buttons[0]) // New Game
        {
            if (isTransitioning) return;
            isTransitioning = true;
            loadingData = false;

            StartCoroutine(StartGame());
        }
        else if (confirmationBox.calledBy == buttons[4]) // Quit Game
        {
            if (isTransitioning) return;
            if(SteamManager.Instance) SteamManager.Instance.DisconnectFromSteam();
            Application.Quit();
            print("Game Exited Successfully :)");
        }

        for (int i = 0; i < loadButtons.Count; i++)
        {

            if (confirmationBox.calledBy == loadButtons[i]) // Load Game
            {
                int pathNum;
                if (i < loadButtons.Count / 2)
                {
                    pathNum = i;
                }
                else
                {
                    pathNum = i - (loadButtons.Count / 2);
                }

                print("pathNum = " + pathNum);

                string fullPath = Application.persistentDataPath + SaveLoad.SaveDirectory + pathNum + SaveLoad.FileName;
                //SaveData tempData = new SaveData();

                if (!File.Exists(fullPath) && !isNewGame)
                {
                    Debug.Log("No save data");
                    return;
                }
                if (isNewGame)
                {
                    if (isTransitioning) return;
                    loadSlotObject.SetActive(false);
                    UpdateNavigation();
                    difficultyOptions.SetActive(true);
                    currentSaveSlot = pathNum;
                    loadingData = false;
                    if (ControlManager.isController) EventSystem.current.SetSelectedGameObject(difficultyDefault);
                    break;

                    /*isTransitioning = true;
                    currentSaveSlot = pathNum;
                    StartCoroutine(StartGame());
                    loadingData = false;
                    loadCanvas.SetActive(false);
                    break;*/
                }

                if (isTransitioning) return;
                isTransitioning = true;
                loadingData = true;
                currentSaveSlot = pathNum;

                currentFileMode = fileDatas[pathNum].difficulty switch
                {
                    "Normal" => FileMode.Normal,
                    "Cozy" => FileMode.Cozy,
                    "Survival" => FileMode.Survival,
                    "Relaxed" => FileMode.Cozy, // In case someone had a save from before the difficulty name change
                    "Arcade" => FileMode.Survival, // In case someone had a save
                    _ => FileMode.Normal //I didnt know I could write a switch like this lol this is so much cleaner
                };

                StartCoroutine(StartGame());
                loadCanvas.SetActive(false);
                break;
            }
        }

        for (int i = 0; i < deleteButtons.Count; i++)
        {
            if (confirmationBox.calledBy == deleteButtons[i])
            {
                currentSaveSlot = i;
                SaveLoad.DeleteSaveData();
                LoadSaveFileInfo();

                for (int o = 0; o < loadOptionsObjects.Count; o++)
                {
                    if (o < loadOptionsObjects.Count / 2)
                    {
                        loadOptionsObjects[o].SetActive(true);
                    }
                    else
                    {
                        loadOptionsObjects[o].SetActive(false);
                    }
                }
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
        if (ControlManager.isGamepad) EventSystem.current.SetSelectedGameObject(confirmationBox.noButton.gameObject);
    }

    public void NewGame() // USELESS!!!!! DIE!!!
    {
        OpenConfirmationBox("Are you sure you want to start a new game?", buttons[0]);
        if (ControlManager.isGamepad) EventSystem.current.SetSelectedGameObject(confirmationBox.noButton.gameObject);
    }

    public void LoadGame(Button loadSlot)
    {
        if (isNewGame)
        {
            OpenConfirmationBox("Are you sure you want to start a new game in this slot?", loadSlot);
            if (ControlManager.isGamepad) EventSystem.current.SetSelectedGameObject(confirmationBox.noButton.gameObject);
            return;
        }
        if (loadSlot.interactable)
        {
            OpenConfirmationBox("Are you sure you want to load this save?", loadSlot);
            if (ControlManager.isGamepad) EventSystem.current.SetSelectedGameObject(confirmationBox.noButton.gameObject);
            return;
        }
    }

    public void DeleteSave(Button slot)
    {
        OpenConfirmationBox("Are you sure you want to delete this save? It cannot be recovered.", slot);
        if (ControlManager.isGamepad) EventSystem.current.SetSelectedGameObject(confirmationBox.noButton.gameObject);
        return;
    }

    IEnumerator StartGame()
    {
        FadeScreen.coverScreen = true;
        yield return new WaitUntil(() => isScreenBlack); // Waits until the bool is true!!! AWESOME!!!
        yield return new WaitForSecondsRealtime(1f);

        

        /*if (!loadingData) operation = SceneManager.LoadSceneAsync(3); //cutscene
        else operation = SceneManager.LoadSceneAsync(1); //game*/
        
        loadingScreen.SetActive(true);
        var loadText = loadingScreen.GetComponentInChildren<TextMeshProUGUI>();
        var loadAnims = FindFirstObjectByType<EnableRandomLoadingObject>();
        if (loadAnims != null) loadAnims.camera.enabled = true;

        yield return new WaitForSecondsRealtime(2f);
        
        AsyncOperation operation;
        operation = SceneManager.LoadSceneAsync(1); //game

        var load1 = "Loading";
        var load2 = "Loading.";
        var load3 = "Loading..";
        var load4 = "Loading...";
        print(currentFileMode.ToString());

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
        //SceneManager.LoadSceneAsync(1);
    }

    public void Credits()
    {
        if (isTransitioning) return;
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
        if (isTransitioning) return;
        settingsCanvas.SetActive(true);
        EventSystem.current.SetSelectedGameObject(settingsValueManager.defaultMenuObject);
    }

    public void OpenControlsScreen()
    {
        if (isTransitioning) return;
        controlsCanvas.SetActive(true);
        EventSystem.current.SetSelectedGameObject(controlsDefault);
        print("Controls Opened");
    }

    public void NewGameBool(bool n)
    {
        isNewGame = n;
    }
    public void SetSelectedLoadSlot(GameObject s)
    {
        selectedLoadSlot = s;
    }

    public void OpenLoadScreen()
    {
        LoadSaveFileInfo();
        for (int i = 0; i < loadButtons.Count / 2; i++)
        {
            if (fileDatas[i].saveDataPresent)
            {
                loadButtons[i].interactable = true;
                //print("SaveData Found");
            }
            else
            {
                loadButtons[i].interactable = false;
                //print("No SaveData Found");
            }
            //fileDatas[i].slotButton.interactable = true;
        }

        if (isTransitioning) return;
        loadCanvas.SetActive(true);
        mainCanvasGroup.interactable = false;
        if (ControlManager.isController) EventSystem.current.SetSelectedGameObject(loadDefault);
    }
    public void OpenResolutionScreen()
    {
        resolutionBox.SetActive(true);
        if (ControlManager.isController) EventSystem.current.SetSelectedGameObject(settingsValueManager.resolutionDefault);
    }

    public void OnHover()
    {
        if (isTransitioning) return;
        AudioPoolManager.Instance.PlayClip(hover, 0.1f);
    }

    public void OnSelect()
    {
        if (isTransitioning) return;
        AudioPoolManager.Instance.PlayClip(select, 0.1f);
    }

    void ChangeMenu(int num)
    {
        if (num == 0 || num == 1)
        {
            SetToMenu1();
            return;
        }
        if (num == 2)
        {
            SetToMenu2();
        }
    }

    void LoadSaveFileInfo()
    {
        var saveCount = 0;
        //for each filedata in fileDatas, load the info. if there is a save file, populate text, else say no file
        for (int i = 0; i < fileDatas.Count; i++)
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
                fileDatas[i].difficultyText.gameObject.SetActive(false);
                fileDatas[i].siegesClearedText.gameObject.SetActive(false);
                fileDatas[i].emptySlot.gameObject.SetActive(true);
                loadButtons[i].interactable = false;
                deleteButtons[i].interactable = false;
                //fileDatas[i].slotButton.interactable = false;
                continue;
            }
            else
            {
                Debug.Log("Save Data Found");
                string json = File.ReadAllText(fullPath);
                tempData = JsonUtility.FromJson<SaveData>(json);
                fileDatas[i].saveDataPresent = true;
                fileDatas[i].dayNum = tempData.allGameSaveData.pDayNumber;
                fileDatas[i].mintsCurrent = tempData.allGameSaveData.pCurrentMoney;
                fileDatas[i].mintsTotal = tempData.allGameSaveData.pTotalMoneyEarned;
                fileDatas[i].completedSieges = tempData.allGameSaveData.siegesCleared;
                if (tempData.allGameSaveData.gameMode != null) // Edge case scenario for saves made before difficulties were added :/
                {
                    if (tempData.allGameSaveData.gameMode == "Cozy")
                    {
                        fileDatas[i].difficulty = "Relaxed";
                    }
                    else if (tempData.allGameSaveData.gameMode == "Survival")
                    {
                        fileDatas[i].difficulty = "Arcade";
                    }
                    else fileDatas[i].difficulty = tempData.allGameSaveData.gameMode;

                }
                else
                {
                    fileDatas[i].difficulty = "Normal";
                }

                //populate the text variables
                fileDatas[i].dayNumText.text = "Day: " + fileDatas[i].dayNum;
                fileDatas[i].mintsCurrentText.text = "Current Mints: " + fileDatas[i].mintsCurrent;
                fileDatas[i].mintsTotalText.text = "Total Mints: " + fileDatas[i].mintsTotal;
                fileDatas[i].difficultyText.text = "Mode: " + fileDatas[i].difficulty.ToString();

                if (fileDatas[i].completedSieges > 0 && fileDatas[i].completedSieges < 5)
                {
                    for (int s = 0; s < fileDatas[i].completedSieges; s++)
                    {
                        fileDatas[i].siegeImages[s].color = Color.white;
                    }
                }
                else if (fileDatas[i].completedSieges >= fileDatas[i].siegeImages.Count)
                {
                    for (int s = 0; s < fileDatas[i].siegeImages.Count; s++)
                    {
                        fileDatas[i].siegeImages[s].color = Color.white;
                    }
                }
                
                //fileDatas[i].siegesClearedText.text = "Sieges Cleared: " + fileDatas[i].completedSieges;

                fileDatas[i].dayNumText.gameObject.SetActive(true);
                fileDatas[i].mintsCurrentText.gameObject.SetActive(true);
                fileDatas[i].mintsTotalText.gameObject.SetActive(true);
                fileDatas[i].difficultyText.gameObject.SetActive(true);
                fileDatas[i].siegesClearedText.gameObject.SetActive(true);
                fileDatas[i].emptySlot.gameObject.SetActive(false);
                saveCount++;
                //print(tempData.fileMode);
                //Enable/Disable uhh the thing idk I forgot
                loadButtons[i].interactable = true;
                deleteButtons[i].interactable = true;
                //fileDatas[i].slotButton.interactable = true;
            }
        }
    }

    public void SetFileMode(int mode) //Also starts the game
    {
        switch (mode)
        {
            case 0: // Normal
                currentFileMode = FileMode.Normal;
                break;
            case 1: // Cozy
                currentFileMode = FileMode.Cozy;
                break;
            case 2: // Survival
                currentFileMode = FileMode.Survival;
                break;
            default:
                Debug.LogError("Invalid file mode selected. Defaulting to Normal.");
                currentFileMode = FileMode.Normal;
                break;
        }
        isTransitioning = true;
        CloseCanvases();
        StartCoroutine(StartGame());
    }
    public void CloseCanvases()
    {
        loadCanvas.SetActive(false);
        settingsCanvas.SetActive(false);
        controlsCanvas.SetActive(false);
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

    private void UpdateNavigation() //Make this more modular later if needed
    {
        if(forceEnableArcade == true)
        {
            fileModeButtons[0].interactable = true; // Normal
            fileModeButtons[1].interactable = true; // Cozy
            fileModeButtons[2].interactable = true; // Survival
            return;
        }
        var f = PlayerPrefs.GetInt("FinaleCompleted", 0);
        if (f == 0)
        {
            fileModeButtons[0].interactable = true; // Normal
            fileModeButtons[1].interactable = true; // Cozy
            fileModeButtons[2].interactable = false; // Survival
        }
        else
        {
            fileModeButtons[0].interactable = true; // Normal
            fileModeButtons[1].interactable = true; // Cozy
            fileModeButtons[2].interactable = true; // Survival
        }
    }

    [ContextMenu("Force Finale Complete")]
    private void ForceFinaleComplete()
    {
        PlayerPrefs.SetInt("FinaleCompleted", 1);
        UpdateNavigation();
    }

    [ContextMenu("Force Finale Incomplete")]
    private void ForceFinaleIncomplete()
    {
        PlayerPrefs.SetInt("FinaleCompleted", 0);
        UpdateNavigation();
    }
}

[System.Serializable]


public enum FileMode
{
    Normal,
    Cozy,
    Survival
}
