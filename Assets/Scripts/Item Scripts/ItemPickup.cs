using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaveLoadSystem;
using System;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(UniqueID))]
public class ItemPickup : MonoBehaviour
{
    public float PickUpRadius = 1f;

    public InventoryItemData ItemData;

    private SphereCollider myCollider;

    public SpriteRenderer r;

    [SerializeField] private ItemPickupSaveData itemSaveData;
    private string id;

    private void Awake()
    {
        id = GetComponent<UniqueID>().ID;
        SaveLoad.OnLoadGame += LoadGame;
        itemSaveData = new ItemPickupSaveData(ItemData, transform.position, transform.rotation);



        myCollider = GetComponent<SphereCollider>();
        myCollider.isTrigger = true;
        myCollider.radius = PickUpRadius;

        if(!r) r = GetComponent<SpriteRenderer>();
    }

   

    private void Start()
    {
        SaveGameManager.data.activeItems.Add(id, itemSaveData);
    }

    private void LoadGame(SaveData data)
    {
        if (data.collectedItems.Contains(id))
        {
            Debug.Log("Destroying");
            Destroy(this.gameObject);
        }
        
    }

    public void RefreshItem(InventoryItemData newItem)
    {
        r.sprite = newItem.icon;
        ItemData = newItem;
    }

    private void OnDestroy()
    {
        if (SaveGameManager.data == null)
        {
            Debug.LogError("SaveGameManager.data is null");
        }
        else if (SaveGameManager.data.activeItems == null)
        {
            Debug.LogError("SaveGameManager.data.activeItems is null");
        }
        else
        {
            if (SaveGameManager.data.activeItems.ContainsKey(id))
            {
                SaveGameManager.data.activeItems.Remove(id);
            }
        }

        SaveLoad.OnLoadGame -= LoadGame;
    }



    private void OnTriggerEnter(Collider other)
    {
        var inventory = other.transform.GetComponent<PlayerInventoryHolder>();

       if (!inventory) return;

        if (inventory.AddToInventory(ItemData, 1))
        {
           
            SaveGameManager.data.collectedItems.Add(id);
            print(SaveGameManager.data.collectedItems[0]);
            FindObjectOfType<PlayerEffectsHandler>().ItemCollectSFX();
            print("Added");
            //Destroy(this.gameObject);
            gameObject.SetActive(false);
        }


    }
}

[System.Serializable]
public struct ItemPickupSaveData
{
    public InventoryItemData itemData;
    public Vector3 position;
    public Quaternion rotation;

    public ItemPickupSaveData(InventoryItemData _itemData, Vector3 _position, Quaternion _rotation)
    {
        itemData = _itemData;
        position = _position;
        rotation = _rotation;
    }
}
