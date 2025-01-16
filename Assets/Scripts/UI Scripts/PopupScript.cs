using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Notification/New Popup Object")]
public class PopupScript : ScriptableObject
{
    public enum EndCondition
    {
        TimeBased,
        TillGround
    }

    [TextArea(2,2)]
    public string text;

    public EndCondition endCondition;

   [Tooltip ("Will be ignored if not set to TimeBased")]
    public float endTimeInSeconds;
}
