using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlantPage : CodexPage
{
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private TextMeshProUGUI harvested, wealth, growthStages, growthSpeed, consumes, produces, trellis, pollen;
    [SerializeField] private GameObject[] consumesIcons, producesIcons;

    public override void UpdatePage(CodexEntries entry, GameObject catContainer, GameObject containerToOpen)
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
        
        if(entry.entryName == "Mandrake") harvested.text = "Times harvested: " + entry.cropData.amountHarvested;
        else harvested.text = "Times Killed: " + entry.cropData.amountKilled;

        wealth.text = "Wealth: " + entry.cropData.wealthValue;
        growthStages.text = "Growth Stages: " + entry.cropData.growthStages;
        growthSpeed.text = "Time per Stage: " + entry.cropData.hoursPerStage;


        // Consumes
        if (entry.cropData.gloamIntake > 0) { consumesIcons[0].SetActive(true); }
        else { consumesIcons[0].SetActive(false); }

        if (entry.cropData.terraIntake > 0) { consumesIcons[1].SetActive(true); }
        else { consumesIcons[1].SetActive(false); }

        if (entry.cropData.ichorIntake > 0) { consumesIcons[2].SetActive(true); }
        else { consumesIcons[2].SetActive(false); }

        if (entry.cropData.waterIntake > 0) { consumesIcons[3].SetActive(true); }
        else { consumesIcons[3].SetActive(false); }

        //Produces
        if (entry.cropData.gloamIntake < 0) { producesIcons[0].SetActive(true); }
        else { producesIcons[0].SetActive(false); }

        if (entry.cropData.terraIntake < 0) { producesIcons[1].SetActive(true); }
        else { producesIcons[1].SetActive(false); }

        if (entry.cropData.ichorIntake < 0) { producesIcons[2].SetActive(true); }
        else { producesIcons[2].SetActive(false); }

        if (entry.cropData.waterIntake < 0) { producesIcons[3].SetActive(true); }
        else { producesIcons[3].SetActive(false); }


        trellis.text = cropItem.requireTrellis ? "Requires a Trellis" : "Does not Require a Trellis";
        pollen.text = entry.cropData.requirePollination ? "Requires Pollination" : "Does not Require Pollination";

        catContainer.SetActive(false);
        containerToOpen.SetActive(true);
    }
}
