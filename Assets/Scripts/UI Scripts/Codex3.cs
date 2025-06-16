using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;

public class Codex3 : MonoBehaviour
{
    private CodexEntries[] TutorialEntries, ToolEntries, StructureEntries, PlantEntries, CreatureEntries, BugEntries; //Looks dumb but I need a reference to the SO's cached or else this gets really messy
    private List<CodexEntries> TutorialList, ToolList, StructureList, PlantList, CreatureList, BugList;
    private int menuIndex; //0 = closed, 1 = open, 2 = entry page open
    private string defaultName = "???";
    private ControlManager controlManager;
    private QuestManager questManager;
    [SerializeField] private ChildActivator childActivator;
    public enum OpenCategory
    {
        Tutorial,
        Tools,
        Structures,
        Plants,
        Creatures,
        Bugs,
        Quests, // This is not implemented yet
    }
    OpenCategory openCategory;
    [SerializeField] private GameObject codex;
    [SerializeField] private Button[] categoryButtons;
    [SerializeField] private GameObject[] containers;
    [SerializeField] private GameObject[] secondaryContainers;
    [SerializeField] private List<CodexPage> codexPages;
    [SerializeField] private TextMeshProUGUI categoryTitle;
    [SerializeField] private GameObject categoryContainer;
    [SerializeField] private List<Quest> activeQuests = new List<Quest>();
    //[SerializeField] private GameObject tutorialPage, toolPage, plantPage;

    [Header("Prefabs")]
    [SerializeField] private GameObject entryButtonPrefab;
    [SerializeField] private GameObject entryButtonHorizontalPrefab;

    [Header("Override References")]
    [SerializeField] private CodexEntries mandrakeEntry;

    private void Awake()
    {
        controlManager = FindFirstObjectByType<ControlManager>();
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
        StructureEntries = Resources.LoadAll<CodexEntries>("Codex/Structures");
        CreatureEntries = Resources.LoadAll<CodexEntries>("Codex/Creatures/");
        PlantEntries = Resources.LoadAll<CodexEntries>("Codex/Plants/");
        BugEntries = Resources.LoadAll<CodexEntries>("Codex/Bugs");
        questManager = FindAnyObjectByType<QuestManager>();
        openCategory = OpenCategory.Tutorial;

        ResetCodex();
        UpdateEntries();
        ChangeCategory("Tutorial"); // Set the initial category to Tutorial
        codex.SetActive(false);
        menuIndex = 0;
    }

    private void OnEnable()
    {
        controlManager.backCodex.action.started += InputBack;
    }

    private void OnDisable()
    {
        controlManager.backCodex.action.started -= InputBack;
    }

    private void InputBack(InputAction.CallbackContext context)
    {
        Back();
    }

    public void OpenCodex()
    {

        menuIndex = 1;
        codex.SetActive(true);
        PlayerMovement.isCodexOpen = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseCodex()
    {
        menuIndex = 0;
        codex.SetActive(false);
        PlayerMovement.isCodexOpen = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Back()
    {
        switch (menuIndex)
        {
            case 0:
                break;

            case 1:
                CloseCodex();
                break;

            case 2:
                ChangeCategory(openCategory.ToString());
                break;

            default:
                break;
        }
    }

    private void UpdateEntries()
    {
        if (TutorialList != null) ClearCodex(); // Clear the codex before updating entries. Checking the tutorial list should be sufficient.

        // Run this on Start or when the codex is opened to update the entries
        for (int i = 0; i < containers.Length; i++)
        {
            var Cat = TutorialEntries;
            var isQuest = false;
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
                    StructureList = new List<CodexEntries>();
                    Cat = StructureEntries;
                    break;
                case 3:
                    PlantList = new List<CodexEntries>();
                    Cat = PlantEntries;
                    break;
                case 4:
                    CreatureList = new List<CodexEntries>();
                    Cat = CreatureEntries;
                    break;
                case 5:
                    BugList = new List<CodexEntries>();
                    Cat = BugEntries;
                    break; 

                case 6:
                    isQuest = true;
                    Cat = null;
                    break;
            }

            if (isQuest)
            {
                print("Attempting to load Quest Category");

                if (activeQuests.Count == 0)
                {
                    print("No Frests found.");
                    return;
                }
                
                for (int e = 0; e < activeQuests.Count; e++)
                    {
                        var chosenContainer = containers[i];
                        if (e < 10) chosenContainer = containers[i];
                        else chosenContainer = secondaryContainers[i];

                        GameObject entryButton = Instantiate(entryButtonHorizontalPrefab, chosenContainer.transform);
                        entryButton.name = activeQuests[e].name + " Quest";

                        var questTitle = entryButton.GetComponentInChildren<TextMeshProUGUI>();
                        var buttonScript = entryButton.GetComponent<CodexButtonID>();

                        questTitle.text = activeQuests[e].name;
                        buttonScript.assignedQuest = activeQuests[e];
                    }
            }

            if (Cat == null) continue;

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
                    if (Cat[e] == mandrakeEntry)
                    {
                        if (mandrakeEntry.cropData.amountKilled > 0)
                        {
                            mandrakeEntry.unlocked = true;
                            continue;
                        }
                        else mandrakeEntry.unlocked = false;
                    }


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
                        StructureList.Add(Cat[e]);
                        break;
                    case 3:
                        PlantList.Add(Cat[e]);
                        break;
                    case 4:
                        CreatureList.Add(Cat[e]);
                        break;
                    case 5:
                        BugList.Add(Cat[e]);
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

        GameObject pageToOpen = null;

        switch ((int)entry.entryType)
        {
            case 0:
                pageToOpen = codexPages[0].gameObject;
                break;

            case 1:
                pageToOpen = codexPages[1].gameObject;
                break;

            case 2:
                pageToOpen = codexPages[2].gameObject;
                break;

            case 3:
                pageToOpen = codexPages[3].gameObject;
                break;

            case 4:
                pageToOpen = codexPages[4].gameObject;
                break;

            case 5:
                pageToOpen = codexPages[5].gameObject;
                break;
        }
        print(pageToOpen.gameObject);

        if (pageToOpen == null) return;

        codexPages[(int)entry.entryType].UpdatePage(entry);

        categoryContainer.SetActive(false);
        pageToOpen.SetActive(true);

        menuIndex = 2;
    }

    public void UpdateQuest(Quest quest)
    {
        codexPages[(int)OpenCategory.Quests].UpdatePage(null, quest);

        categoryContainer.SetActive(false);
        codexPages[(int)OpenCategory.Quests].gameObject.SetActive(true);
        menuIndex = 2;
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
        StructureList.Clear();
        CreatureList.Clear();
        PlantList.Clear();
        BugList.Clear();
    }

    private void ResetCodex() //Sets the codex to its default state
    {
        print("Resetting Codex to default state and updating entries.");
        UpdateEntries();
        ChangeCategory("Tutorial"); // Start with the Tutorial category open
        activeQuests = questManager.activeQuests;
        menuIndex = 1;
    }

    public void ChangeCategory(string categoryToOpen)
    {
        CloseEntryPages();

        // Change the open category based on the index of the button pressed
        openCategory = (OpenCategory)Enum.Parse(typeof(OpenCategory), categoryToOpen); //Wow
        var catInt = (int)openCategory;

        /*if (openCategory == OpenCategory.Quests)
        {
            Debug.LogWarning("Quests category is not implemented yet. Defaulting to Tutorial.");
            openCategory = OpenCategory.Tutorial; // Reset to Tutorial if Quests is selected
            catInt = 0; // Reset category index to Tutorial
        }*/
        /*if (openCategory == OpenCategory.Bugs)
        {
            Debug.LogWarning("Bugs category is not implemented yet. Defaulting to Tutorial.");
            openCategory = OpenCategory.Tutorial; // Reset to Tutorial if Quests is selected
            catInt = 0; // Reset category index to Tutorial
        }*/

        for (int i = 0; i < containers.Length; i++)
        {
            containers[i].SetActive(i == catInt); //i is true when i = categoryIndex. Did not know I could do this lol
            secondaryContainers[i].SetActive(i == catInt);
        }
        categoryTitle.text = openCategory.ToString(); // Update the category title
    }

    private void CloseEntryPages()
    {
        categoryContainer.SetActive(true);
        foreach (CodexPage page in codexPages)
        {
            if (page != null) page.gameObject.SetActive(false);
        }

        menuIndex = 1;
        /*tutorialPage.SetActive(false);
        toolPage.SetActive(false);
        plantPage.SetActive(false); // Hide entry pages when changing categories*/
    }
}
