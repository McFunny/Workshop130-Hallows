using System.Collections.Generic;
using UnityEngine;

public class RotatingPillarManager : MonoBehaviour
{
    public List<RotatingPillar> puzzleSet1 = new List<RotatingPillar>();
    public List<RotatingPillar> puzzleSet2 = new List<RotatingPillar>();
    public List<RotatingPillar> puzzleSet3 = new List<RotatingPillar>();
    public List<CropKey> cropKeys = new List<CropKey>();
    public List<CropData> cropData = new List<CropData>();

    private int puzzlesSolved = 0;

    private void Start()
    {
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
            cropKey.cropData = specifiedCrop;
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
        foreach (var pillar in puzzleSet)
        {
            if (!pillar.correctlyOrientated)
            {
                return;
            }
        }
        OnPuzzleSolved(puzzleSet);
    }

    private void OnPuzzleSolved(List<RotatingPillar> puzzleSet)
    {
        foreach (RotatingPillar pillar in puzzleSet)
        {
            pillar.LockPuzzle();
        }
        puzzlesSolved++;

        if (puzzlesSolved == 3)
        {
            Debug.Log("All puzzles solved! Great job!");
        }
    }
}
