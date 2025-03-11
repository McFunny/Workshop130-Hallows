using SaveLoadSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance;

    public MoneyPuzzle moneyPuzzle;
    public RotatingPillarManager pillarPuzzle;
    public BrazierPuzzleManager brazierPuzzle;
    public WaterPuzzleManager waterPuzzle;

    public AudioSource audioSource;
    public bool allPuzzlesSolved;

    public GameObject puzzleBeforeMove;
    public GameObject puzzleAfterMove;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        SaveLoad.OnLoadGame += LoadData;
        SaveLoad.OnSaveGame += SaveData;
    }

    private void OnDisable()
    {
        SaveLoad.OnLoadGame -= LoadData;
        SaveLoad.OnSaveGame -= SaveData;
    }

    public void CheckToSeeIfPuzzlesAreComplete()
    {
        if (moneyPuzzle.donationComplete && pillarPuzzle.rotatingPillarPuzzleSolved
            && brazierPuzzle.brazierPuzzleSolved && waterPuzzle.waterPuzzleSolved)
        {
            allPuzzlesSolved = true;
            audioSource.Play();
            puzzleBeforeMove.SetActive(false);
            puzzleAfterMove.SetActive(true);
        }
    }

    private void SaveData()
    {
        SaveLoad.CurrentSaveData.puzzleSaveData = GetPuzzleData();
    }

    private void LoadData(SaveData data)
    {
        LoadFromData(data.puzzleSaveData);
    }

    public PuzzleManagerSaveData GetPuzzleData()
    {
        return new PuzzleManagerSaveData
        {
            moneyPuzzleCompleted = moneyPuzzle.donationComplete,
            waterPuzzleData = waterPuzzle.GetPuzzleData(),
            rotatingPuzzleData = pillarPuzzle.ExportSaveData(),
            brazierPuzzleData = brazierPuzzle.ExportSaveData()
        };
    }

    public void LoadFromData(PuzzleManagerSaveData data)
    {
        moneyPuzzle.donationComplete = data.moneyPuzzleCompleted;
        waterPuzzle.LoadFromData(data.waterPuzzleData);
        pillarPuzzle.ImportSaveData(data.rotatingPuzzleData);
        brazierPuzzle.ImportSaveData(data.brazierPuzzleData);
    }
}

[System.Serializable]
public struct PuzzleManagerSaveData
{
    public bool moneyPuzzleCompleted;
    public WaterPuzzleData waterPuzzleData;
    public RotatingPuzzleSaveData rotatingPuzzleData;
    public BrazierPuzzleSaveData brazierPuzzleData;
}
