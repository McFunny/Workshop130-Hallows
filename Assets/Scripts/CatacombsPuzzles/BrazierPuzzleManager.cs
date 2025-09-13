using SaveLoadSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrazierPuzzleManager : ImAPuzzleManager
{
    [SerializeField] private List<PuzzleBrazier> brazierList = new List<PuzzleBrazier>();


    public Color gold;
    public Color gray;

    public SpriteRenderer totalPuzzleWin;

    public static BrazierPuzzleManager Instance;

    [Header("Gachapon Stuff")]
    public InventoryItemData gachaponReward;
    public int gachaponRewardCount;

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
        if(!puzzleSolved) InitializePuzzles();

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
        fireObject.SetActive(puzzleSolved);
        PuzzleManager.Instance.totalPuzzlesSolved++;
        Gachapon.Instance.AddToBacklog(gachaponReward, gachaponRewardCount);
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
        fireObject.SetActive(puzzleSolved);

        if (puzzleSolved) { totalPuzzleWin.color = gold; }
        else if (!puzzleSolved) { totalPuzzleWin.color = gray; }

        for (int i = 0; i < brazierList.Count; i++)
        {
            if(i >= data.braziers.Count) continue;
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
