using Cinemachine;
using SaveLoadSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance;




    public AudioSource audioSource;
    public bool allPuzzlesSolved;

    public GameObject puzzleBeforeMove;
    public GameObject puzzleAfterMove;

    public int totalPuzzlesSolved = 0;

    public GameObject brazierPuzzleSteam;
    public GameObject waterPuzzleSteam;
    public GameObject pillarPuzzleSteam;
    public GameObject slotPuzzleSteam;
    public GameObject shrinePuzzleSteam;
    public GameObject flowerPuzzleSteam;

    private CinemachineImpulseSource impulseSource;

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
        if (SlotMachine.Instance.puzzleSolved && RotatingPillarManager.Instance.puzzleSolved
            && BrazierPuzzleManager.Instance.puzzleSolved && WaterPuzzleManager.Instance.puzzleSolved
            && ShrineBoxManager.Instance.puzzleSolved && FlowerPotManager.Instance.puzzleSolved)
        {
            StartCoroutine(MoveStatue());
        }
        ActivateSteams();
    }

    IEnumerator MoveStatue()
    {
        allPuzzlesSolved = true;
        audioSource.Play();
        puzzleBeforeMove.SetActive(false);
        puzzleAfterMove.SetActive(true);
        PlayerMovement.restrictMovementTokens++;
        impulseSource = GetComponent<CinemachineImpulseSource>();
        impulseSource.GenerateImpulseWithForce(0.25f);
        yield return new WaitForSeconds(5);
        PlayerMovement.restrictMovementTokens--;
    }

    private void ActivateSteams()
    {
        if(SlotMachine.Instance.puzzleSolved) slotPuzzleSteam.SetActive(true);
        if(BrazierPuzzleManager.Instance.puzzleSolved) brazierPuzzleSteam.SetActive(true);
        if(RotatingPillarManager.Instance.puzzleSolved) pillarPuzzleSteam.SetActive(true);
        if(WaterPuzzleManager.Instance.puzzleSolved) waterPuzzleSteam.SetActive(true);
        if(ShrineBoxManager.Instance.puzzleSolved) shrinePuzzleSteam.SetActive(true);
        if (FlowerPotManager.Instance.puzzleSolved) flowerPuzzleSteam.SetActive(true);
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
            slotMachineSaveData = SlotMachine.Instance.ExportSaveData(),
            waterPuzzleData = WaterPuzzleManager.Instance.GetPuzzleData(),
            rotatingPuzzleData = RotatingPillarManager.Instance.ExportSaveData(),
            brazierPuzzleData = BrazierPuzzleManager.Instance.ExportSaveData(),
            shrinePuzzleData = ShrineBoxManager.Instance.ExportSaveData(),
            flowerPuzzleData = FlowerPotManager.Instance.ExportSaveData(),
            totalPuzzlesSolved = totalPuzzlesSolved,
            allPuzzlesSolved = allPuzzlesSolved
        };
    }

    public void LoadFromData(PuzzleManagerSaveData data)
    {
        SlotMachine.Instance.ImportSaveData(data.slotMachineSaveData);
        WaterPuzzleManager.Instance.LoadFromData(data.waterPuzzleData);
        RotatingPillarManager.Instance.ImportSaveData(data.rotatingPuzzleData);
        BrazierPuzzleManager.Instance.ImportSaveData(data.brazierPuzzleData);
        ShrineBoxManager.Instance.ImportSaveData(data.shrinePuzzleData);
        FlowerPotManager.Instance.ImportSaveData(data.flowerPuzzleData);
        totalPuzzlesSolved = data.totalPuzzlesSolved;
        allPuzzlesSolved = data.allPuzzlesSolved;
        if (allPuzzlesSolved)
        {
            puzzleBeforeMove.SetActive(false);
            puzzleAfterMove.SetActive(true);
        }
        ActivateSteams();
    }
}

[System.Serializable]
public struct PuzzleManagerSaveData
{
    public SlotMachineSaveData slotMachineSaveData;
    public WaterPuzzleData waterPuzzleData;
    public RotatingPuzzleSaveData rotatingPuzzleData;
    public BrazierPuzzleSaveData brazierPuzzleData;
    public ShrinePuzzleSaveData shrinePuzzleData;
    public FlowerPuzzleSaveData flowerPuzzleData;
    public int totalPuzzlesSolved;
    public bool allPuzzlesSolved;
}
