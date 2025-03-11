using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugButtonID : MonoBehaviour
{
    PlayerInventoryHolder playerInv;
    public InventoryItemData data;
    // Start is called before the first frame update
    void Start()
    {
        playerInv = FindFirstObjectByType<PlayerInventoryHolder>();
    }

    public void SpawnItem()
    {
        playerInv.AddToInventory(data, 1);
    }
}
