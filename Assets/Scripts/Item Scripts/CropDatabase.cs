using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory System/Crop Database")]

public class CropDatabase : ScriptableObject
{
    private static Database _instance;

    public static Database Instance
    {
        get
        {
            if (_instance == null)
            {
                // Load the instance of the Database if not already set
                _instance = Resources.Load<Database>("ItemDatabase");
            }
            return _instance;
        }
    }

    [SerializeField] private List<CropData> _cropDatabase;

    private static Dictionary<string, CropData> cropLookup = new Dictionary<string, CropData>();

    public static void RegisterCrops(List<CropData> crops)
    {
        cropLookup.Clear();
        foreach (var crop in crops)
        {
            cropLookup[crop.name] = crop;
        }
    }

    public static CropData GetCropByName(string name)
    {
        return cropLookup.ContainsKey(name) ? cropLookup[name] : null;
    }
}
