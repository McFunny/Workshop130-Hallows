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

    bool beingCollected = false;

    private void Awake()
    {
        id = GetComponent<UniqueID>().ID;
        SaveLoad.OnLoadGame += LoadGame;
        itemSaveData = new ItemPickupSaveData(ItemData, transform.position, transform.rotation);



        myCollider = GetComponent<SphereCollider>();
        myCollider.isTrigger = true;
        myCollider.radius = PickUpRadius;

        if(!r) r = GetComponent<SpriteRenderer>();

        if(ItemData) RefreshItem(ItemData);
    }

   

    private void Start()
    {
        //SaveGameManager.data.activeItems.Add(id, itemSaveData);
    }

    private void LoadGame(SaveData data)
    {
        if (data.collectedItems.Contains(id))
        {
            Debug.Log("Destroying");
            Destroy(this.gameObject);
        }
        
    }

    void Update()
    {
        if(beingCollected)
        {
            transform.position = Vector3.MoveTowards(transform.position, PlayerInteraction.Instance.transform.position, 0.1f);
        }
    }

    public void RefreshItem(InventoryItemData newItem)
    {
        r.sprite = newItem.icon;
        ItemData = newItem;
    }

    private void OnDestroy()
    {
        return;
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
            //SaveGameManager.data.collectedItems.Add(id);
            beingCollected = true;
            myCollider.enabled = false;
            StartCoroutine(PickupDelay());
        }
    }

    void OnEnable()
    {
        myCollider.enabled = false;
        StartCoroutine(PickupTimer());
        beingCollected = false;
    }

    IEnumerator PickupTimer()
    {
        yield return new WaitForSeconds(0.5f);
        myCollider.enabled = true;
    }

    IEnumerator PickupDelay()
    {
        yield return new WaitForSeconds(0.2f);
        FindObjectOfType<PlayerEffectsHandler>().ItemCollectSFX();
        beingCollected = false;
        gameObject.SetActive(false); // Make the item disappear
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
