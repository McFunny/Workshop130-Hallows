using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SiegeFlowerPuzzleManager : MonoBehaviour
{
    public static SiegeFlowerPuzzleManager Instance;

    public List<SiegeFlowerTotem> siegeTotems = new List<SiegeFlowerTotem>();

    public bool puzzleSolved = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

    }
    void Start()
    {

    }

    public SiegeFlowerPuzzleSaveData ExportSaveData()
    {
        List<SiegeFlowerSaveData> saveEntries = new List<SiegeFlowerSaveData>();

        foreach (var box in siegeTotems)
        {
            saveEntries.Add(box.ExportSaveData());
        }

        return new SiegeFlowerPuzzleSaveData
        {
            totems = saveEntries,
            siegeFlowerPuzzleSolved = puzzleSolved
        };
    }

    public void ImportSaveData(SiegeFlowerPuzzleSaveData data)
    {
        puzzleSolved = data.siegeFlowerPuzzleSolved;

        for (int i = 0; i < siegeTotems.Count; i++)
        {
            siegeTotems[i].ImportSaveData(data.totems[i]);
        }
    }

    internal void CheckToSeeIfSolved()
    {
        int puzzlesSolved = 0;
        for (int i = 0; i < siegeTotems.Count; i++)
        {
            if (siegeTotems[i].isSolved)
            {
                puzzlesSolved++;
            }
        }

        if (puzzlesSolved == siegeTotems.Count)
        {
            puzzleSolved = true;
        }
    }
}

[System.Serializable]
public struct SiegeFlowerPuzzleSaveData
{
    public List<SiegeFlowerSaveData> totems;
    public bool siegeFlowerPuzzleSolved;
}
