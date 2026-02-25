using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SaveLoadSystem;

public class SurvivalStatsScreen : MonoBehaviour
{
    public Transform statsBox;
    [SerializeField] private UILerp statsLerp, gameOverLerp;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private VerticalLayoutGroup statsContainer, statsBoxLayout;
    private List<SurvivalStatTexts> survivalStatTexts = new List<SurvivalStatTexts>(); // CURRENT CAP IS 7 STATS
    private GameObject survivalStatsParent;
    [SerializeField] private GameObject quotaParent, deathParent;
    public static bool isSurvivalStatsScreenActive = false;

    bool showedUi = false;
    private void Awake()
    {
        survivalStatsParent = transform.GetChild(0).gameObject;
        survivalStatsParent.SetActive(false);
        quotaParent.SetActive(false);
        deathParent.SetActive(false);
        isSurvivalStatsScreenActive = false;

        for (int i = 0; i < statsBox.childCount; i++)
        {
            SurvivalStatTexts temp = new SurvivalStatTexts();
            Transform stat = statsBox.GetChild(i);

            temp.statObject = stat.gameObject;
            temp.statNameText = stat.Find("Name").GetComponentInChildren<TextMeshProUGUI>();
            temp.statValueText = stat.Find("Value").GetComponentInChildren<TextMeshProUGUI>();
            
            stat.gameObject.SetActive(false);
            survivalStatTexts.Add(temp);
        }
    }

    void Start()
    {
        SurvivalModeManager.Instance.statsScreen = this;
    }

    private void Update()
    {
        /*if(Input.GetKeyDown(KeyCode.RightArrow))
        {
            GameOver(false);
        }
        else if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            GameOver(true);
        }*/

        if(ControlManager.isController && survivalStatsParent.activeSelf && EventSystem.current.currentSelectedGameObject == null)
        {
            EventSystem.current.SetSelectedGameObject(mainMenuButton.gameObject);
        }
    }
    public void GameOver(bool isDeath = false)
    {
        if(isDeath)
        {
            deathParent.SetActive(true);
            quotaParent.SetActive(false);
        }
        else
        {
            deathParent.SetActive(false);
            quotaParent.SetActive(true);
        }

        if(showedUi) return;

        if(ControlManager.isController) EventSystem.current.SetSelectedGameObject(mainMenuButton.gameObject);

        showedUi = true;

        StartCoroutine(AmbientAudioManager.Instance.FadeAudio(999));

        PlayerMovement.restrictMovementTokens++;
        PlayerInteraction.Instance.invincible = true;
        //Time.timeScale = 0;

        UpdateStats();
        survivalStatsParent.SetActive(true);
        statsLerp.lerpToStart = false;
        gameOverLerp.lerpToStart = false;
        isSurvivalStatsScreenActive = true;

        StartCoroutine(LayoutRebuildNextFrame());
    }

    private IEnumerator LayoutRebuildNextFrame()
    {
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(statsContainer.GetComponent<RectTransform>());
        
    }
    
    // CURRENT CAP IS 8 STATS
    private void UpdateStats()
    {
        survivalStatTexts[0].statNameText.text = "Nights Lasted:";                 // Name Goes here
        survivalStatTexts[0].statValueText.text = (TimeManager.Instance.dayNum - 1).ToString(); // Value goes here
        survivalStatTexts[0].statObject.SetActive(true);                           // Set this to true or false based on if you want to show them

        survivalStatTexts[1].statNameText.text = "Mints Collected:";
        survivalStatTexts[1].statValueText.text = SurvivalModeManager.Instance.TotalMintsEarned.ToString();
        survivalStatTexts[1].statObject.SetActive(true);

        survivalStatTexts[2].statNameText.text = "Creatures Defeated:";
        survivalStatTexts[2].statValueText.text = CalculateTotalCreatureDeaths().ToString();
        survivalStatTexts[2].statObject.SetActive(true);

        survivalStatTexts[3].statNameText.text = "Crops Grown:";
        survivalStatTexts[3].statValueText.text = CalculateTotalCropsGrown().ToString();
        survivalStatTexts[3].statObject.SetActive(true);

        survivalStatTexts[4].statNameText.text = "Crops Lost:";
        survivalStatTexts[4].statValueText.text = CalculateTotalCropsKilled().ToString();
        survivalStatTexts[4].statObject.SetActive(true);

        survivalStatTexts[5].statNameText.text = "Ranking:";
        survivalStatTexts[5].statValueText.text = Ranking();
        survivalStatTexts[5].statObject.SetActive(true);

        float nightHighscore = PlayerPrefs.GetFloat("NightHighScoreDemo", 0);
        if(nightHighscore < TimeManager.Instance.dayNum) nightHighscore = TimeManager.Instance.dayNum;

        float mintHighScore = PlayerPrefs.GetFloat("MintHighScoreDemo", 0);
        if(mintHighScore < SurvivalModeManager.Instance.TotalMintsEarned) mintHighScore = SurvivalModeManager.Instance.TotalMintsEarned;


        survivalStatTexts[6].statNameText.text = "Highest Night Count:";
        survivalStatTexts[6].statValueText.text = nightHighscore.ToString();
        survivalStatTexts[6].statObject.SetActive(true);

        survivalStatTexts[7].statNameText.text = "Hightest Mint Count:";
        survivalStatTexts[7].statValueText.text = mintHighScore.ToString();
        survivalStatTexts[7].statObject.SetActive(true);

        PlayerPrefs.Save();
    }

    public string Ranking()
    {
        int daysLasted = TimeManager.Instance.dayNum - 1;

        if(daysLasted < 3) return "Lowly Grub";
        else if(daysLasted < 6) return "Hardy Hare";
        else if(daysLasted < 10) return "Adaptable Mimic";
        else if(daysLasted < 15) return "Bodacious Hog";
        else return "Blazing Pyrefly";
    }

    public void ReturnToMainMenu()
    {
        // Implement return to main menu logic here
        var pauseScript = FindObjectOfType<PauseScript>();
        if(pauseScript != null)
        {
            SaveLoad.DeleteSaveData();
            Time.timeScale = 1;
            pauseScript.ForceMainMenu();
        }
        Debug.Log("Returning to Main Menu...");
        survivalStatsParent.SetActive(false);
    }

    int CalculateTotalCreatureDeaths()
    {
        int total = 0;

        foreach(CreatureObject c in CreatureDatabase.Instance.GetCreatureDatabase())
        {
            total += c.amountKilled;
        }
        return total;
    }

    int CalculateTotalCropsGrown()
    {
        int total = 0;

        foreach(CropData c in CropDatabase.Instance.GetCropList())
        {
            if(c.id == 8) continue; //Weeds
            total += c.amountHarvested;
        }
        return total;
    }

    int CalculateTotalCropsKilled()
    {
        int total = 0;

        foreach(CropData c in CropDatabase.Instance.GetCropList())
        {
            total += c.amountKilled;
        }
        return total;
    }
}

public class SurvivalStatTexts
{
    public GameObject statObject;
    public TextMeshProUGUI statNameText;
    public TextMeshProUGUI statValueText;
}