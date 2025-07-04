using SaveLoadSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSaveData : MonoBehaviour
{
    public static GameSaveData Instance;

    public float pStamina, pFatigue;
    public float pWater;
    public int pCurrentMoney;
    public int pTotalMoneyEarned;
    public int pDaysSinceDeath;
    public int pDayNumber;
    public string gameMode;

    public float currentMoney, totalEarnedMoney; //is this used because I dont think so?

    public int hourSaved = 8;

    [Header("Player Upgrade Variables. All must be false when building")]
    public bool gainedInventoryUpgrade = false;
    public bool gainedWaterStorage = false;

    [Header("Main Quest Progression Bools. All must be false when building")]
    public bool tutorialMerchantSpoke; //Tutorial Complete
    public bool rascalWantsFood; //Rascal told the player they want a carrot //Outdated
    public bool rascalMentionedKey; //Rascal got the carrot and told the player about the key
    public bool lumber_offersDeal; //Lumberjack was spoken to and offered to chop the tree for x amount of mints
    public bool lumber_choppedTree; //Lumberjack said he will chop the tree //OBSOLETE
    public bool bridgeCleared; //Tree was cleared //OBSOLETE
    public bool keyCollected; //Key was picked up
    public bool catacombUnlocked; //Key used to unlock door to catacombs
    public bool wildernessIntroduced; //Merchant has informed the player about the wilderness
    public bool playerHasBox; //Player currently has the box in their inventory, chest, or farm
    public bool watergunObtained;
    public bool bugNetObtained;
    public bool scytheObtained;

    public bool mm_giveBarricade; //Merchant handed the player a barricade at the start
    public bool cm_giveChest; //Craftsman handed the player a chest at the start
    public bool mm_giveGun; //Merchant handed the gun after the first day, and gave the "Go to rascal" quest
    public bool bot_giveSeeds; //Botanist gave the player 10 timber ear seeds at the start
    public bool bot_explainedPollen; //Player bought a seed requiring pollination
    public bool ras_askedForNet; //Gave the find my net quest
    public bool bot_explainedTrellis; //Player bought a seed requiring a trellis

    public bool townTreeCleared1; //Tree by bridge
    public bool townTreeCleared2; //Extra tree by cabin

    [Header("NPC Bools. All must be false when building")]
    public bool rascalMet, botMet, lumberMet, barMet, tinkMet, apothMet, culMet, travMet, graveMet, fanMet, butchMet, carpMet, mandrakeMet;

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
        PlayerInteraction.Instance.fatigue = data.allGameSaveData.pFatigue;
        PlayerInteraction.Instance.waterHeld = data.allGameSaveData.pWater;
        PlayerInteraction.Instance.currentMoney = data.allGameSaveData.pCurrentMoney;
        PlayerInteraction.Instance.totalMoneyEarned = data.allGameSaveData.pTotalMoneyEarned;
        PlayerInteraction.Instance.daysSinceDeath = data.allGameSaveData.pDaysSinceDeath;
        PlayerInteraction.Instance.playerUpgrades.LoadData(data.allGameSaveData);
        TimeManager.Instance.dayNum = data.allGameSaveData.pDayNumber;
        TimeManager.Instance.currentHour = data.allGameSaveData.hourSaved;
        if(data.allGameSaveData.hourSaved == 0) TimeManager.Instance.currentHour = 8;
        TimeManager.Instance.RefreshSkybox();

        switch(data.allGameSaveData.gameMode)
        {
            case "Normal":
            MainMenuScript.currentFileMode = FileMode.Normal;
            break;
            case "Cozy":
            MainMenuScript.currentFileMode = FileMode.Cozy;
            break;
            default:
            MainMenuScript.currentFileMode = FileMode.Normal;
            break;
        }

        //for(int i = 0; i < data.allGameSaveData.activeQuests.Length; i++) QuestManager.Instance.activeQuests.Add(data.allGameSaveData.activeQuests[i]);
        QuestManager.Instance.LoadData(data.allGameSaveData);

        CropDatabase.Instance.LoadStats(data.allGameSaveData);
        CreatureDatabase.Instance.LoadStats(data.allGameSaveData);
        if(data.allGameSaveData.bugStats != null) BugDatabase.Instance.LoadStats(data.allGameSaveData);

        tutorialMerchantSpoke = data.allGameSaveData.tutorialMerchantSpoke;
        rascalWantsFood = data.allGameSaveData.rascalWantsFood;
        rascalMentionedKey = data.allGameSaveData.rascalMentionedKey;
        lumber_offersDeal = data.allGameSaveData.lumber_offersDeal;
        lumber_choppedTree = data.allGameSaveData.lumber_choppedTree;
        bridgeCleared = data.allGameSaveData.bridgeCleared;
        keyCollected = data.allGameSaveData.keyCollected;
        catacombUnlocked = data.allGameSaveData.catacombUnlocked;
        wildernessIntroduced = data.allGameSaveData.wildernessIntroduced;
        playerHasBox = data.allGameSaveData.playerHasBox;

        rascalMet = data.allGameSaveData.rascalMet;
        botMet = data.allGameSaveData.botMet;
        lumberMet = data.allGameSaveData.lumberMet;
        barMet = data.allGameSaveData.barMet;
        tinkMet = data.allGameSaveData.tinkMet;
        apothMet = data.allGameSaveData.apothMet;
        culMet = data.allGameSaveData.culMet;

        townTreeCleared1 = data.allGameSaveData.townTreeCleared1;
        townTreeCleared2 = data.allGameSaveData.townTreeCleared2;
        watergunObtained = data.allGameSaveData.watergunObtained;
        bugNetObtained = data.allGameSaveData.bugNetObtained;
        scytheObtained = data.allGameSaveData.scytheObtained;

        mm_giveBarricade = data.allGameSaveData.mm_giveBarricade;
        cm_giveChest = data.allGameSaveData.cm_giveChest;
        mm_giveGun = data.allGameSaveData.mm_giveGun;
        bot_giveSeeds = data.allGameSaveData.bot_giveSeeds;
        bot_explainedPollen = data.allGameSaveData.bot_explainedPollen;
        bot_explainedTrellis = data.allGameSaveData.bot_explainedTrellis;
        ras_askedForNet = data.allGameSaveData.ras_askedForNet;

        travMet = data.allGameSaveData.travMet;
        graveMet = data.allGameSaveData.graveMet;
        fanMet = data.allGameSaveData.fanMet;
        butchMet = data.allGameSaveData.butchMet;
        carpMet = data.allGameSaveData.carpMet;
        mandrakeMet = data.allGameSaveData.mandrakeMet;
    }
}
    [System.Serializable]
    public struct AllGameSaveData
    {
        public float pStamina;
        public float pFatigue;
        public float pWater;
        public int pCurrentMoney;
        public int pTotalMoneyEarned;
        public int pDaysSinceDeath;
        public int pDayNumber;
        public int hourSaved;

        public string gameMode;

        public bool gainedInventoryUpgrade;
        public bool gainedWaterStorage;

        public Quest[] activeQuests;
        public FetchQuest[] activeFetchQuests;
        public HuntQuest[] activeHuntQuests;
        public GrowQuest[] activeGrowQuests;

        public CropPlayerStats[] cropStats;
        public CreaturePlayerStats[] creatureStats;
        public int[] bugStats;

        public bool tutorialMerchantSpoke;
        public bool rascalWantsFood;
        public bool rascalMentionedKey;
        public bool lumber_offersDeal;
        public bool lumber_choppedTree;
        public bool bridgeCleared;
        public bool keyCollected;
        public bool catacombUnlocked;
        public bool wildernessIntroduced;
        public bool playerHasBox;

        public bool rascalMet;
        public bool botMet;
        public bool lumberMet;
        public bool barMet;
        public bool tinkMet;
        public bool apothMet;
        public bool culMet;
        public bool travMet, graveMet, fanMet, butchMet, carpMet, mandrakeMet;

        public bool townTreeCleared1, townTreeCleared2;
        public bool watergunObtained;
        public bool bugNetObtained;
        public bool scytheObtained;

        public bool mm_giveBarricade;
        public bool cm_giveChest;
        public bool mm_giveGun;
        public bool bot_giveSeeds;
        public bool bot_explainedPollen;
        public bool ras_askedForNet;
        public bool bot_explainedTrellis;

    public AllGameSaveData(GameSaveData data)
    {
        gainedInventoryUpgrade = PlayerInteraction.Instance.playerUpgrades.gainedInventoryUpgrade;
        gainedWaterStorage = PlayerInteraction.Instance.playerUpgrades.gainedWaterStorage; //Put this first so the maxwater amount will be correct

        pStamina = PlayerInteraction.Instance.stamina;
        pFatigue = PlayerInteraction.Instance.fatigue;
        pWater = PlayerInteraction.Instance.waterHeld;
        pCurrentMoney = PlayerInteraction.Instance.currentMoney;
        pTotalMoneyEarned = PlayerInteraction.Instance.totalMoneyEarned;
        pDayNumber = TimeManager.Instance.dayNum;
        hourSaved = TimeManager.Instance.currentHour;
        pDaysSinceDeath = PlayerInteraction.Instance.daysSinceDeath;
        gameMode = MainMenuScript.currentFileMode.ToString();

        

        //activeQuests = QuestManager.Instance.activeQuests.ToArray();

        QuestManager.Instance.SaveQuestData(out activeQuests, out activeFetchQuests, out activeHuntQuests, out activeGrowQuests);

        CropDatabase.Instance.SaveStats(out cropStats);
        CreatureDatabase.Instance.SaveStats(out creatureStats);
        BugDatabase.Instance.SaveStats(out bugStats);


        tutorialMerchantSpoke = data.tutorialMerchantSpoke;
        rascalWantsFood = data.rascalWantsFood;
        rascalMentionedKey = data.rascalMentionedKey;
        lumber_offersDeal = data.lumber_offersDeal;
        lumber_choppedTree = data.lumber_choppedTree;
        bridgeCleared = data.bridgeCleared;
        keyCollected = data.keyCollected;
        catacombUnlocked = data.catacombUnlocked;
        wildernessIntroduced = data.wildernessIntroduced;
        playerHasBox = data.playerHasBox;

        rascalMet = data.rascalMet;
        botMet = data.botMet;
        lumberMet = data.lumberMet;
        barMet = data.barMet;
        tinkMet = data.tinkMet;
        apothMet = data.apothMet;
        culMet = data.culMet;

        townTreeCleared1 = data.townTreeCleared1;
        townTreeCleared2 = data.townTreeCleared2;
        watergunObtained = data.watergunObtained;
        bugNetObtained = data.bugNetObtained;
        scytheObtained = data.scytheObtained;

        mm_giveBarricade = data.mm_giveBarricade;
        cm_giveChest = data.cm_giveChest;
        mm_giveGun = data.mm_giveGun;
        bot_giveSeeds = data.bot_giveSeeds;
        bot_explainedPollen = data.bot_explainedPollen;
        bot_explainedTrellis = data.bot_explainedTrellis;
        ras_askedForNet = data.ras_askedForNet;

        travMet = data.travMet;
        graveMet = data.graveMet;
        fanMet = data.fanMet;
        butchMet = data.butchMet;
        carpMet = data.carpMet;
        mandrakeMet = data.mandrakeMet;

//Debug.Log("Saving stamina. Result: " + pStamina);
    }
    }

