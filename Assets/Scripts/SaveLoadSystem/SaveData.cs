using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaveLoadSystem
{
    [System.Serializable]
    public class SaveData
    {
        public List<string> collectedItems; // List of collected item IDs
        public SerializableDictionary<string, ChestSaveData> chestDictionary; // Chest data with UniqueID
        public SerializableDictionary<string, ItemPickupSaveData> activeItems; // Active pickups with UniqueID

        // Instead of a dictionary, we store a single instance of player inventory
        public PlayerInventorySaveData playerInventoryData;
        public AllGameSaveData allGameSaveData;

        //public AllStructuresSaveData allStructuresSaveData;

        public SaveData()
        {
            collectedItems = new List<string>();
            activeItems = new SerializableDictionary<string, ItemPickupSaveData>();
            chestDictionary = new SerializableDictionary<string, ChestSaveData>();
            playerInventoryData = new PlayerInventorySaveData(); // Initialize player inventory data
            allGameSaveData = new AllGameSaveData();
            //allStructuresSaveData = new AllStructuresSaveData();
        }
    }
}
