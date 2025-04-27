using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowerPotDecor : FurnitureBehaviorScript
{
    public SpriteRenderer r;

    public List<Pottable> potItems;

    public GameObject fogChimeLight;
    public InventoryItemData fogChime;

    [System.Serializable]
    public class Pottable
    {
        public InventoryItemData item;
        public Sprite sprite;
    }

    public void Awake()
    {
        base.Awake();
        fogChimeLight.SetActive(false);
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
        if(!CanBeRemoved()/* || (absentFromGrid && !onTable)*/)
        {
            addedSuccessfully = PlayerInventoryHolder.Instance.AddToInventory(savedItems[0], 1);
            if (!addedSuccessfully) return;

            r.sprite = null;
            savedItems[0] = null;

            PlayerInventoryHolder.Instance.UpdateInventory();
            fogChimeLight.SetActive(false);
            return;
        }
        addedSuccessfully = PlayerInventoryHolder.Instance.AddToInventory(recoveredItem, 1);
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
            StartCoroutine(DugUp());
            success = true;
        }
    }

    public override void ItemInteraction(InventoryItemData item)
    {
        if(item && (savedItems.Count == 0 || savedItems[0] == null))
        {
            InsertItem(item);
        }
    }

    IEnumerator DugUp()
    {
        yield return new WaitForSeconds(1);
        PlayerInventoryHolder.Instance.AddToInventory(recoveredItem, 1);

        Destroy(this.gameObject);
    }

    void InsertItem(InventoryItemData _item)
    {
        for(int i = 0; i < potItems.Count; i++)
        {
            if(potItems[i].item == _item)
            {
                r.sprite = potItems[i].sprite;
                savedItems[0] = _item;
                HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
                PlayerInventoryHolder.Instance.UpdateInventory();

                if(savedItems[0] == fogChime)
                {
                    fogChimeLight.SetActive(true);
                }
                return;
            }
        }
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
            return;
        }

        for(int i = 0; i < potItems.Count; i++)
        {
            if(potItems[i].item == savedItems[0])
            {
                r.sprite = potItems[i].sprite;

                if(savedItems[0] == fogChime)
                {
                    fogChimeLight.SetActive(true);
                }
                return;
            }
        }
    }
}