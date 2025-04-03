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

    public AudioClip placeSound;

    public GridType gridType;
    public GridSize gridSize;
    [Header("Furnature Variables")]
    public bool canPlaceOnFloor = true;
    public bool canPlaceOnTable = false;

    public void PlaceStructure(Transform player)
    {
        Vector3 fwd = player.TransformDirection(Vector3.forward);
        RaycastHit hit;

        if(Physics.Raycast(player.position, fwd, out hit, 8, 1 << 7))
        {
            Vector3 pos = new Vector3(0,0,0);
            if(gridSize == GridSize.OneByOne)
            {
                pos = StructureManager.Instance.CheckTile(hit.point);
            }
            if(gridSize == GridSize.TwoByTwo)
            {
                pos = StructureManager.Instance.CheckLargeTile(hit.point);
            }
            if(gridSize == GridSize.OneByTwo)
            {
                pos = StructureManager.Instance.CheckOneByTwoTile(hit.point, currentHologram.transform.rotation);
            }

            if(pos != new Vector3(0,0,0) && StructureManager.Instance.ValidateGridType(pos, gridType)) 
            {
                GameObject newStruct = StructureManager.Instance.SpawnStructureWithInstance(placedPrefab, pos);
                if(gridSize == GridSize.TwoByTwo) StructureManager.Instance.SetLargeTile(pos);
                if(gridSize == GridSize.OneByTwo) StructureManager.Instance.SetOneByTwoTile(pos);
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
                if(placeSound) HandItemManager.Instance.toolSource.PlayOneShot(placeSound);
            }

        }
    }

    public void DisplayHologram(Transform player)
    {
        if(!hologramPrefab) return;
        if(!currentHologram)
        {
            currentHologram = Instantiate(hologramPrefab, new Vector3(0,0,0), Quaternion.identity);
            currentHologram.SetActive(false);
            currentTilePos = new Vector3(0,0,0);
        }

        Vector3 fwd = player.TransformDirection(Vector3.forward);
        RaycastHit hit;

        if(Physics.Raycast(player.position, fwd, out hit, 8, 1 << 7))
        {
            //Debug.Log("Displaying");
            Vector3 pos = new Vector3(0,0,0);
            if(gridSize == GridSize.OneByOne)
            {
                pos = StructureManager.Instance.CheckTile(hit.point);
            }
            if(gridSize == GridSize.TwoByTwo)
            {
                pos = StructureManager.Instance.CheckLargeTile(hit.point);
            }
            if(gridSize == GridSize.OneByTwo)
            {
                pos = StructureManager.Instance.CheckOneByTwoTile(hit.point, currentHologram.transform.rotation);
            }

            if(pos == new Vector3(0,0,0) || !StructureManager.Instance.ValidateGridType(pos, gridType)) 
            {
                //Debug.Log("CantDisplay");
                if(currentHologram.activeSelf)
                {
                    currentHologram.SetActive(false);
                    currentTilePos = new Vector3(0,0,0);
                }
                return;
            }
            if(pos != new Vector3(0,0,0) && (pos != currentTilePos || !currentHologram.activeSelf))
            {
                //Debug.Log("PlacedHologram");
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
        if(!currentHologram) return;
        currentHologram.transform.Rotate(0, 90, 0);
    }

    public void DisableHologram()
    {
        if(!currentHologram) return;
        currentHologram.SetActive(false);
    }

}

public enum GridSize
{
    OneByOne,
    OneByTwo,
    TwoByTwo
}
