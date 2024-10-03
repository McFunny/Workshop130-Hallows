using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Crop Object", menuName = "Crop")]
public class CropData : ScriptableObject
{
    public int growthStages = 5; //How many different stages of life does it have
    public int hoursPerStage = 6; //How many in game hours must take place before each growth change
    public Sprite[] cropSprites; //should equal growth stages

    public InventoryItemData cropYield; //what does the crop drop
    public InventoryItemData cropSeed;
    public int cropYieldAmount = 1;
    public int seedYieldAmount = 2;
    
    public float waterIntake = 1; //how many units of water does it consume per [hour?]
    //light requirement


}

