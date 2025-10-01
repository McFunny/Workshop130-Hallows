using System;
using System.Collections.Generic;
using UnityEngine;

public class CatacombsTorchManager : MonoBehaviour
{
    public static CatacombsTorchManager Instance;

    public List<CatacombsTorch> catacombsTorches = new List<CatacombsTorch>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public TorchSaveData ExportSaveData()
    {
        var data = new TorchSaveData { torches = new List<TorchEntry>(catacombsTorches.Count) };

        foreach (var torch in catacombsTorches)
        {
            if (torch == null) continue;
            data.torches.Add(torch.ExportTorchData());
        }

        return data;
    }

    public void ImportSaveData(TorchSaveData data)
    {
        if (data.torches == null) return;


        foreach (var entry in data.torches)
        {
            var torch = GetTorchById(entry.savedID);
            if (torch != null)
            {
                torch.ImportTorchData(entry);
            }
            else
            {
                Debug.LogWarning($"TorchManager: No torch found with ID {entry.savedID}");
            }
        }
    }

    private CatacombsTorch GetTorchById(int id)
    {
        for (int i = 0; i < catacombsTorches.Count; i++)
        {
            var t = catacombsTorches[i];
            if (t == null) continue;
            if (t.ID == id) return t;
        }
        return null;
    }

#if UNITY_EDITOR
   //If new torches need to be added to the list
    [ContextMenu("Populate From Scene")]
    private void PopulateFromScene()
    {
        catacombsTorches.Clear();
        catacombsTorches.AddRange(FindObjectsByType<CatacombsTorch>(FindObjectsSortMode.None));
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}

[Serializable]
public struct TorchEntry
{
    public int savedID;
    public bool savedIsLit;
}

[Serializable]
public struct TorchSaveData
{
    public List<TorchEntry> torches;
}

