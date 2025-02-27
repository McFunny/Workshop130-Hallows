using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CodexRework : MonoBehaviour
{
    CodexEntries[] CurrentCategory, CreatureEntries, ToolEntries, GettingStarted, PlantEntries; //, QuestEntries;
    public CodexEntries currentEntry;
    public Quest currentQuest;
    [SerializeField] private GameObject codex, gridContentObject, horizontalContentObject;
    [SerializeField] private TextMeshProUGUI nameText, horizontalEntryName, horizontalDescriptionText, descriptionText, largeDescriptionText, pageNumberText, contentsText;
    [SerializeField] private int currentPage = 0;

    [SerializeField] private GameObject entryButton, horizontalEntryButton, grid, horizontal;

    string defaultName = "???";
    string defaultDesc = "There is more to discover...";
    ControlManager controlManager;
    [SerializeField] private Image largeImage, smallImage;
    [SerializeField] private GameObject defaultButton;
    [SerializeField] private Button[] categoryButtons;
    private Button currentCategoryButton;
    [SerializeField] private List<GameObject> categoryList;
    bool isGridCategory, isQuestCategoty;
    private QuestManager questManager;
    public List<Quest> activeQuests = new List<Quest>();

    void Awake()
    {
        controlManager = FindFirstObjectByType<ControlManager>();
        questManager = FindFirstObjectByType<QuestManager>();
    }

    void Start()
    {
        CreatureEntries = Resources.LoadAll<CodexEntries>("Codex/Creatures/");
        ToolEntries = Resources.LoadAll<CodexEntries>("Codex/Tools/");
        GettingStarted = Resources.LoadAll<CodexEntries>("Codex/GettingStarted/");
        PlantEntries = Resources.LoadAll<CodexEntries>("Codex/Plants/");
        //QuestEntries = Resources.LoadAll<CodexEntries>("Codex/Quests/");
        CurrentCategory = GettingStarted;
        contentsText.text = "Getting Started";
        currentEntry = null;

        nameText.text = "";     //CurrentCategory[0].entryName;
        descriptionText.text = "";    //CurrentCategory[0].description[0];
        largeDescriptionText.text = "";
        pageNumberText.text = "";
        horizontalEntryName.text = "";
        horizontalDescriptionText.text = "";

        codex.SetActive(false);

        //PopulateCodex();
    }

    void OnEnable()
    {
        controlManager.openCodex.action.started += OpenCodexPressed;
        //controlManager.closeCodex.action.started += CloseCodexPressed;
    }

    void OnDisable()
    {
        controlManager.openCodex.action.started -= OpenCodexPressed;
        //controlManager.closeCodex.action.started -= CloseCodexPressed;
    }

    void Update()
    {
        //print(EventSystem.current.currentSelectedGameObject);
        activeQuests = questManager.activeQuests;

        if(codex.activeInHierarchy && EventSystem.current.currentSelectedGameObject == null && ControlManager.isController)
        {
            EventSystem.current.SetSelectedGameObject(defaultButton);
        }

        if(codex.activeInHierarchy)
        {
            if(controlManager.codexPageUp.action.WasPressedThisFrame())
            {
                UpdatePage(-1, currentEntry, false);
            }
            else if (controlManager.codexPageDown.action.WasPressedThisFrame())
            {
                UpdatePage(1, currentEntry, false);
            }
        }

        PlayerMovement.isCodexOpen = codex.activeInHierarchy;
    }

    void OpenCodexPressed(InputAction.CallbackContext obj)
    {  
        if(codex.activeInHierarchy)
        {
            print("Closing");
            if(codex.activeSelf){OpenCloseCodex();}
        }
    }

    void CloseCodexPressed(InputAction.CallbackContext obj) // This doesn't do anything for some reason
    {
        if(codex.activeInHierarchy)
        {
            OpenCloseCodex();
        } 
    }

    public void OpenCloseCodex()
    {
        print("Codex Opened");
        currentPage = 0;
        CurrentCategory = GettingStarted;
        nameText.text = "";
        descriptionText.text = "";
        largeDescriptionText.text = "";
        horizontalEntryName.text = "";
        horizontalDescriptionText.text = "";
        pageNumberText.text = "";
        isGridCategory = false;
        PopulateCodex(); 
        ImageCheck();

        codex.SetActive(!codex.activeInHierarchy);

        if(!codex.activeInHierarchy)
        {
            ClearCodex();
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(defaultButton);
        }
    }
    public void UpdatePage(int page, CodexEntries entry, bool reset)
    {
        if (!reset) currentPage = currentPage + page;
        else currentPage = page;
        currentPage = Mathf.Clamp(currentPage,0,entry.description.Length - 1);

        if(currentPage == 0) ImageCheck();
        else 
        {
            largeImage.gameObject.SetActive(false);
            descriptionText.gameObject.SetActive(false);
            largeDescriptionText.gameObject.SetActive(true);
        }      

        pageNumberText.text = "Page " + (currentPage + 1) + "/" + entry.description.Length;
        if (entry.unlocked == true)
        {
            nameText.text = entry.entryName;
            descriptionText.text = entry.description[currentPage];
            largeDescriptionText.text = entry.description[currentPage];
            horizontalEntryName.text = entry.entryName;
            horizontalDescriptionText.text = entry.description[currentPage];
        }
        else
        {
            nameText.text = defaultName;
            descriptionText.text = defaultDesc;
            largeDescriptionText.text = defaultDesc;
            horizontalEntryName.text = defaultName;
            horizontalDescriptionText.text = defaultDesc;
        }

        //if(entry.description.Length == 1) pageNumberText.gameObject.SetActive(false); //Fix this later. Not very important :/
        //else pageNumberText.gameObject.SetActive(true);                 
    }

    public void ChangeCategory(int cat)
    {
        switch (cat)
        {
            case 0:
            CurrentCategory = GettingStarted;
            currentPage = 0;
            isGridCategory = false;
            isQuestCategoty = false;
            contentsText.text = "Getting Started";
            UpdatePage(0, CurrentCategory[0], true);
            break;

            case 1:
            CurrentCategory = ToolEntries;
            currentPage = 0;
            isGridCategory = true;
            isQuestCategoty = false;
            contentsText.text = "Tools";
            UpdatePage(0, CurrentCategory[0], true);
            break;

            case 2:
            CurrentCategory = PlantEntries;
            currentPage = 0;
            isGridCategory = true;
            isQuestCategoty = false;
            contentsText.text = "Plants";
            UpdatePage(0, CurrentCategory[0], true);
            break;

            case 3:
            CurrentCategory = CreatureEntries;
            currentPage = 0;
            isGridCategory = true;
            isQuestCategoty = false;
            contentsText.text = "Creatures";
            UpdatePage(0, CurrentCategory[0], true);
            break;

            case 4:
            CurrentCategory = null;
            currentPage = 0;
            isGridCategory = false;
            isQuestCategoty = true;
            contentsText.text = "Quests";
            //UpdatePage(0, CurrentCategory[0], true);
            break;

            default:
            CurrentCategory = GettingStarted;
            currentPage = 0;
            isGridCategory = false;
            UpdatePage(0, CurrentCategory[0], true);
            print("су́ка блядь! Category not found! Defaulting to GettingStarted... блядь...");
            break;
        }

        //currentCategoryButton = categoryButtons[cat];

        for(int i = 0; i < categoryList.Count; i++)
        {
            Destroy(categoryList[i].gameObject);
        }

        ClearCodex();
        categoryList.Clear();
        PopulateCodex();
        print(CurrentCategory);
    }

    void PopulateCodex()
    {
        if(isGridCategory)
        {
            grid.SetActive(true);
            horizontal.SetActive(false);

            for (int i = 0; i < CurrentCategory.Length; i++)
            {
                var tempButton = Instantiate(entryButton, gridContentObject.transform, worldPositionStays:false);
                var tempName = tempButton.gameObject.transform.GetChild(0).gameObject;
                var tempImage = tempButton.gameObject.transform.GetChild(1).gameObject;
                var tempUnlock = tempButton.gameObject.transform.GetChild(2).gameObject;

                var tempID = tempButton.GetComponent<CodexButtonID>();
                var tempText = tempName.GetComponent<TextMeshProUGUI>();
                var tempSprite = tempImage.GetComponent<Image>();

                tempButton.name = "GridButton" + i;

                tempID.assignedEntry = CurrentCategory[i];

                if(CurrentCategory[i].unlocked)
                {
                    tempText.text = CurrentCategory[i].entryName;
                    tempImage.SetActive(true);
                    tempUnlock.SetActive(false);
                    tempSprite.sprite = CurrentCategory[i].buttonIcon;
                }
                else
                {
                    tempText.text = defaultName;
                    tempImage.SetActive(false);
                    tempUnlock.SetActive(true);
                }
                categoryList.Add(tempButton);
            }
        }
        else if (!isQuestCategoty)
        {
            grid.SetActive(false);
            horizontal.SetActive(true);

            for (int i = 0; i < CurrentCategory.Length; i++)
            {
                if(!CurrentCategory[i].unlocked) {continue;}
                
                var tempButton = Instantiate(horizontalEntryButton, horizontalContentObject.transform, worldPositionStays:false);
                var button = tempButton.gameObject.transform.GetChild(0).gameObject;
                var tempName = tempButton.gameObject.transform.GetChild(1).gameObject;
                var tempID = tempButton.GetComponent<CodexButtonID>();
                var tempText = tempName.GetComponent<TextMeshProUGUI>();

                button.name = "HorizontalButton" + i;


                tempText.text = CurrentCategory[i].entryName;

                tempID.assignedEntry = CurrentCategory[i];

                categoryList.Add(tempButton);
            }
        }
        else
        {
            for (int i = 0; i < activeQuests.Count; i++)
            {
                var tempButton = Instantiate(horizontalEntryButton, horizontalContentObject.transform, worldPositionStays:false);
                var button = tempButton.gameObject.transform.GetChild(0).gameObject;
                var tempName = tempButton.gameObject.transform.GetChild(1).gameObject;
                var tempID = tempButton.GetComponent<CodexButtonID>();
                var tempText = tempName.GetComponent<TextMeshProUGUI>();

                button.name = "QuestButton" + i;

                tempText.text = activeQuests[i].name;
                tempID.assignedQuest = activeQuests[i];

                categoryList.Add(tempButton);
            }
            return;
        }
            
        print(categoryList.Count);
        currentEntry = null;
        for(int i = 0; i < categoryList.Count; i++)
        {
            var temp = categoryList[i].GetComponent<CodexButtonID>();
            //print(temp.assignedEntry.entryName);
            if(temp.assignedEntry.unlocked) 
            {
                HasUnlockedEntry(temp.assignedEntry);
                print("Unlocked Entry Found");
                break;
            }
        }
        if(currentEntry == null) 
        {
            largeImage.gameObject.SetActive(false);
            nameText.text = defaultName;
            largeDescriptionText.text = defaultDesc;
            horizontalEntryName.text = defaultName;
            horizontalDescriptionText.text = defaultDesc;
            descriptionText.text = defaultDesc;
            largeImage.gameObject.SetActive(false);
            descriptionText.gameObject.SetActive(false);
            largeDescriptionText.gameObject.SetActive(true);
            largeImage.sprite = null;
            smallImage.sprite = null;
            pageNumberText.text = "";
            UpdateNavigation();
            return;
        }

        nameText.text = currentEntry.entryName;     //CurrentCategory[0].entryName;
        descriptionText.text = currentEntry.description[0];     //CurrentCategory[0].description[0];
        largeDescriptionText.text = currentEntry.description[0];
        horizontalEntryName.text = currentEntry.entryName;
        horizontalDescriptionText.text = currentEntry.description[0];
        pageNumberText.text = "Page 1" + "/" + currentEntry.description.Length;
        
        ImageCheck();
        UpdateNavigation();
    }

    void ClearCodex()
    {
        for(int i = 0; i < categoryList.Count; i++)
        {
            Destroy(categoryList[i].gameObject);
        }
        categoryList.Clear();
    }

    void ImageCheck()
    {
        if (currentEntry == null)
        {
            largeImage.gameObject.SetActive(false);
            descriptionText.gameObject.SetActive(false);
            largeDescriptionText.gameObject.SetActive(true);
            largeImage.sprite = null;
            smallImage.sprite = null;
            return;
        }

        if(currentEntry.mainImage != null && currentEntry.unlocked)
        {
            if(isGridCategory)
            {
                largeImage.sprite = currentEntry.mainImage;
                largeImage.gameObject.SetActive(true);
                descriptionText.gameObject.SetActive(true);
                largeDescriptionText.gameObject.SetActive(false);
            }
            else
            {
                largeImage.gameObject.SetActive(false);
                descriptionText.gameObject.SetActive(false);
                largeDescriptionText.gameObject.SetActive(true);
                smallImage.gameObject.SetActive(true);
                smallImage.sprite = currentEntry.mainImage;
            } 
        }
        else
        {
            descriptionText.gameObject.SetActive(false);
            largeDescriptionText.gameObject.SetActive(true);
            largeImage.gameObject.SetActive(false);
            smallImage.gameObject.SetActive(false);
            largeImage.sprite = null;
            smallImage.sprite = null;
        }
    }

    void HasUnlockedEntry(CodexEntries entry)
    {
        currentEntry = entry;
    }

    void UpdateNavigation() //Why cant unity let me change ONE THING with menu navigation without changing EVERYTHING about it I HATE UNITY
    {
        Navigation startNav = new Navigation(); // This fucking sucks dude I was almost finished with controller navigation for the Codex
        Navigation toolNav = new Navigation();  // I have no idea what I'm doing with this man I hate unity
        Navigation plantNav = new Navigation(); // I could be playing Trials of Osiris rn getting stomped by a 0.6kd bubble Titan but NOOOOO
        Navigation creatureNav = new Navigation(); // Fuck my stupid chungus baka life
        Navigation questNav = new Navigation(); // I blame Cam btw

        startNav.mode = Navigation.Mode.Explicit;
        startNav.selectOnUp = categoryButtons[4];
        startNav.selectOnDown = categoryButtons[1];
        if(isGridCategory){startNav.selectOnRight = categoryList[0].GetComponent<Button>();}
        else{startNav.selectOnRight = categoryList[0].GetComponentInChildren<Button>();}

        toolNav.mode = Navigation.Mode.Explicit;
        toolNav.selectOnUp = categoryButtons[0];
        toolNav.selectOnDown = categoryButtons[2];
        if(isGridCategory){toolNav.selectOnRight = categoryList[0].GetComponent<Button>();}
        else{toolNav.selectOnRight = categoryList[0].GetComponentInChildren<Button>();}

        plantNav.mode = Navigation.Mode.Explicit;
        plantNav.selectOnUp = categoryButtons[1];
        plantNav.selectOnDown = categoryButtons[3];
        if(isGridCategory){plantNav.selectOnRight = categoryList[0].GetComponent<Button>();}
        else{plantNav.selectOnRight = categoryList[0].GetComponentInChildren<Button>();}

        creatureNav.mode = Navigation.Mode.Explicit;
        creatureNav.selectOnUp = categoryButtons[2];
        creatureNav.selectOnDown = categoryButtons[4];
        if(isGridCategory){creatureNav.selectOnRight = categoryList[0].GetComponent<Button>();}
        else{creatureNav.selectOnRight = categoryList[0].GetComponentInChildren<Button>();}

        questNav.mode = Navigation.Mode.Explicit;
        questNav.selectOnUp = categoryButtons[3];
        questNav.selectOnDown = categoryButtons[0];
        if(isGridCategory){questNav.selectOnRight = categoryList[0].GetComponent<Button>();}
        else{questNav.selectOnRight = categoryList[0].GetComponentInChildren<Button>();}

        categoryButtons[0].navigation = startNav;
        categoryButtons[1].navigation = toolNav;
        categoryButtons[2].navigation = plantNav;
        categoryButtons[3].navigation = creatureNav;
        categoryButtons[4].navigation = questNav;
    }
}
