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
    public TextMeshProUGUI nameText, descriptionText, pageNumberText;
    public int currentPage = 0;

    public GameObject entryButton;

    string defaultName = "Undiscovered";
    string defaultDescLeft = "";
    string defaultDesc = "Undiscovered";
    ControlManager controlManager;

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
        currentEntry = GettingStarted[0];
        nameText.text = GettingStarted[0].entryName;
        descriptionText.text = GettingStarted[0].description[0];
        pageNumberText.text = "Page 1" + "/" + currentEntry.description.Length;

        for (int i = 0; i < CurrentCategory.Length; i++)
        {
            var tempButton = Instantiate(entryButton, contentObject.transform, worldPositionStays:false);
            var tempName = tempButton.gameObject.transform.GetChild(0).gameObject;
            var tempImage = tempButton.gameObject.transform.GetChild(1).gameObject;

            var tempID = tempButton.GetComponent<CodexButtonID>();
            var tempText = tempName.GetComponent<TextMeshProUGUI>();
            var tempSprite = tempImage.GetComponent<Image>();

            tempID.assignedEntry = CurrentCategory[i];
            tempText.text = CurrentCategory[i].entryName;
            tempSprite.sprite = CurrentCategory[i].buttonIcon;
            categoryList.Add(tempButton);
        }
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
    }
    
    void OpenCodexPressed(InputAction.CallbackContext obj)
    {  
        if(codex.activeInHierarchy)
        {
            print("Closing");
            PlayerMovement.isCodexOpen = false;
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
        nameText.text = GettingStarted[0].entryName;
        descriptionText.text = GettingStarted[0].description[0];
        pageNumberText.text = "Page 1"  + "/" + GettingStarted[0].description.Length;
        codex.SetActive(!codex.activeInHierarchy);
        PlayerMovement.isCodexOpen = codex.activeInHierarchy;

    }
    public void UpdatePage(int page, CodexEntries entry, bool reset)
    {
        if (!reset) currentPage = currentPage + page;
        else currentPage = page;

        currentPage = Mathf.Clamp(currentPage,0,entry.description.Length - 1);
        pageNumberText.text = "Page " + (currentPage + 1) + "/" + entry.description.Length;
        if (entry.unlocked == true)
        {
            nameText.text = entry.entryName;
            descriptionText.text = entry.description[currentPage];
        }
        else
        {
            nameText.text = defaultName;
            descriptionText.text = defaultDesc;
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

        categoryList.Clear();

        for (int i = 0; i < CurrentCategory.Length; i++)
        {
            var tempButton = Instantiate(entryButton, contentObject.transform, worldPositionStays:false);
            var tempName = tempButton.gameObject.transform.GetChild(0).gameObject;
            var tempImage = tempButton.gameObject.transform.GetChild(1).gameObject;

            var tempID = tempButton.GetComponent<CodexButtonID>();
            var tempText = tempName.GetComponent<TextMeshProUGUI>();
            var tempSprite = tempImage.GetComponent<Image>();

            tempID.assignedEntry = CurrentCategory[i];
            tempText.text = CurrentCategory[i].entryName;
            tempSprite.sprite = CurrentCategory[i].buttonIcon;

            categoryList.Add(tempButton);
        }

        currentEntry = CurrentCategory[0];
        currentPage = 0;
    }
}
