using SaveLoadSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrazierPuzzleManager : MonoBehaviour
{
    [SerializeField] private List<PuzzleBrazier> brazierList = new List<PuzzleBrazier>();

    public bool puzzleSolved;

    public Color gold;
    public Color gray;

    public SpriteRenderer totalPuzzleWin;

    public static BrazierPuzzleManager Instance;

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
        puzzleSolved = false;
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
                    totalPuzzleWin.color = gold;
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

        puzzleSolved = true;
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
            brazierPuzzleSolved = puzzleSolved
        };
    }

    public void ImportSaveData(BrazierPuzzleSaveData data)
    {
        puzzleSolved = data.brazierPuzzleSolved;

        if (puzzleSolved) { totalPuzzleWin.color = gold; }
        else if (!puzzleSolved) { totalPuzzleWin.color = gray; }

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
