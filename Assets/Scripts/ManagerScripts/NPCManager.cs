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

    //Look at how quantums saves their bool system with a dictionary. Take pics for ref
    //static Dictionary<BoolKey, bool> NarrativeBools = new Dictionary<BoolKey, bool>(); 
    //NarrativeBools.Add("BoolKey.RascalWantsFood", rascalWantsFood);
    //NarrativeBools.Add("BoolKey.RascalMentionedKey", rascalMentionedKey);

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

    public void InitializeDictionary()
    {
        //called after the save data becomes updated
    }

}

public enum BoolKey
{
    RascalWantsFood,
    RascalMentionedKey
}
