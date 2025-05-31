using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SurvivalModeManager : MonoBehaviour
{
    //// SURVIVAL MODE RULES
    /// Player must make enought mints profit each night, with each night getting a higher mint requirement
    /// Difficulty points are increased each night, regardless what i on the player farm
    /// Player can hold onto carrots between waves to use for the next nights currency requirements
    /// Death/Failure to gain enough mints before the new day transition triggers a game over
    /// Merchant refuses items sold to him during the day, only during the morning after a night before transition
    //// 

    public int mintsEarned = 0; //how many mints were earned on this night. Resets every night
    public int currentMintsRequired = 150; //how many mints are needed to progress to the next day
    public int minIncrease, maxIncrease; //how much the threshold of mints increases per day

    public TextMeshProUGUI mintsText;

    public static SurvivalModeManager Instance;

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

    void Start()
    {
        if(MainMenuScript.currentFileMode != FileMode.Survival)
        {
            gameObject.SetActive(false);
            return;
        }

        TimeManager.OnHourlyUpdate += HourlyUpdate;
    }

    void Update()
    {
        mintsText.text = mintsEarned + "/" + currentMintsRequired + "<sprite index=0>";
    }

    void HourlyUpdate()
    {
        if(TimeManager.Instance.currentHour == 8)
        {
            CheckProgress();
        }
    }

    public void CheckProgress()
    {
        mintsEarned = 0;
        currentMintsRequired += Random.Range(minIncrease, maxIncrease);
    }

    void OnDisable()
    {
        TimeManager.OnHourlyUpdate -= HourlyUpdate;
    }


}
