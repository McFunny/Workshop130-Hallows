using SaveLoadSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSaveData : MonoBehaviour
{
    public static GameSaveData Instance;

    [Header("References to pets in scene. These must be filled manually")]
    public PetBehaviorScript catRef;
    public PetBehaviorScript grubRef;
    public PetBehaviorScript dogRef;
    public PetBehaviorScript rockRef;
    [HideInInspector] public PetBehaviorScript currentPet;

    [Header("Player Variables")]

    public float pStamina, pFatigue;
    public float pWater;
    public int pCurrentMoney;
    public int pTotalMoneyEarned;
    public int pDaysSinceDeath;
    public int pDayNumber;
    public string gameMode;
    public bool lostKukri;

    public int hourSaved = 8;

    [Header("Player Upgrade Variables. All must be false when building")]
    public bool gainedInventoryUpgrade = false;
    public bool gainedWaterStorage = false;
    public bool gainedWaterPack = false;
    public int trinketSlotsGiven = 0; //Specifically by the fanatic

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
    public bool pistolObtained;
    public bool kukriObtained;
    public bool testerObtained;
    public bool upg_can, upg_hoe, upg_scythe, upg_torch;

    public bool mm_giveBarricade; //Merchant handed the player a barricade at the start
    public bool cm_giveChest; //Craftsman handed the player a chest at the start
    public bool mm_giveGun; //Merchant handed the gun after the first day, and gave the "Go to rascal" quest
    public bool bot_giveSeeds; //Botanist gave the player 10 timber ear seeds at the start
    public bool bot_giveScytheQuest; //Botanist gave the player 10 Gloomstalk seeds after the timber quest
    public bool bot_explainedPollen; //Player bought a seed requiring pollination
    public bool ras_askedForNet; //Gave the find my net quest
    public bool bot_explainedTrellis; //Player bought a seed requiring a trellis
    public bool fan_giveBombs; //Fanatic gave player bathbombs at the start
    public bool apo_readScroll; //Apoth has recieved the scroll and will start selling the seeds
    public bool apo_explainedSiege; //Apoth has explained they read the scroll and have explained the seeds
    public bool apo_explainedTitanSeed; //Apoth has explained how the titan seeds work
    public bool mm_introducedPets; //Merchant explained pets
    public bool mm_soldPet; //Player got their first pet from the merchant
    public bool tra_askedForFood; //Traveller offered kukri for food
    public bool mil_gavePen; // Miller gave the player a hog pen after they cleared the barn
    public bool cm_offersKit; //Player told craftsman about the broken wagon. He will start selling the kit
    public bool playerWagonFound; // Player found the broken wagon in the barn
    public bool playerWagonUnlocked; // Player repaired the broken wagon in the barn
    public bool apo_wasKidnapped; //Apothocary is gone
    public bool apo_rescued; //Apothocary is saved
    public bool apo_thanked; //Apothocary thanked player for rescueing
    public bool tav_reportedApoMissing; //Tavernkeep told player she is missing
    public bool fan_ApoGoneComment; //Extra dialogue from the fanatic from the missing apoth
    public bool cm_refusedRepairs; //Craftsman refused to repair the wagon. Apoth must be kidnapped first
    public bool tink_explainedTickets;
    public bool bot_newWares; // The bot needs to explain they have new seeds. Turns to false after explaination given
    public bool tink_newWares; // The bot needs to explain they have new gear. Turns to false after explaination given
    public bool mm_introducedTickets; // Merchant explained the ticket box
    public bool apo_gaveTissueQuest; // Apoth asked for the tissue samples
    public bool apo_gaveCure; // Apoth gave the recipe to the purifying flask, meaning the player completed that quest, and she will now sell the marigleam
    public bool cul_gaveCrock; // Player completed cooking tutorial
    public bool mm_sellOnlyRocks;
    public bool tink_foundTrinketRecipes; //Player spoke to tinkerer after getting a trinket slot from the fanatic. She explains he filled her machine with doodles

    public bool townTreeCleared1; //Tree by bridge
    public bool townTreeCleared2; //Extra tree by cabin blocking barn

    public int botShopLevel;
    public int mintsDonatedToBot;
    public int tTicketsHeld; // How many tickets the player has that they have not redeemed
    public float tTicketMintProgress; // How much progress was made since last recipe unlock
    public int tTicketsAvailable; // How many are sitting in the box

    [Header("Siege Progression Bools. All must be false when building")]
    public int siegesCleared = 0;
    public bool siegeCropInHand; //The Player is holding the seed but hasnt planted it
    public int siegesLost = 0; //Tracks how many times this CURRENT siege was failed. Resets after a siege is completed

    [Header("NPC Bools. All must be false when building")]
    public bool rascalMet, botMet, lumberMet, barMet, tinkMet, apothMet, culMet, travMet, graveMet, fanMet, butchMet, carpMet, mandrakeMet, millerMet;

    [Header("Critter Save Array")]
    public List<CritterData> critterData = new List<CritterData>();
    public int manikkinsAlive = 0;
    public List<int> deadHenIDs = new List<int>();

    [Header("Survival Mode")]
    public List<int> boughtItemIDs = new List<int>();

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
        PlayerInteraction.Instance.lostKukri = data.allGameSaveData.lostKukri;
        PlayerInteraction.Instance.currentMoney = data.allGameSaveData.pCurrentMoney;
        PlayerInteraction.Instance.totalMoneyEarned = data.allGameSaveData.pTotalMoneyEarned;
        if(MainMenuScript.currentFileMode == FileMode.Survival) SurvivalModeManager.Instance.TotalMintsEarned = data.allGameSaveData.pTotalMoneyEarned;
        PlayerInteraction.Instance.daysSinceDeath = data.allGameSaveData.pDaysSinceDeath;
        PlayerInteraction.Instance.playerUpgrades.LoadData(data.allGameSaveData);

        trinketSlotsGiven = data.allGameSaveData.trinketSlotsGiven;
        TimeManager.Instance.dayNum = data.allGameSaveData.pDayNumber;
        TimeManager.Instance.currentHour = data.allGameSaveData.hourSaved;
        if(data.allGameSaveData.hourSaved == 0) TimeManager.Instance.currentHour = 8;
        TimeManager.Instance.RefreshSkybox();

        WagonManager.Instance.wagonHealth = data.allGameSaveData.wagonHealth;
        WagonManager.Instance.maxWagonHealth = data.allGameSaveData.maxWagonHealth;
        WagonManager.Instance.daysToRepair = data.allGameSaveData.daysToRepairWagon;


        switch(data.allGameSaveData.gameMode)
        {
            case "Normal":
            MainMenuScript.currentFileMode = FileMode.Normal;
            break;
            case "Cozy":
            MainMenuScript.currentFileMode = FileMode.Cozy;
            break;
            case "Survival":
            MainMenuScript.currentFileMode = FileMode.Survival;
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
        if(data.allGameSaveData.critterStats != null) BarnManager.Instance.LoadStats(data.allGameSaveData);
        if(data.allGameSaveData.structStats != null) StructureDatabase.Instance.LoadStats(data.allGameSaveData);
        if(data.allGameSaveData.craftingStats != null) CraftingDatabase.Instance.LoadStats(data.allGameSaveData);
        if(data.allGameSaveData.cookingStats != null) CookingDatabase.Instance.LoadStats(data.allGameSaveData);
        // Place the code for recipe loading

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
        pistolObtained = data.allGameSaveData.pistolObtained;
        kukriObtained = data.allGameSaveData.kukriObtained;
        testerObtained = data.allGameSaveData.testerObtained;
        upg_can = data.allGameSaveData.upg_can;
        upg_hoe = data.allGameSaveData.upg_hoe;
        upg_scythe = data.allGameSaveData.upg_scythe;
        upg_torch = data.allGameSaveData.upg_torch;

        mm_giveBarricade = data.allGameSaveData.mm_giveBarricade;
        cm_giveChest = data.allGameSaveData.cm_giveChest;
        mm_giveGun = data.allGameSaveData.mm_giveGun;
        bot_giveSeeds = data.allGameSaveData.bot_giveSeeds;
        bot_giveScytheQuest = data.allGameSaveData.bot_giveScytheQuest;
        bot_explainedPollen = data.allGameSaveData.bot_explainedPollen;
        bot_explainedTrellis = data.allGameSaveData.bot_explainedTrellis;
        ras_askedForNet = data.allGameSaveData.ras_askedForNet;
        fan_giveBombs = data.allGameSaveData.fan_giveBombs;
        apo_readScroll = data.allGameSaveData.apo_readScroll;
        apo_explainedSiege = data.allGameSaveData.apo_explainedSiege;
        apo_explainedTitanSeed = data.allGameSaveData.apo_explainedTitanSeed;
        mm_soldPet = data.allGameSaveData.mm_soldPet;
        mm_introducedPets = data.allGameSaveData.mm_introducedPets;
        tra_askedForFood = data.allGameSaveData.tra_askedForFood;
        mil_gavePen = data.allGameSaveData.mil_gavePen;
        cm_offersKit = data.allGameSaveData.cm_offersKit;
        playerWagonUnlocked = data.allGameSaveData.playerWagonUnlocked;
        playerWagonFound = data.allGameSaveData.playerWagonFound;
        apo_wasKidnapped = data.allGameSaveData.apo_wasKidnapped;
        apo_rescued = data.allGameSaveData.apo_rescued;
        apo_thanked = data.allGameSaveData.apo_thanked;
        tav_reportedApoMissing = data.allGameSaveData.tav_reportedApoMissing;
        fan_ApoGoneComment = data.allGameSaveData.fan_ApoGoneComment;
        cm_refusedRepairs = data.allGameSaveData.cm_refusedRepairs;
        tink_explainedTickets = data.allGameSaveData.tink_explainedTickets;
        bot_newWares = data.allGameSaveData.bot_newWares;
        tink_newWares = data.allGameSaveData.tink_newWares;
        mm_introducedTickets = data.allGameSaveData.mm_introducedTickets;
        apo_gaveTissueQuest = data.allGameSaveData.apo_gaveTissueQuest;
        apo_gaveCure = data.allGameSaveData.apo_gaveCure;
        cul_gaveCrock = data.allGameSaveData.cul_gaveCrock;
        mm_sellOnlyRocks = data.allGameSaveData.mm_sellOnlyRocks;
        tink_foundTrinketRecipes = data.allGameSaveData.tink_foundTrinketRecipes;


        travMet = data.allGameSaveData.travMet;
        graveMet = data.allGameSaveData.graveMet;
        fanMet = data.allGameSaveData.fanMet;
        butchMet = data.allGameSaveData.butchMet;
        carpMet = data.allGameSaveData.carpMet;
        mandrakeMet = data.allGameSaveData.mandrakeMet;
        millerMet = data.allGameSaveData.millerMet;

        botShopLevel = data.allGameSaveData.botShopLevel;
        mintsDonatedToBot = data.allGameSaveData.mintsDonatedToBot;
        tTicketsHeld = data.allGameSaveData.tTicketsHeld;
        tTicketMintProgress = data.allGameSaveData.tTicketMintProgress;
        tTicketsAvailable = data.allGameSaveData.tTicketsAvailable;

        siegesCleared = data.allGameSaveData.siegesCleared;
        siegeCropInHand = data.allGameSaveData.siegeCropInHand;
        siegesLost = data.allGameSaveData.siegesLost;

        switch(data.allGameSaveData.petType)
        {
            case "Cat":
                currentPet = catRef;
                break;
            case "Grub":
                currentPet = grubRef;
                break;
            case "Dog":
                currentPet = dogRef;
                break;
            case "Rock":
                currentPet = dogRef;
                break;
            default:
                break;
        }

        if(currentPet)
        {
            currentPet.hunger = data.allGameSaveData.petHunger;
            currentPet.thirst = data.allGameSaveData.petThirst;
            currentPet.name = data.allGameSaveData.petName;
            currentPet.friendPoints = data.allGameSaveData.petProgress;
            currentPet.friendshipLevel = data.allGameSaveData.petLevel;
            currentPet.gameObject.SetActive(true);
        }

        //Spawn Manikkins
        GameObject manikkinPrefab = CreatureDatabase.Instance.GetCreature(20).objectPrefab;
        for(int i = 0; i < manikkinsAlive; i++)
        {
            Instantiate(manikkinPrefab, NightSpawningManager.Instance.RandomMistPosition(), Quaternion.identity);
        }

        if(data.allGameSaveData.deadHenIDs != null && data.allGameSaveData.deadHenIDs.Length > 0) deadHenIDs = new List<int>(data.allGameSaveData.deadHenIDs);

        if(data.allGameSaveData.boughtItemIDs != null && data.allGameSaveData.boughtItemIDs.Length > 0) boughtItemIDs = new List<int>(data.allGameSaveData.boughtItemIDs);
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
        public bool lostKukri;

        public string gameMode;

        public float wagonHealth;
        public float maxWagonHealth;
        public int daysToRepairWagon;

        public bool gainedInventoryUpgrade;
        public bool gainedWaterStorage;
        public bool gainedWaterPack;
        public int trinketSlotsGiven;

        public Quest[] activeQuests;
        public FetchQuest[] activeFetchQuests;
        public HuntQuest[] activeHuntQuests;
        public GrowQuest[] activeGrowQuests;

        public CropPlayerStats[] cropStats;
        public CreaturePlayerStats[] creatureStats;
        public int[] bugStats;
        public CritterData[] critterStats;
        public bool[] structStats;
        public CraftingPlayerStats[] craftingStats;
        public CookingPlayerStats[] cookingStats;

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
        public bool travMet, graveMet, fanMet, butchMet, carpMet, mandrakeMet, millerMet;

        public bool townTreeCleared1, townTreeCleared2;
        public bool watergunObtained;
        public bool bugNetObtained;
        public bool scytheObtained;
        public bool pistolObtained;
        public bool kukriObtained;
        public bool testerObtained;
        public bool upg_can, upg_hoe, upg_scythe, upg_torch;

        public bool mm_giveBarricade;
        public bool cm_giveChest;
        public bool mm_giveGun;
        public bool bot_giveSeeds;
        public bool bot_giveScytheQuest;
        public bool bot_explainedPollen;
        public bool ras_askedForNet;
        public bool bot_explainedTrellis;
        public bool fan_giveBombs;
        public bool apo_readScroll;
        public bool apo_explainedSiege;
        public bool apo_explainedTitanSeed;
        public bool mm_soldPet;
        public bool mm_introducedPets;
        public bool tra_askedForFood;
        public bool mil_gavePen;
        public bool cm_offersKit;
        public bool playerWagonUnlocked;
        public bool playerWagonFound;
        public bool apo_wasKidnapped;
        public bool apo_rescued; 
        public bool apo_thanked; 
        public bool tav_reportedApoMissing;
        public bool fan_ApoGoneComment;
        public bool cm_refusedRepairs;
        public bool tink_explainedTickets;
        public bool bot_newWares; 
        public bool tink_newWares;
        public bool mm_introducedTickets;
        public bool apo_gaveTissueQuest; 
        public bool apo_gaveCure;
        public bool cul_gaveCrock;
        public bool mm_sellOnlyRocks;
        public bool tink_foundTrinketRecipes;

        public int botShopLevel;
        public int mintsDonatedToBot;
        public int tTicketsHeld;
        public float tTicketMintProgress;
        public int tTicketsAvailable;

        public int siegesCleared;
        public bool siegeCropInHand; 
        public int siegesLost;

        public float petHunger, petProgress, petThirst;
        public int petLevel;
        public string petType, petName;

        public int manikkinsAlive;
        public int[] deadHenIDs;
        public int[] boughtItemIDs;

    public AllGameSaveData(GameSaveData data)
    {
        gainedInventoryUpgrade = PlayerInteraction.Instance.playerUpgrades.gainedInventoryUpgrade;
        gainedWaterStorage = PlayerInteraction.Instance.playerUpgrades.gainedWaterStorage; //Put this first so the maxwater amount will be correct
        gainedWaterPack = PlayerInteraction.Instance.playerUpgrades.gainedWaterPack;
        trinketSlotsGiven = data.trinketSlotsGiven;

        pStamina = PlayerInteraction.Instance.stamina;
        pFatigue = PlayerInteraction.Instance.fatigue;
        pWater = PlayerInteraction.Instance.waterHeld;
        lostKukri = PlayerInteraction.Instance.lostKukri;
        pCurrentMoney = PlayerInteraction.Instance.currentMoney;
        pTotalMoneyEarned = PlayerInteraction.Instance.totalMoneyEarned;
        if(MainMenuScript.currentFileMode == FileMode.Survival) pTotalMoneyEarned = SurvivalModeManager.Instance.TotalMintsEarned;
        pDayNumber = TimeManager.Instance.dayNum;
        hourSaved = TimeManager.Instance.currentHour;
        pDaysSinceDeath = PlayerInteraction.Instance.daysSinceDeath;
        gameMode = MainMenuScript.currentFileMode.ToString();

        wagonHealth = WagonManager.Instance.wagonHealth;
        maxWagonHealth = WagonManager.Instance.maxWagonHealth;
        daysToRepairWagon = WagonManager.Instance.daysToRepair;

        

        //activeQuests = QuestManager.Instance.activeQuests.ToArray();

        QuestManager.Instance.SaveQuestData(out activeQuests, out activeFetchQuests, out activeHuntQuests, out activeGrowQuests);

        CropDatabase.Instance.SaveStats(out cropStats);
        CreatureDatabase.Instance.SaveStats(out creatureStats);
        BugDatabase.Instance.SaveStats(out bugStats);
        BarnManager.Instance.SaveStats(out critterStats);
        StructureDatabase.Instance.SaveStats(out structStats);
        CraftingDatabase.Instance.SaveStats(out craftingStats);
        CookingDatabase.Instance.SaveStats(out cookingStats);


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
        travMet = data.travMet;
        graveMet = data.graveMet;
        fanMet = data.fanMet;
        butchMet = data.butchMet;
        carpMet = data.carpMet;
        mandrakeMet = data.mandrakeMet;
        millerMet = data.millerMet;

        townTreeCleared1 = data.townTreeCleared1;
        townTreeCleared2 = data.townTreeCleared2;
        watergunObtained = data.watergunObtained;
        bugNetObtained = data.bugNetObtained;
        scytheObtained = data.scytheObtained;
        kukriObtained = data.kukriObtained;
        pistolObtained = data.pistolObtained;
        testerObtained = data.testerObtained;
        upg_can = data.upg_can;
        upg_hoe = data.upg_hoe;
        upg_scythe = data.upg_scythe;
        upg_torch = data.upg_torch;

        mm_giveBarricade = data.mm_giveBarricade;
        cm_giveChest = data.cm_giveChest;
        mm_giveGun = data.mm_giveGun;
        bot_giveSeeds = data.bot_giveSeeds;
        bot_giveScytheQuest = data.bot_giveScytheQuest;
        bot_explainedPollen = data.bot_explainedPollen;
        bot_explainedTrellis = data.bot_explainedTrellis;
        ras_askedForNet = data.ras_askedForNet;
        fan_giveBombs = data.fan_giveBombs;
        apo_readScroll = data.apo_readScroll;
        apo_explainedSiege = data.apo_explainedSiege;
        apo_explainedTitanSeed = data.apo_explainedTitanSeed;
        mm_soldPet = data.mm_soldPet;
        mm_introducedPets = data.mm_introducedPets;
        tra_askedForFood = data.tra_askedForFood;
        mil_gavePen = data.mil_gavePen;
        cm_offersKit = data.cm_offersKit;
        playerWagonUnlocked = data.playerWagonUnlocked;
        playerWagonFound = data.playerWagonFound;
        apo_wasKidnapped = data.apo_wasKidnapped;
        apo_rescued = data.apo_rescued;
        apo_thanked = data.apo_thanked;
        tav_reportedApoMissing = data.tav_reportedApoMissing;
        fan_ApoGoneComment = data.fan_ApoGoneComment;
        cm_refusedRepairs = data.cm_refusedRepairs;
        tink_explainedTickets = data.tink_explainedTickets;
        bot_newWares = data.bot_newWares;
        tink_newWares = data.tink_newWares;
        mm_introducedTickets = data.mm_introducedTickets;
        apo_gaveTissueQuest = data.apo_gaveTissueQuest;
        apo_gaveCure = data.apo_gaveCure;
        cul_gaveCrock = data.cul_gaveCrock;
        mm_sellOnlyRocks = data.mm_sellOnlyRocks;
        tink_foundTrinketRecipes = data.tink_foundTrinketRecipes;

        botShopLevel = data.botShopLevel;
        mintsDonatedToBot = data.mintsDonatedToBot;
        tTicketsHeld = data.tTicketsHeld;
        tTicketMintProgress = data.tTicketMintProgress;
        tTicketsAvailable = data.tTicketsAvailable;

        siegesCleared = data.siegesCleared;
        siegeCropInHand = data.siegeCropInHand;
        siegesLost = data.siegesLost;

        if(data.currentPet)
        {
            petHunger = data.currentPet.hunger;
            petThirst = data.currentPet.thirst;
            petProgress = data.currentPet.friendPoints;
            petLevel = data.currentPet.friendshipLevel;
            petType = data.currentPet.petType.ToString();
            petName = data.currentPet.name;
        }
        else
        {
            petHunger = 100;
            petThirst = 100;
            petProgress = 0;
            petLevel = 0;
            petType = "";
            petName = "Kevin";
        }

        manikkinsAlive = data.manikkinsAlive;
        deadHenIDs = data.deadHenIDs.ToArray();
        boughtItemIDs = data.boughtItemIDs.ToArray();


//Debug.Log("Saving stamina. Result: " + pStamina);
    }
    }

