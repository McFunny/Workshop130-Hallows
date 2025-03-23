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
    public CodexEntries currentEntry, mandrakeEntry;
    [SerializeField] private GameObject codex, gridContentObject, horizontalContentObject, questContentObject;
    [SerializeField] private TextMeshProUGUI nameText, horizontalEntryName, horizontalDescriptionText, descriptionText, cropDescriptionText, largeDescriptionText, pageNumberText, contentsText, questNameText, questDescriptionText, questProgressText, questCompleteText;
    [SerializeField] private TextMeshProUGUI growthStageText, hoursPerStage;
    [SerializeField] private TextMeshProUGUI timesDone;
    [SerializeField] private int currentPage = 0;
    [SerializeField] private Slider questSlider;
    [SerializeField] private GameObject entryButton, horizontalEntryButton, grid, horizontal, questObj, cropInfoParent;

    string defaultName = "???";
    string defaultDesc = "There is more to discover...";
    ControlManager controlManager;
    [SerializeField] private Image largeImage, smallImage, questImage, bgImage;
    [SerializeField] private GameObject defaultButton;
    [SerializeField] private Button[] categoryButtons;
    private Button currentCategoryButton;
    [SerializeField] private List<GameObject> categoryList;
    bool isGridCategory, isQuestCategory;
    private QuestManager questManager;
    public Sprite[] characterPortraits;
    public List<Quest> activeQuests = new List<Quest>();
    private PauseScript pauseScript;
    [SerializeField] private GameObject RBLB;
    [SerializeField] private List<GameObject> input = new List<GameObject>();
    [SerializeField] private List<GameObject> output = new List<GameObject>();
    GameSaveData gameSaveData;

    void Awake()
    {
        controlManager = FindFirstObjectByType<ControlManager>();
        questManager = FindFirstObjectByType<QuestManager>();
        pauseScript = FindFirstObjectByType<PauseScript>();
        gameSaveData = FindFirstObjectByType<GameSaveData>();
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

        for(int i = 0; i < 4; i++)
        {
            input.Add(cropInfoParent.transform.GetChild(0).GetChild(1).GetChild(i).gameObject); //For the love of god don't reorganize the cropInfoParent!!!
        }
        for(int i = 0; i < 4; i++)
        {
            output.Add(cropInfoParent.transform.GetChild(1).GetChild(1).GetChild(i).gameObject); //For the love of god don't reorganize the cropInfoParent!!!
        }

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

        if(ControlManager.isController) RBLB.SetActive(true);
        else RBLB.SetActive(false);

        bgImage.gameObject.SetActive(largeImage.gameObject.activeSelf);
    }

    void OpenCodexPressed(InputAction.CallbackContext obj)
    {  
        if(codex.activeInHierarchy && !pauseScript.gameObject.transform.GetChild(0).gameObject.activeSelf) //ts pmo.....
        {
            print("Closing");
            if(codex.activeSelf){OpenCloseCodex();}
        }
    }

    /*void CloseCodexPressed(InputAction.CallbackContext obj) // This doesn't do anything for some reason
    {
        if(codex.activeInHierarchy)
        {
            OpenCloseCodex();
        } 
    }*/

    public void OpenCloseCodex()
    {
        codex.SetActive(!codex.activeInHierarchy);

        if(!codex.activeInHierarchy)
        {
            ClearCodex();
            //TimeManager.Instance.stopTime = false;
            if(!PauseScript.isPaused) EventSystem.current.SetSelectedGameObject(null);
            else EventSystem.current.SetSelectedGameObject(pauseScript.buttons[4].gameObject); //Codex button in pause menu
            Time.timeScale = 1;
        }
        else
        {
            //TimeManager.Instance.stopTime = false;
            Time.timeScale = 0;

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
        currentEntry = entry;

        if (entry.unlocked == true || entry.cropData != null || entry.creatureData != null)
        {
            if(entry.unlocked) goto EntryUnlockOverride; //I don't like using these but it works lol
            if(entry.cropData != null && entry.cropData.amountHarvested == 0) return;
            if(entry.creatureData != null && entry.creatureData.amountKilled == 0) return;

            EntryUnlockOverride:

            print("Entry is Unlocked");
            pageNumberText.text = "Page " + (currentPage + 1) + "/" + entry.description.Length;
            nameText.text = entry.entryName;
            descriptionText.text = entry.description[currentPage];
            cropDescriptionText.text = entry.description[currentPage];
            largeDescriptionText.text = entry.description[currentPage];
            horizontalEntryName.text = entry.entryName;
            horizontalDescriptionText.text = entry.description[currentPage];
            ImageCheck();

            if(entry.cropData != null) //Entry is a crop
            {
                if(entry != mandrakeEntry) timesDone.text = "Times harvested: " + entry.cropData.amountHarvested;
                else timesDone.text = "Times Killed: " + entry.cropData.amountKilled;
                //print("Crop Data Found");
                if(currentPage == 0) 
                {

                    //Consumes

                    if(entry.cropData.gloamIntake > 0){input[0].SetActive(true);}
                    else{input[0].SetActive(false);}

                    if(entry.cropData.terraIntake > 0){input[1].SetActive(true);}
                    else{input[1].SetActive(false);}

                    if(entry.cropData.ichorIntake > 0){input[2].SetActive(true);}
                    else{input[2].SetActive(false);}

                    if(entry.cropData.waterIntake > 0){input[3].SetActive(true);}
                    else{input[3].SetActive(false);}

                    //Produces

                    if(entry.cropData.gloamIntake < 0){output[0].SetActive(true);}
                    else{output[0].SetActive(false);}

                    if(entry.cropData.terraIntake < 0){output[1].SetActive(true);}
                    else{output[1].SetActive(false);}

                    if(entry.cropData.ichorIntake < 0){output[2].SetActive(true);}
                    else{output[2].SetActive(false);}

                    if(entry.cropData.waterIntake < 0){output[3].SetActive(true);}
                    else{output[3].SetActive(false);}

                    growthStageText.text = "Growth Stages: " + entry.cropData.growthStages;
                    hoursPerStage.text = "Hours per Stage: " + entry.cropData.hoursPerStage;

                    cropDescriptionText.gameObject.SetActive(true);
                    cropInfoParent.SetActive(true);
                    descriptionText.text = "";
                }
                else
                {
                    cropDescriptionText.gameObject.SetActive(false);
                    cropInfoParent.SetActive(false);
                }
                timesDone.gameObject.SetActive(true);
            }
            else if(entry.creatureData != null) //Entry is a creature
            {
                timesDone.text = "Times Killed: " + entry.creatureData.amountKilled;
                //print("Creature Data Found");
                timesDone.gameObject.SetActive(true);
                cropDescriptionText.gameObject.SetActive(false);
                cropInfoParent.SetActive(false);
            }
            else //Entry is neither
            {
                //print("No Data Found");
                timesDone.gameObject.SetActive(false);
                cropDescriptionText.gameObject.SetActive(false);
                cropInfoParent.SetActive(false);
            }
            
        }
        else
        {
            return;
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
            //print("Fetch Quest");
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
            //print("Hunt Quest");
            var q = quest as HuntQuest;
            var t = q.description;

            if (q.amount > 1) t = t.Replace("{itemName}", q.targetCreature.name.ToString() + "s");
            else t = t.Replace("{itemName}", q.targetCreature.name.ToString());

            t = t.Replace("{itemAmount}", q.amount.ToString());

            questDescriptionText.text = t;
            questProgressText.text = q.targetCreature.name + " eliminated: " + q.progress + "/" + q.maxProgress;
        }
        if(type.Equals(typeof(GrowQuest)))
        {
            //print("Grow Quest");
            var q = quest as GrowQuest;
            var t = q.description;

            if (q.amount > 1) t = t.Replace("{itemName}", q.desiredItem.displayName.ToString() + "s");
            else t = t.Replace("{itemName}", q.desiredItem.name.ToString());

            t = t.Replace("{itemAmount}", q.amount.ToString());

            questDescriptionText.text = t;
            questProgressText.text = q.progress + "/" + q.maxProgress;
            questProgressText.text = q.desiredItem.displayName + " grown: " + q.progress + "/" + q.maxProgress;
        }
        if(quest.displayProgress == false)
        {
            questProgressText.text = "";
            questSlider.gameObject.SetActive(false);
        } 
        else
        {
            questSlider.gameObject.SetActive(true);
            questSlider.minValue = 0;
            questSlider.maxValue = quest.maxProgress;
            questSlider.value = quest.progress;
        }

        if(quest.progress >= quest.maxProgress && quest.alreadyCompleted != true && quest.assignee != 0) questCompleteText.text = "Return to " + quest.assignee;
        else if (quest.alreadyCompleted == true) questCompleteText.text = "Completed";
        else questCompleteText.text = "";

        if(characterPortraits[(int)quest.assignee] != null)
        {
            questImage.sprite = characterPortraits[(int)quest.assignee];
            questImage.preserveAspect = true;
        }
        else
        {
            questImage.sprite = characterPortraits[0];
            questImage.preserveAspect = true;
        }

        //print(type);
    }

    private void SetTextToDefault()
    {
        nameText.text = defaultName;
        descriptionText.text = defaultDesc;
        largeDescriptionText.text = defaultDesc;
        horizontalEntryName.text = defaultName;
        horizontalDescriptionText.text = defaultDesc;
        questCompleteText.text = "";
        questDescriptionText.text = defaultDesc;
        questNameText.text = defaultName;
        questProgressText.text = "";
        cropInfoParent.SetActive(false);
        cropDescriptionText.text = "";
        timesDone.text = "";
        pageNumberText.text = "";
    }

    private void NoEntries()
    {
        largeImage.gameObject.SetActive(false);
        smallImage.gameObject.SetActive(false);
        questImage.gameObject.SetActive(false);
        SetTextToDefault();
        questSlider.gameObject.SetActive(false);
        descriptionText.gameObject.SetActive(false);
        largeDescriptionText.gameObject.SetActive(true);
        largeImage.sprite = null;
        smallImage.sprite = null;
        //questImage.sprite = null;
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
            CurrentCategory[0].unlocked = gameSaveData.watergunObtained;
            UpdatePage(0, CurrentCategory[0], true);
            break;

            case 2:
            CurrentCategory = PlantEntries;
            currentPage = 0;
            isGridCategory = true;
            isQuestCategory = false;
            contentsText.text = "Plants";
            if(mandrakeEntry.cropData.amountKilled > 0) mandrakeEntry.unlocked = true;
            else mandrakeEntry.unlocked = false;
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
            if(activeQuests.Count > 0) 
            {
                UpdateQuests(activeQuests[0]); 
                print("Active Quests = " + activeQuests.Count);
                questImage.gameObject.SetActive(true);
            }
            else
            {
                NoEntries();
            }
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

                /*if(CurrentCategory[i].cropData != null) //Uncomment to update crop description 0
                {
                    CurrentCategory[i].entryName = CurrentCategory[i].cropData.name;
                    if(CurrentCategory[i].cropData.cropYield != null) CurrentCategory[i].description[0] = CurrentCategory[i].cropData.cropYield.description; 

                }/*/

                if(CurrentCategory[i].unlocked) //Unlock Override
                {
                    tempText.text = CurrentCategory[i].entryName;
                    tempImage.SetActive(true);
                    tempUnlock.SetActive(false);
                    tempSprite.sprite = CurrentCategory[i].buttonIcon;
                }
                else if (CurrentCategory[i].cropData != null)
                {
                    if(CurrentCategory[i].cropData.amountHarvested > 0) //Unlocks if amount of crop harvested > 0
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
                }
                else if (CurrentCategory[i].creatureData != null) //Unlocks if amount of enemy killed > 0
                {
                    if(CurrentCategory[i].creatureData.amountKilled > 0)
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
                var tempName = tempButton.gameObject.transform.GetChild(0).gameObject;
                var tempID = tempButton.GetComponent<CodexButtonID>();
                var tempText = tempName.GetComponent<TextMeshProUGUI>();

                tempButton.name = "HorizontalButton" + i;


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
                var tempName = tempButton.gameObject.transform.GetChild(0).gameObject;
                var tempID = tempButton.GetComponent<CodexButtonID>();
                var tempText = tempName.GetComponent<TextMeshProUGUI>();

                var type = activeQuests[i].GetType();
                print(type);
                if(type.Equals(typeof(FetchQuest)))
                {
                    //print("Fetch Quest");
                    var q = activeQuests[i] as FetchQuest;
                    var t = q.name;

                    if (q.amount > 1) t = t.Replace("{itemName}", q.desiredItem.displayName.ToString() + "s");
                    else t = t.Replace("{itemName}", q.desiredItem.name.ToString());

                    t = t.Replace("{itemAmount}", q.amount.ToString());

                    tempText.text = t;
                }
                else if(type.Equals(typeof(HuntQuest)))
                {
                    //print("Hunt Quest");
                    var q = activeQuests[i] as HuntQuest;
                    var t = q.name;

                    if (q.amount > 1) t = t.Replace("{creatureName}", q.targetCreature.name.ToString() + "s");
                    else t = t.Replace("{creatureName}", q.targetCreature.name.ToString());

                    t = t.Replace("{creatureAmount}", q.amount.ToString());

                    tempText.text = t;
                }
                else if(type.Equals(typeof(GrowQuest)))
                {
                    //print("Grow Quest");
                    var q = activeQuests[i] as GrowQuest;
                    var t = q.name;

                    if (q.amount > 1) t = t.Replace("{itemName}", q.desiredItem.displayName.ToString() + "s");
                    else t = t.Replace("{itemName}", q.desiredItem.name.ToString());

                    t = t.Replace("{itemAmount}", q.amount.ToString());

                    tempText.text = t;
                }
                else
                {
                    tempText.text = activeQuests[i].name;
                }

                if(!activeQuests[i].alreadyCompleted) tempText.text = tempText.text;
                else tempText.text = "<s>" + tempText.text + "</s>";

                tempButton.name = "QuestButton" + i;
                
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
            var temp = categoryList[i].GetComponentInChildren<CodexButtonID>();
            //print(temp.assignedEntry.entryName);
            if(temp.assignedEntry.unlocked || CreatureCheck(temp.assignedEntry) || CropCheck(temp.assignedEntry)) 
            {
                HasUnlockedEntry(temp.assignedEntry);

                if(temp.assignedEntry.cropData != null) //Entry is a crop
                {
                timesDone.text = "Times harvested: " + temp.assignedEntry.cropData.amountHarvested;
                //print("Crop Data Found");
                if(currentPage == 0) 
                {
                    if(temp.assignedEntry.cropData.gloamIntake > 0){input[0].SetActive(true);}
                    else{input[0].SetActive(false);}

                    if(temp.assignedEntry.cropData.terraIntake > 0){input[1].SetActive(true);}
                    else{input[1].SetActive(false);}

                    if(temp.assignedEntry.cropData.ichorIntake > 0){input[2].SetActive(true);}
                    else{input[2].SetActive(false);}

                    if(temp.assignedEntry.cropData.waterIntake > 0){input[3].SetActive(true);}
                    else{input[3].SetActive(false);}

                    //Produces

                    if(temp.assignedEntry.cropData.gloamIntake < 0){output[0].SetActive(true);}
                    else{output[0].SetActive(false);}

                    if(temp.assignedEntry.cropData.terraIntake < 0){output[1].SetActive(true);}
                    else{output[1].SetActive(false);}

                    if(temp.assignedEntry.cropData.ichorIntake < 0){output[2].SetActive(true);}
                    else{output[2].SetActive(false);}

                    if(temp.assignedEntry.cropData.waterIntake < 0){output[3].SetActive(true);}
                    else{output[3].SetActive(false);}

                    growthStageText.text = "Growth Stages: " + temp.assignedEntry.cropData.growthStages;
                    hoursPerStage.text = "Hours per Stage: " + temp.assignedEntry.cropData.hoursPerStage;

                    cropDescriptionText.gameObject.SetActive(true);
                    cropInfoParent.SetActive(true);
                    descriptionText.text = "";

                    cropDescriptionText.gameObject.SetActive(true);
                    cropInfoParent.SetActive(true);
                }
                else
                {
                    cropDescriptionText.gameObject.SetActive(false);
                    cropInfoParent.SetActive(false);
                }
                timesDone.gameObject.SetActive(true);
            }
            else if(temp.assignedEntry.creatureData != null) //Entry is a creature
            {
                timesDone.text = "Times Killed: " + temp.assignedEntry.creatureData.amountKilled;
                //print("Creature Data Found");
                timesDone.gameObject.SetActive(true);
                cropDescriptionText.gameObject.SetActive(false);
                cropInfoParent.SetActive(false);
            }
            else //Entry is neither
            {
                //print("No Data Found");
                timesDone.gameObject.SetActive(false);
                cropDescriptionText.gameObject.SetActive(false);
                cropInfoParent.SetActive(false);
            }

                print("Unlocked Entry Found");
                break;
            }
            else
            {
                if(temp.assignedEntry.cropData != null)
                {
                    timesDone.text = "";
                    //print("Crop Data Found");
                    timesDone.gameObject.SetActive(true);
                }
                else if(temp.assignedEntry.creatureData != null)
                {
                    timesDone.text = "";
                    //print("Creature Data Found");
                    timesDone.gameObject.SetActive(true);
                }
                else
                {
                    //print("No Data Found");
                    timesDone.text = "";
                }
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
            cropDescriptionText.text = currentEntry.description[0];
            horizontalEntryName.text = currentEntry.entryName;
            horizontalDescriptionText.text = currentEntry.description[0];
            pageNumberText.text = "Page 1" + "/" + currentEntry.description.Length;
            if(currentEntry.cropData != null) descriptionText.text = "";
            
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
            //timesDone.gameObject.SetActive(false);
            largeImage.sprite = null;
            smallImage.sprite = null;
            return;
        }

        if(currentEntry.mainImage != null && (currentEntry.unlocked || currentEntry.cropData != null || currentEntry.creatureData != null))
        {
            if(isGridCategory)
            {
                largeImage.sprite = currentEntry.mainImage;
                largeImage.gameObject.SetActive(true);
                descriptionText.gameObject.SetActive(true);
                largeDescriptionText.gameObject.SetActive(false);
                //timesDone.gameObject.SetActive(true);
            }
            else
            {
                largeImage.gameObject.SetActive(false);
                descriptionText.gameObject.SetActive(false);
                largeDescriptionText.gameObject.SetActive(true);
                smallImage.gameObject.SetActive(true);
                smallImage.sprite = currentEntry.mainImage;
                //timesDone.gameObject.SetActive(false);
            } 
        }
        else
        {
            descriptionText.gameObject.SetActive(false);
            largeDescriptionText.gameObject.SetActive(true);
            largeImage.gameObject.SetActive(false);
            smallImage.gameObject.SetActive(false);
            //timesDone.gameObject.SetActive(false);
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

        if(categoryList.Count != 0)
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

    bool CreatureCheck(CodexEntries assignedEntry)
    {
        if(assignedEntry.creatureData != null)
        {
            if(assignedEntry.creatureData.amountKilled > 0)
            {
                print("Is a creature");
                return true;
            }
            else return false;
        }
        else return false;
    }

    bool CropCheck(CodexEntries assignedEntry)
    {
        if(assignedEntry.cropData != null)
        {
            if(assignedEntry.cropData.amountHarvested > 0)
            {
                 print("Is a crop");
                return true;
            }
            else return false;
        }
        else return false;
    }
}
