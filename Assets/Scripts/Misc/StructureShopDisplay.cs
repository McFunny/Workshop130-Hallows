using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureShopDisplay : MonoBehaviour
{
    public Transform structureSpawnLocation;
    GameObject spawnedStructure;

    public void DisplayStructure(InventoryItemData itemData)
    {
        if (spawnedStructure != null) { LeaveShop(); }
       PlaceableItem pItem = itemData as PlaceableItem;
       if (pItem != null)
        {
            spawnedStructure = Instantiate(pItem.placedPrefab, structureSpawnLocation.position, structureSpawnLocation.rotation);
            StructureBehaviorScript structureScript = spawnedStructure.GetComponent<StructureBehaviorScript>();
            if (structureScript != null ) 
            {
                structureScript.absentFromGrid = true; 
                structureScript.muteSound = true;
            }
            Destroy(structureScript);
            //AudioPoolManager.Instance.PlayClipAtPosition(pItem.placeSound, structureSpawnLocation.position);
        }
    }

    public void LeaveShop()
    {
       Destroy(spawnedStructure);
    }
}
