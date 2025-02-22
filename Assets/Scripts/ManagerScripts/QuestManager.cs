using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    public List<Quest> activeQuests = new List<Quest>();

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
}

[System.Serializable]
public class Quest
{
    public string name;
    [TextArea(5,10)]
    public string description;
    public QuestType type;
    public bool isMajorQuest = false;
    public int mintReward;
    public int progress;
    public int maxProgress;
    //public InventoryItemData[] itemRewards;

    //Reference for the codex page maybe? idk how he set it up
}
[System.Serializable]
public class FetchQuest: Quest
{
    public InventoryItemData desiredItem;
    public int amount;

    public FetchQuest(InventoryItemData _desiredItem, int _amount)
    {
        desiredItem = _desiredItem;
        amount = _amount;
    }
}

[System.Serializable]
public class HuntQuest: Quest
{
    public CreatureObject targetCreature;
    public int amount;

    public HuntQuest(CreatureObject _targetCreature, int _amount)
    {
        targetCreature = _targetCreature;
        amount = _amount;
    }
}

public enum QuestType
{
    Main,
    Collect,
    Tinkerer,
    Hunt
}
