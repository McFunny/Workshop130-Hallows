using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ApplySettings : MonoBehaviour
{
    [SerializeField] Volume globalVolume;
    private int isFinaleCompleted; // Used to check if the finale has been completed, 0 = not completed, 1 = completed
    [SerializeField] private bool forceFinaleIncomplete;
    // Start is called before the first frame update
    void Awake()
    {
        globalVolume = FindFirstObjectByType<Volume>();
        if (globalVolume.profile.TryGet(out ColorAdjustments colorAdjustments))
        {
            colorAdjustments.postExposure.overrideState = true;
        }
        
        if (forceFinaleIncomplete)
        {
            PlayerPrefs.SetInt("FinaleCompleted", 0);
        }

        isFinaleCompleted = PlayerPrefs.GetInt("FinaleCompleted", 0);

        print("Finale Completed: " + isFinaleCompleted);
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
