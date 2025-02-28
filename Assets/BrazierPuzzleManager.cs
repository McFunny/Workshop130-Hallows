using System.Collections;
using System.Collections.Generic;
using System.Threading;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class BrazierPuzzleManager : MonoBehaviour
{
    [SerializeField] private List<PuzzleBrazier> brazierList = new List<PuzzleBrazier>();

    private void Start()
    {
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
    }
}
