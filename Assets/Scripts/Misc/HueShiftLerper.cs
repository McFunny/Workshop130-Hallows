using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class HueShiftLerper : MonoBehaviour
{
    [Header("Volume Settings")]
    [Tooltip("The Volume in your scene that contains the Color Adjustments override.")]
    public Volume targetVolume;

    [Header("Hue Shift Settings")]
    [Tooltip("Minimum hue shift value (degrees).")]
    [Range(-180f, 180f)] public float minHue = -30f;
    [Tooltip("Maximum hue shift value (degrees).")]
    [Range(-180f, 180f)] public float maxHue = 30f;
    [Tooltip("How quickly the hue oscillates between min and max.")]
    public float speed = 1f;

    private ColorAdjustments colorAdjustments;
    private float t = 0f;
    private bool increasing = true;

    private void Start()
    {
        if (targetVolume == null)
        {
            Debug.LogError("HueShiftLerper: No Volume assigned! Please assign one in the Inspector.");
            enabled = false;
            return;
        }

        // Try to get the ColorAdjustments override from the Volume Profile
        if (targetVolume.profile.TryGet(out colorAdjustments) == false)
        {
            Debug.LogError("HueShiftLerper: No ColorAdjustments override found in Volume Profile!");
            enabled = false;
            return;
        }
    }

    private void Update()
    {
        if (colorAdjustments == null) return;

        // Lerp value between min and max
        float lerpValue = Mathf.PingPong(Time.unscaledTime * speed, 1f);
        colorAdjustments.hueShift.value = Mathf.Lerp(minHue, maxHue, lerpValue);


    }
}
