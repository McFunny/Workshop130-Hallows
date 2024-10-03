using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPoolManager : MonoBehaviour
{
    public static ItemPoolManager Instance;

    public List<GameObject> itemPool = new List<GameObject>();
    public GameObject itemPrefab;

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }

        PopulateItemPool();
    }

    void Start()
    {
        
    }


    void Update()
    {
        
    }

    void PopulateItemPool()
    {
        //

        for(int i = 0; i < 30; i++)
        {
            GameObject newItem = Instantiate(itemPrefab);
            itemPool.Add(newItem);
            newItem.SetActive(false);
        }
    }

    public GameObject GrabItem(InventoryItemData data)
    {
        foreach (GameObject item in itemPool)
        {
            if(!item.activeSelf)
            {
                item.SetActive(true);
                item.GetComponent<ItemPickup>().RefreshItem(data);
                return item;
            }
        }

        //No available items, must make a new one
        GameObject newItem = Instantiate(itemPrefab);
        itemPool.Add(newItem);
        newItem.GetComponent<ItemPickup>().RefreshItem(data);
        return newItem;
    }
}
