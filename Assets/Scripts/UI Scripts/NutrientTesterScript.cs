using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NutrientTesterScript : MonoBehaviour
{
    [SerializeField] private GameObject statsParent, seedParent;
    [SerializeField] private TextMeshProUGUI gloamText, terraText, ichorText, waterText;
    [SerializeField] private Image seedImage, checkmarkImage;
    public static NutrientTesterScript Instance;
    private CropItem currentSeed = null;
    private NutrientStorage currentNutrients = null;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.LogError("HELP!!! TOO MANY INSTANCES!!! HEEEEEEELP!!!");
        }

        Debug.Log("Nutrient Tester Instance: " + Instance);
    }

    private void Start()
    {
        UpdateTile(null);
        UpdateSeed(null);
    }

    public void UpdateSeed(CropItem seed)
    {
        currentSeed = seed;

        if (seed == null)
        {
            seedParent.SetActive(false);
            return;
        }

        seedImage.sprite = seed.icon;

        //Debug.Log(currentSeed);
        //Debug.Log(seedData);

        if (currentNutrients != null)
        {
            bool _canFullyGrow = true;
            CropData seedData = currentSeed.cropData;

            if (seedData.gloamIntake * seedData.growthStages > currentNutrients.gloamLevel)
            {
                _canFullyGrow = false;
            }

            if (seedData.terraIntake * seedData.growthStages > currentNutrients.terraLevel)
            {
                _canFullyGrow = false;
            }

            if (seedData.ichorIntake * seedData.growthStages > currentNutrients.ichorLevel)
            {
                _canFullyGrow = false;
            }

            if (_canFullyGrow) checkmarkImage.color = Color.green;
            else checkmarkImage.color = Color.red;
        }
        else
        {
            checkmarkImage.color = Color.clear;
        }
        
        seedParent.SetActive(true);
    }

    public void UpdateTile(NutrientStorage nutrients)
    {
        if (nutrients == null)
        {
            statsParent.SetActive(false);
            return;
        }

        currentNutrients = nutrients;

        gloamText.text = nutrients.gloamLevel + "/10";
        terraText.text = nutrients.terraLevel + "/10";
        ichorText.text = nutrients.ichorLevel + "/10";
        waterText.text = nutrients.waterLevel + "/10";
        statsParent.SetActive(true);

        UpdateSeed(currentSeed);
    }
}
