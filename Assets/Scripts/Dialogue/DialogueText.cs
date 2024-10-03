using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Dialogue/New Dialogue Container")]
public class DialogueText : ScriptableObject
{
    public string speakerName;
    [TextArea(5,10)]
    public string[] paragraphs;
    public Emotion[] emotions;
}

public enum Emotion
{
    //To dictate which audio is played alongside the dialogue
    Neutral,
    Happy,
    Sad,
    Angry,
    Confused,
    Shocked
}
