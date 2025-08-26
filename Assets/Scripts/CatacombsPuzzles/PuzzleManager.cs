using Cinemachine;
using SaveLoadSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance;


    public List<ImAPuzzleManager> allPuzzleManagers = new List<ImAPuzzleManager>();
    public List<GameObject> firesUponCompletion = new List<GameObject>();

    public AudioSource audioSource;
    public bool allPuzzlesSolved;

    public GameObject puzzleBeforeMove;
    public GameObject puzzleAfterMove;

    public int totalPuzzlesSolved = 0;

    public GameObject brazierPuzzleSteam;
    public GameObject waterPuzzleSteam;
    public GameObject pillarPuzzleSteam;
    public GameObject shrinePuzzleSteam;
    public GameObject flowerPuzzleSteam;
    public GameObject bugPuzzleSteam;

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

    private void Update()
    {
        /*if (Input.GetKeyUp(KeyCode.K))
        {
            StartCoroutine(MoveStatue());
        }*/
    }

    public void CheckToSeeIfPuzzlesAreComplete()
    {
       GetTotalSolved();
        FireCheck();
        if (totalPuzzlesSolved == allPuzzleManagers.Count)
        {
            StartCoroutine(MoveStatue());
        }
        ActivateSteams();
    }

    public void GetTotalSolved()
    {
        totalPuzzlesSolved = 0;
        foreach (var puzzle in allPuzzleManagers)
        {
            if (puzzle.puzzleSolved == true)
            {
                totalPuzzlesSolved++;
            }
        }
    }

    public void FireCheck()
    {
        GetTotalSolved();
        for (int i = 0; i < totalPuzzlesSolved; i++)
        {
            firesUponCompletion[i].SetActive(true);
        }
    }

    IEnumerator MoveStatue()
    {
        allPuzzlesSolved = true;
        audioSource.Play();
        PlayerMovement.restrictMovementTokens++;
        impulseSource = GetComponent<CinemachineImpulseSource>();
        impulseSource.GenerateImpulseWithForce(0.25f);
        FadeScreen.coverScreen = true;
        yield return new WaitForSeconds(0.75f);
        FadeScreen.coverScreen = false;
        yield return new WaitForSeconds(1.5f);
        FadeScreen.coverScreen = true;
        yield return new WaitForSeconds(0.75f);
        FadeScreen.coverScreen = false;
        yield return new WaitForSeconds(1.5f);
        FadeScreen.coverScreen = true;
        yield return new WaitForSeconds(0.75f);
        puzzleBeforeMove.SetActive(false);
        puzzleAfterMove.SetActive(true);
        FadeScreen.coverScreen = false;
        yield return new WaitForSeconds(1f);
        PlayerMovement.restrictMovementTokens--;
    }

    private void ActivateSteams()
    {
       /* if(BrazierPuzzleManager.Instance.puzzleSolved) brazierPuzzleSteam.SetActive(true);
        if(RotatingPillarManager.Instance.puzzleSolved) pillarPuzzleSteam.SetActive(true);
        if(WaterPuzzleManager.Instance.puzzleSolved) waterPuzzleSteam.SetActive(true);
        if(ShrineBoxManager.Instance.puzzleSolved) shrinePuzzleSteam.SetActive(true);
        if (FlowerPotManager.Instance.puzzleSolved) flowerPuzzleSteam.SetActive(true);
        if (BugPuzzleManager.Instance.puzzleSolved) bugPuzzleSteam.SetActive(true);*/
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

            waterPuzzleData = WaterPuzzleManager.Instance.ExportSaveData(),
            rotatingPuzzleData = RotatingPillarManager.Instance.ExportSaveData(),
            brazierPuzzleData = BrazierPuzzleManager.Instance.ExportSaveData(),
            shrinePuzzleData = ShrineBoxManager.Instance.ExportSaveData(),
            flowerPuzzleData = FlowerPotManager.Instance.ExportSaveData(),
            bugPuzzleSaveData = BugPuzzleManager.Instance.ExportSaveData(),
            siegeFlowerSaveData = SiegeFlowerPuzzleManager.Instance.ExportSaveData(),
            gachaponSaveData = Gachapon.Instance.ExportSaveData(),
            totalPuzzlesSolved = totalPuzzlesSolved,
            allPuzzlesSolved = allPuzzlesSolved
        };
    }

    public void LoadFromData(PuzzleManagerSaveData data)
    {
        
        WaterPuzzleManager.Instance.ImportSaveData(data.waterPuzzleData);
        RotatingPillarManager.Instance.ImportSaveData(data.rotatingPuzzleData);
        BrazierPuzzleManager.Instance.ImportSaveData(data.brazierPuzzleData);
        ShrineBoxManager.Instance.ImportSaveData(data.shrinePuzzleData);
        FlowerPotManager.Instance.ImportSaveData(data.flowerPuzzleData);
        BugPuzzleManager.Instance.ImportSaveData(data.bugPuzzleSaveData);
        SiegeFlowerPuzzleManager.Instance.ImportSaveData(data.siegeFlowerSaveData);
        Gachapon.Instance.ImportSaveData(data.gachaponSaveData);
        totalPuzzlesSolved = data.totalPuzzlesSolved;
        allPuzzlesSolved = data.allPuzzlesSolved;
        if (allPuzzlesSolved)
        {
            puzzleBeforeMove.SetActive(false);
            puzzleAfterMove.SetActive(true);
        }
        FireCheck();
        ActivateSteams();
    }
}

[System.Serializable]
public struct PuzzleManagerSaveData
{
    
    public WaterPuzzleData waterPuzzleData;
    public RotatingPuzzleSaveData rotatingPuzzleData;
    public BrazierPuzzleSaveData brazierPuzzleData;
    public ShrinePuzzleSaveData shrinePuzzleData;
    public FlowerPuzzleSaveData flowerPuzzleData;
    public BugPuzzleSaveData bugPuzzleSaveData;
    public SiegeFlowerPuzzleSaveData siegeFlowerSaveData;
    public GachaponSaveData gachaponSaveData;
    public int totalPuzzlesSolved;
    public bool allPuzzlesSolved;
}
