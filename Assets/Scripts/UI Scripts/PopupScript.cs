using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Notification/New Popup Object")]
public class PopupScript : ScriptableObject
{
    public enum EndCondition
    {
        TimeBased,
        TillGround,
        ShovelSwing,
        PlantSeed,
        KillStructure,
        WeedDug,
        WateredCrop,
        ClearCorpse,
        KillCreature,
        OpenCodex,
        PlaceStructure
    }

    [TextArea(2,2)]
    public string text;

    public EndCondition endCondition;

    public bool skippable = false; //If true, will skip this popup if another is enqued

   [Tooltip ("Will be ignored if not set to TimeBased")]
    public float endTimeInSeconds;
}
