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
        PlaceStructure,
        PetCritter
    }

    [TextArea(2,2)]
    public string text;

    public EndCondition endCondition;

    public bool skippable = false; //If true, will skip this popup if another is enqued

    public bool canWaitInQueue = true; //If false, if there is already popups in the queue, this does not get added

   [Tooltip ("Will be ignored if not set to TimeBased")]
    public float endTimeInSeconds;
}
