using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowerPotCatacombs : MonoBehaviour
{
    private FlowerPotDecor flowerPot;

    public InventoryItemData containedFlower;
    public InventoryItemData requiredItem;

    public bool isCorrect;
    void Start()
    {
        flowerPot = GetComponent<FlowerPotDecor>();
    }

    public void UpdateFlower(InventoryItemData flower)
    {
        containedFlower = flower;
        isCorrect = containedFlower == requiredItem;
        FlowerPotManager.Instance.CheckToSeeIfSolved();

    }

    public FlowerSaveData ExportSaveData()
    {
        return new FlowerSaveData
        { isCorrectData = isCorrect };
    }

    public void ImportSaveData(FlowerSaveData data)
    {
        isCorrect = data.isCorrectData;
    }

    internal void LockPuzzle()
    {
        flowerPot.LockFlowerPot();
    }
}

[System.Serializable]

public struct FlowerSaveData
{
    public bool isCorrectData;
}
