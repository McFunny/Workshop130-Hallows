using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CodexButtonID : MonoBehaviour
{
    public CodexEntries assignedEntry;
    public Quest assignedQuest;
    private CodexRework codex;
    private Codex3 newCodex;

    void Awake()
    {
        codex = GetComponentInParent<CodexRework>();
        newCodex = GetComponentInParent<Codex3>();
    }

    public void ShowEntry()
    {
        if (codex != null)
        {
            if (assignedEntry != null)
            {
                print(assignedEntry.entryName);

                if (!assignedEntry.unlocked && CreatureCheck() && CropCheck()) return; // I mean, it works, I guess

                codex.currentEntry = assignedEntry;
                codex.UpdatePage(0, assignedEntry, true);
            }
            else
            {
                codex.currentEntry = null;
                codex.UpdateQuests(assignedQuest);
            }
        }
        else if(newCodex != null)
        {
            if (assignedEntry != null)
            {
                print(assignedEntry.entryName);

                if (!assignedEntry.unlocked && CreatureCheck() && CropCheck()) return; // I mean, it works, I guess
                newCodex.UpdatePage(assignedEntry);
            }
        }
        
    }

    bool CreatureCheck()
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

    bool CropCheck()
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
