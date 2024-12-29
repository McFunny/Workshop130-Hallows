using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimeManager : MonoBehaviour
{
    //Time
    public int currentMinute = 0; //30 in an hour
    int minPerHour = 30;
    public int currentHour = 6; //caps at 24, day is from 6-20. Military time. Night begins at 8PM,(20) and ends at 6AM, lasting 10 hours.
                                        /// <summary>
                                        /// /Day lasts 14 hours. Morning starts at 6, town opens at 8
                                        /// </summary>
    public bool isDay;
    public int dayNum = 1; //what day is it?
    public TextMeshProUGUI timeText;
    public Light dayLight;

    //Sun and moon Variables
    public Transform sunMoonPivot;
    float oldRotation; //snaps the rotation to this
    float newRotation; //lerps to this
    Vector3 targetRotation;
    Quaternion targetQuaternion;

    //Events
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
        if(!dayLight) Debug.Log("Error, did not apply daylight variable in the inspector");
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

        if(sunMoonPivot && targetQuaternion != sunMoonPivot.rotation)
        {
            sunMoonPivot.rotation = Quaternion.RotateTowards(sunMoonPivot.rotation, targetQuaternion, 0.5f * Time.deltaTime);
        }
    }

    IEnumerator TimePassage()
    {
        do
        {
            yield return new WaitForSeconds(1);
            if(!DialogueController.Instance.IsTalking())
            {
                currentMinute++;
                LerpSunAndMoon();
                if(currentMinute >= minPerHour)
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
            
        OnHourlyUpdate?.Invoke();
        //print("Hour passed. Time is now " + currentHour);
        //print("Is it day? " + isDay);

        switch (currentHour)
        {
            case 5:
                SetSkyBox(0.4f);
                break;
            case 6:
                dayNum++;
                SetSkyBox(0.8f);
                break;
            case 7:
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
        CalculateSunAndMoonRotation();

        if (timeText)
        {
            timeText.text = currentHour + ":00";
        }
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
        DynamicGI.UpdateEnvironment();
        CalculateSunAndMoonRotation();
    }

    private void OnDestroy()
    {
        skyMat.SetFloat("_BlendCubemaps", 1f);
        if(Instance != null && Instance == this)
        {
            Instance = null;
        }
    }

    public void GameOver()
    {
        StopCoroutine(TimePassage());
        int timeDif = 0;
        currentMinute = 0;
        //change time and day
        if(isDay)
        {
            int targetHour = currentHour + 5;
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
        else
        {
            while(currentHour != 7)
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
        isDay = true;
        InitializeSkyBox();
        StartCoroutine(TimePassage());
    }

    [ContextMenu("Set To Start Of Morning")]
    public void SetToMorning()
    {
        currentHour = 6;
        InitializeSkyBox();
    }

    [ContextMenu("Set To 8 AM")]
    public void SetTo8AM()
    {
        currentHour = 8;
        InitializeSkyBox();
    }

    [ContextMenu("Set To Start Of Night")]
    public void SetToNight()
    {
        currentHour = 19;
        InitializeSkyBox();
    }

    void LerpSunAndMoon()
    {
        if(!sunMoonPivot) return;
        //Lerp between current minute and the set/new rotation
        Vector3 from = new Vector3(oldRotation, 0, 0);
        Vector3 to = new Vector3(newRotation, 0, 0);
        float blend = ((float)currentMinute) / minPerHour;

        targetRotation = new Vector3(Mathf.LerpAngle(from.x, to.x, blend), 0, 0);

        targetQuaternion.eulerAngles = targetRotation;
        
        //sunMoonPivot.eulerAngles = target;
    }

    void CalculateSunAndMoonRotation()
    {
        if(!sunMoonPivot) return;
        float _oldRotation = 0; //snaps the rotation to this
        float _newRotation = 12.8f; //lerps to this
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
    }  //rotate it given a euler angler, and if it hasnt met the angle yet, have it lerp in update
}
