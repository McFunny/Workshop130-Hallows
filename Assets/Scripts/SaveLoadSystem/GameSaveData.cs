using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSaveData : MonoBehaviour
{
    public static GameSaveData Instance;

    public float pStamina;
    public float pWater;

    [Header("Main Quest Progression Bools. All must be false when building")]
    public bool tutorialMerchantSpoke;
    public bool rascalWantsFood;
    public bool rascalMentionedKey;
    public bool lumber_offersDeal;
    public bool lumber_choppedTree;
    public bool keyCollected;
    public bool catacombUnlocked;

    //[Header("NPC FriendShip Levels")]
    //public float rascalFriendShip = 0;
    //public float lumberJackFriendShip = 0;

    //IF WE HAVE THE GAME ONLY SAVE AT THE MORNING LIKE STARDEW, WE DONT HAVE TO SAVE ALOT OF STUFF LIKE TOWNSPEOPLE POS AND SHOP ITEMS
    



    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }
        //load all the data, then have each class that uses the ISaveable namespace call their load function
    }
}
