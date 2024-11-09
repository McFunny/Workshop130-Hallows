using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Diagnostics;

public class CodexScript : MonoBehaviour
{
    CodexEntries[] CurrentCategory, CreatureEntries, ToolEntries;
    int currentCategoryLength;
    public GameObject codex;
    public TextMeshProUGUI nameText, descriptionText;
    public int currentEntry = 0;

    string defaultName = "Undiscovered";
    string defaultDesc = "Undiscovered";

    // Start is called before the first frame update
    void Start()
    {
        CreatureEntries = Resources.LoadAll<CodexEntries>("Codex/Creatures/");
        ToolEntries = Resources.LoadAll<CodexEntries>("Codex/Tools/");
        CurrentCategory = CreatureEntries;
        nameText.text = CreatureEntries[0].entryName;
        descriptionText.text = CreatureEntries[0].description;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown("c"))
        {
            currentEntry = 0;
            CurrentCategory = CreatureEntries;
            nameText.text = CreatureEntries[currentEntry].entryName;
            descriptionText.text = CreatureEntries[currentEntry].description;
            codex.SetActive(!codex.activeInHierarchy);
            PlayerMovement.isCodexOpen = codex.activeInHierarchy;
        }

        if(codex.activeInHierarchy)
        {
            if(Input.GetKeyDown(KeyCode.LeftArrow))
            {
                UpdatePage(-1, CurrentCategory);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                UpdatePage(1, CurrentCategory);
            }
        }
    }

    void UpdatePage(int page, CodexEntries[] currentCat)
    {
        currentEntry = currentEntry + page;
        currentEntry = Mathf.Clamp(currentEntry,0,currentCat.Length - 1);
        if (currentCat[currentEntry].unlocked == true)
        {
            nameText.text = currentCat[currentEntry].entryName;
            descriptionText.text = currentCat[currentEntry].description;
        }
        else
        {
            nameText.text = defaultName;
            descriptionText.text = defaultDesc;
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
