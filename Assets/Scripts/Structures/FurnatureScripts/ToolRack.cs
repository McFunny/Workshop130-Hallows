using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolRack : FurnitureBehaviorScript
{
    public List<ToolRackTool> rackSlots;

    public List<ToolType> allowedTypes;

    public AudioClip placeItemSFX;

    public void Awake()
    {
        base.Awake();
        for(int i = 0; i < rackSlots.Count; i++)
        {
            savedItems.Add(null);
        }
    }

    public void Start()
    {
        base.Start();
        FurnitureStart();
        RefreshRack();
    }

    public override void StructureInteraction()
    {
        /*InventoryItemData item = HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData;
        if(item != null)
        {
            ToolItem t = item as ToolItem;
            if(t != null)
            {
                ItemInteraction(item);
                return;
            }
        }*/


        if(!CanBeRemoved())
        {
            RemoveClosestSocket();
            return;
        }
        bool addedSuccessfully = PlayerInventoryHolder.Instance.AddToInventory(itemForm, 1);
        if (addedSuccessfully)
        {
            Destroy(this.gameObject);
        }
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(allowedTypes.Contains(type))
        {
            PlaceOnClosestSocket(HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData);
        }
    }

    public override void ItemInteraction(InventoryItemData item)
    {
        ToolItem t = item as ToolItem;
        if(t == null) return;

        if(allowedTypes.Contains(t.tool))
        {
            PlaceOnClosestSocket(item);
        }
    }

    public override void DigAction()
    {
        PlayerInventoryHolder.Instance.AddToInventory(itemForm, 1);

        Destroy(this.gameObject);
    }

    public void RefreshRack()
    {
        for(int i = 0; i < rackSlots.Count; ++i)
        {
            if(rackSlots[i].activeObject) //Clear the model
            {
                rackSlots[i].activeObject.SetActive(false);
                rackSlots[i].activeObject = null;
            }

            if(savedItems.Count < i) savedItems.Add(null);

            if(savedItems[i] == null)
            {
                continue;
            }

            ToolItem t = savedItems[i] as ToolItem;
            if(t == null) continue;

            switch(t.tool)
            {
                case ToolType.Scythe:
                    if(t.isUpgrade) rackSlots[i].activeObject = rackSlots[i].toolObjects[4];
                    else rackSlots[i].activeObject = rackSlots[i].toolObjects[3];
                    break;
                case ToolType.Hoe:
                    if(t.isUpgrade) rackSlots[i].activeObject = rackSlots[i].toolObjects[1];
                    else rackSlots[i].activeObject = rackSlots[i].toolObjects[0];
                    break;
                case ToolType.Shovel:
                    rackSlots[i].activeObject = rackSlots[i].toolObjects[2];
                    break;
                case ToolType.NutTester:
                    rackSlots[i].activeObject = rackSlots[i].toolObjects[6];
                    break;
                case ToolType.ShotGun:
                    rackSlots[i].activeObject = rackSlots[i].toolObjects[5];
                    break;
                case ToolType.BugNet:
                    rackSlots[i].activeObject = rackSlots[i].toolObjects[7];
                    break;
            }
            if(rackSlots[i].activeObject) rackSlots[i].activeObject.SetActive(true);
        }
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

        for(int i = 0; i < rackSlots.Count; i++)
        {
            dist = Vector3.Distance(rackSlots[i].pivot.position, hitPos);
            if(dist < minDist && savedItems[i] == null)
            {
                minDist = dist;
                closestSocket = i;
            }
        }
        if(closestSocket != -1)
        {
            savedItems[closestSocket] = item;
            RefreshRack();

            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            PlayerInventoryHolder.Instance.UpdateInventory();

            AudioPoolManager.Instance.PlayClipAtPosition(placeItemSFX, transform.position, 0.4f, 40);
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

        for(int i = 0; i < rackSlots.Count; i++)
        {
            dist = Vector3.Distance(rackSlots[i].pivot.position, hitPos);
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

            savedItems[closestSocket] = null;

            RefreshRack();

            PlayerInventoryHolder.Instance.UpdateInventory();

            AudioPoolManager.Instance.PlayClipAtPosition(placeItemSFX, transform.position, 0.4f, 40);
        }
    }

    bool CanBeRemoved()
    {
        if(savedItems.Count == 0) return true;
        for(int i = 0; i < rackSlots.Count; i++)
        {
            if(savedItems[i] != null) return false;
        }
        return true;
    }

    public override void LoadVariables()
    {
        if(savedItems.Count == 0)
        {
            for(int i = 0; i < rackSlots.Count; i++)
            {
                savedItems.Add(null);
            }
        }
        RefreshRack();
    }
}
[System.Serializable]
public class ToolRackTool
{
    //public GameObject holderParent;
    public Transform pivot;
    public List<GameObject> toolObjects;
    public GameObject activeObject;
}
