using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;

public class Codex3 : MonoBehaviour
{
    public System.Action onCodexClosed;
    private GameSaveData gameSaveData;
    private CodexEntries[] TutorialEntries, ToolEntries, StructureEntries, PlantEntries, CreatureEntries, BugEntries; //Looks dumb but I need a reference to the SO's cached or else this gets really messy
    private List<CodexEntries> TutorialList, ToolList, StructureList, PlantList, CreatureList, BugList;
    private List<UILerp> buttonLerps = new List<UILerp>();

    public int menuIndex; //0 = closed, 1 = open, 2 = entry page open
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
        Quests,
    }
    OpenCategory openCategory;
    [SerializeField] private UIAlphaController bgPanel;
    private Image bgPanelImage;
    [SerializeField] private GameObject codex;
    [SerializeField] private UILerp uiLerp;
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

    [Header("Images")]
    [SerializeField] private List<Image> controllerImages = new List<Image>();
    [SerializeField] private GameObject backControllerObject;
    [SerializeField] private GameObject backKBMObject;

    [Header("Override References")]
    [SerializeField] private CodexEntries mandrakeCreatureEntry;
    [SerializeField] private CodexEntries mandrakeCropEntry;
    [SerializeField] private CodexEntries waterGunEntry;
    [SerializeField] private CodexEntries scytheEntry;
    [SerializeField] private CodexEntries bugNetEntry;

    private void Awake()
    {
        controlManager = FindFirstObjectByType<ControlManager>();
        childActivator = GetComponentInChildren<ChildActivator>();
        questManager = FindAnyObjectByType<QuestManager>();
        gameSaveData = FindFirstObjectByType<GameSaveData>();
        bgPanelImage = bgPanel.GetComponent<Image>();

        /*if (childActivator != null)
        {
            childActivator.onChildActivated += ResetCodex; // Reset the codex when the child is activated
        }*/

        for (int i = 0; i < categoryButtons.Length; i++)
        {
            buttonLerps.Add(categoryButtons[i].GetComponent<UILerp>());
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        uiLerp.lerpToStart = false;
        TutorialEntries = Resources.LoadAll<CodexEntries>("Codex/GettingStarted/");
        ToolEntries = Resources.LoadAll<CodexEntries>("Codex/Tools/");
        StructureEntries = Resources.LoadAll<CodexEntries>("Codex/Structures");
        CreatureEntries = Resources.LoadAll<CodexEntries>("Codex/Creatures/");
        PlantEntries = Resources.LoadAll<CodexEntries>("Codex/Plants/");
        BugEntries = Resources.LoadAll<CodexEntries>("Codex/Bugs");
        openCategory = OpenCategory.Tutorial;

        ResetCodex();
        codex.SetActive(false);
        bgPanelImage.raycastTarget = false; // Disable raycasting on the background panel
        menuIndex = 0;
    }

    private void OnEnable()
    {
        controlManager.backCodex.action.started += InputBack;
        controlManager.hotbarUp.action.started += InputDown;
        controlManager.hotbarDown.action.started += InputUp;
        controlManager.codexOpen.action.started += InputOpen;
    }

    private void OnDisable()
    {
        controlManager.backCodex.action.started -= InputBack;
        controlManager.hotbarUp.action.started -= InputDown;
        controlManager.hotbarDown.action.started -= InputUp;
        controlManager.codexOpen.action.started -= InputOpen;
    }

    private void InputOpen(InputAction.CallbackContext context)
    {
        if (menuIndex > 0 && !ControlManager.isController)
        {
            CloseCodex();
            return;
        }

        if (menuIndex != 0) return;
        if (PauseScript.isPaused) return;
        if (PlayerMovement.restrictMovementTokens != 0) return;
        if (PlayerMovement.accessingInventory) return;

        OpenCodex();
    }

    private void InputBack(InputAction.CallbackContext context)
    {
        Back();
    }

    private void InputUp(InputAction.CallbackContext context)
    {
        if (menuIndex > 0) // If the codex is open
        {
            int currentIndex = (int)openCategory;
            currentIndex = currentIndex + 1;
            if (currentIndex == categoryButtons.Length) currentIndex = 0; // Wrap around to the first category if at the last one

            OpenCategory c = (OpenCategory)currentIndex;
            ChangeCategory(c.ToString());

            EventSystem.current.SetSelectedGameObject(null);
            if (containers[(int)openCategory].transform.childCount > 0)
            {
                EventSystem.current.SetSelectedGameObject(containers[(int)openCategory].transform.GetChild(0).gameObject);
            }
        }
    }

    private void InputDown(InputAction.CallbackContext context)
    {
        if (menuIndex > 0) // If the codex is open
        {
            int currentIndex = (int)openCategory;
            currentIndex = currentIndex - 1;
            if (currentIndex < 0) currentIndex = categoryButtons.Length - 1; // Wrap around to the last category if at the first one

            OpenCategory c = (OpenCategory)currentIndex;
            ChangeCategory(c.ToString());

            EventSystem.current.SetSelectedGameObject(null);
            if (containers[(int)openCategory].transform.childCount > 0)
            {
                EventSystem.current.SetSelectedGameObject(containers[(int)openCategory].transform.GetChild(0).gameObject);
            }
        }
    }

    private void Update()
    {
        //print(EventSystem.current.currentSelectedGameObject);

        if (menuIndex == 0 || menuIndex == 2) return;
        if (!PlayerMovement.isCodexOpen) return;

        if (ControlManager.isController)
        {
            backControllerObject.SetActive(true);
            backKBMObject.SetActive(false);
            for (int i = 0; i < controllerImages.Count; i++) controllerImages[i].enabled = true;

            if (menuIndex == 1)
            {
                // Set the selected game object to the first child of the current category container
                // I think this will work idk
                if (EventSystem.current.currentSelectedGameObject == null)
                {
                    if (containers[(int)openCategory].transform.childCount > 0)
                    {
                        EventSystem.current.SetSelectedGameObject(containers[(int)openCategory].transform.GetChild(0).gameObject);
                    }
                }
            }
        }
        else
        {
            backControllerObject.SetActive(false);
            backKBMObject.SetActive(true);
            for (int i = 0; i < controllerImages.Count; i++) controllerImages[i].enabled = false;
        }
    }

    public void OpenCodex()
    {
        uiLerp.lerpToStart = true; // Start the UI lerp animation
        bgPanelImage.raycastTarget = true; // Enable raycasting on the background panel
        menuIndex = 1;
        codex.SetActive(true);
        ResetCodex();

        if (ControlManager.isController && containers[(int)openCategory].transform.childCount > 0)
        {
            EventSystem.current.SetSelectedGameObject(containers[(int)openCategory].transform.GetChild(0).gameObject);
        }

        Time.timeScale = 0f;
        PlayerMovement.isCodexOpen = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if(Tutorial.Instance) Tutorial.Instance.OpenCodex();
    }

    public void CloseCodex()
    {
        onCodexClosed?.Invoke();
        bgPanelImage.raycastTarget = false; // Enable raycasting on the background panel
        uiLerp.lerpToStart = false; // Reverse the UI lerp animation
        menuIndex = 0;

        EventSystem.current.SetSelectedGameObject(null);

        Time.timeScale = 1f;
        PlayerMovement.isCodexOpen = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Back()
    {
        switch (menuIndex)
        {
            case 0:
                break;

            case 1:
                CloseCodex();
                break;

            case 2:
                UpdateEntries();
                if (ControlManager.isController) EventSystem.current.SetSelectedGameObject(null);
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

                    var type = activeQuests[e].GetType();

                    print(type);
                    if (type.Equals(typeof(FetchQuest)))
                    {
                        //print("Fetch Quest");
                        var q = activeQuests[e] as FetchQuest;
                        var t = q.name;

                        if (q.amount > 1 && !q.desiredItem.displayName.EndsWith("s")) t = t.Replace("{itemName}", q.desiredItem.displayName.ToString() + "s");
                        else t = t.Replace("{itemName}", q.desiredItem.displayName.ToString());

                        t = t.Replace("{itemAmount}", q.amount.ToString());

                        questTitle.text = t;
                    }
                    else if (type.Equals(typeof(HuntQuest)))
                    {
                        //print("Hunt Quest");
                        var q = activeQuests[e] as HuntQuest;
                        var t = q.name;

                        if (q.amount > 1 && !q.targetCreature.name.EndsWith("s")) t = t.Replace("{creatureName}", q.targetCreature.name.ToString() + "s");
                        else t = t.Replace("{creatureName}", q.targetCreature.name.ToString());

                        t = t.Replace("{creatureAmount}", q.amount.ToString());

                        questTitle.text = t;
                    }
                    else if (type.Equals(typeof(GrowQuest)))
                    {
                        //print("Grow Quest");
                        var q = activeQuests[e] as GrowQuest;
                        var t = q.name;

                        if (q.amount > 1 && !q.desiredItem.displayName.EndsWith("s")) t = t.Replace("{itemName}", q.desiredItem.displayName.ToString() + "s");
                        else t = t.Replace("{itemName}", q.desiredItem.displayName.ToString());

                        t = t.Replace("{itemAmount}", q.amount.ToString());

                        questTitle.text = t;
                    }
                    else
                    {
                        questTitle.text = activeQuests[e].name;
                    }

                    if (!activeQuests[e].alreadyCompleted) questTitle.text = questTitle.text;
                    else questTitle.text = "<s>" + questTitle.text + "</s>";

                    buttonScript.assignedQuest = activeQuests[e];
                }
            }

            if (Cat == null) continue;
            // Attempt to load all other categories

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
                else if (Cat[e].bugData != null) //Unlocks if amount of bug caught > 0
                {
                    if (Cat[e].bugData.amountCaught > 0)
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
                else if (Cat[e].structureData != null) //Unlocks if structure has been placed
                {
                    if (Cat[e].structureData.hasBeenPlaced)
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
        Canvas.ForceUpdateCanvases();
        menuIndex = 2;
    }

    public void UpdateQuest(Quest quest)
    {
        codexPages[(int)OpenCategory.Quests].UpdatePage(null, quest);

        categoryContainer.SetActive(false);
        codexPages[(int)OpenCategory.Quests].gameObject.SetActive(true);
        Canvas.ForceUpdateCanvases();
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

            foreach (Transform child in secondaryContainers[i].transform)
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
        OverrideEntries(); // Override unlocks for specific entries
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

        for (int i = 0; i < categoryButtons.Length; i++) //help
        {
            if (i == catInt) buttonLerps[i].lerpToStart = false;
            else buttonLerps[i].lerpToStart = true;
        }

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
    }

    private void OverrideEntries()
    {
        waterGunEntry.unlocked = gameSaveData.watergunObtained;
        bugNetEntry.unlocked = gameSaveData.bugNetObtained;
        scytheEntry.unlocked = gameSaveData.scytheObtained;

        if (mandrakeCreatureEntry.creatureData.amountKilled > 0)
        {
            mandrakeCreatureEntry.unlocked = true;
            mandrakeCropEntry.unlocked = true;
        }
        else
        {
            mandrakeCreatureEntry.unlocked = false;
            mandrakeCropEntry.unlocked = false;
        }    
    }
}
