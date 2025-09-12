using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DryingRack : StructureBehaviorScript
{

    //public InventoryItemData meat, jerky;

   // public InventoryItemData jerkySmall, jerkyLarge;

    //public InventoryItemData meatSmall, meatLarge;

    public Transform itemDropTransform;

    public SpriteRenderer itemSprite;

    public int progress = 0;
    int maxProgress = 6;
    int maxContainedItems = 1;

    bool ignoreNextHour = false;

    void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        base.Start();
        SpriteChange();
    }


    void Update()
    {
        base.Update();
    }

    public override void StructureInteraction()
    {
        if(progress < maxProgress && savedItems.Count > 0) return; //smth is hangin

        if(progress >= maxProgress)
        {
            progress = 0;

            //get item
            GameObject droppedItem = null;

            /*if(savedItems[0] == meat) droppedItem = ItemPoolManager.Instance.GrabItem(jerky);
            if(savedItems[0] == meatSmall) droppedItem = ItemPoolManager.Instance.GrabItem(jerkySmall);
            if(savedItems[0] == meatLarge) droppedItem = ItemPoolManager.Instance.GrabItem(jerkyLarge);*/

            droppedItem = ItemPoolManager.Instance.GrabItem(savedItems[0].FetchConversion(ItemConversionMethod.Drying).newItem);

            if(droppedItem == null)
            {
                Debug.LogError("What did you put inside this drying rack??");
                return;
            }

            droppedItem.transform.position = itemDropTransform.position;

            Rigidbody itemRB = droppedItem.GetComponent<Rigidbody>();
            itemRB = droppedItem.GetComponent<Rigidbody>();
            itemRB.AddForce(Vector3.forward * 20);
            itemRB.AddForce(Vector3.up * 10);
            //audioHandler.PlaySound(audioHandler.activatedSound);

            GameObject poofParticle;
            poofParticle = ParticlePoolManager.Instance.GrabCloudParticle();
            poofParticle.transform.position = itemDropTransform.position;

            savedItems.Clear();

            SpriteChange();

            audioHandler.PlaySound(audioHandler.interactSound);

        }
    }


    public override void ItemInteraction(InventoryItemData item)
    {
        if(item.FetchConversion(ItemConversionMethod.Drying) != null && (savedItems.Count < maxContainedItems/* || !savedItems.Contains(item)*/))
        {
            //
            savedItems.Add(item);
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            PlayerInventoryHolder.Instance.UpdateInventory();

            //audioHandler.PlaySound(audioHandler.activatedSound);

            GameObject poofParticle;
            poofParticle = ParticlePoolManager.Instance.GrabCloudParticle();
            poofParticle.transform.position = itemDropTransform.position;

            SpriteChange();

            ignoreNextHour = true;

            audioHandler.PlaySound(audioHandler.itemInteractSound);

        }
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(type == ToolType.Shovel)
        {
            //StartCoroutine(DugUpForItem());
            success = true;
        }
    }

    public override void HourPassed()
    {
        if(progress < maxProgress && savedItems.Count >= maxContainedItems)
        {
            if(ignoreNextHour)
            {
                ignoreNextHour = false;
                SpriteChange();
                return;
            }
            progress++;
        }
        SpriteChange();
    }

    void SpriteChange()
    {
        if(progress >= maxProgress && savedItems.Count >= maxContainedItems)
        {
            itemSprite.sprite = savedItems[0].FetchConversion(ItemConversionMethod.Drying).newItem.icon;

            /*itemSprite.sprite = jerky.icon;
            if(savedItems[0] == meatSmall) itemSprite.sprite = jerkySmall.icon;
            if(savedItems[0] == meatLarge) itemSprite.sprite = jerkyLarge.icon;*/
        }
        else if(savedItems.Count == 1)
        {
            itemSprite.sprite = savedItems[0].icon;
            /*itemSprite.sprite = meat.icon;
            if(savedItems[0] == meatSmall) itemSprite.sprite = meatSmall.icon;
            if(savedItems[0] == meatLarge) itemSprite.sprite = meatLarge.icon;*/
        }
        else itemSprite.sprite = null;
    }

    void OnDestroy()
    {
        base.OnDestroy();
        if (!gameObject.scene.isLoaded) return; 
        //drop items
        GameObject droppedItem = null;
        foreach(InventoryItemData item in savedItems)
        {
            if(progress == maxProgress)
            {
                droppedItem = ItemPoolManager.Instance.GrabItem(savedItems[0].FetchConversion(ItemConversionMethod.Drying).newItem);
                /*if(item == meat) droppedItem = ItemPoolManager.Instance.GrabItem(jerky);
                if(item == meatSmall) droppedItem = ItemPoolManager.Instance.GrabItem(jerkySmall);
                if(item == meatLarge) droppedItem = ItemPoolManager.Instance.GrabItem(jerkyLarge);*/
            }
            else droppedItem = ItemPoolManager.Instance.GrabItem(item);
            droppedItem.transform.position = itemDropTransform.position;
        }
    }

    public override void LoadVariables()
    {
        progress = saveInt1;
        SpriteChange();
    }

    public override void SaveVariables()
    {
        saveInt1 = progress;
    }

    public override List<StructureUIValueGroup> GetStructureUIValues()
    {
        if(!structureUIVariables.enableUI || structureUIVariables.valueGroups.Count == 0) return null;
        structureUIVariables.valueGroups[0].value = health;

        structureUIVariables.valueGroups[1].value = progress;
        structureUIVariables.valueGroups[1].maxValue = maxProgress;
        return structureUIVariables.valueGroups;
    }
}
