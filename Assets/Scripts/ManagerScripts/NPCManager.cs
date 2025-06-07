using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    //MANAGER FOR NPC QUEST AND DIALOGUE PROGRESSION

    public static NPCManager Instance;

    //[Header("Main Quest Progression Bools")]
    //public bool rascalWantsFood;
    //public bool rascalMentionedKey;


    /*[Header("NPC Fed Bools")]
    public bool rascalFed = false;
    public bool lumberjackFed = false;
    public bool botanistFed = false;
    public bool barkeepFed = false;
    public bool tinkererFed = false;
    public bool apothFed = false;
    public bool culinarianFed = false;*/
    //we can add more npcs later when we decide more about them - abner

    public List<NPC> townsPeople = new List<NPC>(); //Dont include any of the wagon merchants

    [Header("NPC Spoken Bools")]
    public bool rascalSpoke = false;
    public bool lumberjackSpoke = false;
    public bool botanistSpoke = false;
    public bool barkeepSpoke = false;
    public bool tinkererSpoke = false;
    public bool apothSpoke = false;
    public bool culinarianSpoke = false;
    public bool travSpoke = false;
    public bool graveSpoke = false;
    public bool fanSpoke = false;
    public bool butchSpoke = false;
    public bool carpSpoke = false;

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else Instance = this;

        //print(nameof(rascalWantsFood));

    }

    void Start()
    {
        TimeManager.OnHourlyUpdate += HourUpdate;
        HourUpdate();
    }

    public void HourUpdate()
    {
        if(TimeManager.Instance.currentHour == 8)
        {
            rascalSpoke = false;
            lumberjackSpoke = false;
            botanistSpoke = false;
            barkeepSpoke = false;
            tinkererSpoke = false;
            apothSpoke = false;
            culinarianSpoke = false;
            travSpoke = false;
            graveSpoke = false;
            fanSpoke = false;
            butchSpoke = false;
            carpSpoke = false;

            GiveDailyQuests();
        }
        
    }

    private void OnDisable()
    {
        TimeManager.OnHourlyUpdate -= HourUpdate;
    }

    void GiveDailyQuests()
    {
        foreach(Quest q in QuestManager.Instance.activeQuests)
        {
            if(q.alreadyCompleted && !q.isMajorQuest) QuestManager.Instance.ForceRemoveQuest(q);
        }

        //Add limit to how many quests, or make sure an npc cannot give multiple quests

        Debug.Log ("Giving Daily");

        int recipients = 1;//Random.Range(0,2);
        int x = 0;
        List<NPC> selectedNPCs = new List<NPC>();
        while(selectedNPCs.Count < recipients && x < 20)
        {
            int r = 0;//Random.Range(0, townsPeople.Count);
            if(!selectedNPCs.Contains(townsPeople[r]))
            {
                selectedNPCs.Add(townsPeople[r]);
                townsPeople[r].GiveDailyQuest(QuestDatabase.Instance.GetDailyQuest(townsPeople[r].character));
                Debug.Log ("NPC Chosen");
            }
            x++;
        }
    }

}
