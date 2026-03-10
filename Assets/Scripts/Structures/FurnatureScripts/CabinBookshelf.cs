using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaveLoadSystem;

public class CabinBookshelf : FurnitureBehaviorScript
{
    public List<SpriteRenderer> itemSockets = new List<SpriteRenderer>();

    public void Awake()
    {
        base.Awake();
        for(int i = 0; i < itemSockets.Count; i++)
        {
            savedItems.Add(null);
        }
        SaveLoad.OnLoadGame += LoadData;
        SaveLoad.OnSaveGame += SaveData;
    }

    private void OnDisable()
    {
        SaveLoad.OnLoadGame -= LoadData;
        SaveLoad.OnSaveGame -= SaveData;
    }

    public void Start()
    {
        base.Start();
        FurnitureStart();
        RefreshSockets();
    }

    public override void StructureInteraction()
    {
        RemoveClosestSocket();
    }

    public override void ItemInteraction(InventoryItemData item)
    {
        if(item && !item.isKeyItem)
        {
            PlaceOnClosestSocket(item);
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

    public void SaveData()
    {
        List<int> itemIDs = new List<int>();

        for(int i = 0; i < savedItems.Count; i++)
        {
            if(savedItems[i] == null) itemIDs.Add(-1);
            else itemIDs.Add(savedItems[i].ID);
        }

        SaveLoad.CurrentSaveData.bookcaseItemIDs = itemIDs;

    }

    private void LoadData(SaveData data)
    {
        if(data.bookcaseItemIDs.Count == 0)
        {
            for(int i = 0; i < itemSockets.Count; i++)
            {
                savedItems.Add(null);
            }
            return;
        }
        
        for(int i = 0; i < data.bookcaseItemIDs.Count; i++)
        {
            if(data.bookcaseItemIDs[i] == -1) savedItems.Add(null);
            else savedItems.Add(Database.Instance.GetItem(data.bookcaseItemIDs[i]));
        }


        RefreshSockets();
    }

}
