using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class SurvivalModeManager : MonoBehaviour
{
    //// SURVIVAL MODE RULES
    /// Player must make enought mints profit each night, with each night getting a higher mint requirement
    /// Player can hold onto carrots between waves to use for the next nights currency requirements
    /// Death/Failure to gain enough mints before the new day transition triggers a game over
    /// Merchant refuses items sold to him during the day, only during the morning after a night before transition
    //// 
    private const int MONEY_CAP = 2000;
    public int mintsEarned = 0; //how many mints were earned on this night. Resets every night
    public int TotalMintsEarned
    {
        get
        {
            return _totalMintsEarned;
        }
        set
        {
            
            _totalMintsEarned = value;
            if(PlayerInteraction.Instance.totalMoneyEarned > MONEY_CAP) PlayerInteraction.Instance.totalMoneyEarned = MONEY_CAP; //cap total money earned display
            CheckMintValue(_totalMintsEarned);
        }
    }

    private int _totalMintsEarned = 0;

    public int currentMintsRequired = 60; //how many mints are needed to progress to the next day
    public int minIncrease, maxIncrease; //how much the threshold of mints increases per day

    private int _tierOneThreshold = 500;
    private int _tierTwoThreshold = 1500;
    private int _tierThreeThreshold = 3000;
    private bool _hasReachedTierOne = false;
    private bool _hasReachedTierTwo = false;
    private bool _hasReachedTierThree = false;

    public TextMeshProUGUI mintsText;

    public static SurvivalModeManager Instance;

    public SurvivalModeMerchant SurvivalModeMerchant;

    public PopupScript sellStuffP;

    public SurvivalStatsScreen statsScreen;

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
        AchievementManager.Instance.ClearAchievementsListForSurvivalMode();
        TimeManager.OnHourlyUpdate += HourlyUpdate;
    }

    void Update()
    {
        mintsText.text = mintsEarned + "/" + currentMintsRequired + "<sprite index=0>";
    }

    void HourlyUpdate()
    {
        if(TimeManager.Instance.currentHour == 6)
        {
            PopupHandler.Instance.AddToQueue(sellStuffP);
        }
        if(TimeManager.Instance.currentHour == 8)
        {
            CheckProgress();
        }
    }

    private void CheckMintValue(int totalEarned)
    {
        if(!_hasReachedTierOne && totalEarned >= _tierOneThreshold)
        {
            SurvivalModeMerchant.AddItemsToAllowedItems(1);
            _hasReachedTierOne = true;

        }
        else if(!_hasReachedTierTwo && totalEarned >= _tierTwoThreshold)
        {
            SurvivalModeMerchant.AddItemsToAllowedItems(2);
            _hasReachedTierTwo = true;
        }
        else if(!_hasReachedTierThree && totalEarned >= _tierThreeThreshold)
        {
            SurvivalModeMerchant.AddItemsToAllowedItems(3);
            _hasReachedTierThree = true;
        }
    }

    public void CheckProgress()
    {
        if(mintsEarned < currentMintsRequired)
        {
            //SceneManager.LoadSceneAsync(1);
            PlayerInteraction.Instance.stamina = 0;
            return;
        }

        mintsEarned = 0;
        currentMintsRequired += Random.Range(minIncrease, maxIncrease);
    }

    public IEnumerator GameOver()
    {
        statsScreen.GameOver();
        yield return new WaitForSeconds(5);
        //statsScreen.ReturnToMainMenu();
    }

    void OnDisable()
    {
        TimeManager.OnHourlyUpdate -= HourlyUpdate;
    }


}
