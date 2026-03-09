using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaveLoadSystem;
using System;
using DG.Tweening;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(UniqueID))]
public class ItemPickup : MonoBehaviour
{
    public float PickUpRadius = 1f;

    public InventoryItemData ItemData;

    [SerializeField] private InventoryItemData mintItem;

    private SphereCollider myCollider;

    public SpriteRenderer r;

    Rigidbody rb;

    public int stackSize = 1;

    [SerializeField] private ItemPickupSaveData itemSaveData;
    private string id;

    bool beingCollected = false;
    bool canBeCollected = false;

    private void Awake()
    {
        id = GetComponent<UniqueID>().ID;
        SaveLoad.OnLoadGame += LoadGame;
        itemSaveData = new ItemPickupSaveData(ItemData, transform.position, transform.rotation);

        rb = GetComponent<Rigidbody>();

        myCollider = GetComponent<SphereCollider>();
        myCollider.isTrigger = true;
        myCollider.radius = PickUpRadius;
        stackSize = 1;

        if(!r) r = GetComponent<SpriteRenderer>();

        if(ItemData) RefreshItem(ItemData);
    }

    private void OnDisable()
    {
        stackSize = 1;
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
        if(newItem == null) return;
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
        if((other.gameObject.layer == 7 || other.gameObject.layer == 19) && rb && rb.isKinematic == false)
        {
            rb.isKinematic = true;
            rb.velocity = new Vector3(0,0,0);
        }

        if (mintItem && ItemData.ID == mintItem.ID && canBeCollected)
        {
            PlayerInteraction.Instance.currentMoney += ItemData.maxStackSize;
            beingCollected = true;
            myCollider.enabled = false;
            StartCoroutine(PickupDelay());
            return;
        }

        var inventory = other.transform.GetComponent<PlayerInventoryHolder>();

        if (!inventory || !canBeCollected) return;

        int remaining = TryAddToInventoryManually(inventory, ItemData, stackSize);

        if (remaining <= 0)
        {
            // Full pickup successful
            beingCollected = true;
            myCollider.enabled = false;
            StartCoroutine(PickupDelay());
        }
        else
        {
            // Partial pickup; update remaining amount
            stackSize = remaining;
        }
    }

    void OnEnable()
    {
        //myCollider.enabled = false;
        StartCoroutine(PickupTimer());
    }

    IEnumerator PickupTimer()
    {
        Transform spriteT = r.gameObject.transform;
        spriteT.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        spriteT.DOScale(1, 0.5f);
        yield return new WaitForSeconds(0.5f);
        canBeCollected = true;
        myCollider.enabled = false;
        myCollider.enabled = true;
    }

    IEnumerator PickupDelay()
    {
        //FindObjectOfType<PlayerEffectsHandler>().ItemCollectSFX();
        PlayerInteraction.Instance.PickupItem();
        yield return new WaitForSeconds(0.2f);
        beingCollected = false;
        canBeCollected = false;
        if(rb) 
        {
            rb.isKinematic = false;
            rb.velocity = new Vector3(0,0,0);
        }
        myCollider.enabled = true;
        ParticlePoolManager.Instance.GrabSparkParticle().transform.position = transform.position;
        AchievementManager.Instance.NotifyItemCollected(ItemData);
        gameObject.SetActive(false); // Make the item disappear
    }

    private int TryAddToInventoryManually(PlayerInventoryHolder inventory, InventoryItemData item, int totalToAdd)
    {
        int remaining = totalToAdd;

        // Fill existing in primary
        if (inventory.PrimaryInventorySystem.ContainsItem(item, out List<InventorySlot> primarySlots))
        {
            foreach (var slot in primarySlots)
            {
                if (remaining <= 0) break;

                int space = item.maxStackSize - slot.StackSize;
                int toAdd = Mathf.Min(space, remaining);
                if (toAdd > 0)
                {
                    if(item.itemBehavior) item.itemBehavior.OnRecieve(item);
                    slot.AddToStack(toAdd);
                    remaining -= toAdd;
                }
            }
        }

        // Fill existing in seconday
        if (inventory.secondaryInventorySystem.ContainsItem(item, out List<InventorySlot> secondarySlots))
        {
            foreach (var slot in secondarySlots)
            {
                if (remaining <= 0) break;

                int space = item.maxStackSize - slot.StackSize;
                int toAdd = Mathf.Min(space, remaining);
                if (toAdd > 0)
                {
                    if(item.itemBehavior) item.itemBehavior.OnRecieve(item);
                    slot.AddToStack(toAdd);
                    remaining -= toAdd;
                }
            }
        }

        // Find free slots
        while (remaining > 0)
        {
            int toAdd = Mathf.Min(item.maxStackSize, remaining);

            if (inventory.PrimaryInventorySystem.HasFreeSlot(out InventorySlot freePrimary))
            {
                freePrimary.UpdateInventorySlot(item, toAdd);
                remaining -= toAdd;
            }
            else if (inventory.secondaryInventorySystem.HasFreeSlot(out InventorySlot freeSecondary))
            {
                freeSecondary.UpdateInventorySlot(item, toAdd);
                remaining -= toAdd;
            }
            else
            {
                break; // inventory full
            }
        }

        PlayerInventoryHolder.OnPlayerInventoryChanged?.Invoke(inventory.PrimaryInventorySystem);
        PlayerInventoryHolder.OnPlayerInventoryChanged?.Invoke(inventory.secondaryInventorySystem);

        return remaining; // return leftover amount
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
