using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    //MANAGER FOR NPC QUEST AND DIALOGUE PROGRESSION

    public static NPCManager Instance;

    [Header("Main Quest Progression Bools")]
    public bool rascalWantsFood;
    public bool rascalMentionedKey;


    //[Header("NPC Fed Bools")]
    //public bool rascalFed = false;
    //public bool lumberjackFed = false;

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

}
