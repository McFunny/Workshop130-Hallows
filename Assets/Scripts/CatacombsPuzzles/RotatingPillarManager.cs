using System.Collections.Generic;
using UnityEngine;
using static RotatingPillar;

public class RotatingPillarManager : MonoBehaviour
{
    public List<RotatingPillar> puzzleSet1 = new List<RotatingPillar>();
    public List<RotatingPillar> puzzleSet2 = new List<RotatingPillar>();
    public List<RotatingPillar> puzzleSet3 = new List<RotatingPillar>();
    public List<CropKey> cropKeys = new List<CropKey>();
    public List<CropData> cropData = new List<CropData>();

    public int puzzlesSolved = 0;

    private bool puzzleSet1Solved = false;
    private bool puzzleSet2Solved = false;
    private bool puzzleSet3Solved = false;


    public bool rotatingPillarPuzzleSolved = false;

    [SerializeField] private Database _database;

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        AssignCropsToPuzzles();
    }

    private void AssignCropsToPuzzles()
    {
       
        var cropDataGroups = GroupCropDataByLength();

       
        var puzzleSets = new List<(List<RotatingPillar> set, CropKey cropKey)>
        {
            (puzzleSet1, cropKeys[0]),
            (puzzleSet2, cropKeys[1]),
            (puzzleSet3, cropKeys[2])
        };

       
        foreach (var (puzzleSet, cropKey) in puzzleSets)
        {
            AssignCropToPuzzleSet(puzzleSet, cropKey, cropDataGroups);
        }
    }

    private Dictionary<int, List<CropData>> GroupCropDataByLength()
    {
        var cropDataGroups = new Dictionary<int, List<CropData>>();
        foreach (var crop in cropData)
        {
            int cropLength = crop.cropSprites.Length;
            if (!cropDataGroups.ContainsKey(cropLength))
            {
                cropDataGroups[cropLength] = new List<CropData>();
            }
            cropDataGroups[cropLength].Add(crop);
        }
        return cropDataGroups;
    }

    private void AssignCropToPuzzleSet(
        List<RotatingPillar> puzzleSet,
        CropKey cropKey,
        Dictionary<int, List<CropData>> cropDataGroups
    )
    {
        int puzzleSetLength = puzzleSet.Count;
        if (cropDataGroups.TryGetValue(puzzleSetLength, out var matchingCrops) && matchingCrops.Count > 0)
        {
            // Randomly select a CropData
            int randomIndex = Random.Range(0, matchingCrops.Count);
            CropData specifiedCrop = matchingCrops[randomIndex];
            matchingCrops.RemoveAt(randomIndex);

            // Assign the selected crop to the CropKey and set up its sprites
            Debug.Log(specifiedCrop);
            cropKey.cropData = specifiedCrop;
            Debug.Log(cropKey.cropData);
            cropKey.SetUpSprites();

            // Link the CropKey to the RotatingPillars
            foreach (var pillar in puzzleSet)
            {
                pillar.SetUpSprites(specifiedCrop);
                pillar.OnInteractionComplete += (completedPillar) => CheckPuzzleCompletion(puzzleSet);
                pillar.SetCropInsertionListener(cropKey); // Link CropKey to pillar
            }
        }
        else
        {
            Debug.LogWarning($"No matching crops found for puzzle set with {puzzleSetLength} pillars.");
        }
    }

    private void CheckPuzzleCompletion(List<RotatingPillar> puzzleSet)
    {
        if (IsPuzzleSetSolved(puzzleSet)) return; 

        foreach (var pillar in puzzleSet)
        {
            if (!pillar.correctlyOrientated)
            {
                return;
            }
        }

       
        SetPuzzleSetSolved(puzzleSet);

        OnPuzzleSolved(puzzleSet);
    }

    private bool IsPuzzleSetSolved(List<RotatingPillar> puzzleSet)
    {
        if (puzzleSet == puzzleSet1) return puzzleSet1Solved;
        if (puzzleSet == puzzleSet2) return puzzleSet2Solved;
        if (puzzleSet == puzzleSet3) return puzzleSet3Solved;
        return false;
    }

    private void SetPuzzleSetSolved(List<RotatingPillar> puzzleSet)
    {
        if (puzzleSet == puzzleSet1) puzzleSet1Solved = true;
        if (puzzleSet == puzzleSet2) puzzleSet2Solved = true;
        if (puzzleSet == puzzleSet3) puzzleSet3Solved = true;
    }


    private void OnPuzzleSolved(List<RotatingPillar> puzzleSet)
    {
        foreach (RotatingPillar pillar in puzzleSet)
        {
            pillar.LockPuzzle();
        }
        puzzlesSolved++;
        audioSource.Play();

        if (puzzlesSolved == 3)
        {
            Debug.Log("All puzzles solved! Great job!");
            rotatingPillarPuzzleSolved = true;
            PuzzleManager.Instance.totalPuzzlesSolved++;
            PuzzleManager.Instance.CheckToSeeIfPuzzlesAreComplete();
        }
    }

    public RotatingPuzzleSaveData ExportSaveData()
    {
        return new RotatingPuzzleSaveData
        {
            PuzzleSet1 = ExportPuzzleSet(puzzleSet1),
            PuzzleSet2 = ExportPuzzleSet(puzzleSet2),
            PuzzleSet3 = ExportPuzzleSet(puzzleSet3),
            CropKeys = ExportCropKeys(),
            PuzzlesSolved = puzzlesSolved,
            PuzzleSet1Solved = puzzleSet1Solved,
            PuzzleSet2Solved = puzzleSet2Solved,
            PuzzleSet3Solved = puzzleSet3Solved
        };
    }

    public void ImportSaveData(RotatingPuzzleSaveData data)
    {
        puzzlesSolved = data.PuzzlesSolved;
        rotatingPillarPuzzleSolved = (puzzlesSolved == 3);

        puzzleSet1Solved = data.PuzzleSet1Solved;
        puzzleSet2Solved = data.PuzzleSet2Solved;
        puzzleSet3Solved = data.PuzzleSet3Solved;

        ImportCropKeys(data.CropKeys);
        ImportPuzzleSet(puzzleSet1, data.PuzzleSet1);
        ImportPuzzleSet(puzzleSet2, data.PuzzleSet2);
        ImportPuzzleSet(puzzleSet3, data.PuzzleSet3);
       
    }


    private CropData GetCropDataByYieldID(int cropYieldID)
    {
        foreach (var crop in cropData)
        {
            if (crop.cropYield != null && crop.cropYield.ID == cropYieldID)
            {
                return crop;
            }
        }
        return null;
    }


    private List<RotatingPillarSaveData> ExportPuzzleSet(List<RotatingPillar> puzzleSet)
    {
        List<RotatingPillarSaveData> saveData = new List<RotatingPillarSaveData>();
        foreach (var pillar in puzzleSet)
        {
            saveData.Add(pillar.ExportSaveData());
        }
        return saveData;
    }

    private void ImportPuzzleSet(List<RotatingPillar> puzzleSet, List<RotatingPillarSaveData> saveData)
    {
        for (int i = 0; i < puzzleSet.Count; i++)
        {
            puzzleSet[i].ImportSaveData(saveData[i]);
        }
    }

    private List<CropKeySaveData> ExportCropKeys()
    {
        List<CropKeySaveData> saveData = new List<CropKeySaveData>();
        foreach (var cropKey in cropKeys)
        {
            saveData.Add(cropKey.ExportSaveData());
        }
        return saveData;
    }

    private void ImportCropKeys(List<CropKeySaveData> saveData)
    {
        for (int i = 0; i < cropKeys.Count; i++)
        {
            var item = Database.Instance.GetItem(saveData[i].CropYieldID);
            cropKeys[i].ImportSaveData(saveData[i], item);
        }
    }
}

[System.Serializable]
public struct RotatingPuzzleSaveData
{
    public List<RotatingPillarSaveData> PuzzleSet1;
    public List<RotatingPillarSaveData> PuzzleSet2;
    public List<RotatingPillarSaveData> PuzzleSet3;
    public List<CropKeySaveData> CropKeys;
    public int PuzzlesSolved;
    public bool PuzzleSet1Solved;
    public bool PuzzleSet2Solved;
    public bool PuzzleSet3Solved;
}


