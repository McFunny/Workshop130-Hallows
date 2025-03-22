using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class SettingsValueManager : MonoBehaviour
{
    public ConfirmationBox confirmationBox;
    [SerializeField] GameObject containerObject, previousMenuObject, defaultMenuObject;
    [SerializeField] private Button applyButton, defaultButton, backButton, resolutionButton;
    [SerializeField] private TextMeshProUGUI sensitivityDisplay, masterVolDisplay, musicDisplay, sfxDisplay;
    [SerializeField] private Slider sensitivitySlider, masterVolSlider, musicSlider, sfxSlider;
    //[SerializeField] private TMP_Dropdown resolutionDropDown;
    [SerializeField] private GameObject horizontalMenuButton, resolutionBox, resolutionContent;
    public GameObject resolutionDefault;
    private Resolution[] resolutions;
    private List<Resolution> filteredResolutions;

    [SerializeField] private List<GameObject> resolutionButtons;
    private float currentRefreshRate;
    private int currentResolutionIndex;
    private int tempResolutionIndex;
    private float defaultSensitivity, defaultVolume; // Default values
    private float sensitivity, masterVolume, musicVolume, sfxVolume; // Current Values
    private VolumeManager volumeManager;

    private InputSystemUIInputModule inputSystem;

    void Awake()
    {
        defaultSensitivity = 1.0f;
        defaultVolume = 1.0f;
        sensitivity = PlayerPrefs.GetFloat("Sensitivity", defaultSensitivity);
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", defaultVolume);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", defaultVolume);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", defaultVolume);
        volumeManager = FindFirstObjectByType<VolumeManager>();

        resolutions = Screen.resolutions;
        filteredResolutions = new List<Resolution>();

        currentRefreshRate = (float)Screen.currentResolution.refreshRateRatio.value;

        for(int i = 0; i < resolutions.Length; i++)
        {
            if((float)resolutions[i].refreshRateRatio.value == currentRefreshRate)
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
            if(filteredResolutions[i].width == Screen.width && filteredResolutions[i].height == Screen.height)
            {
                currentResolutionIndex = i;
            }

            if(i == 0) resolutionDefault = buttonObj;

            resolutionButtons.Add(buttonObj);
        }
        tempResolutionIndex = currentResolutionIndex;

        for(int i = 0; i < resolutionButtons.Count; i++)
        {
            if(i == 0) continue;
            if(i == resolutionButtons.Count - 1) continue;

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

    }

    void Start()
    {
        print("Test");
        inputSystem = FindObjectOfType<InputSystemUIInputModule>(); // try to change input module settings when the settings menu is opened
    }

    void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(defaultMenuObject);
        //inputSystem.leftClick = null;
        sensitivitySlider.value = sensitivity;
        sensitivityDisplay.SetText($"{sensitivity.ToString("N2")}");

        masterVolSlider.value = masterVolume;
        masterVolDisplay.SetText($"{(masterVolSlider.value * 100).ToString("N1")}" + "%");

        musicSlider.value = musicVolume;
        musicDisplay.SetText($"{(musicSlider.value * 100).ToString("N1")}" + "%");

        sfxSlider.value = sfxVolume;
        sfxDisplay.SetText($"{(sfxSlider.value * 100).ToString("N1")}" + "%");

        //print("Sensitivity Multiplier: " + sensitivity);
        applyButton.interactable = false;
    }

    void OnDisable()
    {
        sensitivity = PlayerPrefs.GetFloat("Sensitivity", defaultSensitivity);
    }

    void Update()
    {
        if(Input.GetKeyDown("1"))
        {
            print(PlayerPrefs.GetFloat("Sensitivity", sensitivity));
        }
        if(Input.GetKeyDown("2"))
        {
            print(PlayerPrefs.GetFloat("MusicVolume", musicVolume));
        }
        if(Input.GetKeyDown("3"))
        {
            print(PlayerPrefs.GetFloat("SFXVolume", sfxVolume));
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
        if(ControlManager.isGamepad) EventSystem.current.SetSelectedGameObject(confirmationBox.noButton.gameObject);
        confirmationBox.gameObject.SetActive(true);
        confirmationBox.calledBy = b;
        confirmationBox.yesButton.onClick.AddListener(YesPressed);
        confirmationBox.noButton.onClick.AddListener(NoPressed);
    }

    private void YesPressed()
    {
        if(confirmationBox.calledBy == applyButton) 
        {
            PlayerPrefs.SetFloat("Sensitivity", sensitivity);
            PlayerPrefs.SetFloat("MasterVolume", masterVolume);
            PlayerPrefs.SetFloat("MusicVolume", musicVolume);
            PlayerPrefs.SetFloat("SFXVolume", sfxVolume);

            Resolution resolution = filteredResolutions[tempResolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, true); 
            //print("Sensitivity Multiplier: " + PlayerPrefs.GetFloat("Sensitivity", defaultSensitivity));

            if(applyButton.interactable == true)
            {
                applyButton.interactable = false;
                EventSystem.current.SetSelectedGameObject(applyButton.gameObject);
            }

            PlayerPrefs.Save();
            volumeManager.SettingsChanged(); 
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

    public void SetResolution(int resolutionIndex)
    {
        tempResolutionIndex = resolutionIndex;
        resolutionBox.SetActive(false);
        EventSystem.current.SetSelectedGameObject(resolutionButton.gameObject);
        applyButton.interactable = true;
    } 

    public void Back()
    {
        if(resolutionBox.activeSelf)
        {
            resolutionBox.SetActive(false);
            EventSystem.current.SetSelectedGameObject(resolutionButton.gameObject);
            return;
        } 

        if(applyButton.interactable == true && !confirmationBox.gameObject.activeSelf) OpenConfirmationBox("Changed settings will not be applied. Continue?", backButton);
        else if(confirmationBox.gameObject.activeSelf) confirmationBox.noButton.onClick.Invoke();
        else
        {
            EventSystem.current.SetSelectedGameObject(previousMenuObject);
            containerObject.SetActive(false);
        }
        
    }
}
