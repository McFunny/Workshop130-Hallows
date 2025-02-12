using System;
using System.Collections;
using System.Collections.Generic;
//using Microsoft.Unity.VisualStudio.Editor;
//using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.UI;

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
    public Button entryButton;
    public Image buttonIcon;

    [Tooltip("Unlocked??? Yes or... no....? (True is yes, false is no)")]
    public bool unlocked = false;

    [Tooltip("If the entry has an image??? Yes or no.....")]
    public bool hasImage = false;

    [Tooltip("The assigned image")]
    public Image largeImage;

    [Tooltip("Name of the entry personally I thought this was pretty self explanatory tho")]
    public string entryName;

    [Tooltip("Description of the entry")] [TextArea(4,10)]
    public string[] description;

}
