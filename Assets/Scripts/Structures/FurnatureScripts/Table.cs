using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Table : FurnitureBehaviorScript
{
    //when inserting onto a table, have the object rotate to the table's rotation

    public List<TableSocket> sockets = new List<TableSocket>();

    public void Awake()
    {
        base.Awake();
    }

    public void Start()
    {
        base.Start();
        FurnitureStart();
        StartCoroutine(DelayedStart());
    }

    public override void StructureInteraction()
    {
        if(!CanBeRemoved() || (absentFromGrid && !onTable)) return;
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
    }

    public override void ItemInteraction(InventoryItemData item)
    {
        PlaceableItem p = item as PlaceableItem;
        if(p && p.canPlaceOnTable)
        {
            PlaceOnClosestSocket(p);
        }
    }

    public override void DigAction()
    {
        PlayerInventoryHolder.Instance.AddToInventory(itemForm, 1);

        Destroy(this.gameObject);
    }

    void PlaceOnClosestSocket(PlaceableItem item)
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

        for(int i = 0; i < sockets.Count; i++)
        {
            dist = Vector3.Distance(sockets[i].socketTransform.position, hitPos);
            if(dist < minDist && sockets[i].socketedObject == null)
            {
                minDist = dist;
                closestSocket = i;
            }
        }
        if(closestSocket != -1)
        {
            GameObject placedObject = Instantiate(item.placedPrefab, sockets[closestSocket].socketTransform.position, transform.rotation);
            sockets[closestSocket].socketedObject = placedObject.GetComponentInChildren<FurnitureBehaviorScript>();
            sockets[closestSocket].socketedObject.onTable = true;

            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            PlayerInventoryHolder.Instance.UpdateInventory();
            item.DisableHologram();
        }
    }

    void RefreshSockets()
    {
        for(int i = 0; i < sockets.Count; i++)
        {
            Collider[] hitColliders = Physics.OverlapSphere(sockets[i].socketTransform.position, 1);
            foreach(Collider collider in hitColliders)
            {
                var f = collider.gameObject.GetComponentInParent<FurnitureBehaviorScript>();
                if(f && f != this)
                {
                    sockets[i].socketedObject = f;
                    f.onTable = true;
                    break;
                }
            }
        }
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(1);
        RefreshSockets();
    }

    bool CanBeRemoved()
    {
        for(int i = 0; i < sockets.Count; i++)
        {
            if(sockets[i].socketedObject != null) return false;
        }
        return true;
    }
}

[System.Serializable]
public class TableSocket
{
    public Transform socketTransform;
    public FurnitureBehaviorScript socketedObject;
}
