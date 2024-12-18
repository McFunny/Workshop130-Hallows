using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory System/Placeable Item")]
public class PlaceableItem : InventoryItemData
{
    public GameObject placedPrefab, hologramPrefab;
    public bool removeAfterUse = true;

    Vector3 currentTilePos;
    GameObject currentHologram;

    public void PlaceStructure(Transform player)
    {
        Vector3 fwd = player.TransformDirection(Vector3.forward);
        RaycastHit hit;

        if(Physics.Raycast(player.position, fwd, out hit, 6, 1 << 7))
        {
            Vector3 pos = StructureManager.Instance.CheckTile(hit.point);
            if(pos != new Vector3(0,0,0)) 
            {
                GameObject newStruct = StructureManager.Instance.SpawnStructureWithInstance(placedPrefab, pos);
                if(currentHologram)
                {
                    Quaternion rotate = currentHologram.transform.rotation;
                    newStruct.transform.rotation = rotate;
                }
                if (removeAfterUse)
                {
                    HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
                    HotbarDisplay.currentSlot.UpdateUISlot();
                    DisableHologram();
                }
            }

        }
    }

    public void DisplayHologram(Transform player)
    {
        if(!currentHologram)
        {
            currentHologram = Instantiate(hologramPrefab, new Vector3(0,0,0), Quaternion.identity);
            currentHologram.SetActive(false);
            currentTilePos = new Vector3(0,0,0);
        }

        Vector3 fwd = player.TransformDirection(Vector3.forward);
        RaycastHit hit;

        if(Physics.Raycast(player.position, fwd, out hit, 6, 1 << 7))
        {
            Debug.Log("Displaying");
            Vector3 pos = StructureManager.Instance.CheckTile(hit.point);
            if(pos == new Vector3(0,0,0)) 
            {
                Debug.Log("CantDisplay");
                if(currentHologram.activeSelf)
                {
                    currentHologram.SetActive(false);
                    currentTilePos = new Vector3(0,0,0);
                }
                return;
            }
            if(pos != currentTilePos)
            {
                Debug.Log("PlacedHologram");
                currentTilePos = pos;
                if(!currentHologram.activeSelf) currentHologram.SetActive(true);
                currentHologram.transform.position = currentTilePos;
            }

        }
        else if(currentHologram.activeSelf)
        {
            currentHologram.SetActive(false);
            currentTilePos = new Vector3(0,0,0);
        }
    }

    public void RotateHologram()
    {
        currentHologram.transform.Rotate(0, 90, 0);
    }

    public void DisableHologram()
    {
        currentHologram.SetActive(false);
    }

}
