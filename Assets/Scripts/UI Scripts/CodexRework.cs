using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CodexRework : MonoBehaviour
{
    CodexEntries[] CurrentCategory, CreatureEntries, ToolEntries, GettingStarted;
    public CodexEntries currentEntry;
    int currentCategoryLength;
    public GameObject codex, contentObject;
    public TextMeshProUGUI nameText, descriptionText, largeDescriptionText, pageNumberText;
    public int currentPage = 0;

    public GameObject entryButton;

    string defaultName = "???";
    string defaultDesc = "Undiscovered";
    ControlManager controlManager;
    public Image largeImage;
    public List<GameObject> categoryList;

    void Awake()
    {
        controlManager = FindFirstObjectByType<ControlManager>();
    }

    void Start()
    {
        CreatureEntries = Resources.LoadAll<CodexEntries>("Codex/Creatures/");
        ToolEntries = Resources.LoadAll<CodexEntries>("Codex/Tools/");
        GettingStarted = Resources.LoadAll<CodexEntries>("Codex/GettingStarted");
        CurrentCategory = GettingStarted;
        currentEntry = null;

        nameText.text = "";     //CurrentCategory[0].entryName;
        descriptionText.text = "";    //CurrentCategory[0].description[0];
        largeDescriptionText.text = "";
        pageNumberText.text = "";

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
        pageNumberText.text = "";

        PopulateCodex(); 
        ImageCheck();

        codex.SetActive(!codex.activeInHierarchy);

        if(!codex.activeInHierarchy)
        {
            ClearCodex();
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
        }
        else
        {
            nameText.text = defaultName;
            descriptionText.text = defaultDesc;
            largeDescriptionText.text = defaultDesc;
        }
    }

    public void ChangeCategory(int cat)
    {
        switch (cat)
        {
            case 0:
            CurrentCategory = GettingStarted;
            currentPage = 0;
            UpdatePage(0, CurrentCategory[0], true);
            break;

            case 1:
            CurrentCategory = CreatureEntries;
            currentPage = 0;
            UpdatePage(0, CurrentCategory[0], true);
            break;

            case 2:
            CurrentCategory = ToolEntries;
            currentPage = 0;
            UpdatePage(0, CurrentCategory[0], true);
            break;

            default:
            print("Default");
            break;
        }

        for(int i = 0; i < categoryList.Count; i++)
        {
            Destroy(categoryList[i].gameObject);
        }

        ClearCodex();
        categoryList.Clear();
        PopulateCodex();
    }

    void PopulateCodex()
    {
        for (int i = 0; i < CurrentCategory.Length; i++)
        {
            var tempButton = Instantiate(entryButton, contentObject.transform, worldPositionStays:false);
            var tempName = tempButton.gameObject.transform.GetChild(0).gameObject;
            var tempImage = tempButton.gameObject.transform.GetChild(1).gameObject;
            var tempUnlock = tempButton.gameObject.transform.GetChild(2).gameObject;

            var tempID = tempButton.GetComponent<CodexButtonID>();
            var tempText = tempName.GetComponent<TextMeshProUGUI>();
            var tempSprite = tempImage.GetComponent<Image>();

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
        print(categoryList.Count);
        for(int i = 0; i < categoryList.Count; i++)
        {
            var temp = categoryList[i].GetComponent<CodexButtonID>();
            print(temp.assignedEntry.entryName);
            if(temp.assignedEntry.unlocked) 
            {
                HasUnlockedEntry(temp.assignedEntry);
                print("Entry Found");
                break;
            }
        }
        if(currentEntry == null) 
        {
            largeImage.gameObject.SetActive(false);
            return;
        }

        nameText.text = currentEntry.entryName;     //CurrentCategory[0].entryName;
        descriptionText.text = currentEntry.description[0];     //CurrentCategory[0].description[0];
        largeDescriptionText.text = currentEntry.description[0];
        pageNumberText.text = "Page 1" + "/" + currentEntry.description.Length;
        
        ImageCheck();
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
        if(currentEntry.mainImage != null && currentEntry.unlocked)
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
        } 
    }

    void HasUnlockedEntry(CodexEntries entry)
    {
        currentEntry = entry;
    }
}
