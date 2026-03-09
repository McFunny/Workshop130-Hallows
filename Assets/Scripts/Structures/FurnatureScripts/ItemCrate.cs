using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCrate : FurnitureBehaviorScript
{
    public List<SpriteRenderer> itemSockets = new List<SpriteRenderer>();

    public void Awake()
    {
        base.Awake();
        for(int i = 0; i < itemSockets.Count; i++)
        {
            savedItems.Add(null);
        }
    }

    public void Start()
    {
        base.Start();
        FurnitureStart();
        RefreshSockets();
    }

    public override void StructureInteraction()
    {
        if(!CanBeRemoved()/* || (absentFromGrid && !onTable)*/)
        {
            RemoveClosestSocket();
            return;
        }

        //if not on cabin, return
        if(PlacedInCabin() == false) return;

        bool addedSuccessfully = PlayerInventoryHolder.Instance.AddToInventory(itemForm, 1);
        if (addedSuccessfully)
        {
            Destroy(this.gameObject);
        }
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(!CanBeRemoved()) return;
        if(type == ToolType.Shovel && PlayerInventoryHolder.Instance.IsInventoryFull() == false)
        {
            //StartCoroutine(DugUp());
            success = true;
        }
        if(type == ToolType.Pyrefly || type == ToolType.Hydrofly)
        {
            ItemInteraction(HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData);
        }
    }

    public override void ItemInteraction(InventoryItemData item)
    {
        if(item && !item.isKeyItem)
        {
            PlaceOnClosestSocket(item);
        }
    }

    public override bool RepairWithSealant(int amount)
    {
        if(base.RepairWithSealant(amount) == false)
        {
            ItemInteraction(HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData);
            return false;
        }
        else return true;
    }

    public override void DigAction()
    {
        PlayerInventoryHolder.Instance.AddToInventory(itemForm, 1);

        Destroy(this.gameObject);
    }

    void PlaceOnClosestSocket(InventoryItemData item)
    {
        Vector3 fwd = PlayerInteraction.Instance.mainCam.transform.TransformDirection(Vector3.forward);
        RaycastHit hit;
        Vector3 hitPos;
        if (Physics.Raycast(PlayerInteraction.Instance.mainCam.transform.position, fwd, out hit, 15, 1 << 6))
        {
            hitPos = hit.point;
        }
        else return;

        float minDist = 100;
        float dist;
        int closestSocket = -1;

        for(int i = 0; i < itemSockets.Count; i++)
        {
            dist = Vector3.Distance(itemSockets[i].transform.position, hitPos);
            if(dist < minDist && savedItems[i] == null)
            {
                minDist = dist;
                closestSocket = i;
            }
        }
        if(closestSocket != -1)
        {
            itemSockets[closestSocket].sprite = item.icon;
            savedItems[closestSocket] = item;

            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            PlayerInventoryHolder.Instance.UpdateInventory();
        }
    }

    void RemoveClosestSocket()
    {
        Vector3 fwd = PlayerInteraction.Instance.mainCam.transform.TransformDirection(Vector3.forward);
        RaycastHit hit;
        Vector3 hitPos;
        if (Physics.Raycast(PlayerInteraction.Instance.mainCam.transform.position, fwd, out hit, 15, 1 << 6))
        {
            hitPos = hit.point;
        }
        else return;

        float minDist = 100;
        float dist;
        int closestSocket = -1;

        for(int i = 0; i < itemSockets.Count; i++)
        {
            dist = Vector3.Distance(itemSockets[i].transform.position, hitPos);
            if(dist < minDist && savedItems[i] != null)
            {
                minDist = dist;
                closestSocket = i;
            }
        }
        if(closestSocket != -1)
        {
            bool addedSuccessfully = PlayerInventoryHolder.Instance.AddToInventory(savedItems[closestSocket], 1);
            if (!addedSuccessfully) return;

            itemSockets[closestSocket].sprite = null;
            savedItems[closestSocket] = null;

            PlayerInventoryHolder.Instance.UpdateInventory();
        }
    }

    void RefreshSockets()
    {
        for(int i = 0; i < itemSockets.Count; i++)
        {
            if(i >= savedItems.Count)
            {
                itemSockets[i].sprite = null;
                continue;
            }
            if(savedItems[i] != null) itemSockets[i].sprite = savedItems[i].icon;
            else itemSockets[i].sprite = null;
        }
    }

    bool CanBeRemoved()
    {
        if(savedItems.Count == 0) return true;
        for(int i = 0; i < itemSockets.Count; i++)
        {
            if(savedItems[i] != null) return false;
        }
        return true;
    }

    bool PlacedInCabin()
    {
        return StructureManager.Instance.ValidateGridType(transform.position, GridType.Cabin);
    }

    public override void LoadVariables()
    {
        if(savedItems.Count == 0)
        {
            for(int i = 0; i < itemSockets.Count; i++)
            {
                savedItems.Add(null);
            }
        }
        RefreshSockets();
    }

    void OnDestroy()
    {
        base.OnDestroy();
        OnFurnitureDestroy();
        if (!gameObject.scene.isLoaded) return; 
        //drop seeds
        GameObject droppedItem;
        foreach(InventoryItemData item in savedItems)
        {
            if(item == null) continue;
            droppedItem = ItemPoolManager.Instance.GrabItem(item);
            droppedItem.transform.position = focalPoint.position;
        }
    }
}
