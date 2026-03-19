using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Dialogue/New Dialogue Container")]
public class DialogueText : ScriptableObject
{
    public string speakerName;
    public DialoguePath defaultPath;
    //public DialoguePath questCompletePath; //Change to be an array, so there are diff dialogues for each type of quest completed
    public DialoguePath repeatedItemPath;
    public DialoguePath[] paths; //misc paths
    public DialoguePath[] fillerPaths; //the random text the NPC will say each day. 50% chance they say this or one from their friendship path
    public DialoguePath[] questPaths;
    public DialoguePath[] itemRecievedPaths; //Legacy system of item paths. Use itemPaths for new ones
    public DialoguePath[] itemSpecificRemarks;
    public DialoguePath[] friendshipPath1, friendshipPath2, friendshipPath3; //Random Text they can say depending on friendship levels
    public DialoguePath[] alreadySpoken;
    public DialoguePath[] branchingPaths;
    public DialoguePath[] questCompletePaths;
    public DialoguePath[] dailyQuestPaths; //Dialogue for giving a daily

    public ItemDialoguePath[] itemPaths; // Not to be confused with item specific remarks, which is the outdated system

    /*[ContextMenu("Numerate all paths")]
    public void NumerateDialoguePaths()
    {
        for(int i = 0; i < paths.Length; i++)
        {
            paths[i].pathName = i + 
        }
    }*/

}

public enum Emotion
{
    //To dictate which audio is played alongside the dialogue
    Neutral,
    Happy,
    Sad,
    Angry,
    Confused,
    Shocked,
    Null
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
    ItemSpecific,
    AlreadySpoken,
    BranchingPaths,
    GivingDaily,
    ItemPath
}

[System.Serializable]
public class DialoguePath
{
    public string pathName;
    //public string functionName; //for calling a specific function
    [TextArea(5,10)]
    public string[] paragraphs;
    public List<Emotion> emotions;
}

[System.Serializable]
public class ItemDialoguePath
{
    public string pathName;
    public DialoguePath dialoguePath;
    public List<InventoryItemData> validItems = new List<InventoryItemData>(); //Items that trigger this path
    public DialoguePathType pathType; //Handle groups of items such as bugs, edibles, ect. Call these groups after looking for applicable Default paths
}

public enum DialoguePathType
{
    Default,
    Bug,
    Consumable,
    Seed
}
