using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StructureDatabase", menuName = "Databases/StructureDatabase")]
public class StructureDatabase : ScriptableObject
{

    /////////////////CAMS STUFF///////////////////
    [Header("ALWAYS ADD NEW STRUCTURE OBJECTS AND UPDATE ID'S")]
    public StructureObject[] Structures;
    [ContextMenu("Update ID's")]
    public void UpdateID()
    {
        for(int i = 0; i < Structures.Length; i++)
        {
            Structures[i].data.Id = i;
            Structures[i].id = i;
        }
    }
    //////////////////////////////////////////////

    public List<StructurePrefabEntry> structurePrefabs = new List<StructurePrefabEntry>(); //Why not have a list with the structure data?
    

    private Dictionary<string, GameObject> prefabLookup;

    private void OnEnable()
    {
        UpdateID();
        prefabLookup = new Dictionary<string, GameObject>();
        foreach (var entry in structurePrefabs)
        {
            prefabLookup[entry.structureName] = entry.prefab;
        }
    }

    public GameObject GetPrefab(string structureName)
    {
        return prefabLookup.ContainsKey(structureName) ? prefabLookup[structureName] : null;
    }
}

[System.Serializable]
public class StructurePrefabEntry
{
    public string structureName;
    public GameObject prefab;   
}
