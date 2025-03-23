using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrazierPuzzleManager : MonoBehaviour
{
    [SerializeField] private List<PuzzleBrazier> brazierList = new List<PuzzleBrazier>();

    public bool brazierPuzzleSolved;

    private void Start()
    {
        brazierPuzzleSolved = false;
        InitializePuzzles();
    }

    private void InitializePuzzles()
    {
        foreach (var brazier in brazierList)
        {
            brazier.OnInteractionComplete += (completedPillar) => CheckPuzzleCompletion();
        }
    }

    private void CheckPuzzleCompletion()
    {
        int puzzlesCorrect = 0;
        foreach (var brazier in brazierList)
        {
            if (brazier.correctFire == brazier.currentFire)
            {
                puzzlesCorrect++;
                if (puzzlesCorrect == brazierList.Count)
                {
                    LockPuzzle();
                }
            }
        }
    }

    private void LockPuzzle()
    {
        Debug.Log("Brazier Puzzle Completed");
        foreach (var brazier in brazierList)
        {
            brazier.isLocked = true;
        }

        brazierPuzzleSolved = true;
        PuzzleManager.Instance.totalPuzzlesSolved++;
        PuzzleManager.Instance.CheckToSeeIfPuzzlesAreComplete();
    }

    public BrazierPuzzleSaveData ExportSaveData()
    {
        List<BrazierSaveData> braziers = new List<BrazierSaveData>();
        foreach (var brazier in brazierList)
        {
            braziers.Add(brazier.ExportSaveData());
        }

        return new BrazierPuzzleSaveData
        {
            braziers = braziers,
            brazierPuzzleSolved = brazierPuzzleSolved
        };
    }

    public void ImportSaveData(BrazierPuzzleSaveData data)
    {
        brazierPuzzleSolved = data.brazierPuzzleSolved;

        for (int i = 0; i < brazierList.Count; i++)
        {
            brazierList[i].ImportSaveData(data.braziers[i]);
        }
    }
}

[System.Serializable]
public struct BrazierPuzzleSaveData
{
    public List<BrazierSaveData> braziers;
    public bool brazierPuzzleSolved;
}
