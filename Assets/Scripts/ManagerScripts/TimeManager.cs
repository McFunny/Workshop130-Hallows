using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimeManager : MonoBehaviour
{
    public int currentHour = 6; //caps at 24, day is from 6-20. Military time. Night begins at 8PM,(20) and ends at 6AM, lasting 10 hours.
                                        /// <summary>
                                        /// /Day lasts 14 hours. Each hour lasts 45/30 seconds. Morning starts at 6, town opens at 8
                                        /// </summary>
    public bool isDay;
    public int dayNum = 1; //what day is it?
    public TextMeshProUGUI timeText;
    public Light dayLight;

    public delegate void HourlyUpdate();
    public static event HourlyUpdate OnHourlyUpdate;

    public Material skyMat;
    float desiredBlend;
    public Color nightColor, dayColor;

    public Transform playerRespawn;

    public static TimeManager Instance;

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }
    }

    
    // Start is called before the first frame update
    void Start()
    {
        if(currentHour >= 6 && currentHour < 20) isDay = true;
        else isDay = false;
        if(!dayLight) dayLight = FindObjectOfType<Light>();
        StartCoroutine("TimePassage");
        InitializeSkyBox();
        if(timeText)
        {
            timeText.text = currentHour + ":00";
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown("t"))
        {
            if(Time.timeScale == 1) Time.timeScale = 8;
            else Time.timeScale = 1;
        }
    }

    IEnumerator TimePassage()
    {
        do
        {
            yield return new WaitForSeconds(30);
            currentHour++;
            if(currentHour >= 24) currentHour = 0;

            if(currentHour >= 6 && currentHour < 20) isDay = true;
            else isDay = false;
            
            OnHourlyUpdate?.Invoke();
            print("Hour passed. Time is now " + currentHour);
            print("Is it day? " + isDay);

            switch (currentHour)
            {
                case 6:
                    SetSkyBox(0.4f);
                    break;
                case 7:
                    SetSkyBox(0.8f);
                    break;
                case 8:
                    dayNum++;
                    SetSkyBox(1f);
                    break;
                case 18:
                    SetSkyBox(0.8f);
                    break;
                case 19:
                    SetSkyBox(0.4f);
                    break;
                case 20:
                    SetSkyBox(0f);
                    break;
                default:
                    //
                    break;
            }
            DynamicGI.UpdateEnvironment();

            if (timeText)
        {
            timeText.text = currentHour + ":00";
        }

        }
        while(gameObject.activeSelf);
    }

    void SetSkyBox(float b)
    {
        desiredBlend = b;
        StartCoroutine("SkyColorLerp");
    }

    IEnumerator SkyColorLerp()
    {
        print("Changing Light");
        float newValue;
        Color lerpedColor;
        do
        {
            yield return new WaitForSeconds(0.5f);
            newValue = skyMat.GetFloat("_BlendCubemaps");
            if(newValue < desiredBlend) newValue += 0.01f;
            else newValue -= 0.01f;

            newValue = Mathf.Round(newValue * 100f) / 100f;
            skyMat.SetFloat("_BlendCubemaps", newValue);
            lerpedColor = Color.Lerp(nightColor, dayColor, newValue);
            dayLight.color = lerpedColor;
        }
        while(skyMat.GetFloat("_BlendCubemaps") != desiredBlend);
    }

    void InitializeSkyBox()
    { 
        Color lerpedColor;
        if(currentHour < 6 || currentHour >= 20)
        {
            skyMat.SetFloat("_BlendCubemaps", 0f);
            lerpedColor = Color.Lerp(nightColor, dayColor, 0f);
            dayLight.color = lerpedColor;
            return;
        }
        if(currentHour >= 8 && currentHour < 18)
        {
            skyMat.SetFloat("_BlendCubemaps", 1f);
            lerpedColor = Color.Lerp(nightColor, dayColor, 1f);
            dayLight.color = lerpedColor;
            return;
        }
        switch (currentHour)
        {
            case 6:
                skyMat.SetFloat("_BlendCubemaps", 0.2f);
                lerpedColor = Color.Lerp(nightColor, dayColor, 0.2f);
                break;
            case 7:
                skyMat.SetFloat("_BlendCubemaps", 0.4f);
                lerpedColor = Color.Lerp(nightColor, dayColor, 0.4f);
                break;
            case 8:
                skyMat.SetFloat("_BlendCubemaps", 1f);
                lerpedColor = Color.Lerp(nightColor, dayColor, 1f);
                break;
            case 18:
                skyMat.SetFloat("_BlendCubemaps", 0.5f);
                lerpedColor = Color.Lerp(nightColor, dayColor, 0.4f);
                break;
            case 19:
                skyMat.SetFloat("_BlendCubemaps", 0.25f);
                lerpedColor = Color.Lerp(nightColor, dayColor, 0.2f);
                break;
            case 20:
                skyMat.SetFloat("_BlendCubemaps", 0f);
                lerpedColor = Color.Lerp(nightColor, dayColor, 0f);
                break;
            default:
                lerpedColor = dayColor;
                break;
        }
        dayLight.color = lerpedColor;
        DynamicGI.UpdateEnvironment();
    }

    private void OnDestroy()
    {
        skyMat.SetFloat("_BlendCubemaps", 1f);
    }

    public void GameOver()
    {
        StopCoroutine(TimePassage());
        int timeDif = 0;
        //change time and day
        if(isDay)
        {
            while(currentHour != 19)
            {
                currentHour++;
                timeDif++;
            }
        }
        else
        {
            while(currentHour != 8)
            {
                currentHour++;
                timeDif++;
                if(currentHour == 24) currentHour = 0;
            }
        }
        foreach(StructureBehaviorScript structure in StructureManager.Instance.allStructs)
        {
            structure.TimeLapse(timeDif);
        }
        InitializeSkyBox();
        StartCoroutine(TimePassage());
    }
}
