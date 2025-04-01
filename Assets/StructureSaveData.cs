using SaveLoadSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureSaveData : MonoBehaviour
{

    public static StructureSaveData Instance;

    public StructureInventory structureList;
    public StructureDatabase database; //DONT FORGET TO PLUG THIS REFERENCE IN

    int x = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        SaveLoad.OnSaveGame += SaveStructures;
        SaveLoad.OnLoadGame += LoadStructures;
    }


    private void Start()
    {
       //SaveStructures();
    }

    private void OnDisable()
    {
        SaveLoad.OnSaveGame -= SaveStructures;
        SaveLoad.OnLoadGame -= LoadStructures;
    }


    public void SaveStructures()
    {
        x = 0;
        structureList.Clear();
        foreach (StructureBehaviorScript structure in StructureManager.Instance.allStructs)
        {
            if(structure.structData != null)
            {
                structure.SaveVariables();
                //Debug.Log("Saved " + x + " structures");
                structureList.Structures[x] = structure.structData.CreateStructure();
                structureList.Structures[x].health = structure.health;
                structureList.Structures[x].position[0] = structure.gameObject.transform.position.x;
                structureList.Structures[x].position[1] = structure.gameObject.transform.position.y;
                structureList.Structures[x].position[2] = structure.gameObject.transform.position.z;

                structureList.Structures[x].rotation[0] = structure.gameObject.transform.eulerAngles.x;
                structureList.Structures[x].rotation[1] = structure.gameObject.transform.eulerAngles.y;
                structureList.Structures[x].rotation[2] = structure.gameObject.transform.eulerAngles.z;

                //structureList.Structures[x].savedItemList1 = structure.savedItems; //CHANGE TO SAVE A LIST OF ITEM ID'S
                for(int i = 0; i < structure.savedItems.Count; i++)
                {
                    structureList.Structures[x].savedItemIDList1.Add(structure.savedItems[i].ID);
                }

                structureList.Structures[x].savedInt1 = structure.saveInt1;
                structureList.Structures[x].savedInt2 = structure.saveInt2;
                structureList.Structures[x].savedInt3 = structure.saveInt3;
                structureList.Structures[x].savedFloat1 = structure.saveFloat1;
                structureList.Structures[x].savedFloat2 = structure.saveFloat2;
                structureList.Structures[x].savedFloat3 = structure.saveFloat3;
                structureList.Structures[x].savedString1 = structure.saveString1;
                structureList.Structures[x].savedString2 = structure.saveString2;
                structureList.Structures[x].savedString3 = structure.saveString3;
            }
            x++;
        }

        for(int n = 0; n < StructureManager.Instance.Storage.Count; n++)
        {
            structureList.Nutrients[n] = StructureManager.Instance.Storage[n];
            if(structureList.Nutrients[n].waterLevel > 3) print("Water!!!");
        }

        var structureData = new StructureInventory(structureList.Structures, structureList.Nutrients);
        SaveLoad.CurrentSaveData.allStructuresSaveData = structureData;

        Debug.Log("Structures saved successfully. Total: " + x);

    }

    public void LoadStructures(SaveData data)
    {
        Debug.Log("Loading Stuff");
        if (data.allStructuresSaveData == null) return;

            StructureManager.Instance.LoadNutrients(data.allStructuresSaveData.Nutrients);

            for(int i = 0; i < data.allStructuresSaveData.Structures.Length; i++)
            {
                //spawn the Structure and give it it's stats
                if(data.allStructuresSaveData.Structures[i] != null && data.allStructuresSaveData.Structures[i].Id != -1)
                {
                    //Debug.Log("Structure Found: Spawning");
                    //GameObject newStructurePrefab = database.Structures[data.allStructuresSaveData.Structures[i].Id].objectPrefab; //Reference the structure database to grab the prefab
                    GameObject newStructurePrefab = database.Structures[data.allStructuresSaveData.Structures[i].Id].objectPrefab;
                    GameObject newStructure = Instantiate(newStructurePrefab);
                
                    StructureBehaviorScript StructureStats = newStructure.GetComponent<StructureBehaviorScript>();
                    StructureStats.health = data.allStructuresSaveData.Structures[i].health;

                    Vector3 loadedPosition;
                    loadedPosition.x = data.allStructuresSaveData.Structures[i].position[0];
                    loadedPosition.y = data.allStructuresSaveData.Structures[i].position[1];
                    loadedPosition.z = data.allStructuresSaveData.Structures[i].position[2];

                    Vector3 loadedRotation;
                    loadedRotation.x = data.allStructuresSaveData.Structures[i].rotation[0];
                    loadedRotation.y = data.allStructuresSaveData.Structures[i].rotation[1];
                    loadedRotation.z = data.allStructuresSaveData.Structures[i].rotation[2];

                    StructureStats.savedItems = data.allStructuresSaveData.Structures[i].savedItemList1; //CHANGE TO POPULATE A LIST OF ITEMS FROM THE LOADED ITEM IDS
                    for(int c = 0; c < data.allStructuresSaveData.Structures[i].savedItemIDList1.Count; c++)
                    {
                        StructureStats.savedItems.Add(Database.Instance.GetItem(data.allStructuresSaveData.Structures[i].savedItemIDList1[c]));
                    }

                    StructureStats.saveInt1 = data.allStructuresSaveData.Structures[i].savedInt1;
                    StructureStats.saveInt2 = data.allStructuresSaveData.Structures[i].savedInt2;
                    StructureStats.saveInt3 = data.allStructuresSaveData.Structures[i].savedInt3;
                    StructureStats.saveFloat1 = data.allStructuresSaveData.Structures[i].savedFloat1;
                    StructureStats.saveFloat2 = data.allStructuresSaveData.Structures[i].savedFloat2;
                    StructureStats.saveFloat3 = data.allStructuresSaveData.Structures[i].savedFloat3;
                    StructureStats.saveString1 = data.allStructuresSaveData.Structures[i].savedString1;
                    StructureStats.saveString2 = data.allStructuresSaveData.Structures[i].savedString2;
                    StructureStats.saveString3 = data.allStructuresSaveData.Structures[i].savedString3;

                    newStructure.transform.position = loadedPosition;
                    newStructure.transform.rotation = Quaternion.Euler(loadedRotation.x, loadedRotation.y, loadedRotation.z);
                    StructureStats.LoadVariables();
                }
                
            }

            Debug.Log("Structures loaded successfully.");
    }

}

[System.Serializable]
public class StructureInventory
{
    public Structure[] Structures = new Structure[800];
    public NutrientStorage[] Nutrients = new NutrientStorage[800];
    public void Clear()
    {
        for(int i = 0; i < Structures.Length; i++)
        {
            Structures[i] = null;
        }
        for(int i = 0; i < Nutrients.Length; i++)
        {
            Nutrients[i] = null;
        }
    }
    public StructureInventory(Structure[] structureList, NutrientStorage[] nutrientList)
    {
        //Clear();
        //for (int i = 0; i < structureList.Count; i++)
        //{
        //    Structures[i] = structureList[i];
        //}
        Structures = structureList;
        Nutrients = nutrientList;
    }

    public StructureInventory() //Just as a precaution, unsure if redundant
    {
        Structures = new Structure[800];
        Nutrients = new NutrientStorage[800];
    }
}

