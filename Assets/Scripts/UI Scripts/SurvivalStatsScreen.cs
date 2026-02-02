using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SurvivalStatsScreen : MonoBehaviour
{
    public Transform statsBox;
    [SerializeField] private UILerp statsLerp, gameOverLerp;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private VerticalLayoutGroup statsContainer, statsBoxLayout;
    private List<SurvivalStatTexts> survivalStatTexts = new List<SurvivalStatTexts>(); // CURRENT CAP IS 7 STATS
    private GameObject survivalStatsParent;
    private void Awake()
    {
        survivalStatsParent = transform.GetChild(0).gameObject;
        survivalStatsParent.SetActive(false);

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

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.RightArrow))
        {
            GameOver();
        }

        if(ControlManager.isController && survivalStatsParent.activeSelf && EventSystem.current.currentSelectedGameObject == null)
        {
            EventSystem.current.SetSelectedGameObject(mainMenuButton.gameObject);
        }
    }
    public void GameOver()
    {
        UpdateStats();
        survivalStatsParent.SetActive(true);
        statsLerp.lerpToStart = false;
        gameOverLerp.lerpToStart = false;

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
        survivalStatTexts[0].statValueText.text = Random.Range(0, 100).ToString(); // Value goes here
        survivalStatTexts[0].statObject.SetActive(true);                           // Set this to true or false based on if you want to show them

        survivalStatTexts[1].statNameText.text = "Mints Collected:";
        survivalStatTexts[1].statValueText.text = Random.Range(0, 100).ToString();
        survivalStatTexts[1].statObject.SetActive(true);

        survivalStatTexts[2].statNameText.text = "";
        survivalStatTexts[2].statValueText.text = "";
        survivalStatTexts[2].statObject.SetActive(false);

        survivalStatTexts[3].statNameText.text = "";
        survivalStatTexts[3].statValueText.text = "";
        survivalStatTexts[3].statObject.SetActive(false);

        survivalStatTexts[4].statNameText.text = "";
        survivalStatTexts[4].statValueText.text = "";
        survivalStatTexts[4].statObject.SetActive(false);

        survivalStatTexts[5].statNameText.text = "";
        survivalStatTexts[5].statValueText.text = "";
        survivalStatTexts[5].statObject.SetActive(false);

        survivalStatTexts[6].statNameText.text = "";
        survivalStatTexts[6].statValueText.text = "";
        survivalStatTexts[6].statObject.SetActive(false);

        survivalStatTexts[7].statNameText.text = "";
        survivalStatTexts[7].statValueText.text = "";
        survivalStatTexts[7].statObject.SetActive(false);
    }

    public void ReturnToMainMenu()
    {
        // Implement return to main menu logic here
        var pauseScript = FindObjectOfType<PauseScript>();
        if(pauseScript != null)
        {
            pauseScript.ForceMainMenu();
        }
        Debug.Log("Returning to Main Menu...");
        survivalStatsParent.SetActive(false);
    }
}

public class SurvivalStatTexts
{
    public GameObject statObject;
    public TextMeshProUGUI statNameText;
    public TextMeshProUGUI statValueText;
}