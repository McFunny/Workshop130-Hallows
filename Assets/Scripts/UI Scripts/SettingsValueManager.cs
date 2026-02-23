using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.InputSystem;

public class SettingsValueManager : MonoBehaviour
{
    public ConfirmationBox confirmationBox;
    [SerializeField] GameObject containerObject, previousMenuObject;
    public GameObject defaultMenuObject;
    [SerializeField] private Button applyButton, defaultButton, backButton, resolutionButton;
    [SerializeField] private TextMeshProUGUI title, brightnessDisplay, sensitivityDisplay, masterVolDisplay, musicDisplay, sfxDisplay, resolutionDisplay;
    [SerializeField] private Slider brightnessSlider, sensitivitySlider, masterVolSlider, musicSlider, sfxSlider;
    [SerializeField] private Toggle sprint, detailedUI, emptyHand, dialogueAnimation;
    //[SerializeField] private TMP_Dropdown resolutionDropDown;
    [SerializeField] private GameObject horizontalMenuButton, resolutionBox, resolutionContent;
    public GameObject resolutionDefault;
    [SerializeField] private List<Button> previousMenuButtons;
    private Resolution[] resolutions;
    private List<Resolution> filteredResolutions;
    [SerializeField] private List<GameObject> resolutionButtons;
    [SerializeField] private List<SettingsPage> settingsPages;
    private float currentRefreshRate;
    private int currentResolutionIndex;
    private int tempResolutionIndex;
    private int sprintValue, detailedUIValue, emptyHandValue, dialogueAnimationValue;
    private float brightnessValue;
    private float defaultSensitivity, defaultVolume; // Default values
    private float sensitivity, masterVolume, musicVolume, sfxVolume; // Current Values
    private int currentPage;
    private VolumeManager volumeManager;
    private ApplySettings applySettings;
    public delegate void SettingsChanged();
    public static event SettingsChanged OnSettingsChanged;

    void Awake() // 0 is false, 1 is true
    {
        defaultSensitivity = 1.0f;
        defaultVolume = 1.0f;
        sensitivity = PlayerPrefs.GetFloat("Sensitivity", defaultSensitivity);
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", defaultVolume);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", defaultVolume);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", defaultVolume);
        brightnessValue = PlayerPrefs.GetFloat("Brightness", 0);
        sprintValue = PlayerPrefs.GetInt("ToggleSprint", 0);
        detailedUIValue = PlayerPrefs.GetInt("DetailedUI", 0);
        emptyHandValue = PlayerPrefs.GetInt("EmptyHand", 0);
        dialogueAnimationValue = PlayerPrefs.GetInt("DialogueAnimation", 1);
        volumeManager = FindFirstObjectByType<VolumeManager>();
        applySettings = FindFirstObjectByType<ApplySettings>();

        resolutions = Screen.resolutions;
        filteredResolutions = new List<Resolution>();

        currentRefreshRate = (float)Screen.currentResolution.refreshRateRatio.value;

        for (int i = 0; i < resolutions.Length; i++)
        {
            if ((float)resolutions[i].refreshRateRatio.value == currentRefreshRate)
            {
                filteredResolutions.Add(resolutions[i]);
            }
        }

        filteredResolutions.Reverse();

        for (int i = 0; i < filteredResolutions.Count; i++)
        {
            string resolutionOption = filteredResolutions[i].width + "x" + filteredResolutions[i].height;

            var buttonObj = Instantiate(horizontalMenuButton, resolutionContent.transform, false);
            var buttonID = buttonObj.GetComponent<ResolutionButtonID>();
            buttonID.ID = i;
            buttonID.text.text = resolutionOption;
            buttonID.settingsValueManager = this;

            buttonObj.name = resolutionOption;
            if (filteredResolutions[i].width == Screen.width && filteredResolutions[i].height == Screen.height)
            {
                currentResolutionIndex = i;
            }

            if (i == 0) resolutionDefault = buttonObj;

            resolutionButtons.Add(buttonObj);
        }
        tempResolutionIndex = currentResolutionIndex;

        for (int i = 0; i < resolutionButtons.Count; i++)
        {
            if (i == 0) continue;
            if (i == resolutionButtons.Count - 1) continue;

            Navigation Nav = new Navigation();
            Nav.mode = Navigation.Mode.Explicit;

            Nav.selectOnUp = resolutionButtons[i - 1].GetComponent<Button>();
            Nav.selectOnDown = resolutionButtons[i + 1].GetComponent<Button>();

            resolutionButtons[i].GetComponent<Button>().navigation = Nav;
        }

        Navigation TopNav = new Navigation();
        Navigation BottomNav = new Navigation();
        TopNav.mode = Navigation.Mode.Explicit;
        BottomNav.mode = Navigation.Mode.Explicit;

        TopNav.selectOnDown = resolutionButtons[1].GetComponent<Button>();
        BottomNav.selectOnUp = resolutionButtons[resolutionButtons.Count - 2].GetComponent<Button>();

        resolutionButtons[0].GetComponent<Button>().navigation = TopNav;
        resolutionButtons[resolutionButtons.Count - 1].GetComponent<Button>().navigation = BottomNav;

        for (int i = 0; i < settingsPages.Count; i++)
        {
            settingsPages[i].categoryButton.GetComponentInChildren<TextMeshProUGUI>().text = settingsPages[i].pageName;
        }

        for (int i = 0; i < settingsPages.Count; i++) //This actually works really well save ts
        {
            List<List<Selectable>> groupedSelectables = new List<List<Selectable>>(); //I did not know I could make a list within a list wow thanks chatgpt very cool very swag

            // 1. Gather all children per setting group
            for (int o = 0; o < settingsPages[i].settingsToDisplay.Count; o++)
            {
                GameObject groupObj = settingsPages[i].settingsToDisplay[o].gameObject;
                var childSelectables = GetAllSelectablesInChildren(groupObj);

                if (childSelectables.Count == 0)
                {
                    Debug.LogWarning($"No selectables found in {groupObj.name}", groupObj);
                    continue;
                }

                groupedSelectables.Add(childSelectables);
            }

            // 2. Horizontal navigation within each group
            foreach (var group in groupedSelectables)
            {
                for (int j = 0; j < group.Count; j++)
                {
                    Navigation nav = group[j].navigation;
                    nav.mode = Navigation.Mode.Explicit;

                    if (j > 0)
                        nav.selectOnLeft = group[j - 1];
                    if (j < group.Count - 1)
                        nav.selectOnRight = group[j + 1];

                    group[j].navigation = nav;
                }
            }

            for (int g = 0; g < groupedSelectables.Count; g++)
            {
                var currentGroup = groupedSelectables[g];

                for (int j = 0; j < currentGroup.Count; j++)
                {
                    Selectable current = currentGroup[j];
                    Navigation nav = current.navigation;
                    nav.mode = Navigation.Mode.Explicit;

                    // --- UP ---
                    if (g == 0)
                    {
                        //nav.selectOnUp = settingsPages[i].categoryButton;
                    }
                    else
                    {
                        var upGroup = groupedSelectables[g - 1];
                        nav.selectOnUp = upGroup[Mathf.Min(j, upGroup.Count - 1)];
                    }

                    // --- DOWN ---
                    if (g == groupedSelectables.Count - 1)
                    {
                        nav.selectOnDown = backButton;
                    }
                    else
                    {
                        var downGroup = groupedSelectables[g + 1];
                        nav.selectOnDown = downGroup[Mathf.Min(j, downGroup.Count - 1)];
                    }

                    current.navigation = nav;
                }
            }
        }

        currentPage = 0;
        ChangeSettingsPage(currentPage);
    }

    void OnEnable()
    {
        EnableDisablePreviousMenuButtons(false);
        ChangeSettingsPage(0);
        if(ControlManager.isController) EventSystem.current.SetSelectedGameObject(defaultMenuObject);
        //inputSystem.leftClick = null;
        sensitivitySlider.value = sensitivity;
        sensitivityDisplay.SetText($"{sensitivity.ToString("N2")}");

        masterVolSlider.value = masterVolume;
        masterVolDisplay.SetText($"{(masterVolSlider.value * 100).ToString("N1")}" + "%");

        musicSlider.value = musicVolume;
        musicDisplay.SetText($"{(musicSlider.value * 100).ToString("N1")}" + "%");

        sfxSlider.value = sfxVolume;
        sfxDisplay.SetText($"{(sfxSlider.value * 100).ToString("N1")}" + "%");

        brightnessSlider.value = brightnessValue;
        brightnessDisplay.SetText($"{(brightnessSlider.value * 100).ToString("N1")}" + "%");

        if (sprintValue == 0) sprint.isOn = false;
        else sprint.isOn = true;

        if (detailedUIValue == 0) detailedUI.isOn = false;
        else detailedUI.isOn = true;

        if (emptyHandValue == 0) emptyHand.isOn = false;
        else emptyHand.isOn = true;

        if (dialogueAnimationValue == 0) dialogueAnimation.isOn = false;
        else dialogueAnimation.isOn = true;

        resolutionDisplay.text = $"{filteredResolutions[currentResolutionIndex].width} x {filteredResolutions[currentResolutionIndex].height}";


        //print("Sensitivity Multiplier: " + sensitivity);
        applyButton.interactable = false;
    }

    void OnDisable()
    {
        EnableDisablePreviousMenuButtons(true);
        sensitivity = PlayerPrefs.GetFloat("Sensitivity", defaultSensitivity);
    }

    private void EnableDisablePreviousMenuButtons(bool b)
    {
        if (previousMenuButtons == null) return;
        for (int i = 0; i < previousMenuButtons.Count; i++)
        {
            previousMenuButtons[i].enabled = b;
        }
    }

    void Update()
    {
        if(Gamepad.current != null)
        {
            if (Gamepad.current.rightShoulder.wasPressedThisFrame)
            {
                ChangeSettingsPage(currentPage + 1 >= settingsPages.Count ? currentPage : currentPage + 1);
            }
            else if (Gamepad.current.leftShoulder.wasPressedThisFrame)
            {
                ChangeSettingsPage(currentPage - 1 < 0 ? currentPage : currentPage - 1);
            }
        }


        /*if(Input.GetKeyDown("0"))
        {
            containerObject.SetActive(!containerObject.activeSelf);
        }*/
    }

    public void SaveSettings()
    {
        OpenConfirmationBox("Are you sure you want to apply your current settings?", applyButton);
    }
    public void OpenConfirmationBox(string message, Button b)
    {
        confirmationBox.messageText.text = message;
        if (ControlManager.isGamepad) EventSystem.current.SetSelectedGameObject(confirmationBox.noButton.gameObject);
        confirmationBox.gameObject.SetActive(true);
        confirmationBox.calledBy = b;
        confirmationBox.yesButton.onClick.AddListener(YesPressed);
        confirmationBox.noButton.onClick.AddListener(NoPressed);
    }

    private void YesPressed()
    {
        if (confirmationBox.calledBy == applyButton)
        {
            PlayerPrefs.SetFloat("Sensitivity", sensitivity);
            PlayerPrefs.SetFloat("MasterVolume", masterVolume);
            PlayerPrefs.SetFloat("MusicVolume", musicVolume);
            PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
            PlayerPrefs.SetFloat("Brightness", brightnessValue);
            PlayerPrefs.SetInt("ToggleSprint", sprintValue);
            PlayerPrefs.SetInt("DetailedUI", detailedUIValue);
            PlayerPrefs.SetInt("EmptyHand", emptyHandValue);
            PlayerPrefs.SetInt("DialogueAnimation", dialogueAnimationValue);

            Resolution resolution = filteredResolutions[tempResolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, true);
            //print("Sensitivity Multiplier: " + PlayerPrefs.GetFloat("Sensitivity", defaultSensitivity));

            if (applyButton.interactable == true)
            {
                applyButton.interactable = false;
                EventSystem.current.SetSelectedGameObject(applyButton.gameObject);
            }

            PlayerPrefs.Save();
            if (OnSettingsChanged != null) OnSettingsChanged.Invoke();
            volumeManager.SettingsChanged();
            applySettings.UpdateSettings();
        }
        else if (confirmationBox.calledBy == backButton)
        {
            EventSystem.current.SetSelectedGameObject(previousMenuObject);
            containerObject.SetActive(false);
        }

        confirmationBox.gameObject.SetActive(false);
        confirmationBox.yesButton.onClick.RemoveListener(YesPressed);
        confirmationBox.yesButton.onClick.RemoveListener(NoPressed);
    }

    private void NoPressed()
    {
        confirmationBox.gameObject.SetActive(false);
        confirmationBox.yesButton.onClick.RemoveListener(YesPressed);
        confirmationBox.yesButton.onClick.RemoveListener(NoPressed);
        EventSystem.current.SetSelectedGameObject(confirmationBox.calledBy.gameObject);
    }

    public void defaultSettings()
    {
        sensitivity = defaultSensitivity;
        sensitivitySlider.value = sensitivity;
        sensitivityDisplay.SetText($"{sensitivity.ToString("N2")}");

        masterVolume = defaultVolume;
        masterVolSlider.value = masterVolume;
        masterVolDisplay.SetText($"{(masterVolSlider.value * 100).ToString("N1")}" + "%");

        musicVolume = defaultVolume;
        musicSlider.value = musicVolume;
        musicDisplay.SetText($"{(musicSlider.value * 100).ToString("N1")}" + "%");

        sfxVolume = defaultVolume;
        sfxSlider.value = sfxVolume;
        sfxDisplay.SetText($"{(sfxSlider.value * 100).ToString("N1")}" + "%");

        brightnessValue = 0;
        brightnessSlider.value = brightnessValue;
        brightnessDisplay.SetText($"{(brightnessSlider.value * 100).ToString("N1")}" + "%");

        sprintValue = 0;
        sprint.isOn = false;

        detailedUIValue = 0;
        detailedUI.isOn = false;

        emptyHandValue = 0;
        emptyHand.isOn = false;

        dialogueAnimationValue = 1;
        dialogueAnimation.isOn = true;
        //print("Sensitivity Multiplier: " + PlayerPrefs.GetFloat("Sensitivity", defaultSensitivity));

        applyButton.interactable = true;
    }

    public void UpdateSensitivity(float sens)
    {
        sensitivity = sens;
        sensitivityDisplay.SetText($"{sensitivity.ToString("N2")}");

        applyButton.interactable = true;
    }

    public void UpdateMasterVol(float vol)
    {
        masterVolume = vol;
        masterVolDisplay.SetText($"{(masterVolSlider.value * 100).ToString("N1")}" + "%");

        applyButton.interactable = true;
    }

    public void UpdateMusicVol(float vol)
    {
        musicVolume = vol;
        musicDisplay.SetText($"{(musicSlider.value * 100).ToString("N1")}" + "%");

        applyButton.interactable = true;
    }

    public void UpdateSFXVol(float vol)
    {
        sfxVolume = vol;
        sfxDisplay.SetText($"{(sfxSlider.value * 100).ToString("N1")}" + "%");

        applyButton.interactable = true;
    }

    public void UpdateSprintToggle(bool s)
    {
        if (s == false) sprintValue = 0;
        else sprintValue = 1;
        applyButton.interactable = true;
    }

    public void UpdateDetailedUIToggle(bool u)
    {
        if (u == false) detailedUIValue = 0;
        else detailedUIValue = 1;
        applyButton.interactable = true;
    }

    public void UpdateEmptyHandInteractToggle(bool h)
    {
        if (h == false) emptyHandValue = 0;
        else emptyHandValue = 1;
        applyButton.interactable = true;
    }

    public void UpdateDialogueAnimationToggle(bool d)
    {
        if (d == false) dialogueAnimationValue = 0;
        else dialogueAnimationValue = 1;
        applyButton.interactable = true;
    }

    public void UpdatebrightnessValue(float gam)
    {
        brightnessValue = gam;
        brightnessDisplay.SetText($"{(brightnessSlider.value * 100).ToString("N1")}" + "%");
        applyButton.interactable = true;
    }

    public void SetResolution(int resolutionIndex)
    {
        tempResolutionIndex = resolutionIndex;
        resolutionBox.SetActive(false);
        EventSystem.current.SetSelectedGameObject(resolutionButton.gameObject);
        applyButton.interactable = true;
    }

    public void Back()
    {
        if (resolutionBox.activeSelf)
        {
            resolutionBox.SetActive(false);
            EventSystem.current.SetSelectedGameObject(resolutionButton.gameObject);
            return;
        }

        if (applyButton.interactable == true && !confirmationBox.gameObject.activeSelf) OpenConfirmationBox("Changed settings will not be applied. Continue?", backButton);
        else if (confirmationBox.gameObject.activeSelf) confirmationBox.noButton.onClick.Invoke();
        else
        {
            EventSystem.current.SetSelectedGameObject(previousMenuObject);
            containerObject.SetActive(false);
        }
    }

    public void UpdateTempResolution(int r)
    {
        var tr = tempResolutionIndex + r;

        if (tr < 0 || tr > filteredResolutions.Count - 1) return;

        tempResolutionIndex += r;
        resolutionDisplay.text = $"{filteredResolutions[tempResolutionIndex].width} x {filteredResolutions[tempResolutionIndex].height}";

        applyButton.interactable = true;
    }
    
    List<Selectable> GetAllSelectablesInChildren(GameObject obj)
    {
        return obj.GetComponentsInChildren<Selectable>(true).ToList();
    }

    public void ChangeSettingsPage(int page)
    {
        currentPage = page;
        title.text = "Settings - " + settingsPages[currentPage].pageName;
        defaultMenuObject = settingsPages[currentPage].settingsToDisplay[0].GetComponentInChildren<Selectable>().gameObject;

        foreach (SettingsPage settingsPage in settingsPages)
        {
            if(currentPage == settingsPages.IndexOf(settingsPage))
            {
                settingsPage.lerpHandler.lerpToStart = true;
            }
            else
            {
                settingsPage.lerpHandler.lerpToStart = false;
            }
        }

        Navigation nav = new Navigation();
        nav.mode = Navigation.Mode.Explicit;
        //nav.selectOnDown = settingsPages[currentPage].settingsToDisplay[0].GetComponentInChildren<Selectable>();

        for (int i = 0; i < settingsPages.Count; i++)
        {
            for (int o = 0; o < settingsPages[i].settingsToDisplay.Count; o++)
            {
                if (i == currentPage)
                {
                    settingsPages[i].settingsToDisplay[o].SetActive(true);
                    settingsPages[i].settingsToDisplay[o].gameObject.transform.SetAsLastSibling();
                }
                else settingsPages[i].settingsToDisplay[o].SetActive(false);
            }

            //Navigation categoryNav = settingsPages[i].categoryButton.navigation;
            //categoryNav.selectOnDown = settingsPages[currentPage].firstOnList;

            //settingsPages[i].categoryButton.navigation = categoryNav;
        }

        

        nav.selectOnUp = settingsPages[currentPage].settingsToDisplay.Last().GetComponentInChildren<Selectable>();

        nav.selectOnLeft = null;
        nav.selectOnRight = defaultButton;
        backButton.navigation = nav;

        nav.selectOnLeft = backButton;
        nav.selectOnRight = applyButton;
        defaultButton.navigation = nav;

        nav.selectOnLeft = defaultButton;
        nav.selectOnRight = null;
        applyButton.navigation = nav;

    }
}

[System.Serializable]
public class SettingsPage
{
    public string pageName;
    public Button categoryButton;
    public UILerp lerpHandler;
    public List<GameObject> settingsToDisplay;
    
}
