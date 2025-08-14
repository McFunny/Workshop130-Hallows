using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SiegeFlowerPuzzleManager : ImAPuzzleManager
{
    public static SiegeFlowerPuzzleManager Instance;

    public List<SiegeFlowerTotem> pots = new List<SiegeFlowerTotem>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void CheckToSeeIfSolved()
    {
        int correctPots = 0;
        for (int i = 0; i < pots.Count; i++)
        {
            if (pots[i].isLocked) // ✅ Use .isLocked to track final state
            {
                correctPots++;
            }
        }

        if (correctPots == pots.Count)
        {
            foreach (var pot in pots)
            {
                pot.LockPuzzle();
            }
            puzzleSolved = true;
            PuzzleManager.Instance.CheckToSeeIfPuzzlesAreComplete();
        }
    }

    public SiegeFlowerPuzzleSaveData ExportSaveData()
    {
        List<SiegeFlowerSaveData> saveEntries = new List<SiegeFlowerSaveData>();
        foreach (var box in pots)
        {
            saveEntries.Add(box.ExportSaveData());
        }

        return new SiegeFlowerPuzzleSaveData
        {
            pots = saveEntries,
            siegeflowerPuzzleSolved = puzzleSolved,
        };
    }

    public void ImportSaveData(SiegeFlowerPuzzleSaveData data)
    {
        puzzleSolved = data.siegeflowerPuzzleSolved;

        for (int i = 0; i < pots.Count; i++)
        {
            if(data.pots == null || i >= data.pots.Count) return;
            pots[i].ImportSaveData(data.pots[i]);
        }

        if (puzzleSolved)
        {
            foreach (var pot in pots)
                pot.LockPuzzle();
        }
    }
}

[System.Serializable]
public struct SiegeFlowerPuzzleSaveData
{
    public List<SiegeFlowerSaveData> pots;
    public bool siegeflowerPuzzleSolved;
}
