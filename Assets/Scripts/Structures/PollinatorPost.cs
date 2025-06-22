using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PollinatorPost : StructureBehaviorScript
{
    public SpriteRenderer r;

    public List<Pottable> potItems;

    public GameObject fogChimeLight, mothLight;
    public InventoryItemData fogChime, nectarItem;

    public int flowerHealth = 0; //How many times until a new flower is needed

    public bool containsMoth, containsNectar;

    public void Awake()
    {
        base.Awake();
        fogChimeLight.SetActive(false);
        mothLight.SetActive(false);
        savedItems.Add(null);
        r.sprite = null;
    }

    public void Start()
    {
        base.Start();
        LoadVariables();
    }

    public override void StructureInteraction()
    {
        bool addedSuccessfully = false;
        if(containsNectar)
        {
            addedSuccessfully = PlayerInventoryHolder.Instance.AddToInventory(nectarItem, 1);
            if (!addedSuccessfully) return;

            flowerHealth--;
            if(flowerHealth == 0)
            {
                r.sprite = null;
                savedItems[0] = null;
            }

            PlayerInventoryHolder.Instance.UpdateInventory();
            fogChimeLight.SetActive(false);
            return;
        }
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(type == ToolType.Shovel && PlayerInventoryHolder.Instance.IsInventoryFull() == false)
        {
            //StartCoroutine(DugUp());
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

    void InsertItem(InventoryItemData _item)
    {
        for(int i = 0; i < potItems.Count; i++)
        {
            if(potItems[i].item == _item)
            {
                if(savedItems.Count > 0) savedItems[0] = _item;
                else savedItems.Add(_item);
                HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
                PlayerInventoryHolder.Instance.UpdateInventory();

                UpdateModel();
                flowerHealth = potItems[i].intValue;
                return;
            }
        }
    }

    void UpdateModel()
    {
        if(containsMoth) mothLight.SetActive(true);
        else mothLight.SetActive(false);

        if(savedItems.Count == 0 || savedItems[0] == null)
        {
            r.sprite = null;
            savedItems.Clear();
            savedItems.Add(null);
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

        if(savedItems[0] == fogChime) fogChimeLight.SetActive(true);
        else fogChimeLight.SetActive(false);
    }

    public override void SaveVariables()
    {
        saveInt1 = flowerHealth;
    }

    public override void LoadVariables()
    {
        UpdateModel();

        flowerHealth = saveInt1;
    }
}
