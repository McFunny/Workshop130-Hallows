using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class CodexRework : MonoBehaviour
{
    CodexEntries[] CurrentCategory, CreatureEntries, ToolEntries, GettingStarted;
    CodexEntries currentEntry;
    int currentCategoryLength;
    public GameObject codex;
    public TextMeshProUGUI nameText, descriptionText, pageNumberText;
    public int currentPage = 0;

    string defaultName = "Undiscovered";
    string defaultDescLeft = "";
    string defaultDesc = "Undiscovered";
    ControlManager controlManager;

    void Awake()
    {
        controlManager = FindFirstObjectByType<ControlManager>();
    }

    void Start()
    {
        
        //CreatureEntries = Resources.LoadAll<CodexEntries>("Codex/Creatures/");
        //ToolEntries = Resources.LoadAll<CodexEntries>("Codex/Tools/");
        GettingStarted = Resources.LoadAll<CodexEntries>("Codex/GettingStarted");
        CurrentCategory = GettingStarted;
        currentEntry = GettingStarted[0];
        nameText.text = GettingStarted[0].entryName;
        descriptionText.text = GettingStarted[0].description[0];
        pageNumberText.text = "Page " + 1 + "/" + GettingStarted.Length;
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
                UpdatePage(-1, currentEntry);
            }
            else if (controlManager.codexPageDown.action.WasPressedThisFrame())
            {
                UpdatePage(1, currentEntry);
            }
        }
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
        nameText.text = GettingStarted[0].entryName;
        descriptionText.text = GettingStarted[0].description[0];
        pageNumberText.text = "Page " + 1 + "/" + GettingStarted.Length;
        codex.SetActive(!codex.activeInHierarchy);
        PlayerMovement.isCodexOpen = codex.activeInHierarchy;
    }
    void UpdatePage(int page, CodexEntries entry)
    {
        currentPage = currentPage + page;
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
            CurrentCategory = CreatureEntries;
            currentPage = 0;
            UpdatePage(0, CurrentCategory[0]);
            break;

            case 1:
            CurrentCategory = CreatureEntries;
            currentPage = 0;
            UpdatePage(0, CurrentCategory[0]);
            break;

            case 2:
            CurrentCategory = ToolEntries;
            currentPage = 0;
            UpdatePage(0, CurrentCategory[0]);
            break;

            default:
            print("Default");
            break;
        }

        currentPage = 0;
    }
}
