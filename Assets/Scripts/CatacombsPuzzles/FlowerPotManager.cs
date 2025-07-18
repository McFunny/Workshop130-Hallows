using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowerPotManager : MonoBehaviour
{

    public static FlowerPotManager Instance;

    public bool isSolved = false;

    public List<FlowerPotCatacombs> pots = new List<FlowerPotCatacombs> ();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

    }

   public void CheckToSeeIfSolved()
    {
        int correctPots = 0;
        for (int i = 0; i < pots.Count; i++)
        {
            if (pots[i].isCorrect)
            {
                correctPots++;
            }
        }
        if (correctPots == pots.Count)
        {
            for (int i = 0; i < pots.Count; i++)
            {
                pots[i].LockPuzzle();
            }
            isSolved = true;
            PuzzleManager.Instance.CheckToSeeIfPuzzlesAreComplete();
        }
     }
}
