using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory System/Crop Item")]
public class CropItem : InventoryItemData
{
    public bool plantable = false; //for seeds
    public CropData cropData;
    public bool ableToBeShot = true;

}
