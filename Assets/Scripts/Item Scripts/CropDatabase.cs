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

    /*void OnEnable()
    {
        RegisterCrops(_cropDatabase);
    }*/

    [SerializeField] private List<CropData> _cropDatabase;

    private Dictionary<string, CropData> cropLookup = new Dictionary<string, CropData>();

    public void RegisterCrops(List<CropData> crops)
    {
        cropLookup.Clear();
        foreach (var crop in crops)
        {
            cropLookup[crop.name] = crop;
        }
    }

    public CropData GetCropByName(string name)
    {
        DumbAbnerFunction();
        Debug.Log(cropLookup.Count);
        return cropLookup.ContainsKey(name) ? cropLookup[name] : null;
    }

    void DumbAbnerFunction()
    {
        RegisterCrops(_cropDatabase);
    }

    /*for(int i = 0; i < cropDatabase.Count; i++)
        {
            if(cropDatabase[i].name == name) return cropDatabase[i];
        }
        return null; */
}
