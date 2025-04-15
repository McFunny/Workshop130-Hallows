using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(menuName = "Inventory System/Crop Database")]

public class CropDatabase : ScriptableObject
{
    private static CropDatabase _instance;

    public static CropDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                // Load the instance of the Database if not already set
                _instance = Resources.Load<CropDatabase>("CropDatabase");
            }
            return _instance;
        }
    }

    /*void OnEnable()
    {
        RegisterCrops(_cropDatabase);
    }*/

    [SerializeField] private List<CropData> _cropDatabase; //DONT ALTER ORDER

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

    [ContextMenu("Update ID's")]
    public void UpdateID()
    {
        for(int i = 0; i < _cropDatabase.Count; i++)
        {
            _cropDatabase[i].id = i;
            #if UNITY_EDITOR

            if (_cropDatabase[i]) EditorUtility.SetDirty(_cropDatabase[i]);

            #endif
        }
        #if UNITY_EDITOR       
            AssetDatabase.SaveAssets();
        #endif
    }

    public CropData GetCrop(int id) //USE THIS FOR GRABBING CROPS WHEN SAVING AND LOADING
    {
        return _cropDatabase.Find(i => i.id == id);
    }

    /*for(int i = 0; i < cropDatabase.Count; i++)
        {
            if(cropDatabase[i].name == name) return cropDatabase[i];
        }
        return null; */

    public void ResetStats()
    {
        for(int i = 0; i < _cropDatabase.Count; i++)
        {
            _cropDatabase[i].amountHarvested = 0;
            _cropDatabase[i].amountKilled = 0;
        }
    }

    public void SaveStats(out CropPlayerStats[] cropStats)
    {
        List<CropPlayerStats> temp = new List<CropPlayerStats>();

        foreach(CropData c in _cropDatabase)
        {
            temp.Add(new CropPlayerStats(c.amountHarvested, c.amountKilled));
        }
        cropStats = temp.ToArray();
    }

    public void LoadStats(AllGameSaveData data)
    {
        int i = 0;
        foreach(CropData c in _cropDatabase)
        {
            c.amountHarvested = data.cropStats[i].amountHarvested;
            c.amountKilled = data.cropStats[i].amountKilled;
            i++;
        }
    }
}
[System.Serializable]
public class CropPlayerStats
{
    public int amountHarvested = 0;
    public int amountKilled = 0;

    public CropPlayerStats(int _harvest, int _killed)
    {
        amountHarvested = _harvest;
        amountKilled = _killed;
    }
}
