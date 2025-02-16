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
    public bool rascalWantsFood; //Rascal told the player they want a carrot //Outdated
    public bool rascalMentionedKey; //Rascal got the carrot and told the player about the key
    public bool lumber_offersDeal; //Lumberjack was spoken to and offered to chop the tree for 200 mints
    public bool lumber_choppedTree; //Lumberjack said he will chop the tree
    public bool bridgeCleared; //Tree was cleared
    public bool keyCollected; //Key was picked up
    public bool catacombUnlocked; //Key used to unlock door to catacombs

    [Header("NPC Bools. All must be false when building")]
    public bool rascalMet, botMet, lumberMet, barMet, tinkMet, apothMet, culMet;

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
        SaveLoad.OnSaveGame += SaveData;
    }

    private void OnDisable()
    {
        SaveLoad.OnLoadGame -= LoadData;
        SaveLoad.OnSaveGame -= SaveData;
    }

    public void SaveData()
    {
        var currentSaveData = new AllGameSaveData(this);

        //Debug.Log("Saving stamina. Result: " + currentSaveData.pStamina);

        SaveLoad.CurrentSaveData.allGameSaveData = currentSaveData;

        //var inventoryData = new PlayerInventorySaveData(primaryInventorySystem, secondaryInventorySystem, secondaryInventorySize);
        //SaveLoad.CurrentSaveData.playerInventoryData = inventoryData;

        Debug.Log("General Stats saved");

    }

    private void LoadData(SaveData data)
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
            rascalMet = data.allGameSaveData.rascalMet;
            botMet = data.allGameSaveData.botMet;
            lumberMet = data.allGameSaveData.lumberMet;
            barMet = data.allGameSaveData.barMet;
            tinkMet = data.allGameSaveData.tinkMet;
            apothMet = data.allGameSaveData.apothMet;
            culMet = data.allGameSaveData.culMet;
    }
}
    [System.Serializable]
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
        public bool rascalMet;
        public bool botMet;
        public bool lumberMet;
        public bool barMet;
        public bool tinkMet;
        public bool apothMet;
        public bool culMet;

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
            rascalMet = data.rascalMet;
            botMet = data.botMet;
            lumberMet = data.lumberMet;
            barMet = data.barMet;
            tinkMet = data.tinkMet;
            apothMet = data.apothMet;
            culMet = data.culMet;
    //Debug.Log("Saving stamina. Result: " + pStamina);
}
    }

