using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class EnableRandomLoadingObject : MonoBehaviour
{
    public List<AdjustmentSettings> settings;
    public Camera camera;
    public Volume volume;
    public bool enableOnStart;
    public bool usePostProcessing;

    void Start()
    {
        if (!enableOnStart) return;
        if (settings == null) return;

        if (!usePostProcessing) volume.gameObject.SetActive(false);

        for (int i = 0; i < settings.Count; i++)
        {
            settings[i]._object.SetActive(false);
        }

        int r = Random.Range(0, settings.Count);

        if (volume.profile.TryGet(out ColorAdjustments colorAdjustments))
        {
            colorAdjustments.postExposure.value = settings[r].postExposure;
        }
        settings[r]._object.SetActive(true);
    }
}

[System.Serializable]
public class AdjustmentSettings
{
    public GameObject _object;
    public float postExposure;
}
