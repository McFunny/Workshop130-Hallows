using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CodexButtonID : MonoBehaviour
{
    public CodexEntries assignedEntry;
    public Quest assignedQuest;
    private CodexRework codex;

    void Awake()
    {
        codex = FindAnyObjectByType<CodexRework>();
    }

    public void ShowEntry()
    {
        if(assignedEntry != null)
        {
            print(assignedEntry.entryName);
            if(!assignedEntry.unlocked) { return; }
            codex.currentEntry = assignedEntry;
            codex.UpdatePage(0, assignedEntry, true);
        }
        else
        {
            print(assignedQuest.name);
        }
    }
}
