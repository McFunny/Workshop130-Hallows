using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New Structure Object", menuName = "Structure")]
public class StructureObject : ScriptableObject
{
    public GameObject objectPrefab;

    public int id = -1;
    [HideInInspector] public float health;

    [HideInInspector]public float[] position = new float[3];

    [HideInInspector]public float[] rotation = new float[3];

    [HideInInspector] public Structure data = new Structure();

    public GridSize gridSize;

    public StructureType structureType;

    //Repair Values//
    public int mintRepairCost;
    public List<ItemWithAmount> repairItems = new List<ItemWithAmount>(); //Will hold things like wood cost to repair and the like
    public int requiredRepairs = 1;
    public int maxMisses = 1;

    public float bugSpawnChance = 0; //Chance of spawning a bug when this is destroyed

    public bool hasBeenPlaced = false; //For codex unlock purposes


    public Structure CreateStructure()
    {
        Structure newStructure = new Structure(this);
        return newStructure;
    }

}

[System.Serializable]
public class Structure
{
    [Header("Variables that need to be saved")]
    public string Name;
    public int Id = -1;
    public float health;
    public float[] position = new float[3];
    public float[] rotation = new float[3];
    public List<InventoryItemData> savedItemList1;
    public List<int> savedItemIDList1 = new List<int>();
    public int savedInt1, savedInt2, savedInt3;
    public float savedFloat1, savedFloat2, savedFloat3;
    public string savedString1, savedString2, savedString3;
    public bool savedBool1;

    public Structure()
    {
        Name = "";
        Id = -1;
        position = new float[3];
    }
    public Structure(StructureObject structure)
    {
        //Name = structure.name;
        Id = structure.data.Id;
    }
}