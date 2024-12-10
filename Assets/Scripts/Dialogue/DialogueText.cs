using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Dialogue/New Dialogue Container")]
public class DialogueText : ScriptableObject
{
    public string speakerName;
    public DialoguePath defaultPath;
    public DialoguePath questCompletePath;
    public DialoguePath repeatedItemPath;
    public DialoguePath[] paths; //misc paths
    public DialoguePath[] fillerPaths; //the random text the NPC will say each day. 50% chance they say this or one from their friendship path
    public DialoguePath[] questPaths;
    public DialoguePath[] itemRecievedPaths;
    public DialoguePath[] itemSpecificRemarks;
    public DialoguePath[] friendshipPath1, friendshipPath2, friendshipPath3; //Random Text they can say depending on friendship levels

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

public enum PathType
{
    Default,
    QuestComplete,
    RepeatItem,
    Misc,
    Filler,
    Quest,
    ItemRecieved,
    ItemSpecific
}

[System.Serializable]
public class DialoguePath
{
    public string pathName;
    [TextArea(5,10)]
    public string[] paragraphs;
    public Emotion[] emotions;
}
