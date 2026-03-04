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
    public static bool isRenamingCritter = false;
    private GameSaveData gameSaveData;
    private CodexEntries[] TutorialEntries, ToolEntries, StructureEntries, PlantEntries, CreatureEntries, BugEntries; //Looks dumb but I need a reference to the SO's cached or else this gets really messy
    private List<CodexEntries> TutorialList, ToolList, StructureList, PlantList, CreatureList, BugList;
    private List<UILerp> buttonLerps = new List<UILerp>();

    public int menuIndex; //0 = closed, 1 = open, 2 = entry page open
    private string defaultName = "???";
    private ControlManager controlManager;
    private QuestManager questManager;
    private List<GameObject> critterObjects = new List<GameObject>();
    private List<GameObject> questObjects = new List<GameObject>();
    private int currentScreenNum = 0;
    private int tutorialScreenNum = 0;
    private int toolsScreenNum = 0;
    private int structuresScreenNum = 0;
    private int plantsScreenNum = 0;
    private int creaturesScreenNum = 0;
    private int bugsScreenNum = 0;
    private int questsScreenNum = 0;
    private bool extraCritterPages = false;
    private bool extraQuestPages = false;
    [SerializeField] private int crittersScreenNum = 0;
    [SerializeField] private ChildActivator childActivator;
    public enum OpenCategory
    {
        Tutorial,
        Tools,
        Structures,
        Plants,
        Creatures,
        Bugs,
        Critters,
        Quests
        
    }
    OpenCategory openCategory;
    [SerializeField] private UIAlphaController bgPanel;
    private Image bgPanelImage;
    [SerializeField] private GameObject codex;
    [SerializeField] private GameObject controlsObject;
    [SerializeField] private UILerp uiLerp;
    [SerializeField] private AudioClip codexOpenSound, codexCloseSound, codexPageTurnSound;
    [SerializeField] private float codexOpenVolume, codexCloseVolume, codexPageTurnVolume;
    [SerializeField] private Button[] categoryButtons;
    [SerializeField] private List<GameObject> containers = new List<GameObject>();
    [SerializeField] private List<GameObject> secondaryContainers = new List<GameObject>();
    [SerializeField] private List<CodexPage> codexPages;
    [SerializeField] private TextMeshProUGUI categoryTitle;
    [SerializeField] private GameObject categoryContainer;
    [SerializeField] private List<Quest> activeQuests = new List<Quest>();
    //[SerializeField] private GameObject tutorialPage, toolPage, plantPage;

    [Header("Caps per Category")]
    [SerializeField] private int maxStandardEntries = 16;
    [SerializeField] private int maxQuestEntries = 10;
    [SerializeField] private int maxCritterEntries = 10;

    [Header("Prefabs")]
    [SerializeField] private GameObject entryButtonPrefab;
    [SerializeField] private GameObject critterButtonPrefab;
    [SerializeField] private GameObject entryButtonHorizontalPrefab;

    [Header("Images")]
    [SerializeField] private List<Image> controllerImages = new List<Image>();
    [SerializeField] private List<Image> keyboardImages = new List<Image>();
    [SerializeField] private List<Sprite> petImages = new List<Sprite>();
    [SerializeField] private List<Sprite> critterImages = new List<Sprite>();
    [SerializeField] private GameObject backControllerObject;
    [SerializeField] private GameObject backKBMObject;
    [SerializeField] private GameObject arrowParent;

    [Header("Override References")]
    [SerializeField] private CodexEntries mandrakeCreatureEntry;
    [SerializeField] private CodexEntries mandrakeCropEntry;
    [SerializeField] private CodexEntries waterGunEntry;
    [SerializeField] private CodexEntries scytheEntry;
    [SerializeField] private CodexEntries bugNetEntry;
    [SerializeField] private CodexEntries nutTesterEntry;
    [SerializeField] private CodexEntries kukriEntry;
    [SerializeField] private CodexEntries pistolEntry;

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

        ResetCodex(true);
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
        if (isRenamingCritter)
        {
            Debug.Log("Currently renaming a critter or pet. Open/Close input will be ignored.");
            return;
        }    

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
        if (isRenamingCritter)
        {
            Debug.Log("Currently renaming a critter or pet. Input will be ignored.");
            return;
        } 
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
                EventSystem.current.SetSelectedGameObject(ReturnFirstActiveChild(containers[(int)openCategory]));
            }
            AudioPoolManager.Instance.PlayClip(codexPageTurnSound, codexPageTurnVolume);
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
                EventSystem.current.SetSelectedGameObject(ReturnFirstActiveChild(containers[(int)openCategory]));
            }
            AudioPoolManager.Instance.PlayClip(codexPageTurnSound, codexPageTurnVolume);
        }
    }

    private void Update()
    {
        //print(EventSystem.current.currentSelectedGameObject);

        if (menuIndex == 0 || menuIndex == 2) return;
        if (!PlayerMovement.isCodexOpen) return;

        if(Input.GetKeyDown(KeyCode.D))
        {
            UpdateSelectedOpenCategory(openCategory, 1);
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            UpdateSelectedOpenCategory(openCategory, -1);
        }

        

        if (ControlManager.isController)
        {

            if(Gamepad.current.rightTrigger.wasPressedThisFrame)
            {
                UpdateSelectedOpenCategory(openCategory, 1);
            }
            else if (Gamepad.current.leftTrigger.wasPressedThisFrame)
            {
                UpdateSelectedOpenCategory(openCategory, -1);
            }

            backControllerObject.SetActive(true);
            backKBMObject.SetActive(false);
            for (int i = 0; i < controllerImages.Count; i++) controllerImages[i].enabled = true;
            for (int i = 0; i < keyboardImages.Count; i++) keyboardImages[i].enabled = false;

            if (menuIndex == 1)
            {
                // Set the selected game object to the first child of the current category container
                // I think this will work idk
                if (EventSystem.current.currentSelectedGameObject == null)
                {
                    if (containers[(int)openCategory].transform.childCount > 0)
                    {
                        if(ControlManager.isGamepad) EventSystem.current.SetSelectedGameObject(ReturnFirstActiveChild(containers[(int)openCategory]));
                    }
                }
            }
        }
        else
        {
            backControllerObject.SetActive(false);
            backKBMObject.SetActive(true);
            for (int i = 0; i < controllerImages.Count; i++) controllerImages[i].enabled = false;
            for (int i = 0; i < keyboardImages.Count; i++) keyboardImages[i].enabled = true;
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
            EventSystem.current.SetSelectedGameObject(ReturnFirstActiveChild(containers[(int)openCategory]));
        }

        Time.timeScale = 0f;
        PlayerMovement.isCodexOpen = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        AudioPoolManager.Instance.PlayClip(codexOpenSound, codexOpenVolume); // Play the codex open sound

        if (Tutorial.Instance) Tutorial.Instance.OpenCodex();
        PopupEvents.current.OpenCodex();
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
        AudioPoolManager.Instance.PlayClip(codexCloseSound, codexCloseVolume); // Play the codex close sound
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
                AudioPoolManager.Instance.PlayClip(codexPageTurnSound, codexPageTurnVolume);
                break;

            default:
                break;
        }
    }

    private void UpdateEntries()
    {
        if (TutorialList != null) ClearCodex(); // Clear the codex before updating entries. Checking the tutorial list should be sufficient.
        print(containers.Count);
        // Run this on Start or when the codex is opened to update the entries
        for (int i = 0; i < 8; i++) // CHANGE THIS IF WE EVER ADD MORE CATEGORIES
        {
            var Cat = TutorialEntries;
            var isQuest = false;
            var isCritter = false;
            Debug.Log(i);
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
                    critterObjects = new List<GameObject>();
                    isCritter = true;
                    Cat = null;
                    break;
                case 7:
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
                int buttonsPlaced = 0;

                for (int e = 0; e < activeQuests.Count; e++)
                {
                    int batchIndex = buttonsPlaced / maxQuestEntries;
                    Transform currentParent = (batchIndex % 2 == 0) ? containers[7].transform : secondaryContainers[7].transform;

                    GameObject entryButton = Instantiate(entryButtonHorizontalPrefab, currentParent.transform);
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
                    questObjects.Add(entryButton);
                    buttonsPlaced++;
                }

                if(activeQuests.Count > maxQuestEntries * 2) extraQuestPages = true;
                else extraQuestPages = false;

                questsScreenNum = activeQuests.Count / (maxQuestEntries * 2); // Calculate the number of screens needed for quests
                HideAndShowEntries(OpenCategory.Quests, maxQuestEntries);
            }

            if (isCritter)
            {
                print("Attempting to load Critter Category");
                int buttonsPlaced = 0;
                //Pet
                PetBehaviorScript pet = gameSaveData.currentPet;
                if (pet != null)
                {
                    var petButton = Instantiate(critterButtonPrefab, containers[6].transform);
                    petButton.name = pet.name + " Pet";

                    var petVars = petButton.GetComponent<CodexCritter>();
                    petVars.assignedPet = pet;

                    petVars.critterName.text = pet.name;
                    petVars.critterIcon.sprite = petImages[(int)pet.petType];
                    petVars.homeIcon.gameObject.SetActive(false); // Hide home icon for pets
                    petVars.friendshipText.text = pet.friendshipLevel.ToString();
                    petVars.healthSlider.transform.parent.gameObject.SetActive(false); // Hide health slider for pets
                    petVars.hungerSlider.value = pet.hunger / pet.maxHunger;
                    petVars.thirstSlider.value = pet.thirst / pet.maxThirst;

                    critterObjects.Add(petButton);
                    buttonsPlaced++;
                }

                //Critters
                List<CritterBehaviorScript> critters = BarnManager.Instance.allCritters;
                if (critters.Count == 0) continue;

                for (int c = 0; c < critters.Count; c++)
                {
                    int batchIndex = buttonsPlaced / maxCritterEntries;
                    Transform currentParent = (batchIndex % 2 == 0) ? containers[6].transform : secondaryContainers[6].transform;
                    Color homelessColor = new Color(1.0f, 1.0f, 1.0f, 0.75f);
                    Color hasHomeColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);

                    var critter = critters[c];
                    if (critter == null) continue;

                    var critterButton = Instantiate(critterButtonPrefab, currentParent.transform);
                    critterButton.name = critter.GetCritterName() + " " + c;
                    var critterVars = critterButton.GetComponent<CodexCritter>();
                    critterVars.assignedCritter = critter;

                    critterVars.critterName.text = critter.GetCritterName();
                    critterVars.critterIcon.sprite = critterImages[(int)critter.critterType];
                    critterVars.homeIcon.color = critter.IsCritterHomeless() ? homelessColor : hasHomeColor;
                    critterVars.homeIcon.gameObject.SetActive(true);
                    critterVars.friendshipText.text = critter.friendshipLevel.ToString();
                    critterVars.healthSlider.value = critter.health / critter.maxHealth;
                    critterVars.hungerSlider.value = critter.hunger / critter.maxHunger;
                    critterVars.thirstSlider.value = critter.thirst / critter.maxThirst;

                    critterObjects.Add(critterButton);
                    buttonsPlaced++;
                }
                if(critterObjects.Count > maxCritterEntries * 2) extraCritterPages = true;
                else extraCritterPages = false;

                crittersScreenNum = critterObjects.Count / (maxCritterEntries * 2); // Calculate the number of screens needed for critters
                HideAndShowEntries(OpenCategory.Critters, maxCritterEntries);
            }

            if (Cat == null) continue;
            // Attempt to load all other categories

            print(Cat.Length + " entries found in category " + i);
            for (int e = 0; e < Cat.Length; e++)
            {
                var chosenContainer = containers[i];
                if (e < maxStandardEntries) chosenContainer = containers[i];
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

    private void UpdateSelectedOpenCategory(OpenCategory cat, int incrementDirection) //Page turning and such left and right
    {
        switch (cat)
        {
            case OpenCategory.Tutorial:
                //tutorialScreenNum += incrementDirection;
                break;

            case OpenCategory.Tools:
                //toolsScreenNum += incrementDirection;
                break;

            case OpenCategory.Structures:
                //structuresScreenNum += incrementDirection;
                break;

            case OpenCategory.Plants:
                //plantsScreenNum += incrementDirection;
                break;

            case OpenCategory.Creatures:
                //creaturesScreenNum += incrementDirection;
                break;

            case OpenCategory.Bugs:
                //bugsScreenNum += incrementDirection;
                break;

            case OpenCategory.Quests:
                if (!AreThereEnoughPages(questsScreenNum, incrementDirection)) return;
                currentScreenNum += incrementDirection;
                HideAndShowEntries(cat, maxQuestEntries);
                break;

            case OpenCategory.Critters:
                if (!AreThereEnoughPages(crittersScreenNum, incrementDirection)) return;
                currentScreenNum += incrementDirection;
                HideAndShowEntries(cat, maxCritterEntries);
                break;
        }
    }

    private bool AreThereEnoughPages(int maxScreenNum, int incrementDirection)
    {
        // Check if there are enough pages to display
        if (incrementDirection == 1)
        {
            if (currentScreenNum == maxScreenNum) return false;
            else return true;
        }
        else if (incrementDirection == -1)
        {
            if (currentScreenNum == 0) return false;
            else return true;
        }
        Debug.LogError("Invalid increment direction: " + incrementDirection);
        return false; // If we get here there is a problem please help
    }

    private void HideAndShowEntries(OpenCategory cat, int maxPerScreen) // Currently only used for critters because this is a mess
    {
        var catInt = (int)cat;

        if(cat.Equals(OpenCategory.Quests))
        {
            for (int i = 0; i < questObjects.Count; i++)
            {
                if (i >= currentScreenNum * (maxPerScreen * 2) && i < (currentScreenNum + 1) * (maxPerScreen * 2))
                {
                    questObjects[i].SetActive(true);
                }
                else
                {
                    questObjects[i].SetActive(false);
                }
            }
            if(ControlManager.isGamepad) EventSystem.current.SetSelectedGameObject(null);
            return;
        }
        else if(cat.Equals(OpenCategory.Critters))
        {
           for (int i = 0; i < critterObjects.Count; i++)
            {
                if (i >= currentScreenNum * (maxPerScreen * 2) && i < (currentScreenNum + 1) * (maxPerScreen * 2))
                {
                    critterObjects[i].SetActive(true);
                }
                else
                {
                    critterObjects[i].SetActive(false);
                }
            }
            if(ControlManager.isGamepad) EventSystem.current.SetSelectedGameObject(null);
        }
        
    }
    

    private void ClearCodex()
    {
        // Clear all the entries in the codex
        for (int i = 0; i < containers.Count; i++)
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

        tutorialScreenNum = 0;
        toolsScreenNum = 0;
        structuresScreenNum = 0;
        plantsScreenNum = 0;
        creaturesScreenNum = 0;
        bugsScreenNum = 0;
        questsScreenNum = 0;
        crittersScreenNum = 0;

        TutorialList.Clear();
        ToolList.Clear();
        StructureList.Clear();
        CreatureList.Clear();
        PlantList.Clear();
        BugList.Clear();
        critterObjects.Clear();
        questObjects.Clear();
    }

    private void ResetCodex(bool fullReset = false) //Sets the codex to its default state
    {
        print("Resetting Codex to default state and updating entries.");
        activeQuests = questManager.activeQuests;
        OverrideEntries(); // Override unlocks for specific entries
        UpdateEntries();
        if(fullReset) ChangeCategory("Tutorial"); // Start with the Tutorial category open
        else ChangeCategory(openCategory.ToString()); // Keep the current category open
        

        
        menuIndex = 1;
    }

    public void ChangeCategory(string categoryToOpen)
    {
        CloseEntryPages();
        currentScreenNum = 0;

        // Change the open category based on the index of the button pressed
        openCategory = (OpenCategory)Enum.Parse(typeof(OpenCategory), categoryToOpen); //Wow
        var catInt = (int)openCategory;
        print("Changing category to: " + openCategory.ToString());

        for (int i = 0; i < categoryButtons.Length; i++) //help
        {
            if (i == catInt) buttonLerps[i].lerpToStart = false;
            else buttonLerps[i].lerpToStart = true;
        }

        for (int i = 0; i < containers.Count; i++)
        {
            containers[i].SetActive(i == catInt); //i is true when i = categoryIndex. Did not know I could do this lol
            secondaryContainers[i].SetActive(i == catInt);
        }
        categoryTitle.text = openCategory.ToString(); // Update the category title

        if (openCategory == OpenCategory.Quests)
        {
            HideAndShowEntries(OpenCategory.Quests, maxQuestEntries);
            arrowParent.SetActive(extraQuestPages);
        }
        else if (openCategory == OpenCategory.Critters)
        {
            HideAndShowEntries(OpenCategory.Critters, maxCritterEntries);
            arrowParent.SetActive(extraCritterPages);
        }
        else
        {
            arrowParent.SetActive(false);
        }
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
        nutTesterEntry.unlocked = gameSaveData.testerObtained;
        kukriEntry.unlocked = gameSaveData.kukriObtained;
        pistolEntry.unlocked = gameSaveData.pistolObtained;

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

    private GameObject ReturnFirstActiveChild(GameObject parent)
    {

        for (int i = 0; i < parent.transform.childCount; i++)
        {
            if(parent.transform.GetChild(i).gameObject.activeSelf == true)
            {
                return parent.transform.GetChild(i).gameObject;
            }
        }
        return null;
    }
}
