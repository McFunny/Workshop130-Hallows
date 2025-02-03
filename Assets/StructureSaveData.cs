using SaveLoadSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureSaveData : MonoBehaviour
{

   /* public static StructureSaveData Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        SaveLoad.OnLoadGame += LoadStructures;
    }


    private void Start()
    {
       SaveStructures();
    }

    private void OnDisable()
    {
        SaveLoad.OnLoadGame -= LoadStructures;
    }


    public void SaveStructures()
    {
        List<FarmLandSaveData> farmlandDataList = new List<FarmLandSaveData>();
        List<FarmTreeSaveData> farmTreeDataList = new List<FarmTreeSaveData>();

        foreach (var structure in StructureManager.Instance.allStructs)
        {
            if (structure is FarmLand farmLand)
                farmlandDataList.Add(new FarmLandSaveData(farmLand));

            if (structure is FarmTree farmTree)
                farmTreeDataList.Add(new FarmTreeSaveData(farmTree));

          
        }

        var structureData = new AllStructuresSaveData(farmlandDataList, farmTreeDataList);
        SaveLoad.CurrentSaveData.allStructuresSaveData = structureData;

        Debug.Log("Structures saved successfully.");
    }

    public void LoadStructures(SaveData data)
    {
        if (data.allStructuresSaveData.farmlandSaveData != null)
        {
            foreach (var farmlandData in data.allStructuresSaveData.farmlandSaveData)
            {
                GameObject newFarmLand = Instantiate(StructureManager.Instance.farmTile, farmlandData.position, Quaternion.identity);
                FarmLand farmLand = newFarmLand.GetComponent<FarmLand>();

                farmLand.health = farmlandData.health;
                farmLand.onFire = farmlandData.onFire;
                farmLand.isObstacle = farmlandData.isObstacle;
                farmLand.crop = CropDatabase.GetCropByName(farmlandData.cropID);
                farmLand.growthStage = farmlandData.growthStage;
                farmLand.hoursSpent = farmlandData.hoursSpent;
                farmLand.plantStress = farmlandData.plantStress;
                farmLand.harvestable = farmlandData.harvestable;
                farmLand.rotted = farmlandData.rotted;
                farmLand.isWeed = farmlandData.isWeed;
                farmLand.isFrosted = farmlandData.isFrosted;

                // Restore Nutrients
                NutrientStorage nutrients = new NutrientStorage
                {
                    ichorLevel = farmlandData.ichorLevel,
                    terraLevel = farmlandData.terraLevel,
                    gloamLevel = farmlandData.gloamLevel,
                    waterLevel = farmlandData.waterLevel
                };
                StructureManager.Instance.UpdateStorage(farmlandData.position, nutrients);
                StructureManager.Instance.SetTile(farmlandData.position);
            }
        }

        if (data.allStructuresSaveData.farmTreeSaveData != null)
        {
            foreach (var farmTreeData in data.allStructuresSaveData.farmTreeSaveData)
            {
                GameObject newFarmTree = Instantiate(StructureManager.Instance.farmTree, farmTreeData.position, Quaternion.identity);
                FarmTree farmTree = newFarmTree.GetComponent<FarmTree>();

                farmTree.health = farmTreeData.health;
                farmTree.onFire = farmTreeData.onFire;
                farmTree.isObstacle = farmTreeData.isObstacle;
                farmTree.structData.isLarge = farmTreeData.isLargeObject;

                if (farmTree.structData.isLarge)
                {
                    StructureManager.Instance.SetLargeTile(farmTreeData.position);
                }
                else
                {
                    StructureManager.Instance.SetTile(farmTreeData.position);
                }
            }
        }

        Debug.Log("Structures loaded successfully.");
    }
*/
}

[System.Serializable]
public struct AllStructuresSaveData
{
    public List<FarmLandSaveData> farmlandSaveData;
    public List<FarmTreeSaveData> farmTreeSaveData;
   
    public AllStructuresSaveData(List<FarmLandSaveData> farmlands, List<FarmTreeSaveData> farmTrees)
    {
        farmlandSaveData = farmlands;
        farmTreeSaveData = farmTrees;
    }
}

