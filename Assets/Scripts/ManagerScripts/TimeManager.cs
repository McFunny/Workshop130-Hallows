using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimeManager : MonoBehaviour
{
    //Time
    public bool stopSaving = false;

    public int currentMinute = 0; 
    int minPerDayHour = 75; //how long an hour lasts at day
    int minPerNightHour = 35; //how long an hour lasts at night
    public int currentHour = 6; //caps at 24, day is from 6-20. Military time. Night begins at 8PM,(20) and ends at 6AM, lasting 10 hours.
                                        /// <summary>
                                        /// /Day lasts 14 hours. Morning starts at 6, town opens at 8
                                        //23 minutes long day and night cycle currently
                                        /// </summary>
    public bool isDay;
    public int dayNum = 1; //what day is it?
    public TimeOfDay timeOfDay;
    public Light dayLight, nightLight, cryptLight;

    //Sun and moon Variables
    public Transform sunMoonPivot;
    float oldRotation; //snaps the rotation to this
    float newRotation; //lerps to this
    Quaternion toQuaternion, fromQuaternion;
    bool canRotate;
    float seconds; //Used for sun and moon rotation, NOT time keeping
    public SpriteRenderer sunRenderer;
    public GameObject stupidSunGlow;
    public Sprite[] sunSprites;

    //Events
    public delegate void HourlyUpdate();
    public static event HourlyUpdate OnHourlyUpdate;
    public bool timeSkipping = false; //Use for game over and sleeping
    public bool stopTime = false;
    bool ignoreQuickSave = true;
    //Maybe make an event for onSecond, or at least a stoptime bool

    public Material skyMat;
    float desiredBlend;
    public Color nightColor, dayColor;
    bool changingLights = false;
    public bool clockDarkenEffect;

    public Transform playerRespawn, respawnFocus;

    public static TimeManager Instance;
    public delegate void UpdateCraftTimes(int val);
    public static event UpdateCraftTimes OnUpdateCraftTimes;

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

        if(MainMenuScript.currentFileMode == FileMode.Survival) minPerDayHour = 30;
    }

    
    // Start is called before the first frame update
    void Start()
    {
        if(currentHour >= 6 && currentHour < 20) isDay = true;
        else isDay = false;
        if(!dayLight || !nightLight) Debug.Log("Error, did not apply daylight/nightlight variable in the inspector");
        StartCoroutine("TimePassage");
        InitializeSkyBox();
        if(sunMoonPivot) sunMoonPivot.eulerAngles = new Vector3(oldRotation, 0, 0);
        if(sunRenderer) StartCoroutine(AnimateSun());

        TimeOfDayCheck();

        OnHourlyUpdate += TimeOfDayCheck;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown("t") && StructureManager.Instance.enableCheats)
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
            if(!timeSkipping && !stopTime && (!DialogueController.Instance.IsTalking()) && !PlayerInteraction.Instance.gameOver)
            {
                clockDarkenEffect = false;
                currentMinute++;
                LerpSunAndMoon();
                if((isDay && currentMinute >= minPerDayHour && currentHour >= 8) || (!isDay && currentMinute >= minPerNightHour) || (currentHour < 8 && currentMinute >= 60))
                {
                    currentMinute = 0;
                    HourPassed();
                }

                if((currentHour == 7 && currentMinute == minPerNightHour - 5) || (currentHour == 18 && currentMinute == minPerDayHour - 5)) PopupHandler.Instance.AddToQueue(PopupHandler.Instance.saveWarningPopup);
            }
            else
            {
                clockDarkenEffect = true;
            }

        }
        while(gameObject.activeSelf);
    }

    void HourPassed()
    {

        if(currentHour != 2 || !NightSpawningManager.Instance.finaleActivated) currentHour++; //To keep finale frozen
        
        if(currentHour >= 24) currentHour = 0;

        if(currentHour >= 6 && currentHour < 20) isDay = true;
        else isDay = false;

        TimeOfDayCheck();

        //if hour is 8, new day transition. dark screen, invoke, save, then brighten screen

        if(currentHour == 8 && MainMenuScript.currentFileMode == FileMode.Survival && SurvivalModeManager.Instance.CheckProgress() == false)
        {
            NightSpawningManager.Instance.GameOver();
            TimeManager.Instance.stopTime = true;
            FadeScreen.coverScreen = true;
            SurvivalModeManager.Instance.StartCoroutine(SurvivalModeManager.Instance.GameOver(true));
            return;
        }
            
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
                SetSkyBox(0.7f);
                break;
            case 7:
                SetSkyBox(0.9f);
                ToggleDayNightLights(true);
                break;
            case 8:
                SetSkyBox(1f);
                ToggleDayNightLights(true);
                StartCoroutine(NewDayTransition());
                break;
            case 17:
                //ToggleDayNightLights(true);
                SetSkyBox(0.9f);
                break;
            case 18:
                SetSkyBox(0.7f);
                ToggleDayNightLights(true);
                break;
            case 19:
                SetSkyBox(0.4f);
                StartCoroutine(QuickSaveGame());
                break;
            case 20:
                SetSkyBox(0f);
                ToggleDayNightLights(true);
                break;
        }
        DynamicGI.UpdateEnvironment();
        CalculateSunAndMoonRotation();

        ignoreQuickSave = false;
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
            newValue = skyMat.GetFloat("_Blend");
            if(newValue < desiredBlend) newValue += 0.01f;
            else newValue -= 0.01f;

            newValue = Mathf.Round(newValue * 100f) / 100f;
            skyMat.SetFloat("_Blend", newValue);
            lerpedColor = Color.Lerp(nightColor, dayColor, newValue);
            dayLight.color = lerpedColor;
            nightLight.color = lerpedColor;
        }
        while(skyMat.GetFloat("_Blend") != desiredBlend);
        changingLights = false;
    }

    public void RefreshSkybox()
    {
        InitializeSkyBox();
    }

    void InitializeSkyBox()
    { 
        ToggleDayNightLights(false);
        CalculateSunAndMoonRotation();
        Color lerpedColor;
        if(currentHour < 5 || currentHour >= 20)
        {
            skyMat.SetFloat("_Blend", 0f);
            lerpedColor = Color.Lerp(nightColor, dayColor, 0f);
            dayLight.color = lerpedColor;
            nightLight.color = lerpedColor;
            return;
        }
        if(currentHour >= 8 && currentHour < 18)
        {
            skyMat.SetFloat("_Blend", 1f);
            lerpedColor = Color.Lerp(nightColor, dayColor, 1f);
            dayLight.color = lerpedColor;
            nightLight.color = lerpedColor;
            return;
        }
        switch (currentHour)
        {
            case 5:
                skyMat.SetFloat("_Blend", 0.4f);
                lerpedColor = Color.Lerp(nightColor, dayColor, 0.2f);
                break;
            case 6:
                skyMat.SetFloat("_Blend", 0.8f);
                lerpedColor = Color.Lerp(nightColor, dayColor, 0.4f);
                break;
            case 7:
                skyMat.SetFloat("_Blend", 1f);
                lerpedColor = Color.Lerp(nightColor, dayColor, 1f);
                break;
            case 18:
                skyMat.SetFloat("_Blend", 0.8f);
                lerpedColor = Color.Lerp(nightColor, dayColor, 0.4f);
                break;
            case 19:
                skyMat.SetFloat("_Blend", 0.4f);
                lerpedColor = Color.Lerp(nightColor, dayColor, 0.2f);
                break;
            case 20:
                skyMat.SetFloat("_Blend", 0f);
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
        skyMat.SetFloat("_Blend", 1f);
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
        int minsPassed = 0;
        currentMinute = 0;
        if(sunMoonPivot) sunMoonPivot.eulerAngles = new Vector3(oldRotation, 0, 0);
        //change time and day
        if(isDay) //Died during the day
        {
            int targetHour = currentHour;
            if(currentHour < 8) targetHour = 7;
            else targetHour = currentHour + 5;
            
            if(targetHour > 19) targetHour = 18;
            //else targetHour = 19;
            while(currentHour != targetHour)
            {
                currentHour++;
                minsPassed += minPerDayHour;

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
                minsPassed += minPerNightHour;
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
        OnUpdateCraftTimes?.Invoke(minsPassed);
        ToggleSkyLights();
        isDay = true;
        InitializeSkyBox();
        DynamicGI.UpdateEnvironment();
        StartCoroutine(TimePassage());
        if(sunRenderer) StartCoroutine(AnimateSun());
        timeSkipping = false;
        stopTime = false;
    }

    IEnumerator NewDayTransition()
    {
        yield return new WaitUntil(() => PlayerInteraction.Instance.gameOver == false);
        PlayerInteraction.Instance.daysSinceDeath++;
        PlayerInteraction.Instance.InvokePlayerDeathEvent();
        PlayerInteraction.Instance.rb.velocity = new Vector3(0,0,0);
        PlayerMovement.restrictMovementTokens++;
        Time.timeScale = 0;
        FadeScreen.coverScreen = true;
        yield return new WaitForSecondsRealtime(1.5f);
        dayNum++;
        //save game
        NightSpawningManager.Instance.ClearAllCreatures();
        StructureManager.Instance.IncreaseNutrients();
        QuestManager.Instance.AdvanceDayTimers();
        yield return new WaitForSecondsRealtime(0.2f);
        OnHourlyUpdate?.Invoke();
        yield return new WaitForSecondsRealtime(2);
        if(!stopSaving) SaveGameManager.SaveData();
        FadeScreen.coverScreen = false;
        yield return new WaitForSecondsRealtime(0.5f);
        PlayerMovement.restrictMovementTokens--;
        Time.timeScale = 1;

        if(!stopSaving) PopupHandler.Instance.AddToQueue(PopupHandler.Instance.gameSavePopup);
        PopupHandler.Instance.NewsForNewDay();
        WildernessManager.Instance.visitedWilderness = false;
    }

    public void QuickSave()
    {
        StartCoroutine(QuickSaveGame());
    }

    IEnumerator QuickSaveGame() //Used on the 19th hour and sleeping.
    {
        if(ignoreQuickSave) yield break;

        PlayerInteraction.Instance.rb.velocity = new Vector3(0,0,0);
        PlayerMovement.restrictMovementTokens++;
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(0.1f);
        if(!stopSaving) SaveGameManager.SaveData();
        FadeScreen.coverScreen = false;
        PlayerMovement.restrictMovementTokens--;
        Time.timeScale = 1;

        if(!stopSaving) PopupHandler.Instance.AddToQueue(PopupHandler.Instance.gameSavePopup);
    }

    public IEnumerator Sleep()
    {
        ignoreQuickSave = false;

        StopAllCoroutines();
        timeSkipping = true;
        stopTime = true;
        int timeDif = 0;
        currentMinute = 0;

        int hoursPassed = 0;
        int minsPassed = 0;
        if(sunMoonPivot) sunMoonPivot.eulerAngles = new Vector3(oldRotation, 0, 0);

        FadeScreen.coverScreen = true;
        PlayerMovement.restrictMovementTokens++;
        yield return new WaitForSeconds(2f);
        
        //change time and day
        if(isDay) //Died during the day
        {
            int targetHour = 19;
            
            while(currentHour != targetHour)
            {
                currentHour++;
                hoursPassed++;
                print(currentHour);
                PlayerInteraction.Instance.StaminaChange(5);
                OnHourlyUpdate?.Invoke();
            }
            minsPassed = hoursPassed * minPerDayHour;
        }
        OnUpdateCraftTimes?.Invoke(minsPassed + (minPerDayHour - 20));
        StartCoroutine(QuickSaveGame());

        ToggleSkyLights();
        isDay = true;
        InitializeSkyBox();
        DynamicGI.UpdateEnvironment();
        StartCoroutine(TimePassage());
        if(sunRenderer) StartCoroutine(AnimateSun());
        timeSkipping = false;
        stopTime = false;

        currentMinute = minPerDayHour - 20;
        
        FadeScreen.coverScreen = false;
        PlayerMovement.restrictMovementTokens--;
    }

    void TimeOfDayCheck()
    {
        if(!isDay)
        {
            timeOfDay = TimeOfDay.Twilight;
            return;
        }
        if(currentHour >= 6 && currentHour <= 10)
        {
            timeOfDay = TimeOfDay.Dawn;
            return;
        }
        if(currentHour >= 11 && currentHour <= 16)
        {
            timeOfDay = TimeOfDay.Noon;
            return;
        }
        if(currentHour >= 17 && currentHour <= 21)
        {
            timeOfDay = TimeOfDay.Dusk;
            return;
        }
        Debug.LogError("Somethin aint right");
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

    IEnumerator AnimateSun()
    {
        int currentSprite = -1;
        do
        {
            currentSprite++;
            if(currentSprite >= sunSprites.Length) currentSprite = 0;
            yield return new WaitForSeconds(0.3f);
            sunRenderer.sprite = sunSprites[currentSprite];
            if(currentSprite <= 1) stupidSunGlow.transform.localScale = new Vector3(stupidSunGlow.transform.localScale.x + .4f, stupidSunGlow.transform.localScale.y + .4f, stupidSunGlow.transform.localScale.z + .4f);
            else stupidSunGlow.transform.localScale = new Vector3(stupidSunGlow.transform.localScale.x - .4f, stupidSunGlow.transform.localScale.y - .4f, stupidSunGlow.transform.localScale.z - .4f);
        }
        while(gameObject.activeSelf);
    }

    void ToggleDayNightLights(bool fadeTransition)
    {
        if (TownGate.Instance != null)
        {
            if (TownGate.Instance.location == PlayerLocation.InCrypt) return;
        }

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

    public void ToggleSkyLights() //for moving between town and crypt, without a smooth transition
    {
        if(TownGate.Instance == null) return;
        if (TownGate.Instance.location == PlayerLocation.InCrypt)
        {
            dayLight.enabled = false;
            nightLight.enabled = false;
            cryptLight.enabled = true;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            return;
        }
        cryptLight.enabled = false;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;


        if(currentHour > 5 && currentHour < 18)
        {
            dayLight.enabled = true;
            nightLight.enabled = false;
        }
        else if(currentHour <= 5 || currentHour >= 18)
        {
            dayLight.enabled = false;
            nightLight.enabled = true;
        }
    }
}

public enum TimeOfDay
{
    Dawn, //6 am to 10 pm
    Noon, //11 am to 4 pm
    Dusk, //5 pm to 9 pm
    Twilight //10pm to 5 am
}
