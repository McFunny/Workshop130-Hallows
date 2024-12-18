using UnityEngine;

public class LightController : MonoBehaviour
{
    public Light targetLight;
    public Gradient colorGradient;

    public float intensityVariation = 1f; 
    public float flickerSpeed = 5f;

    private float gradientPosition = 0f;
    private float baseIntensity;
    private float targetIntensityOffset = 0f;
    private float currentIntensityOffset = 0f;
    private float colorChangeSpeed;

    void Start()
    {
        if (targetLight == null)
        {
            targetLight = GetComponent<Light>();
        }

        baseIntensity = targetLight.intensity;
        colorChangeSpeed = Random.Range(0.2f, 0.55f);
        SetNewIntensityOffset();
    }

    void Update()
    {
       
        gradientPosition += Time.deltaTime * colorChangeSpeed;
        if (gradientPosition > 1f)
        {
            gradientPosition -= 1f; 
        }

       
        targetLight.color = colorGradient.Evaluate(gradientPosition);

       
        currentIntensityOffset = Mathf.Lerp(currentIntensityOffset, targetIntensityOffset, Time.deltaTime * flickerSpeed);
        if (Mathf.Abs(currentIntensityOffset - targetIntensityOffset) < 0.05f)
        {
            SetNewIntensityOffset();
        }

        targetLight.intensity = baseIntensity + currentIntensityOffset;
    }

    private void SetNewIntensityOffset()
    {
        targetIntensityOffset = Random.Range(-intensityVariation, intensityVariation);
    }
}
