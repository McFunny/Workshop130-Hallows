using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    public List<Quest> activeQuests = new List<Quest>();

    public List<Quest> completedQuests = new List<Quest>();

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else Instance = this;

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddQuest(Quest q)
    {
        if(!activeQuests.Contains(q))
        {
            activeQuests.Add(q);
        }
    }

    public void ForceCompleteQuest(Quest q)
    {
        for(int i = 0; i < activeQuests.Count; i++)
        {
            if(activeQuests[i] == q)
            {
                activeQuests[i].progress = activeQuests[i].maxProgress;
                activeQuests[i].alreadyCompleted = true;
                print("Quest was successfully completed");
                return;
            }
        }
    }

    //This is probably bad practice, and should be changed into using Unity Events instead
    public void CreatureDeath(CreatureObject c)
    {
        for(int i = 0; i < activeQuests.Count; i++)
        {
            HuntQuest hQuest = activeQuests[i] as HuntQuest;
            if(hQuest == null) continue;
            
            if(hQuest.targetCreature == c && hQuest.progress != hQuest.maxProgress)
            {
                hQuest.progress++;
                return;
            }
        }
    }

    public void CropHarvested(CropData c)
    {
        for(int i = 0; i < activeQuests.Count; i++)
        {
            GrowQuest gQuest = activeQuests[i] as GrowQuest;
            if(gQuest == null) continue;
            
            if(gQuest.desiredCrop == c && gQuest.progress != gQuest.maxProgress)
            {
                gQuest.progress++;
                return;
            }
        }
    }
}

[System.Serializable]
public class Quest
{
    //A quest is completed by either achieving its goal (main quests) or telling an npc it is done (sub quests)
    //If its a subquest, it will use the progress variables to determine if it is done. Main quest stuff is on a case by case basis currently
    //Later I should add a thing to randomize stuff, such as random crop type, random amount, and a multiplier for the money earned
    public string name;
    [TextArea(5,10)]
    public string description; //Use the same method I used in the dialogue controller to parse the code in the strings
    public QuestType type; //Dont worry about this, currently unnused
    public bool isMajorQuest = false;
    public bool alreadyCompleted = false; //if you want to store completed quests, or just store completed main quests.
    public int mintReward;
    public int progress;
    public int maxProgress; //Just because its at max progress does NOT mean a quest is completed. You still need to check in with the assignee if there is one
    //public InventoryItemData[] itemRewards;
    public int daysLeft = -1; //if -1, there is no time limit. Will need to setup this with the new day function to tick these down by 1 and then remove them later. Unimplemented

    public Character assignee; //use an enum to keep track of NPCs, and fill that in here

    public bool displayProgress = false; //if set to false, should hide the progess bar in the codex// BY DEFAULT, IF MAX PROGRESS IS 0, THE BAR SHOULD BE HIDDEN

    //Find a way to tie this quest into an empty new codex page. Should have like 20 empty pages just in case
}
[System.Serializable]
public class FetchQuest: Quest //Should hide progress, and max progress should be 0
{
    public InventoryItemData desiredItem;
    public int amount;

    public FetchQuest(InventoryItemData _desiredItem, int _amount)
    {
        desiredItem = _desiredItem;
        amount = _amount;
    }

    /*public FetchQuest(InventoryItemData _desiredItem, int amountMax, int amountMin) //randomizes fetch quest
    {
        //
    }*/
}

[System.Serializable]
public class HuntQuest: Quest //max progress should be amount
{
    public CreatureObject targetCreature;
    public int amount;

    public HuntQuest(CreatureObject _targetCreature, int _amount)
    {
        targetCreature = _targetCreature;
        amount = _amount;
    }
}

[System.Serializable]
public class GrowQuest: Quest //max progress should be amount
{
    public CropData desiredCrop;
    public InventoryItemData desiredItem;
    public int amount;

    public GrowQuest(InventoryItemData _desiredItem, int _amount, CropData _desiredCrop)
    {
        desiredItem = _desiredItem;
        amount = _amount;
        desiredCrop = _desiredCrop;
    }
}

/*[System.Serializable]
public class MiscQuest: Quest //For odd things like delivering an item or paying money to an npc
{
    public CreatureObject targetCreature;
    public int amount;

    public HuntQuest(CreatureObject _targetCreature, int _amount)
    {
        targetCreature = _targetCreature;
        amount = _amount;
    }
}*/

public enum QuestType
{
    Main,
    Collect,
    Tinkerer,
    Hunt
}
