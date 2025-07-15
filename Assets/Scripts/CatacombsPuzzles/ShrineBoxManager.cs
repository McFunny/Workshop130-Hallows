using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShrineBoxManager : MonoBehaviour
{
   public static ShrineBoxManager Instance;

   public List<ShrineBox> boxes = new List<ShrineBox>();

    public bool puzzleSolved = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

    }
    void Start()
    {
        
    }

    public ShrinePuzzleSaveData ExportSaveData()
    {
        List<ShrineSaveData> saveEntries = new List<ShrineSaveData>();

        foreach (var box in boxes)
        {
            saveEntries.Add(box.ExportSaveData());
        }

        return new ShrinePuzzleSaveData
        {
            shrines = saveEntries,
            shrinePuzzleSolved = puzzleSolved
        };
    }

    public void ImportSaveData(ShrinePuzzleSaveData data)
    {
        puzzleSolved = data.shrinePuzzleSolved;

        for (int i = 0; i < boxes.Count; i++)
        {
            boxes[i].ImportSaveData(data.shrines[i]);
        }
    }

    internal void CheckToSeeIfSolved()
    {
        int puzzlesSolved = 0;
        for (int i = 0; i < boxes.Count; i++)
        {
            if (boxes[i].isSolved)
            {
                puzzlesSolved++;
            }
        }

        if (puzzlesSolved == boxes.Count)
        {
            puzzleSolved = true;
        }
    }
}

    [System.Serializable]
public struct ShrinePuzzleSaveData
{
    public List<ShrineSaveData> shrines;
    public bool shrinePuzzleSolved;
}