using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResolutionButtonID : MonoBehaviour
{
    public SettingsValueManager settingsValueManager;
    public int ID;
    public TextMeshProUGUI text;

    void Start()
    {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(UpdateSettings);
    }

    void UpdateSettings()
    {
        settingsValueManager.SetResolution(ID);
    }

}
