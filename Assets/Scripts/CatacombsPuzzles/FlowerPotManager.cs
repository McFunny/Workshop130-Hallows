using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class FlowerPotManager : MonoBehaviour
{

    public static FlowerPotManager Instance;

    public bool puzzleSolved = false;

    public List<FlowerPotCatacombs> pots = new List<FlowerPotCatacombs> ();

    public List<FlowerAssignments> flowerAssignments = new List<FlowerAssignments>();

    public List<FlowerAssignments> flowerAssignmentsReference = new List<FlowerAssignments>();

    public bool hasSetUpFlowers = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

    }

    private void Start()
    {
        if (!hasSetUpFlowers)
        {
            SetupFlowers();
        }
    }

    private void SetupFlowers()
    {
        Debug.Log("SETTING UP FLOWERS");
        int r;
        for (int i = 0; i < pots.Count; i++)
        {
            r = Random.Range(0, flowerAssignments.Count);
            pots[i].requiredItem = flowerAssignments[r].flower;
            pots[i].flowerTablet.activePopup = flowerAssignments[r].popup;
            pots[i].flowerIndex = flowerAssignments[r].index;
            flowerAssignments.RemoveAt(r);
        }
        hasSetUpFlowers = true;
    }

    public void CheckToSeeIfSolved()
    {
        int correctPots = 0;
        for (int i = 0; i < pots.Count; i++)
        {
            if (pots[i].isCorrect)
            {
                correctPots++;
            }
        }
        if (correctPots == pots.Count)
        {
            for (int i = 0; i < pots.Count; i++)
            {
                pots[i].LockPuzzle();
            }
            puzzleSolved = true;
            PuzzleManager.Instance.CheckToSeeIfPuzzlesAreComplete();
        }
     }

    public FlowerPuzzleSaveData ExportSaveData()
    {
        List<FlowerSaveData> saveEntries = new List<FlowerSaveData>();

        foreach (var box in pots)
        {
            saveEntries.Add(box.ExportSaveData());
        }

        return new FlowerPuzzleSaveData
        {
            pots = saveEntries,
            flowerPuzzleSolved = puzzleSolved,
            hasSetUpFlowersdata = hasSetUpFlowers
        };
    }

    public void ImportSaveData(FlowerPuzzleSaveData data)
    {
        puzzleSolved = data.flowerPuzzleSolved;
        hasSetUpFlowers = data.hasSetUpFlowersdata;

        for (int i = 0; i < pots.Count; i++)
        {
            pots[i].ImportSaveData(data.pots[i]);
        }
    }
}

[System.Serializable]

public struct FlowerPuzzleSaveData
{
    public List<FlowerSaveData> pots;
    public bool flowerPuzzleSolved;
    public bool hasSetUpFlowersdata;
}

[System.Serializable]
public class FlowerAssignments
{
    public InventoryItemData flower;
    public PopupScript popup;
    public int index;
}