using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PollinatorPost : StructureBehaviorScript
{
    public SpriteRenderer r;

    public List<Pottable> potItems;

    public GameObject fogChimeLight, mothLight, nectarObject;
    public InventoryItemData fogChime, nectarItem;

    public int flowerHealth = 0; //How many times until a new flower is needed

    public bool containsMoth, containsNectar;

    public MeshRenderer renderer;
    Material postMat;

    public CreatureObject mothData; //Spawn this if inside and destroyed or daylight

    public void Awake()
    {
        base.Awake();
        fogChimeLight.SetActive(false);
        mothLight.SetActive(false);
        savedItems.Add(null);
        r.sprite = null;
        postMat = renderer.materials[0];
    }

    public void Start()
    {
        base.Start();
        LoadVariables();
    }

    public override void HourPassed()
    {
        if(TimeManager.Instance.isDay && containsMoth)
        {
            containsMoth = false;
            containsNectar = true;
            UpdateModel();
            Instantiate(mothData.objectPrefab, transform.position, Quaternion.identity);
        }
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

    public void InsertMoth()
    {
        containsMoth = true;
        UpdateModel();
    }

    void UpdateModel()
    {
        if(containsMoth)
        {
            //postMat.emission = true;
            postMat.EnableKeyword("_EMISSION");
            mothLight.SetActive(true);
        }
        else 
        {
            postMat.DisableKeyword("_EMISSION");
            //postMat.emission = false;
            mothLight.SetActive(false);
        }

        if(containsNectar)
        {
            nectarObject.SetActive(true);
        }
        else
        {
            nectarObject.SetActive(false);
        }

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

    void OnDestroy()
    {
        base.OnDestroy();
        if (!gameObject.scene.isLoaded) return; 

        if(containsMoth)
        {
            Instantiate(mothData.objectPrefab, transform.position, Quaternion.identity);
        }
    }

    public override void SaveVariables()
    {
        saveInt1 = flowerHealth;
        saveBool1 = containsNectar;
    }

    public override void LoadVariables()
    {
        containsNectar = saveBool1;

        UpdateModel();

        flowerHealth = saveInt1;
    }
}
