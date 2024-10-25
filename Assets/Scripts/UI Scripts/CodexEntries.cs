using System.Collections;
using System.Collections.Generic;
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
        Misc
    }
    public EntryType entryType;

    [Tooltip("Name of the entry personally I thought this was pretty self explanatory tho")]
    public string entryName;

    [Tooltip("Description of the entry")]
    public string description;

}
