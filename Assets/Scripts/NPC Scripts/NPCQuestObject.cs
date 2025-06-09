using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Quest Object", menuName = "NPC Objects/NPC Quest Object")]
public class NPCQuestObject : ScriptableObject
{
    public Character character; //Who is this for

    public List<QuestTemplate> templates = new List<QuestTemplate>();

    public float uniqueQuestChance;

    public List<Quest> uniqueQuests = new List<Quest>();

    [HideInInspector] public int currentDailyQuestPath = 0; //Use this to give unique dialogue per quest 
    
    
    public Quest GrabQuest()
    {
        Debug.Log("Grabbing Quest From Object");
        int iterations = 0;
        Quest chosenQuest = null;
        while(iterations < 10 && chosenQuest == null)
        {
            int index = Random.Range(0, templates.Count);
            if(templates[index].creature) chosenQuest = new HuntQuest(templates[index]);
            else if(templates[index].item) chosenQuest = new FetchQuest(templates[index]);
            else if(templates[index].crop) chosenQuest = new GrowQuest(templates[index]);

            if(QuestManager.Instance.CheckForQuest(chosenQuest)) chosenQuest = null;
            else currentDailyQuestPath = templates[index].dialogPath;
        }
        return chosenQuest; //Make sure its still the correct type
    }

    public Quest GrabSampleQuest()
    {
        Quest chosenQuest = null;
        int index = Random.Range(0, templates.Count);
        if(templates[index].creature) chosenQuest = new HuntQuest(templates[index]);
        else if(templates[index].item) chosenQuest = new FetchQuest(templates[index]);
        else if(templates[index].crop) chosenQuest = new GrowQuest(templates[index]);
        return chosenQuest; 
    }
}

[System.Serializable]
public class QuestTemplate
{
    //This template is used to create a fresh semi-random quest
    public string name; 
    [TextArea(5,10)]
    public string description; 
    public int dialogPath = 0;

    public CreatureObject creature; //For hunt quests
    public InventoryItemData item; //For fetch
    public CropData crop; //For grow

    public float mintMultiplier = 1; //multiplies mint reward by this
    public List<ItemWithAmount> itemRewards = new List<ItemWithAmount>(); //Leave empty or a slot empty for chance to reward mints instead of an item
    public int townFavorReward;

    public int daysLeftMin, daysLeftMax; //if min is 0, then there is no time limit

    public Character assignee; //use an enum to keep track of NPCs, and fill that in here

    public bool displayProgress = false; //if set to false, should hide the progess bar in the codex// BY DEFAULT, IF MAX PROGRESS IS 0, THE BAR SHOULD BE HIDDEN

    public int minObject, maxObject;

    /*public Quest()
    {
        daysLeft = -1;
        questID = -1;
    }*/

}
