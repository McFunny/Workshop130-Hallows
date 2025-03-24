using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ApplySettings : MonoBehaviour
{
    [SerializeField] Volume globalVolume;
    // Start is called before the first frame update
    void Awake()
    {
        globalVolume = FindFirstObjectByType<Volume>();
        if(globalVolume.profile.TryGet(out ColorAdjustments colorAdjustments))
        {
            colorAdjustments.postExposure.overrideState = true;
        }
    }
    void Start()
    {
        UpdateSettings();
    }

    public void UpdateSettings()
    {
        if(globalVolume.profile.TryGet(out ColorAdjustments colorAdjustments))
        {
            colorAdjustments.postExposure.value = PlayerPrefs.GetFloat("Brightness");
            print("Brightness Changed");
        }
    }
}
