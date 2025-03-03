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

    private void Start()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }
    }

    public void CheckToSeeIfPuzzlesAreComplete()
    {
        if(moneyPuzzle.donationComplete && pillarPuzzle.rotatingPillarPuzzleSolved && brazierPuzzle.brazierPuzzleSolved && waterPuzzle.waterPuzzleSolved)
        {
            allPuzzlesSolved = true;
            audioSource.Play();
            puzzleBeforeMove.SetActive(false);
            puzzleAfterMove.SetActive(true);
        }
    }    
}
