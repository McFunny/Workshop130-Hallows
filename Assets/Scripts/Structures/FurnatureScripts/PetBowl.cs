using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetBowl : FurnitureBehaviorScript
{
    public SpriteRenderer r;

    public void Awake()
    {
        base.Awake();
        savedItems.Add(null);
    }

    public void Start()
    {
        base.Start();
        FurnitureStart();
        r.sprite = null;
        LoadVariables();
    }

    public override void StructureInteraction()
    {
        bool addedSuccessfully;
        if(!CanBeRemoved())
        {
            addedSuccessfully = PlayerInventoryHolder.Instance.AddToInventory(savedItems[0], 1);
            if (!addedSuccessfully) return;

            r.sprite = null;
            savedItems[0] = null;

            PlayerInventoryHolder.Instance.UpdateInventory();
            return;
        }
        addedSuccessfully = PlayerInventoryHolder.Instance.AddToInventory(itemForm, 1);
        if (addedSuccessfully)
        {
            Destroy(this.gameObject);
        }
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(!CanBeRemoved()) return;
        if (type == ToolType.Shovel && PlayerInventoryHolder.Instance.IsInventoryFull() == false)
        {
            success = true;
        }
    }

    public override void ItemInteraction(InventoryItemData item)
    {
        if (item && (savedItems.Count == 0 || savedItems[0] == null) && !item.isKeyItem)
        {
            r.sprite = item.icon;
            savedItems.Add(item);
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            PlayerInventoryHolder.Instance.UpdateInventory();
        }
    }

    public void RemoveItem() //For pets taking items
    {
        r.sprite = null;
            savedItems[0] = null;
    }

    public override void DigAction()
    {
        PlayerInventoryHolder.Instance.AddToInventory(itemForm, 1);

        Destroy(this.gameObject);
    }

    bool CanBeRemoved()
    {
        if(savedItems.Count > 0 && savedItems[0] != null) return false;
        return true;
    }

    public override void LoadVariables()
    {
        if(savedItems.Count == 0 || savedItems[0] == null)
        {
            r.sprite = null;
            savedItems.Clear();
            savedItems.Add(null);
            return;
        }

        if(saveInt3 >= 0) savedItems[0] = Database.Instance.GetItem(saveInt3);
    }

    public override void SaveVariables()
    {
        if (savedItems.Count > 0 && savedItems[0] != null)
            saveInt3 = savedItems[0].ID;
    }
}
