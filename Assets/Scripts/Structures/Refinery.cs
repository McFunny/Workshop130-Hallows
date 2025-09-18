using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Refinery : StructureBehaviorScript
{
    //public InventoryItemData timberEar, gloomStalk;
    //public InventoryItemData wood, gloomBundles; //Cost to refine is lets just say 5 units of each

    public PopupScript itemWarning; //Warning that the player does not have enough items
    
    public Transform itemDropTransform, itemInsertPos;

    public Animator anim;

    public int progress = 0;
    int maxProgress = 2;
    int maxContainedItems = 25;
    int itemsFinished = 0;

    bool ignoreNextHour = false;

    public bool isFunctioning = false; //cannot interact with it until its been on the farm at night
    public PopupScript chargingPopup;

    public TextMeshProUGUI storedText, finishedText;

    public ParticleSystem fumes;

    void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        base.Start();
    }


    void Update()
    {
        base.Update();

        //UpdateText();
    }

    public override void StructureInteraction()
    {
        if(!isFunctioning)
        {
            PopupHandler.Instance.AddToQueue(chargingPopup);
            return;
        }

        if(itemsFinished < 1 || savedItems.Count == 0 || savedItems[0] == null) return;

        InventoryItemData itemToSpawn = null;
        anim.SetTrigger("TakeItem");

        for(int i = 0; i < itemsFinished; i++)
        {
            /*if(savedItems[0] == timberEar) itemToSpawn = wood;
            if(savedItems[0] == gloomStalk) itemToSpawn = gloomBundles;*/

            itemToSpawn = savedItems[0].FetchConversion(ItemConversionMethod.Refining).newItem;

            GameObject droppedItem = ItemPoolManager.Instance.GrabItem(itemToSpawn);
            droppedItem.transform.position = itemDropTransform.position;

            Rigidbody itemRB = droppedItem.GetComponent<Rigidbody>();
            itemRB = droppedItem.GetComponent<Rigidbody>();
            itemRB.AddForce(Vector3.forward * 20);
            itemRB.AddForce(Vector3.up * 10);

            ParticlePoolManager.Instance.GrabCloudParticle().transform.position = itemDropTransform.position;

            int itemsToRemove = savedItems[0].FetchConversion(ItemConversionMethod.Refining).itemsNeeded;
            for(int x = 0; x < itemsToRemove; x++) savedItems.RemoveAt(0);
        }

        itemsFinished = 0;
        audioHandler.PlaySound(audioHandler.activatedSound);

    }

    public override void ItemInteraction(InventoryItemData item)
    {
        if(!isFunctioning)
        {
            PopupHandler.Instance.AddToQueue(chargingPopup);
            return;
        }
        ItemConversion ic = item.FetchConversion(ItemConversionMethod.Refining);
        if(ic != null && (ic.itemsNeeded + savedItems.Count) <= maxContainedItems /*&& savedItems.Count < maxContainedItems*/)
        {

            if(HotbarDisplay.currentSlot.AssignedInventorySlot.StackSize < ic.itemsNeeded) //Not enough items
            {
                PopupHandler.Instance.AddToQueue(chargingPopup);
                return;
            }
            for(int i = 0; i < ic.itemsNeeded; i++)
            {
                savedItems.Add(item);
            }
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(ic.itemsNeeded);
            PlayerInventoryHolder.Instance.UpdateInventory();

            audioHandler.PlaySound(audioHandler.activatedSound);

            ParticlePoolManager.Instance.GrabCloudParticle().transform.position = itemDropTransform.position;

            anim.SetTrigger("InsertItem");
            anim.SetBool("IsRunning", true);
            fumes.Play();
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
        if(!TimeManager.Instance.isDay && !isFunctioning) isFunctioning = true;
        if(progress < maxProgress && savedItems.Count > itemsFinished * 5)
        {
            anim.SetBool("IsRunning", true);
            if(ignoreNextHour)
            {
                ignoreNextHour = false;
                return;
            }
            progress++;

            if(progress >= maxProgress)
            {
                progress = 0;
                itemsFinished++;
            }
        }
        else 
        {
            anim.SetBool("IsRunning", false);
            fumes.Stop();
        }
    }

    void UpdateText()
    {
        int storedStacks = 0;
        if(savedItems.Count > 0) storedStacks = savedItems.Count / 5;
        storedText.text = "Stacks Stored:   " + storedStacks + "/" + maxContainedItems/5;

        finishedText.text = "Items Finished: " + itemsFinished + "/" + maxContainedItems/5;
    }

    void OnDestroy()
    {
        base.OnDestroy();
        if (!gameObject.scene.isLoaded) return; 
        //drop items
        GameObject droppedItem;
        foreach(InventoryItemData item in savedItems)
        {
            droppedItem = ItemPoolManager.Instance.GrabItem(item);
            droppedItem.transform.position = itemDropTransform.position;
        }
    }

    public override void LoadVariables()
    {
        progress = saveInt1;
        itemsFinished = saveInt2;

        isFunctioning = true;

        if(progress < maxProgress && savedItems.Count > itemsFinished * 5) fumes.Play();
    }

    public override void SaveVariables()
    {
        saveInt1 = progress;
        saveInt2 = itemsFinished;
    }

    public override List<StructureUIValueGroup> GetStructureUIValues()
    {
        if(!structureUIVariables.enableUI || structureUIVariables.valueGroups.Count == 0) return null;
        structureUIVariables.valueGroups[0].value = health;
        structureUIVariables.valueGroups[0].maxValue = maxHealth;

        structureUIVariables.valueGroups[1].value = progress;
        structureUIVariables.valueGroups[1].maxValue = maxProgress;

        structureUIVariables.valueGroups[2].value = savedItems.Count/5;
        structureUIVariables.valueGroups[2].maxValue = maxContainedItems/5;
        return structureUIVariables.valueGroups;
    }
}
