using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ItemCrate : FurnitureBehaviorScript
{
    public List<SpriteRenderer> itemSockets = new List<SpriteRenderer>();
    public List<TextMeshProUGUI> amountText = new List<TextMeshProUGUI>();
    List<int> stackAmounts = new List<int>();

    public void Awake()
    {
        base.Awake();
        for(int i = 0; i < itemSockets.Count; i++)
        {
            savedItems.Add(null);
            stackAmounts.Add(0);
        }
    }

    public void Start()
    {
        base.Start();
        FurnitureStart();
        RefreshSockets();

        if(PlacedInCabin() == false) return;
    }

    public override void StructureInteraction()
    {
        if(!CanBeRemoved()/* || (absentFromGrid && !onTable)*/)
        {
            RemoveClosestSocket();
            return;
        }

        //if not on cabin, return
        if(StructureManager.Instance.ValidateGridType(transform.position, GridType.Farm)) salvageChance = 80;

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
            if(dist < minDist && (savedItems[i] == null || (savedItems[i] == item && savedItems[i].maxStackSize > stackAmounts[i])))
            {
                minDist = dist;
                closestSocket = i;
            }
        }
        if(closestSocket != -1)
        {
            int amountToAdd = 0;
            if(savedItems[closestSocket] == null) //New item in slot
            {
                itemSockets[closestSocket].sprite = item.icon;
                savedItems[closestSocket] = item;
                stackAmounts[closestSocket] = HotbarDisplay.currentSlot.AssignedInventorySlot.StackSize;
                if(stackAmounts[closestSocket] <= 1) amountText[closestSocket].text = "";
                else amountText[closestSocket].text = "x " + stackAmounts[closestSocket];

                amountToAdd = HotbarDisplay.currentSlot.AssignedInventorySlot.StackSize;
            }
            else //Adding to existing stack
            {
                while(item.maxStackSize > (stackAmounts[closestSocket] + amountToAdd) && HotbarDisplay.currentSlot.AssignedInventorySlot.StackSize > amountToAdd) amountToAdd++;

                stackAmounts[closestSocket] += amountToAdd;
                amountText[closestSocket].text = "x " + stackAmounts[closestSocket];
            }
            audioHandler.PlaySound(audioHandler.itemInteractSound);
            ParticlePoolManager.Instance.GrabSparkParticle().transform.position = itemSockets[closestSocket].transform.position;

            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(amountToAdd);
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
            int amountToGive = stackAmounts[closestSocket];
            if(amountToGive <= 0) amountToGive = 1;

            int amountRemaining = TryAddToInventoryManually(savedItems[closestSocket], amountToGive);
            if(amountRemaining == amountToGive) return; //No room

            //bool addedSuccessfully = PlayerInventoryHolder.Instance.AddToInventory(savedItems[closestSocket], amountToGive);
            //if (!addedSuccessfully) return;

            if(amountRemaining == 0)
            {
                itemSockets[closestSocket].sprite = null;
                savedItems[closestSocket] = null;
                stackAmounts[closestSocket] = 0;
                amountText[closestSocket].text = "";
            }
            else
            {
                stackAmounts[closestSocket] = amountRemaining;
                amountText[closestSocket].text = " " + amountRemaining;
            }
            audioHandler.PlaySound(audioHandler.itemInteractSound);
            ParticlePoolManager.Instance.GrabSparkParticle().transform.position = itemSockets[closestSocket].transform.position;
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
                amountText[i].text = "";
                continue;
            }
            if(savedItems[i] != null) 
            {
                itemSockets[i].sprite = savedItems[i].icon;
                if(stackAmounts[i] <= 1) amountText[i].text = "";
                else amountText[i].text = "x " + stackAmounts[i];
            }
            else 
            {
                itemSockets[i].sprite = null;
                amountText[i].text = "";
            }
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

    public override void SaveVariables()
    {
        saveInt1 = 0;
        saveInt2 = 0;
        saveInt3 = 0;

        if(stackAmounts.Count > 0)
        {
            saveInt1 = stackAmounts[0];
            saveInt2 = stackAmounts[1];
            saveInt3 = stackAmounts[2];
        }
    }

    public override void LoadVariables()
    {
        if(saveInt1 != -1) stackAmounts[0] = saveInt1;
        if(saveInt2 != -1) stackAmounts[1] = saveInt2;
        if(saveInt3 != -1) stackAmounts[2] = saveInt3;

        // Base class stripped nulls, so savedItems may be compacted e.g. [ItemA] instead of [null, ItemA, null]
        // Re-expand it back to 3 slots using stackAmounts to identify which sockets have items
        List<InventoryItemData> compactedItems = new List<InventoryItemData>(savedItems);
        savedItems.Clear();

        int compactedIndex = 0;
        for (int i = 0; i < itemSockets.Count; i++)
        {
            if (stackAmounts[i] > 0 && compactedIndex < compactedItems.Count)
            {
                savedItems.Add(compactedItems[compactedIndex]);
                compactedIndex++;
            }
            else
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
        int i = -1;
        foreach(InventoryItemData item in savedItems)
        {
            i++;
            if(item == null) continue;
            droppedItem = ItemPoolManager.Instance.GrabItem(item);
            droppedItem.transform.position = focalPoint.position;
            if(stackAmounts[i] > 1) droppedItem.GetComponent<ItemPickup>().stackSize = stackAmounts[i];
        }
    }

    private int TryAddToInventoryManually(InventoryItemData item, int totalToAdd) //Called when taking items out of crate
    {
        PlayerInventoryHolder inventory = PlayerInventoryHolder.Instance;
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
