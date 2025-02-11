using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimeManager : MonoBehaviour
{
    //Time
    public int currentMinute = 0; //30 in an hour
    int minPerDayHour = 30;
    int minPerNightHour = 30;
    public int currentHour = 6; //caps at 24, day is from 6-20. Military time. Night begins at 8PM,(20) and ends at 6AM, lasting 10 hours.
                                        /// <summary>
                                        /// /Day lasts 14 hours. Morning starts at 6, town opens at 8
                                        /// </summary>
    public bool isDay;
    public int dayNum = 1; //what day is it?
    public TextMeshProUGUI timeText;
    public Light dayLight, nightLight;

    //Sun and moon Variables
    public Transform sunMoonPivot;
    float oldRotation; //snaps the rotation to this
    float newRotation; //lerps to this
    Quaternion toQuaternion, fromQuaternion;
    bool canRotate;
    float seconds;

    //Events
    public delegate void HourlyUpdate();
    public static event HourlyUpdate OnHourlyUpdate;
    public bool timeSkipping = false; //Use for game over and sleeping
    public bool stopTime = false;
    //Maybe make an event for onSecond, or at least a stoptime bool

    public Material skyMat;
    float desiredBlend;
    public Color nightColor, dayColor;
    bool changingLights = false;

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
        if(!dayLight || !nightLight) Debug.Log("Error, did not apply daylight/nightlight variable in the inspector");
        StartCoroutine("TimePassage");
        InitializeSkyBox();
        if(timeText)
        {
            timeText.text = currentHour + ":00";
        }
        if(sunMoonPivot) sunMoonPivot.eulerAngles = new Vector3(oldRotation, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown("t"))
        {
            if(Time.timeScale == 1) Time.timeScale = 8;
            else Time.timeScale = 1;
        }

        if(!DialogueController.Instance.IsTalking()) seconds += Time.deltaTime;

        if(sunMoonPivot && canRotate && seconds != 0)
        {
            if(isDay) sunMoonPivot.rotation = Quaternion.Lerp(fromQuaternion, toQuaternion, seconds/(minPerDayHour));
            else sunMoonPivot.rotation = Quaternion.Lerp(fromQuaternion, toQuaternion, seconds/(minPerNightHour));
        }
    }

    IEnumerator TimePassage()
    {
        do
        {
            yield return new WaitForSeconds(1);
            if(!timeSkipping && !stopTime)
            {
                currentMinute++;
                LerpSunAndMoon();
                if((isDay && currentMinute >= minPerDayHour) || (!isDay && currentMinute >= minPerNightHour))
                {
                    currentMinute = 0;
                    HourPassed();
                }
            }

        }
        while(gameObject.activeSelf);
    }

    void HourPassed()
    {
        currentHour++;
        if(currentHour >= 24) currentHour = 0;

        if(currentHour >= 6 && currentHour < 20) isDay = true;
        else isDay = false;

        //if hour is 8, new day transition. dark screen, invoke, save, then brighten screen
            
        if(currentHour != 8) OnHourlyUpdate?.Invoke(); //We want this to trigger AFTER the transition
        //print("Hour passed. Time is now " + currentHour);
        //print("Is it day? " + isDay);

        switch (currentHour)
        {
            //case 4:
                //ToggleDayNightLights(true);
                //break;
            case 5:
                SetSkyBox(0.4f);
                ToggleDayNightLights(true);
                break;
            case 6:
                SetSkyBox(0.8f);
                break;
            case 7:
                SetSkyBox(1f);
                ToggleDayNightLights(true);
                break;
            case 8:
                StartCoroutine(NewDayTransition());
                break;
            //case 17:
                //ToggleDayNightLights(true);
                //;
            case 18:
                SetSkyBox(0.8f);
                ToggleDayNightLights(true);
                break;
            case 19:
                SetSkyBox(0.4f);
                break;
            case 20:
                SetSkyBox(0f);
                ToggleDayNightLights(true);
                break;
        }
        DynamicGI.UpdateEnvironment();
        CalculateSunAndMoonRotation();

        if (timeText)
        {
            timeText.text = currentHour + ":00";
        }
    }

    void SetSkyBox(float b)
    {
        desiredBlend = b;
        StartCoroutine(SkyColorLerp());
    }

    IEnumerator SkyColorLerp()
    {
        print("Changing Light");
        float newValue;
        Color lerpedColor;
        changingLights = true;
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
            nightLight.color = lerpedColor;
        }
        while(skyMat.GetFloat("_BlendCubemaps") != desiredBlend);
        changingLights = false;
    }

    void InitializeSkyBox()
    { 
        ToggleDayNightLights(false);
        CalculateSunAndMoonRotation();
        Color lerpedColor;
        if(currentHour < 5 || currentHour >= 20)
        {
            skyMat.SetFloat("_BlendCubemaps", 0f);
            lerpedColor = Color.Lerp(nightColor, dayColor, 0f);
            dayLight.color = lerpedColor;
            nightLight.color = lerpedColor;
            return;
        }
        if(currentHour >= 8 && currentHour < 18)
        {
            skyMat.SetFloat("_BlendCubemaps", 1f);
            lerpedColor = Color.Lerp(nightColor, dayColor, 1f);
            dayLight.color = lerpedColor;
            nightLight.color = lerpedColor;
            return;
        }
        switch (currentHour)
        {
            case 5:
                skyMat.SetFloat("_BlendCubemaps", 0.4f);
                lerpedColor = Color.Lerp(nightColor, dayColor, 0.2f);
                break;
            case 6:
                skyMat.SetFloat("_BlendCubemaps", 0.8f);
                lerpedColor = Color.Lerp(nightColor, dayColor, 0.4f);
                break;
            case 7:
                skyMat.SetFloat("_BlendCubemaps", 1f);
                lerpedColor = Color.Lerp(nightColor, dayColor, 1f);
                break;
            case 18:
                skyMat.SetFloat("_BlendCubemaps", 0.8f);
                lerpedColor = Color.Lerp(nightColor, dayColor, 0.4f);
                break;
            case 19:
                skyMat.SetFloat("_BlendCubemaps", 0.4f);
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
        nightLight.color = lerpedColor;
        DynamicGI.UpdateEnvironment();
    }

    private void OnDestroy()
    {
        skyMat.SetFloat("_BlendCubemaps", 1f);
        /*if(Instance != null && Instance == this)
        {
            Instance = null;
        } */
    }

    public void GameOver()
    {
        StopAllCoroutines();
        timeSkipping = true;
        stopTime = true;
        int timeDif = 0;
        currentMinute = 0;
        if(sunMoonPivot) sunMoonPivot.eulerAngles = new Vector3(oldRotation, 0, 0);
        //change time and day
        if(isDay) //Died during the day
        {
            int targetHour = currentHour + 5;
            if(currentHour < 8) targetHour = 7;
            if(targetHour > 19) targetHour = 19;
            while(currentHour != targetHour)
            {
                currentHour++;

                //this doesnt account for the things that arent structures
                /*foreach(StructureBehaviorScript structure in StructureManager.Instance.allStructs)
                {
                    structure.TimeLapse(1);
                }*/
                OnHourlyUpdate?.Invoke();
            }
        }
        else //Died during the night
        {
            while(currentHour != 8)
            {
                currentHour++;
                if(currentHour >= 24) currentHour = 0;

                //this doesnt account for the things that arent structures
                /*foreach(StructureBehaviorScript structure in StructureManager.Instance.allStructs)
                {
                    structure.TimeLapse(1);
                }*/
                if(currentHour != 8) OnHourlyUpdate?.Invoke();
            }
            StartCoroutine(NewDayTransition());
        }
        isDay = true;
        InitializeSkyBox();
        StartCoroutine(TimePassage());
        timeSkipping = false;
        stopTime = false;
    }

    IEnumerator NewDayTransition()
    {
        yield return new WaitUntil(() => PlayerInteraction.Instance.gameOver == false);

        PlayerInteraction.Instance.rb.velocity = new Vector3(0,0,0);
        PlayerMovement.restrictMovementTokens++;
        Time.timeScale = 0;
        FadeScreen.coverScreen = true;
        yield return new WaitForSecondsRealtime(2);
        dayNum++;
        //save game
        NightSpawningManager.Instance.ClearAllCreatures();
        yield return new WaitForSecondsRealtime(2);
        SaveGameManager.SaveData();
        FadeScreen.coverScreen = false;
        yield return new WaitForSecondsRealtime(0.5f);
        PlayerMovement.restrictMovementTokens--;
        Time.timeScale = 1;
        OnHourlyUpdate?.Invoke();
    }

    [ContextMenu("Set To Start Of Morning")]
    public void SetToMorning()
    {
        currentHour = 6;
        isDay = true;
        InitializeSkyBox();
    }

    [ContextMenu("Set To 8 AM")]
    public void SetTo8AM()
    {
        currentHour = 8;
        isDay = true;
        InitializeSkyBox();
    }

    [ContextMenu("Set To Start Of Night")]
    public void SetToNight()
    {
        currentHour = 19;
        isDay = true;
        InitializeSkyBox();
    }

    [ContextMenu("Set To Middle Of Night")]
    public void SetToMidNight()
    {
        currentHour = 1;
        isDay = false;
        InitializeSkyBox();
    }

    void LerpSunAndMoon()
    {
        if(!sunMoonPivot) return;

        canRotate = false;

        fromQuaternion.eulerAngles = new Vector3(oldRotation, 0, 0);
        toQuaternion.eulerAngles = new Vector3(newRotation, 0, 0);

        seconds = currentMinute;

        canRotate = true;
    }

    void CalculateSunAndMoonRotation()
    {
        if(!sunMoonPivot) return;
        float _oldRotation = 0; //snaps the rotation to this
        float _newRotation = 12.8f; //lerps to this
        if(!isDay) _newRotation += 5.2f;
        int hour = 6;

        while(hour != currentHour)
        {
            if(hour >= 6 && hour < 20)
            {
                _oldRotation += 12.8f;
                _newRotation += 12.8f;
            }
            else
            {
                _oldRotation += 18;
                _newRotation += 18;
            }
            hour++;
            if(hour >= 24) hour = 0;
        }

        oldRotation = _oldRotation;
        newRotation = _newRotation;

        if(sunMoonPivot && !Application.isPlaying) sunMoonPivot.eulerAngles = new Vector3(oldRotation, 0, 0);
    }  

    void ToggleDayNightLights(bool fadeTransition)
    {
        if(currentHour > 5 && currentHour < 18 && nightLight.enabled)
        {
            if(!Application.isPlaying || !fadeTransition)
            {
                dayLight.enabled = true;
                nightLight.enabled = false;
                return;
            }
            StartCoroutine(LightFade());
        }
        else if(currentHour <= 5 || currentHour >= 18 && dayLight.enabled)
        {
            if(!Application.isPlaying || !fadeTransition)
            {
                dayLight.enabled = false;
                nightLight.enabled = true;
                return;
            }
            StartCoroutine(LightFade());
        }
    }

    IEnumerator LightFade()
    {
        print("Ready");
        yield return new WaitForSeconds(0.9f);
        yield return new WaitUntil(() => !changingLights);
        print("Lerp");
        float lerp = 0;
        Color c_DayOriginal = dayLight.color;
        Color c_NightOriginal = nightLight.color;

        if(!dayLight.enabled) //switch to daylight
        {
            dayLight.color = Color.black;

            dayLight.enabled = true;
            while(lerp < 1)
            {
                yield return new WaitForSeconds(0.1f);
                lerp += 0.01f;

                dayLight.color = Color.Lerp(Color.black, c_DayOriginal, lerp);
                nightLight.color = Color.Lerp(c_NightOriginal, Color.black, lerp);
            }
            nightLight.enabled = false;
        }
        else //switch to nightlight
        {
            nightLight.color = Color.black;

            nightLight.enabled = true;
            while(lerp < 1)
            {
                yield return new WaitForSeconds(0.1f);
                lerp += 0.01f;

                dayLight.color = Color.Lerp(c_DayOriginal, Color.black, lerp);
                nightLight.color = Color.Lerp(Color.black, c_NightOriginal, lerp);
            }
            dayLight.enabled = false;
        }
    }
}
