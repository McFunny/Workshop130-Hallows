using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Codex3 : MonoBehaviour
{
    private CodexEntries[] TutorialEntries, ToolEntries, CreatureEntries, PlantEntries;
    private List<CodexEntries> TutorialList, ToolList, CreatureList, PlantList;
    private int menuIndex;
    private string defaultName = "???";
    private ChildActivator childActivator;
    private enum OpenCategory
    {
        Tutorial,
        Tools,
        Plants,
        Creatures,
        Quests // This is not implemented yet
    }
    OpenCategory openCategory;
    [SerializeField] private GameObject codex;
    [SerializeField] private Button[] categoryButtons;
    [SerializeField] private GameObject[] containers;
    [SerializeField] private GameObject[] secondaryContainers;
    [SerializeField] private TextMeshProUGUI categoryTitle;
    [SerializeField] private GameObject categoryContainer;
    [SerializeField] private GameObject plantPage;

    [Header("Plant Page Vars")]
    [SerializeField] private TextMeshProUGUI plantPageTitle;
    [SerializeField] private Image plantPageImage;
    [SerializeField] private TextMeshProUGUI plantPageDescription;
    [SerializeField] private TextMeshProUGUI plantPageHarvested, plantPageWealth, plantPageGrowthStages, plantPageConsumes, plantPageProduces, plantPageTrellis, plantPagePollen;
    [SerializeField] private GameObject[] plantPageConsumesIcons, plantPageProducesIcons;
    


    [Header("Prefabs")]
    [SerializeField] private GameObject entryButtonPrefab;

    private void Awake()
    {
        childActivator = GetComponentInChildren<ChildActivator>();

        if (childActivator != null)
        {
            childActivator.onChildActivated += ResetCodex; // Reset the codex when the child is activated
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        TutorialEntries = Resources.LoadAll<CodexEntries>("Codex/GettingStarted/");
        ToolEntries = Resources.LoadAll<CodexEntries>("Codex/Tools/");
        CreatureEntries = Resources.LoadAll<CodexEntries>("Codex/Creatures/");
        PlantEntries = Resources.LoadAll<CodexEntries>("Codex/Plants/");
        openCategory = OpenCategory.Tutorial;

        ResetCodex();
        UpdateEntries();
        ChangeCategory(0); // Set the initial category to Tutorial
        codex.SetActive(false);
    }

    private void UpdateEntries()
    {
        if (TutorialList != null) ClearCodex(); // Clear the codex before updating entries. Checking the tutorial list should be sufficient.

        // Run this on Start or when the codex is opened to update the entries
        for (int i = 0; i < containers.Length; i++)
        {
            var Cat = TutorialEntries;
            switch (i)
            {
                case 0:
                    TutorialList = new List<CodexEntries>();
                    Cat = TutorialEntries;
                    break;
                case 1:
                    ToolList = new List<CodexEntries>();
                    Cat = ToolEntries;
                    break;
                case 2:
                    PlantList = new List<CodexEntries>();
                    Cat = PlantEntries;
                    break;
                case 3:
                    CreatureList = new List<CodexEntries>();
                    Cat = CreatureEntries;
                    break;
                case 4:
                    return; // Quests category is not implemented yet
            }

            print(Cat.Length + " entries found in category " + i);
            for (int e = 0; e < Cat.Length; e++)
            {
                var chosenContainer = containers[i];
                if (e < 16) chosenContainer = containers[i];
                else chosenContainer = secondaryContainers[i];

    
                GameObject entryButton = Instantiate(entryButtonPrefab, chosenContainer.transform);
                entryButton.name = Cat[e].entryName + " Entry";

                var tempName = entryButton.gameObject.transform.GetChild(0).gameObject;
                var tempImage = entryButton.gameObject.transform.GetChild(1).gameObject;
                var tempUnlock = entryButton.gameObject.transform.GetChild(2).gameObject;

                var tempID = entryButton.GetComponent<CodexButtonID>();
                var tempText = tempName.GetComponent<TextMeshProUGUI>();
                var tempSprite = tempImage.GetComponent<Image>();

                tempID.assignedEntry = Cat[e]; // Assign the entry to the button ID script

                if (Cat[e].unlocked) //Unlock Override
                {
                    tempText.text = Cat[e].entryName;
                    tempImage.SetActive(true);
                    tempUnlock.SetActive(false);
                    tempSprite.sprite = Cat[e].buttonIcon;
                }
                else if (Cat == PlantEntries)
                {
                    if (Cat[e].cropData.amountHarvested > 0) //Unlocks if amount of crop harvested > 0
                    {
                        tempText.text = Cat[e].entryName;
                        tempImage.SetActive(true);
                        tempUnlock.SetActive(false);
                        tempSprite.sprite = Cat[e].buttonIcon;
                    }
                    else
                    {
                        tempText.text = defaultName;
                        tempImage.SetActive(false);
                        tempUnlock.SetActive(true);
                    }
                }
                else if (Cat[e].creatureData != null) //Unlocks if amount of enemy killed > 0
                {
                    if (Cat[e].creatureData.amountKilled > 0 || Cat[e].creatureData.hasSpawned)
                    {
                        tempText.text = Cat[e].entryName;
                        tempImage.SetActive(true);
                        tempUnlock.SetActive(false);
                        tempSprite.sprite = Cat[e].buttonIcon;
                    }
                    else
                    {
                        tempText.text = defaultName;
                        tempImage.SetActive(false);
                        tempUnlock.SetActive(true);
                    }
                }
                else
                {
                    tempText.text = defaultName;
                    tempImage.SetActive(false);
                    tempUnlock.SetActive(true);
                }

                switch (i)
                {
                    case 0:
                        TutorialList.Add(Cat[e]);
                        break;
                    case 1:
                        ToolList.Add(Cat[e]);
                        break;
                    case 2:
                        PlantList.Add(Cat[e]);
                        break;
                    case 3:
                        CreatureList.Add(Cat[e]);
                        break;
                        
                }
            }
        }
    }

    public void UpdatePage(CodexEntries entry)
    {
        if (entry == null)
        {
            Debug.LogWarning("Entry is null, cannot update page.");
            return;
        }

        if (entry.cropData)
        {
            plantPageTitle.text = entry.entryName;
            plantPageImage.sprite = entry.mainImage;
            plantPageDescription.text = entry.description[0];
            plantPageHarvested.text = "Harvested: " + entry.cropData.amountHarvested;
            plantPageWealth.text = "Wealth: " + entry.cropData.wealthValue;
            plantPageGrowthStages.text = "Growth Stages: " + entry.cropData.growthStages;

            

            // Consumes
            if (entry.cropData.gloamIntake > 0) { plantPageConsumesIcons[0].SetActive(true); }
            else { plantPageConsumesIcons[0].SetActive(false); }

            if(entry.cropData.terraIntake > 0){plantPageConsumesIcons[1].SetActive(true);}
            else{plantPageConsumesIcons[1].SetActive(false);}

            if(entry.cropData.ichorIntake > 0){plantPageConsumesIcons[2].SetActive(true);}
            else{plantPageConsumesIcons[2].SetActive(false);}

            if(entry.cropData.waterIntake > 0){plantPageConsumesIcons[3].SetActive(true);}
            else{plantPageConsumesIcons[3].SetActive(false);}

            //Produces
            if(entry.cropData.gloamIntake < 0){plantPageProducesIcons[0].SetActive(true);}
            else{plantPageProducesIcons[0].SetActive(false);}

            if(entry.cropData.terraIntake < 0){plantPageProducesIcons[1].SetActive(true);}
            else{plantPageProducesIcons[1].SetActive(false);}

            if(entry.cropData.ichorIntake < 0){plantPageProducesIcons[2].SetActive(true);}
            else{plantPageProducesIcons[2].SetActive(false);}

            if(entry.cropData.waterIntake < 0){plantPageProducesIcons[3].SetActive(true);}
            else{plantPageProducesIcons[3].SetActive(false);}
            

            //plantPageTrellis.text = "Trellis: " + (entry.cropData.trellis ? "Yes" : "No");
            plantPagePollen.text = entry.cropData.requirePollination ? "Requires Pollination" : "Does not Require Pollination";

            categoryContainer.SetActive(false);
            plantPage.SetActive(true);
        }
        else if (entry.creatureData)
        {
            // Handle creature data if needed
        }
        else
        {
            // Handle other types of entries if needed
        }
        
    }

    private void ClearCodex()
    {
        // Clear all the entries in the codex
        for (int i = 0; i < containers.Length; i++)
        {
            foreach (Transform child in containers[i].transform)
            {
                Destroy(child.gameObject);
            }
        }

        TutorialList.Clear();
        ToolList.Clear();
        CreatureList.Clear();
        PlantList.Clear();
    }

    private void ResetCodex() //Sets the codex to its default state
    {
        print("Resetting Codex to default state.");

        plantPage.SetActive(false);
        categoryContainer.SetActive(true);
        containers[0].SetActive(true); // Start with the Tutorial category open
        containers[1].SetActive(false);
        containers[2].SetActive(false);
        containers[3].SetActive(false);
        containers[4].SetActive(false);
    }

    public void ChangeCategory(int categoryIndex)
    {
        categoryContainer.SetActive(true); // Show category container when changing categories
        plantPage.SetActive(false); // Hide entry pages when changing categories

        // Change the open category based on the index of the button pressed
        openCategory = (OpenCategory)categoryIndex;

        if (openCategory == OpenCategory.Quests)
        {
            Debug.LogWarning("Quests category is not implemented yet. Defaulting to Tutorial.");
            openCategory = OpenCategory.Tutorial; // Reset to Tutorial if Quests is selected
            categoryIndex = 0; // Reset category index to Tutorial
        }

        for (int i = 0; i < containers.Length; i++)
        {
            containers[i].SetActive(i == categoryIndex); //i is true when i = categoryIndex. Did not know I could do this lol
            secondaryContainers[i].SetActive(i == categoryIndex);
        }

        categoryTitle.text = openCategory.ToString(); // Update the category title
    }
}
