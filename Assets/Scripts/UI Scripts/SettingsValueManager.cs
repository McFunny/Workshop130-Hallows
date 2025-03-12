using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class SettingsValueManager : MonoBehaviour
{
    public ConfirmationBox confirmationBox;
    [SerializeField] GameObject containerObject, previousMenuObject, defaultMenuObject;
    [SerializeField] private Button applyButton, defaultButton, backButton;
    [SerializeField] private TextMeshProUGUI sensitivityDisplay, musicDisplay, sfxDisplay;
    [SerializeField] private Slider sensitivitySlider, musicSlider, sfxSlider;
    private float defaultSensitivity, defaultVolume; // Default values
    private float sensitivity, musicVolume, sfxVolume; // Current Values

    private InputSystemUIInputModule inputSystem;

    void Awake()
    {
        defaultSensitivity = 1.0f;
        defaultVolume = 1.0f;
        sensitivity = PlayerPrefs.GetFloat("Sensitivity", defaultSensitivity);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", defaultVolume);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", defaultVolume);
    }

    void Start()
    {
        inputSystem = FindObjectOfType<InputSystemUIInputModule>(); // try to change input module settings when the settings menu is opened
    }

    void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(defaultMenuObject);
        //inputSystem.leftClick = null;
        sensitivitySlider.value = sensitivity;
        sensitivityDisplay.text = (Mathf.Round(sensitivity * 100) * 0.01f).ToString();

        musicSlider.value = musicVolume;
        musicDisplay.text = Mathf.Round(musicVolume * 100).ToString() + "%";

        sfxSlider.value = sfxVolume;
        sfxDisplay.text = Mathf.Round(sfxSlider.value * 100).ToString() + "%";

        print("Sensitivity Multiplier: " + sensitivity);
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
            PlayerPrefs.SetFloat("MusicVolume", musicVolume);
            PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
            print("Sensitivity Multiplier: " + PlayerPrefs.GetFloat("Sensitivity", defaultSensitivity));

            if(applyButton.interactable == true)
            {
                applyButton.interactable = false;
                EventSystem.current.SetSelectedGameObject(applyButton.gameObject);
            } 
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
        sensitivityDisplay.text = sensitivity.ToString();

        musicVolume = defaultVolume;
        musicSlider.value = musicVolume;
        musicDisplay.text = Mathf.Round(musicVolume * 100).ToString() + "%";

        sfxVolume = defaultVolume;
        sfxSlider.value = sfxVolume;
        sfxDisplay.text = Mathf.Round(sfxSlider.value * 100).ToString() + "%";
        //print("Sensitivity Multiplier: " + PlayerPrefs.GetFloat("Sensitivity", defaultSensitivity));

        applyButton.interactable = true;
    }

    public void UpdateSensitivity(float sens)
    {
        sensitivity = sens;
        sensitivityDisplay.text = (Mathf.Round(sens * 100) * 0.01f).ToString();

        applyButton.interactable = true;
    } 

    public void UpdateMusicVol(float vol)
    {
        musicVolume = vol;
        musicDisplay.text = Mathf.Round(vol * 100).ToString() + "%";

        applyButton.interactable = true;
    } 

    public void UpdateSFXVol(float vol)
    {
        sfxVolume = vol;
        sfxDisplay.text = Mathf.Round(vol * 100).ToString() + "%";

        applyButton.interactable = true;
    } 

    public void Back()
    {
        if(applyButton.interactable == true && !confirmationBox.gameObject.activeSelf) OpenConfirmationBox("Changed settings will not be applied. Continue?", backButton);
        else if(confirmationBox.gameObject.activeSelf) confirmationBox.noButton.onClick.Invoke();
        else
        {
            EventSystem.current.SetSelectedGameObject(previousMenuObject);
            containerObject.SetActive(false);
        }
        
    }
}
