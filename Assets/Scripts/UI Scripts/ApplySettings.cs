using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ApplySettings : MonoBehaviour
{
    [SerializeField] Volume globalVolume;
    [SerializeField] private Material pixelRenderer;

    // Start is called before the first frame update if you didnt know it's pretty useful sometimes
    void Awake()
    {
        pixelRenderer.SetFloat("_pixelization", 1);
        globalVolume = GameObject.Find("Global Volume").GetComponent<Volume>();
        if (globalVolume.profile.TryGet(out ColorAdjustments colorAdjustments))
        {
            Debug.Log("Main Global Volume Found", globalVolume.gameObject);
            colorAdjustments.postExposure.overrideState = true;
            colorAdjustments.active = true;
        }
    }
    void Start()
    {
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
            pixelRenderer.SetFloat("_pixelization", 1080);   
        }
        else
        {
            pixelRenderer.SetFloat("_pixelization", Screen.currentResolution.width);
        }
    }
}
