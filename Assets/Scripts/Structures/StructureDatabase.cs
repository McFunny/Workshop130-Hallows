using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StructureDatabase", menuName = "Databases/StructureDatabase")]
public class StructureDatabase : ScriptableObject
{

    private static StructureDatabase _instance;

    public static StructureDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                // Load the instance of the Database if not already set
                _instance = Resources.Load<StructureDatabase>("StructureDatabase");
            }
            return _instance;
        }
    }

    /////////////////CAMS STUFF///////////////////
    [Header("ALWAYS ADD NEW STRUCTURE OBJECTS AND UPDATE ID'S")]
    public List<StructureObject> Structures;

    public StructureObject oneTilePile, twoTilePile, fourTilePile, nineTilePile;
    [ContextMenu("Update ID's")]
    public void UpdateID()
    {
        for(int i = 0; i < Structures.Count; i++)
        {
            Structures[i].data.Id = i;
            Structures[i].id = i;
        }
    }

    public StructureObject GetPile(StructureObject s)
    {
        switch(s.gridSize)
        {
            case GridSize.OneByOne:
                return oneTilePile;
                break;
            case GridSize.OneByTwo:
                return twoTilePile;
                break;
            case GridSize.TwoByTwo:
                return fourTilePile;
                break;
            case GridSize.ThreeByThree:
                return nineTilePile;
                break;
            default:
                return oneTilePile;
                break;
        }
    }

    public StructureObject GetStructure(int id) //USE THIS FOR GRABBING STRUCTURES WITH THE DEBRIS PILE
    {
        return Structures.Find(i => i.id == id);
    }
    //////////////////////////////////////////////

    //public List<StructurePrefabEntry> structurePrefabs = new List<StructurePrefabEntry>(); //Why not have a list with the structure data?
    

    private Dictionary<string, GameObject> prefabLookup;

    private void OnEnable()
    {
        UpdateID();
        /*prefabLookup = new Dictionary<string, GameObject>();
        foreach (var entry in structurePrefabs)
        {
            prefabLookup[entry.structureName] = entry.prefab;
        }*/
    }

    /*public GameObject GetPrefab(string structureName)
    {
        return prefabLookup.ContainsKey(structureName) ? prefabLookup[structureName] : null;
    }*/

    public void ResetStats()
    {
        for(int i = 0; i < Structures.Count; i++)
        {
            Structures[i].hasBeenPlaced = false;
        }
    }

    public void SaveStats(out bool[] structStats)
    {
        List<bool> temp = new List<bool>();

        foreach(StructureObject s in Structures)
        {
            temp.Add(s.hasBeenPlaced);
        }
        structStats = temp.ToArray();
    }

    public void LoadStats(AllGameSaveData data)
    {
        int i = 0;
        foreach(StructureObject s in Structures)
        {
            if(data.structStats != null || i >= data.structStats.Length || data.structStats.Length == 0) return;
            s.hasBeenPlaced = data.structStats[i];
            i++;
        }
    }
}

[System.Serializable]
public class StructurePrefabEntry
{
    public string structureName;
    public GameObject prefab;   
}
