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
    [SerializeField] private GameObject codex, gridContentObject, horizontalContentObject, questContentObject;
    [SerializeField] private TextMeshProUGUI nameText, horizontalEntryName, horizontalDescriptionText, descriptionText, largeDescriptionText, pageNumberText, contentsText, questNameText, questDescriptionText, questProgressText, questCompleteText;
    [SerializeField] private int currentPage = 0;

    [SerializeField] private GameObject entryButton, horizontalEntryButton, grid, horizontal, questObj;

    string defaultName = "???";
    string defaultDesc = "There is more to discover...";
    ControlManager controlManager;
    [SerializeField] private Image largeImage, smallImage;
    [SerializeField] private GameObject defaultButton;
    [SerializeField] private Button[] categoryButtons;
    private Button currentCategoryButton;
    [SerializeField] private List<GameObject> categoryList;
    bool isGridCategory, isQuestCategory;
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

        if(codex.activeInHierarchy && !isQuestCategory)
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
        codex.SetActive(!codex.activeInHierarchy);

        if(!codex.activeInHierarchy)
        {
            ClearCodex();
        }
        else
        {
            print("Codex Opened");
            ChangeCategory(0);

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
            SetTextToDefault();
        }

        //if(entry.description.Length == 1) pageNumberText.gameObject.SetActive(false); //Fix this later. Not very important :/
        //else pageNumberText.gameObject.SetActive(true);                 
    }

    public void UpdateQuests(Quest quest)
    {
        nameText.text = quest.name;

        questNameText.text = quest.assignee.ToString();
        questNameText.text = questNameText.text.Replace("Null", "Task");

        questDescriptionText.text = quest.description;

        largeImage.gameObject.SetActive(false);
        descriptionText.gameObject.SetActive(true);
        largeDescriptionText.gameObject.SetActive(false);
        smallImage.gameObject.SetActive(true);
        //smallImage.sprite = currentEntry.mainImage;

        var type = quest.GetType();

        if(type.Equals(typeof(FetchQuest)))
        {
            var q = quest as FetchQuest;
            var t = q.description;

            if (q.amount > 1) t = t.Replace("{itemName}", q.desiredItem.displayName.ToString() + "s");
            else t = t.Replace("{itemName}", q.desiredItem.name.ToString());

            t = t.Replace("{itemAmount}", q.amount.ToString());

            questDescriptionText.text = t;
            questProgressText.text = q.progress + "/" + q.maxProgress;
            questProgressText.text = q.desiredItem.displayName + " handed in: " + q.progress + "/" + q.maxProgress;
        }
        if(type.Equals(typeof(HuntQuest)))
        {
            var q = quest as HuntQuest;
            var t = q.description;

            if (q.amount > 1) t = t.Replace("{itemName}", q.targetCreature.name.ToString() + "s");
            else t = t.Replace("{itemName}", q.targetCreature.name.ToString());

            t = t.Replace("{itemAmount}", q.amount.ToString());

            questDescriptionText.text = t;
            questProgressText.text = q.targetCreature.name + " eliminited: " + q.progress + "/" + q.maxProgress;
        }
        if(type.Equals(typeof(GrowQuest)))
        {
            var q = quest as GrowQuest;
            var t = q.description;

            if (q.amount > 1) t = t.Replace("{itemName}", q.desiredItem.displayName.ToString() + "s");
            else t = t.Replace("{itemName}", q.desiredItem.name.ToString());

            t = t.Replace("{itemAmount}", q.amount.ToString());

            questDescriptionText.text = t;
            questProgressText.text = q.progress + "/" + q.maxProgress;
            questProgressText.text = q.desiredItem.displayName + " handed in: " + q.progress + "/" + q.maxProgress;
        }

        if(quest.progress >= quest.maxProgress && quest.alreadyCompleted != true) questCompleteText.text = "Return to " + quest.assignee;
        else if (quest.alreadyCompleted == true) questCompleteText.text = "Completed";
        else questCompleteText.text = "";

        //print(type);
    }

    private void SetTextToDefault()
    {
        nameText.text = defaultName;
        descriptionText.text = defaultDesc;
        largeDescriptionText.text = defaultDesc;
        horizontalEntryName.text = defaultName;
        horizontalDescriptionText.text = defaultDesc;
    }

    private void NoEntries()
    {
        largeImage.gameObject.SetActive(false);
        smallImage.gameObject.SetActive(false);
        SetTextToDefault();
        descriptionText.gameObject.SetActive(false);
        largeDescriptionText.gameObject.SetActive(true);
        largeImage.sprite = null;
        smallImage.sprite = null;
        pageNumberText.text = "";
        UpdateNavigation();
        return;
    }

    public void ChangeCategory(int cat)
    {
        switch (cat)
        {
            case 0:
            CurrentCategory = GettingStarted;
            currentPage = 0;
            isGridCategory = false;
            isQuestCategory = false;
            contentsText.text = "Getting Started";
            UpdatePage(0, CurrentCategory[0], true);
            break;

            case 1:
            CurrentCategory = ToolEntries;
            currentPage = 0;
            isGridCategory = true;
            isQuestCategory = false;
            contentsText.text = "Tools";
            UpdatePage(0, CurrentCategory[0], true);
            break;

            case 2:
            CurrentCategory = PlantEntries;
            currentPage = 0;
            isGridCategory = true;
            isQuestCategory = false;
            contentsText.text = "Plants";
            UpdatePage(0, CurrentCategory[0], true);
            break;

            case 3:
            CurrentCategory = CreatureEntries;
            currentPage = 0;
            isGridCategory = true;
            isQuestCategory = false;
            contentsText.text = "Creatures";
            UpdatePage(0, CurrentCategory[0], true);
            break;

            case 4:
            CurrentCategory = null;
            currentPage = 0;
            isGridCategory = false;
            isQuestCategory = true;
            contentsText.text = "Quests";
            if(activeQuests.Count > 0) {UpdateQuests(activeQuests[0]); print("Active Quests = " + activeQuests.Count);}
            else NoEntries();
            pageNumberText.text = "";
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
        //print(CurrentCategory);
    }

    void PopulateCodex()
    {
        if(isGridCategory)
        {
            grid.SetActive(true);
            horizontal.SetActive(false);
            questObj.SetActive(false);

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
        else if (!isQuestCategory)
        {
            grid.SetActive(false);
            horizontal.SetActive(true);
            questObj.SetActive(false);

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
            grid.SetActive(false);
            horizontal.SetActive(false);
            questObj.SetActive(true);

            for (int i = 0; i < activeQuests.Count; i++)
            {
                var tempButton = Instantiate(horizontalEntryButton, questContentObject.transform, worldPositionStays:false);
                var button = tempButton.gameObject.transform.GetChild(0).gameObject;
                var tempName = tempButton.gameObject.transform.GetChild(1).gameObject;
                var tempID = tempButton.GetComponent<CodexButtonID>();
                var tempText = tempName.GetComponent<TextMeshProUGUI>();

                var type = activeQuests[i].GetType();
                if(type.Equals(typeof(FetchQuest)))
                {
                    var q = activeQuests[i] as FetchQuest;
                    var t = q.name;

                    if (q.amount > 1) t = t.Replace("{itemName}", q.desiredItem.displayName.ToString() + "s");
                    else t = t.Replace("{itemName}", q.desiredItem.name.ToString());

                    t = t.Replace("{itemAmount}", q.amount.ToString());

                    tempText.text = t;
                }
                if(type.Equals(typeof(HuntQuest)))
                {
                    var q = activeQuests[i] as HuntQuest;
                    var t = q.name;

                    if (q.amount > 1) t = t.Replace("{creatureName}", q.targetCreature.name.ToString() + "s");
                    else t = t.Replace("{creatureName}", q.targetCreature.name.ToString());

                    t = t.Replace("{creatureAmount}", q.amount.ToString());

                    tempText.text = t;
                }
                if(type.Equals(typeof(GrowQuest)))
                {
                    var q = activeQuests[i] as GrowQuest;
                    var t = q.name;

                    if (q.amount > 1) t = t.Replace("{itemName}", q.desiredItem.displayName.ToString() + "s");
                    else t = t.Replace("{itemName}", q.desiredItem.name.ToString());

                    t = t.Replace("{itemAmount}", q.amount.ToString());

                    tempText.text = t;
                }

                if(!activeQuests[i].alreadyCompleted) tempText.text = tempText.text;
                else tempText.text = "<s>" + tempText.text + "</s>";

                button.name = "QuestButton" + i;
                
                tempID.assignedQuest = activeQuests[i];

                categoryList.Add(tempButton);
            }
            UpdateNavigation();
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
        if(currentEntry == null && !isQuestCategory) 
        {
            NoEntries();
        }
        else
        {
            nameText.text = currentEntry.entryName;     //CurrentCategory[0].entryName;
            descriptionText.text = currentEntry.description[0];     //CurrentCategory[0].description[0];
            largeDescriptionText.text = currentEntry.description[0];
            horizontalEntryName.text = currentEntry.entryName;
            horizontalDescriptionText.text = currentEntry.description[0];
            pageNumberText.text = "Page 1" + "/" + currentEntry.description.Length;
            
            ImageCheck();
            UpdateNavigation();
        }
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

        toolNav.mode = Navigation.Mode.Explicit;
        toolNav.selectOnUp = categoryButtons[0];
        toolNav.selectOnDown = categoryButtons[2];

        plantNav.mode = Navigation.Mode.Explicit;
        plantNav.selectOnUp = categoryButtons[1];
        plantNav.selectOnDown = categoryButtons[3];

        creatureNav.mode = Navigation.Mode.Explicit;
        creatureNav.selectOnUp = categoryButtons[2];
        creatureNav.selectOnDown = categoryButtons[4];

        questNav.mode = Navigation.Mode.Explicit;
        questNav.selectOnUp = categoryButtons[3];
        questNav.selectOnDown = categoryButtons[0];

        if(categoryList[0] != null)
        {
            if(isGridCategory){startNav.selectOnRight = categoryList[0].GetComponent<Button>();}
            else{startNav.selectOnRight = categoryList[0].GetComponentInChildren<Button>();}

            if(isGridCategory){toolNav.selectOnRight = categoryList[0].GetComponent<Button>();}
            else{toolNav.selectOnRight = categoryList[0].GetComponentInChildren<Button>();}

            if(isGridCategory){plantNav.selectOnRight = categoryList[0].GetComponent<Button>();}
            else{plantNav.selectOnRight = categoryList[0].GetComponentInChildren<Button>();}

            if(isGridCategory){creatureNav.selectOnRight = categoryList[0].GetComponent<Button>();}
            else{creatureNav.selectOnRight = categoryList[0].GetComponentInChildren<Button>();}

            if(isGridCategory){questNav.selectOnRight = categoryList[0].GetComponent<Button>();}
            else{questNav.selectOnRight = categoryList[0].GetComponentInChildren<Button>();}

        }
        
        categoryButtons[0].navigation = startNav;
        categoryButtons[1].navigation = toolNav;
        categoryButtons[2].navigation = plantNav;
        categoryButtons[3].navigation = creatureNav;
        categoryButtons[4].navigation = questNav;
    }
}
