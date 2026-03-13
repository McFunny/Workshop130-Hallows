using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlantPage : CodexPage
{
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private TextMeshProUGUI harvested, wealth, growthStages, growthSpeed, trellis, pollen;
    [SerializeField] private GameObject consumesParent, producesParent, targetedByParent;
    [SerializeField] private VerticalLayoutGroup rightPageLayoutGroup;
    [SerializeField] private VerticalLayoutGroup consumesLayoutGroup, producesLayoutGroup;
    [SerializeField] private GameObject[] consumesObject, producesObject;
    [SerializeField] private Slider[] consumesSlider, producesSlider;
    [SerializeField] private Image[] targetedByImages;
    private Color disabledColor = new Color(1f, 1f, 1f, 0f);
    private Color enabledColor = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private TextMeshProUGUI[] targetedByText;
    private RectTransform rightPageRectTransform, consumesRectTransform, producesRectTransform;

    private void Awake()
    {
        consumesSlider = new Slider[consumesObject.Length];
        producesSlider = new Slider[producesObject.Length];
        for (int i = 0; i < consumesObject.Length; i++)
        {
            consumesSlider[i] = consumesObject[i].GetComponentInChildren<Slider>();
            producesSlider[i] = producesObject[i].GetComponentInChildren<Slider>();
        }

        /*for (int i = 0; i < targetedByImages.Length; i++)
        {
            targetedByText[i] = targetedByImages[i].gameObject.GetComponentInChildren<TextMeshProUGUI>();
        }*/

        rightPageRectTransform = rightPageLayoutGroup.GetComponent<RectTransform>();
        consumesRectTransform = consumesLayoutGroup.GetComponent<RectTransform>();
        producesRectTransform = producesLayoutGroup.GetComponent<RectTransform>();
    }
    private void Start()
    {
        trellis.text = "Requires a Trellis";
        pollen.text = "Requires Pollination";
    }

    private void OnEnable()
    {
        Canvas.ForceUpdateCanvases(); //help
        StartCoroutine(DelayedUpdate());
    }

    public override void UpdatePage(CodexEntries entry, Quest quest)
    {
        if (!entry.cropData)
        {
            Debug.LogWarning("No cropdata found");
            return;
        }

        CropItem cropItem = (CropItem)entry.cropData.cropSeed;
        print(cropItem);

        title.text = entry.entryName;
        image.sprite = entry.mainImage;
        description.text = entry.description[0];

        harvested.text = "Times harvested: " + entry.cropData.amountHarvested;

        wealth.text = "Allure Value: " + entry.cropData.wealthValue;
        growthStages.text = "Growth Stages: " + entry.cropData.growthStages;
        growthSpeed.text = "Time per Stage: " + entry.cropData.hoursPerStage;
        Canvas.ForceUpdateCanvases();

        UpdateConsumesAndProduces(entry);

        if (cropItem != null)
        {
            if (cropItem.requireTrellis) trellis.gameObject.SetActive(true);
            else trellis.gameObject.SetActive(false);
        }
        else trellis.gameObject.SetActive(false);

        if (entry.cropData.requirePollination) pollen.gameObject.SetActive(true);
        else pollen.gameObject.SetActive(false);

        if (entry.targetedBy.Length != 0)
        {
            for (int i = 0; i < targetedByImages.Length; i++)
            {
                if (i < entry.targetedBy.Length)
                {
                    targetedByImages[i].sprite = entry.targetedBy[i].buttonIcon;

                    if (entry.targetedBy[i].creatureData.amountKilled > 0 || entry.targetedBy[i].unlocked)
                    {
                        targetedByImages[i].color = enabledColor;
                        targetedByText[i].text = "";
                    }
                    else
                    {
                        targetedByImages[i].color = disabledColor;
                        targetedByText[i].text = "?";
                    }

                    targetedByImages[i].gameObject.SetActive(true);
                }
                else targetedByImages[i].gameObject.SetActive(false);
            }
            targetedByParent.SetActive(true);
        }
        else targetedByParent.SetActive(false);
    }

    private void UpdateConsumesAndProduces(CodexEntries entry)
    {
        var totalGrowthStages = entry.cropData.growthStages;

        // Consumes
        if (entry.cropData.gloamIntake > 0)
        {
            consumesObject[0].SetActive(true);
            consumesSlider[0].value = entry.cropData.gloamIntake * totalGrowthStages;
        }
        else
        {
            consumesObject[0].SetActive(false);
        }

        if (entry.cropData.terraIntake > 0)
        {
            consumesObject[1].SetActive(true);
            consumesSlider[1].value = entry.cropData.terraIntake * totalGrowthStages;
        }
        else
        {
            consumesObject[1].SetActive(false);
        }

        if (entry.cropData.ichorIntake > 0)
        {
            consumesObject[2].SetActive(true);
            consumesSlider[2].value = entry.cropData.ichorIntake * totalGrowthStages;
        }
        else
        {
            consumesObject[2].SetActive(false);
        }

        /*if (entry.cropData.waterIntake > 0)
        {
            consumesObject[3].SetActive(true);
        }
        else
        {
            consumesObject[3].SetActive(false);
        }*/

        if (consumesObject[0].activeSelf || consumesObject[1].activeSelf || consumesObject[2].activeSelf)
        {
            consumesParent.SetActive(true);
        }
        else consumesParent.SetActive(false);

        //Produces
        if (entry.cropData.gloamIntake < 0)
        {
            producesObject[0].SetActive(true);
            producesSlider[0].value = -entry.cropData.gloamIntake * totalGrowthStages;
        }
        else
        {
            producesObject[0].SetActive(false);
        }

        if (entry.cropData.terraIntake < 0)
        {
            producesObject[1].SetActive(true);
            producesSlider[1].value = -entry.cropData.terraIntake * totalGrowthStages;
        }
        else
        {
            producesObject[1].SetActive(false);
        }

        if (entry.cropData.ichorIntake < 0)
        {
            producesObject[2].SetActive(true);
            producesSlider[2].value = -entry.cropData.ichorIntake * totalGrowthStages;
        }
        else
        {
            producesObject[2].SetActive(false);
        }

        /*if (entry.cropData.waterIntake < 0)
        {
            producesObject[3].SetActive(true);
        }
        else
        {
            producesObject[3].SetActive(false);
        }*/

        if (producesObject[0].activeSelf || producesObject[1].activeSelf || producesObject[2].activeSelf)
        {
            producesParent.SetActive(true);
        }
        else producesParent.SetActive(false);

        
    }

    public IEnumerator DelayedUpdate() // this is stupid
    {
        yield return new WaitForEndOfFrame();

        LayoutRebuilder.ForceRebuildLayoutImmediate(producesRectTransform);
        Canvas.ForceUpdateCanvases();
        producesLayoutGroup.enabled = false; 
        producesLayoutGroup.enabled = true; 

        

        LayoutRebuilder.ForceRebuildLayoutImmediate(consumesRectTransform);
        Canvas.ForceUpdateCanvases();
        consumesLayoutGroup.enabled = false; 
        consumesLayoutGroup.enabled = true; 

        

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rightPageRectTransform);
        rightPageLayoutGroup.enabled = false; 
        rightPageLayoutGroup.enabled = true; 

        StopCoroutine(DelayedUpdate());
    }
}
