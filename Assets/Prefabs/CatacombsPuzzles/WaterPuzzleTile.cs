using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;

public class WaterPuzzleTile : StructureBehaviorScript
{
    public MeshRenderer meshRenderer;
    public Material dry, wet, barren, barrenWet;

    public float waterLevel;
    public bool isSolved = false;
    public bool isLocked = false;

    public VisualEffect waterSplash;
    public GameObject waterCanvas;

    void Start()
    {
        meshRenderer.material = dry;
        waterSplash.Stop();
    }

    void Update()
    {
        if (!isLocked)
        {
            waterLevel -= Time.deltaTime;
        }

        if (waterLevel <= 0 && !isLocked)
        {
            waterCanvas.SetActive(true);
            meshRenderer.material = dry;
            isSolved = false;
        }
        else if (isLocked)
        {
            waterCanvas.SetActive(false);
            meshRenderer.material = wet;
        }
        else
        {
            waterCanvas.SetActive(false);
            meshRenderer.material = wet;
            isSolved = true;
        }
    }

    public void WaterCrops()
    {
        meshRenderer.material = wet;
        waterCanvas.SetActive(false);
        waterLevel = 10;
        waterSplash.Play();
        isSolved = true;
    }

    public override void HitWithWater()
    {
        WaterCrops();
    }

    public void SetSolvedState(bool solved)
    {
        isSolved = solved;
        if (solved)
        {
            isLocked = true;
            meshRenderer.material = wet;
            waterCanvas.SetActive(false);
            waterSplash.Stop();
            waterLevel = 10;
        }
    }
}
