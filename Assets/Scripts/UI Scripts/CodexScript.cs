using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class CodexScript : MonoBehaviour
{
    CodexEntries[] CurrentCategory, CreatureEntries, ToolEntries, GettingStarted;
    int currentCategoryLength;
    public GameObject codex;
    public TextMeshProUGUI nameText, leftDescriptionText, descriptionText, pageNumberText;
    public int currentEntry = 0;

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
        nameText.text = GettingStarted[0].entryName;
        descriptionText.text = GettingStarted[0].description;
        pageNumberText.text = "Page " + 1 + "/" + GettingStarted.Length;
        if(!GettingStarted[currentEntry].hasImage)
        {
            leftDescriptionText.text = GettingStarted[currentEntry].leftDescription;
        }
    }

    void OnEnable()
    {
        controlManager.openCodex.action.started += OpenCodexPressed;
        controlManager.closeCodex.action.started += CloseCodexPressed;
    }

    void OnDisable()
    {
        controlManager.openCodex.action.started -= OpenCodexPressed;
        controlManager.closeCodex.action.started -= CloseCodexPressed;
    }

    // Update is called once per frame
    void Update()
    {
        if(codex.activeInHierarchy)
        {
            if(controlManager.codexPageUp.action.WasPressedThisFrame())
            {
                UpdatePage(-1, CurrentCategory);
            }
            else if (controlManager.codexPageDown.action.WasPressedThisFrame())
            {
                UpdatePage(1, CurrentCategory);
            }
        }
    }
    
    void OpenCodexPressed(InputAction.CallbackContext obj)
    {  
        if(!PlayerMovement.accessingInventory && !PauseScript.isPaused && PlayerMovement.restrictMovementTokens > 0)
        {
            if(codex.activeSelf){OpenCloseCodex();}
        }
    }

    void CloseCodexPressed(InputAction.CallbackContext obj) // This doesn't do anything for some reason
    {
        if(!PlayerMovement.accessingInventory && !PauseScript.isPaused && PlayerMovement.restrictMovementTokens > 0)
        {
            OpenCloseCodex();
        }
        
    }

    public void OpenCloseCodex()
    {
        print("Codex Opened");
        currentEntry = 0;
        CurrentCategory = GettingStarted;
        nameText.text = GettingStarted[currentEntry].entryName;
        descriptionText.text = GettingStarted[currentEntry].description;
        pageNumberText.text = "Page " + 1 + "/" + GettingStarted.Length;
        if(!GettingStarted[currentEntry].hasImage)
        {
            leftDescriptionText.text = GettingStarted[currentEntry].leftDescription;
        }

        codex.SetActive(!codex.activeInHierarchy);
        PlayerMovement.isCodexOpen = codex.activeInHierarchy;
        if(PlayerMovement.isCodexOpen) PlayerMovement.restrictMovementTokens++;
        else PlayerMovement.restrictMovementTokens--;
    }
    void UpdatePage(int page, CodexEntries[] currentCat)
    {
        currentEntry = currentEntry + page;
        currentEntry = Mathf.Clamp(currentEntry,0,currentCat.Length - 1);
        pageNumberText.text = "Page " + (currentEntry + 1) + "/" + currentCat.Length;
        if (currentCat[currentEntry].unlocked == true)
        {
            nameText.text = currentCat[currentEntry].entryName;
            descriptionText.text = currentCat[currentEntry].description;
            if(!currentCat[currentEntry].hasImage)
            {
                leftDescriptionText.text = currentCat[currentEntry].leftDescription;
            }
        }
        else
        {
            nameText.text = defaultName;
            descriptionText.text = defaultDesc;
            leftDescriptionText.text = defaultDescLeft;
        }
    }

    public void SwitchCategories(int cat)
    {
        switch (cat)
        {
            case 0:
            CurrentCategory = CreatureEntries;
            currentEntry = 0;
            UpdatePage(0, CurrentCategory);
            break;

            case 1:
            CurrentCategory = CreatureEntries;
            currentEntry = 0;
            UpdatePage(0, CurrentCategory);
            break;

            case 2:
            CurrentCategory = ToolEntries;
            currentEntry = 0;
            UpdatePage(0, CurrentCategory);
            break;

            default:
            print("Default");
            break;
        }
    }
}
