using SaveLoadSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance;

    public SlotMachine slotMachinePuzzle;
    public RotatingPillarManager pillarPuzzle;
    public BrazierPuzzleManager brazierPuzzle;
    public WaterPuzzleManager waterPuzzle;

    public AudioSource audioSource;
    public bool allPuzzlesSolved;

    public GameObject puzzleBeforeMove;
    public GameObject puzzleAfterMove;

    public int totalPuzzlesSolved = 0;

    public GameObject[] fireObjects;

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
        if (slotMachinePuzzle.puzzleSolved && pillarPuzzle.rotatingPillarPuzzleSolved
            && brazierPuzzle.brazierPuzzleSolved && waterPuzzle.waterPuzzleSolved)
        {
            allPuzzlesSolved = true;
            audioSource.Play();
            puzzleBeforeMove.SetActive(false);
            puzzleAfterMove.SetActive(true);
        }
        RunForLoop();
    }

    private void RunForLoop()
    {
        for (int i = 0; i < totalPuzzlesSolved; i++)
        {
            fireObjects[i].gameObject.SetActive(true);
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
            slotMachineSaveData = slotMachinePuzzle.ExportSaveData(),
            waterPuzzleData = waterPuzzle.GetPuzzleData(),
            rotatingPuzzleData = pillarPuzzle.ExportSaveData(),
            brazierPuzzleData = brazierPuzzle.ExportSaveData(),
            totalPuzzlesSolved = totalPuzzlesSolved,
            allPuzzlesSolved = allPuzzlesSolved
        };
    }

    public void LoadFromData(PuzzleManagerSaveData data)
    {
        slotMachinePuzzle.ImportSaveData(data.slotMachineSaveData);
        waterPuzzle.LoadFromData(data.waterPuzzleData);
        pillarPuzzle.ImportSaveData(data.rotatingPuzzleData);
        brazierPuzzle.ImportSaveData(data.brazierPuzzleData);
        totalPuzzlesSolved = data.totalPuzzlesSolved;
        allPuzzlesSolved = data.allPuzzlesSolved;
        if (allPuzzlesSolved)
        {
            puzzleBeforeMove.SetActive(false);
            puzzleAfterMove.SetActive(true);
        }
        RunForLoop();
    }
}

[System.Serializable]
public struct PuzzleManagerSaveData
{
    public SlotMachineSaveData slotMachineSaveData;
    public WaterPuzzleData waterPuzzleData;
    public RotatingPuzzleSaveData rotatingPuzzleData;
    public BrazierPuzzleSaveData brazierPuzzleData;
    public int totalPuzzlesSolved;
    public bool allPuzzlesSolved;
}
