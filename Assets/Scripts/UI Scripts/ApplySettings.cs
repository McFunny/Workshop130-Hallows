using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ApplySettings : MonoBehaviour
{
    [SerializeField] Volume globalVolume;
    [SerializeField] private Material pixelRenderer;
    public static int pixelResolution = 1080;

    // Start is called before the first frame update if you didnt know it's pretty useful sometimes
    void Awake()
    {
        pixelResolution = PlayerPrefs.GetInt("PixelFilter", 1) == 1 ? 1080 : Screen.currentResolution.width;
        globalVolume = GameObject.Find("Global Volume").GetComponent<Volume>();
        if (globalVolume.profile.TryGet(out ColorAdjustments colorAdjustments))
        {
            Debug.Log("Main Global Volume Found", globalVolume.gameObject);
            colorAdjustments.postExposure.overrideState = true;
            colorAdjustments.active = true;
        }
        UpdateSettings();
    }

    public void UpdateSettings()
    {
        if (globalVolume.profile.TryGet(out ColorAdjustments colorAdjustments))
        {
            colorAdjustments.postExposure.value = PlayerPrefs.GetFloat("Brightness");
            print("Brightness Changed");
        }
        else
        {
            Debug.LogWarning("No Global Volume Found");
        }

        if(PlayerPrefs.GetInt("PixelFilter", 1) == 1)
        {
            pixelResolution = 1080;
        }
        else
        {
            pixelResolution = Screen.currentResolution.width;
        }

        pixelRenderer.SetFloat("_pixelization", pixelResolution);
    }
}
