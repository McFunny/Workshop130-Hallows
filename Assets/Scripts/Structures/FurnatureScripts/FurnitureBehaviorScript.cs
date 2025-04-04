using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FurnitureBehaviorScript : StructureBehaviorScript
{
    //have the furnature that is meant for tables to not be placeable objects, but misc instead.
    //have tables have sockets on them that represent grid space. On load, have each socket do a check to find loaded structs that are on them. 
    //For interacting with the table, have a distance check from the raycast hit point across all sockets

    public InventoryItemData recoveredItem;

    public bool onTable = false; //dictates if this should affect tile grid when removed

    [ContextMenu("Initialize Furnature Stats")]
    public void InitializeFurnitureStats()
    {
        flammable = false;
        wealthValue = 0;
        isObstacle = false;
        destructable = false;
    }

    void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        FurnitureStart();
    }

    public void FurnitureStart()
    {
        if(StructureManager.Instance.ValidateGridType(transform.position, GridType.Any) == false) absentFromGrid = true;
        if(!absentFromGrid) canShowHighlight = false;
        //print("Furniture Start");
        base.Start();
    }

    public override void StructureInteraction()
    {
        bool addedSuccessfully = PlayerInventoryHolder.Instance.AddToInventory(recoveredItem, 1);
        if (addedSuccessfully)
        {
            Destroy(this.gameObject);
        }
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(type == ToolType.Shovel && PlayerInventoryHolder.Instance.IsInventoryFull() == false)
        {
            StartCoroutine(DugUp());
            success = true;
        }
    }

    IEnumerator DugUp()
    {
        yield return new WaitForSeconds(1);
        PlayerInventoryHolder.Instance.AddToInventory(recoveredItem, 1);

        Destroy(this.gameObject);
    }

    void OnDestroy()
    {
        OnFurnitureDestroy();
        base.OnDestroy();
    }

    public void OnFurnitureDestroy()
    {
        if(onTable) clearTileOnDestroy = false;
    }

}
