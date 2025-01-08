using System.Collections.Generic;
using UnityEngine;

public class RotatingPillarManager : MonoBehaviour
{
    public List<RotatingPillar> puzzleSet1 = new List<RotatingPillar>();
    public List<RotatingPillar> puzzleSet2 = new List<RotatingPillar>();
    public List<RotatingPillar> puzzleSet3 = new List<RotatingPillar>();
    public List<CropData> cropData = new List<CropData>();

    private int puzzlesSolved = 0;

    private CropData puzzleSet1SpecifiedCrop;
    private CropData puzzleSet2SpecifiedCrop;
    private CropData puzzleSet3SpecifiedCrop;

    private void Start()
    {
        AssignCropsToPuzzles();
    }

    private void AssignCropsToPuzzles()
    {
        // Filter CropData based on length
        var cropDataGroups = new Dictionary<int, List<CropData>>(); //dictionary that holds the length of each crop
        foreach (var crop in cropData)
        {
            int cropLength = crop.cropSprites.Length;
            if (!cropDataGroups.ContainsKey(cropLength))
            {
                cropDataGroups[cropLength] = new List<CropData>();
            }
            cropDataGroups[cropLength].Add(crop);
        }

        // Assign crops to each puzzle set
        AssignCropToPuzzleSet(puzzleSet1, ref puzzleSet1SpecifiedCrop, cropDataGroups);
        AssignCropToPuzzleSet(puzzleSet2, ref puzzleSet2SpecifiedCrop, cropDataGroups);
        AssignCropToPuzzleSet(puzzleSet3, ref puzzleSet3SpecifiedCrop, cropDataGroups);
    }

    private void AssignCropToPuzzleSet(List<RotatingPillar> puzzleSet, ref CropData specifiedCrop, Dictionary<int, List<CropData>> cropDataGroups)
    {
        int puzzleSetLength = puzzleSet.Count;
        if (cropDataGroups.TryGetValue(puzzleSetLength, out var matchingCrops) && matchingCrops.Count > 0)
        {
            // Randomly select a CropData from the matching group
            int randomIndex = Random.Range(0, matchingCrops.Count);
            specifiedCrop = matchingCrops[randomIndex];
            matchingCrops.RemoveAt(randomIndex);

            // Assign CropData to each pillar in the puzzle set
            foreach (var pillar in puzzleSet)
            {
                pillar.SetUpSprites(specifiedCrop);
                pillar.OnInteractionComplete += (completedPillar) => CheckPuzzleCompletion(puzzleSet);
            }
        }
        else
        {
            Debug.LogWarning($"No matching crops found for puzzle set with {puzzleSetLength} pillars.");
        }
    }

    private void CheckPuzzleCompletion(List<RotatingPillar> puzzleSet)
    {
        foreach (RotatingPillar pillar in puzzleSet)
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
