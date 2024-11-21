using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSaveData : MonoBehaviour
{
    public static GameSaveData Instance;

    public float pStamina;
    public float pWater;

    [Header("Main Quest Progression Bools. All must be false when building")]
    public bool rascalWantsFood;
    public bool rascalMentionedKey;

    //[Header("NPC Fed Bools")]
    //public bool rascalFed = false;
    //public bool lumberjackFed = false;

    //[Header("NPC FriendShip Levels")]
    //public int rascalFriendShip = 0;
    //public int lumberJackFriendShip = 0;

    //IF WE HAVE THE GAME ONLY SAVE AT THE MORNING LIKE STARDEW, WE DONT HAVE TO SAVE ALOT OF STUFF LIKE TOWNSPOPLE POS AND SHOP ITEMS
    //[Header("Current Shop Items")]
    //public int rascalFriendShip = 0;
    //public int lumberJackFriendShip = 0;
    



    void Awake()
    {
        //load all the data, then have each class that uses the ISaveable namespace call their load function
    }
}
