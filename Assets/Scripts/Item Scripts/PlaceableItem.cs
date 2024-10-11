using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory System/Placeable Item")]
public class PlaceableItem : InventoryItemData
{
    public GameObject placedPrefab;
    public bool removeAfterUse = true;

    public void PlaceStructure(Transform player)
    {
        Vector3 fwd = player.TransformDirection(Vector3.forward);
        RaycastHit hit;
        StructureManager structManager = FindObjectOfType<StructureManager>();

        if(Physics.Raycast(player.position, fwd, out hit, 10, 1 << 7))
        {
            Vector3 pos = structManager.CheckTile(hit.point);
            if(pos != new Vector3(0,0,0)) 
            {
                structManager.SpawnStructure(placedPrefab, pos);
                if(removeAfterUse) HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            }

        }
    }

}
