using SaveLoadSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSaveData : MonoBehaviour
{
    public static GameSaveData Instance;

    public float pStamina;
    public float pWater;
    public int pCurrentMoney;
    public int pTotalMoneyEarned;
    public int pDayNumber;

    public float currentMoney, totalEarnedMoney;

    public int dayNum;

    //

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
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }
        SaveLoad.OnLoadGame += LoadData;
    }

    private void OnDisable()
    {
        SaveLoad.OnLoadGame -= LoadData;
    }

    private void LoadData(SaveData data)
    {
        if (data.allGameSaveData.tutorialMerchantSpoke)
        {
            PlayerInteraction.Instance.stamina = data.allGameSaveData.pStamina;
            PlayerInteraction.Instance.waterHeld = data.allGameSaveData.pWater;
            PlayerInteraction.Instance.currentMoney = data.allGameSaveData.pCurrentMoney;
            PlayerInteraction.Instance.totalMoneyEarned = data.allGameSaveData.pTotalMoneyEarned;
            TimeManager.Instance.dayNum = data.allGameSaveData.pDayNumber;


            tutorialMerchantSpoke = data.allGameSaveData.tutorialMerchantSpoke;
            rascalWantsFood = data.allGameSaveData.rascalWantsFood;
            rascalMentionedKey = data.allGameSaveData.rascalMentionedKey;
            lumber_offersDeal = data.allGameSaveData.lumber_offersDeal;
            lumber_choppedTree = data.allGameSaveData.lumber_choppedTree;
            bridgeCleared = data.allGameSaveData.bridgeCleared;
            keyCollected = data.allGameSaveData.keyCollected;
            catacombUnlocked = data.allGameSaveData.catacombUnlocked;
        }
    }
}

    public struct AllGameSaveData
    {
        public float pStamina;
        public float pWater;
        public int pCurrentMoney;
        public int pTotalMoneyEarned;
        public int pDayNumber;

        public bool tutorialMerchantSpoke;
        public bool rascalWantsFood;
        public bool rascalMentionedKey;
        public bool lumber_offersDeal;
        public bool lumber_choppedTree;
        public bool bridgeCleared;
        public bool keyCollected;
        public bool catacombUnlocked;

        public AllGameSaveData(GameSaveData data)
        {
            pStamina = PlayerInteraction.Instance.stamina;
            pWater = PlayerInteraction.Instance.waterHeld;
            pCurrentMoney = PlayerInteraction.Instance.currentMoney;
            pTotalMoneyEarned = PlayerInteraction.Instance.totalMoneyEarned;
            pDayNumber = TimeManager.Instance.dayNum;
            tutorialMerchantSpoke = data.tutorialMerchantSpoke;
            rascalWantsFood = data.rascalWantsFood;
            rascalMentionedKey = data.rascalMentionedKey;
            lumber_offersDeal = data.lumber_offersDeal;
            lumber_choppedTree = data.lumber_choppedTree;
            bridgeCleared = data.bridgeCleared;
            keyCollected = data.keyCollected;
            catacombUnlocked = data.catacombUnlocked;
        }
    }

