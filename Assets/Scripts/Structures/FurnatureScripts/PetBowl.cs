using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetBowl : FurnitureBehaviorScript
{
    public SpriteRenderer r;
    public GameObject water;
    public SpriteRenderer waterR;
    public Sprite[] waterSprites;
    [HideInInspector] public bool containsWater;
    public ParticleSystem splash;

    public AudioClip waterFillSFX;

    public void Awake()
    {
        base.Awake();
        //savedItems.Add(null);
    }

    public void Start()
    {
        base.Start();
        FurnitureStart();
        r.sprite = null;
        LoadVariables();
        StartCoroutine(AnimateWater());
    }

    public override void StructureInteraction()
    {
        bool addedSuccessfully;
        if(!CanBeRemoved())
        {
            addedSuccessfully = PlayerInventoryHolder.Instance.AddToInventory(savedItems[0], 1);
            if (!addedSuccessfully) return;

            r.sprite = null;
            savedItems.Clear();

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
        if (type == ToolType.WateringCan && PlayerInteraction.Instance.waterHeld > 0)
        {
            PlayerInteraction.Instance.waterHeld--;
            WaterChange(true);
            splash.Play();
            AudioPoolManager.Instance.PlayClipAtPosition(waterFillSFX, transform.position);
            success = true;
        }
    }

    public override void ItemInteraction(InventoryItemData item)
    {
        if(containsWater) return;
        if (item && (savedItems.Count == 0 || savedItems[0] == null) && item.animalHungerValue > 0)
        {
            r.sprite = item.icon;
            savedItems.Add(item);
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            PlayerInventoryHolder.Instance.UpdateInventory();
        }
    }

    public void RemoveItem(out InventoryItemData itemRemoved) //For pets taking items
    {
        itemRemoved = savedItems[0];
        r.sprite = null;
        savedItems.Clear();
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

    public bool ContainsEdibleItem(List<InventoryItemData> petDiet)
    {
        if(savedItems.Count == 0 || savedItems[0] == null) return false;

        if(petDiet.Contains(savedItems[0])) return true;

        return false;
    }

    public void WaterChange(bool hasWater)
    {
        if(hasWater == containsWater) return;

        containsWater = hasWater;

        if(containsWater) water.SetActive(true);
        else water.SetActive(false);
    }

    IEnumerator AnimateWater()
    {
        int currentSprite = 0;
        do
        {
            currentSprite++;
            if(currentSprite >= waterSprites.Length) currentSprite = 0;
            yield return new WaitForSeconds(0.15f);
            waterR.sprite = waterSprites[currentSprite];
        }
        while(gameObject.activeSelf);
    }

    public override void HitWithWater()
    {
        if(savedItems.Count == 0) WaterChange(true);
    }

    public override void LoadVariables()
    {
        if(savedItems.Count == 0 || savedItems[0] == null)
        {
            r.sprite = null;
            savedItems.Clear();
            //savedItems.Add(null);
        }
        else r.sprite = savedItems[0].icon;
        /*if(saveInt3 >= 0)
        {
            savedItems[0] = Database.Instance.GetItem(saveInt3);
        }*/
        WaterChange(saveBool1);
    }

    public override void SaveVariables()
    {
        /*if (savedItems.Count > 0 && savedItems[0] != null)
            saveInt3 = savedItems[0].ID;*/

        saveBool1 = containsWater;
    }
}
