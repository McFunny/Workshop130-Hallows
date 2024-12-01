using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEditor.EditorTools;
using UnityEngine;

[CreateAssetMenu]
public class CodexEntries : ScriptableObject
{
    public enum EntryType{
        Creature,
        Plant,
        NPC,
        Tool,
        Lore,
        Misc,
        GettingStarted
    }
    public EntryType entryType;

    [Tooltip("Unlocked??? Yes or... no....? (True is yes, false is no)")]
    public bool unlocked = false;

    [Tooltip("If the entry has an image??? Yes or no.....")]
    public bool hasImage = false;

    [Tooltip("The assigned image")]
    public Image image;

    [Tooltip("Name of the entry personally I thought this was pretty self explanatory tho")]
    public string entryName;

    [Tooltip("Text for the left page if there is no image")] [TextArea(4,10)]
    public string leftDescription;

    [Tooltip("Description of the entry")] [TextArea(4,10)]
    public string description;

}
