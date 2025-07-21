using SaveLoadSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterPuzzleManager : MonoBehaviour
{
    public WaterPuzzleTile puzzle1;
    public WaterPuzzleTile puzzle2;
    public WaterPuzzleTile puzzle3;

    public bool puzzleSolved = false;

    public static WaterPuzzleManager Instance;

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
        // Load puzzle state when the game starts
        /*if (PuzzleManager.Instance != null)
        {
            LoadFromData(PuzzleManager.Instance.GetPuzzleData().waterPuzzleData);
        }*/
    }

    void Update()
    {
        if (puzzle1.isSolved && puzzle2.isSolved && puzzle3.isSolved && !puzzleSolved)
        {
            puzzle1.isLocked = true;
            puzzle2.isLocked = true;
            puzzle3.isLocked = true;
            puzzleSolved = true;

            //SavePuzzleState();
            PuzzleManager.Instance.totalPuzzlesSolved++;
            PuzzleManager.Instance.CheckToSeeIfPuzzlesAreComplete();
        }
    }


    public WaterPuzzleData GetPuzzleData()
    {
        return new WaterPuzzleData
        {
            waterPuzzleSolved = puzzleSolved,
            puzzle1Solved = puzzle1.isSolved,
            puzzle2Solved = puzzle2.isSolved,
            puzzle3Solved = puzzle3.isSolved
        };
    }


    public void LoadFromData(WaterPuzzleData data)
    {
        puzzleSolved = data.waterPuzzleSolved;

        puzzle1.SetSolvedState(data.puzzle1Solved);
        puzzle2.SetSolvedState(data.puzzle2Solved);
        puzzle3.SetSolvedState(data.puzzle3Solved);
    }

    private void SavePuzzleState()
    {
        // Tell PuzzleManager to save all puzzles
        //PuzzleManager.Instance.SavePuzzleStates();
    }
}


[System.Serializable]
public struct WaterPuzzleData
{
    public bool waterPuzzleSolved;
    public bool puzzle1Solved;
    public bool puzzle2Solved;
    public bool puzzle3Solved;
}
