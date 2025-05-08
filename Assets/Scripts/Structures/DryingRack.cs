using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DryingRack : StructureBehaviorScript
{

    public InventoryItemData meat, jerky;

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
            GameObject droppedItem;
            float r = Random.Range(0,10);
            droppedItem = ItemPoolManager.Instance.GrabItem(jerky);
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

        }
    }


    public override void ItemInteraction(InventoryItemData item)
    {
        if(item == meat && (savedItems.Count < maxContainedItems || !savedItems.Contains(meat)))
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

        }
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(type == ToolType.Shovel)
        {
            StartCoroutine(DugUpForItem());
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
            itemSprite.sprite = jerky.icon;
        }
        else if(savedItems.Count == 1) itemSprite.sprite = meat.icon;
        else itemSprite.sprite = null;
    }

    void OnDestroy()
    {
        base.OnDestroy();
        if (!gameObject.scene.isLoaded) return; 
        //drop items
        GameObject droppedItem;
        foreach(InventoryItemData item in savedItems)
        {
            if(progress == maxProgress) droppedItem = ItemPoolManager.Instance.GrabItem(jerky);
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
}
