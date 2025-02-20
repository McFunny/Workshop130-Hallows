using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CodexButtonID : MonoBehaviour
{
    public CodexEntries assignedEntry;
    private CodexRework codex;

    void Awake()
    {
        codex = FindAnyObjectByType<CodexRework>();
    }

    public void ShowEntry()
    {
        print(assignedEntry.entryName);
        if(!assignedEntry.unlocked) { return; }
        codex.currentEntry = assignedEntry;
        codex.UpdatePage(0, assignedEntry, true);
    }
}
