using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VolumeLerper : MonoBehaviour
{
    [Header("Volume Settings")]
    [Tooltip("The Volume in your scene that contains the Color Adjustments override.")]
    public Volume targetVolume;

    private ColorAdjustments colorAdjustments;

    [Header("Hue Shift Settings")]
    public bool enableHueShift = true;
    [Range(-180f, 180f)] public float minHue = -30f;
    [Range(-180f, 180f)] public float maxHue = 30f;
    [Tooltip("Speed at which the hue oscillates between min and max.")]
    public float hueSpeed = 1f;

    [Header("Contrast Settings")]
    public bool enableContrast = false;
    [Range(-100f, 100f)] public float minContrast = -10f;
    [Range(-100f, 100f)] public float maxContrast = 10f;
    [Tooltip("Speed at which the contrast oscillates between min and max.")]
    public float contrastSpeed = 1f;

    private void Start()
    {
        if (targetVolume == null)
        {
            Debug.LogError("VolumeLerper: No Volume assigned! Please assign one in the Inspector.");
            enabled = false;
            return;
        }

        if (!targetVolume.profile.TryGet(out colorAdjustments))
        {
            Debug.LogError("VolumeLerper: No ColorAdjustments override found in Volume Profile!");
            enabled = false;
            return;
        }
    }

    private void Update()
    {
        if (colorAdjustments == null) return;

        // --- Hue Shift ---
        if (enableHueShift)
        {
            float hueT = Mathf.PingPong(Time.unscaledTime * hueSpeed, 1f);
            colorAdjustments.hueShift.value = Mathf.Lerp(minHue, maxHue, hueT);
        }

        // --- Contrast ---
        if (enableContrast)
        {
            float contrastT = Mathf.PingPong(Time.unscaledTime * contrastSpeed, 1f);
            colorAdjustments.contrast.value = Mathf.Lerp(minContrast, maxContrast, contrastT);
        }
    }
}
