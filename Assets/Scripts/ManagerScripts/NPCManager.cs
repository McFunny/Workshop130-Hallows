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


    [Header("NPC Fed Bools")]
    public bool rascalFed = false;
    public bool lumberjackFed = false;
    public bool botanistFed = false;
    public bool barkeepFed = false;

    [Header("NPC Spoken Bools")]
    public bool rascalSpoke = false;
    public bool lumberjackSpoke = false;
    public bool botanistSpoke = false;
    public bool barkeepSpoke = false;

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
    }

    public void HourUpdate()
    {
        if(TimeManager.Instance.currentHour == 6)
        {
            rascalSpoke = false;
            lumberjackSpoke = false;
            botanistSpoke = false;
            barkeepSpoke = false;
        }
    }

    private void OnDisable()
    {
        TimeManager.OnHourlyUpdate -= HourUpdate;
    }

}
