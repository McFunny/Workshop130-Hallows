using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StructureDatabase", menuName = "Databases/StructureDatabase")]
public class StructureDatabase : ScriptableObject
{
    public List<StructurePrefabEntry> structurePrefabs = new List<StructurePrefabEntry>();

    private Dictionary<string, GameObject> prefabLookup;

    private void OnEnable()
    {
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
