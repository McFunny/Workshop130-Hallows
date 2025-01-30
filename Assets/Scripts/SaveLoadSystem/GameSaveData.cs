using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSaveData : MonoBehaviour
{
    public static GameSaveData Instance;

    public float pStamina;
    public float pWater;

    [Header("Main Quest Progression Bools. All must be false when building")]
    public bool tutorialMerchantSpoke; //Tutorial Complete
    public bool rascalWantsFood; //Rascal told the player they want a carrot
    public bool rascalMentionedKey; //Rascal got the carrot and told the player about the key
    public bool lumber_offersDeal; //Lumberjack was spoken to and offered to chop the tree for 200 mints
    public bool lumber_choppedTree; //Lumberjack said he will chop the tree
    public bool bridgeCleared; //Tree was cleared
    public bool keyCollected; //Key was picked up
    public bool catacombUnlocked; //Key used to unlock door to catacombs

    [Header("NPC Bools. All must be false when building")]
    public bool rascalMet, botMet, lumberMet;

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
