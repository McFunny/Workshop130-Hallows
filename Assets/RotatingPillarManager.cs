using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

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
        GenerateCrops();
    }

    private void GenerateCrops()
    {
        int randomCropIndex;

        if (puzzleSet1 != null && cropData.Count > 0)
        {
            randomCropIndex = Random.Range(0, cropData.Count);
            foreach (RotatingPillar p in puzzleSet1)
            {
                p.SetUpSprites(cropData[randomCropIndex]);

                
                p.OnInteractionComplete += (pillar) => CheckPuzzleCompletion(puzzleSet1);
            }
            puzzleSet1SpecifiedCrop = cropData[randomCropIndex];
            cropData.RemoveAt(randomCropIndex);
        }

        if (puzzleSet2 != null && cropData.Count > 0)
        {
            randomCropIndex = Random.Range(0, cropData.Count);
            foreach (RotatingPillar p in puzzleSet2)
            {
                p.SetUpSprites(cropData[randomCropIndex]);

                
                p.OnInteractionComplete += (pillar) => CheckPuzzleCompletion(puzzleSet2);
            }
            puzzleSet2SpecifiedCrop = cropData[randomCropIndex];
            cropData.RemoveAt(randomCropIndex);
        }

        if (puzzleSet3 != null && cropData.Count > 0)
        {
            randomCropIndex = Random.Range(0, cropData.Count);
            foreach (RotatingPillar p in puzzleSet3)
            {
                p.SetUpSprites(cropData[randomCropIndex]);

                
                p.OnInteractionComplete += (pillar) => CheckPuzzleCompletion(puzzleSet3);
            }
            puzzleSet3SpecifiedCrop = cropData[randomCropIndex];
            cropData.RemoveAt(randomCropIndex);
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
            Debug.Log("WOW CAM GREAT JOB YOU FIGURED IT OUT HOW DO YOU FEEL? DOES IT FEEL GOOD TO BE A SMARTY PANTS??!!!! YAY CAMERON! HIP HIP HOORAY HIP HIP HOORAY!!!!!!!!!!!!");
        }
    }


}
