using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeedExtractor : StructureBehaviorScript
{
    public Transform itemDropTransform, itemInsertPos;

    public InventoryItemData dullSeedItem;

    public int progress = 0;
    int maxProgress = 2;

    bool ignoreNextHour = false;

    public ParticleSystem fumes;

    void Start()
    {
        base.Start();
        if(StructureManager.Instance.ValidateGridType(transform.position, GridType.Farm) == false) maxProgress *= 3; //Takes longer when not on the farm
    }

    public override void StructureInteraction()
    {

        if(progress >= maxProgress || savedItems[0] == null) return;

        InventoryItemData itemToSpawn = savedItems[0].FetchConversion(ItemConversionMethod.SeedExtract).newItem;

        int r = Random.Range(3, 8);
        int dullSeeds = Random.Range(-3, 2);
        for(int i = 0; i < r; i++)
        {
            GameObject droppedItem;
            if(dullSeeds > 0)
            {
                droppedItem = ItemPoolManager.Instance.GrabItem(dullSeedItem);
                dullSeeds--;
            }
            else droppedItem = ItemPoolManager.Instance.GrabItem(itemToSpawn);
            droppedItem.transform.position = itemDropTransform.position;

            Rigidbody itemRB = droppedItem.GetComponent<Rigidbody>();
            itemRB = droppedItem.GetComponent<Rigidbody>();
            itemRB.AddForce(Vector3.forward * 20);
            itemRB.AddForce(Vector3.up * 10);

            ParticlePoolManager.Instance.GrabCloudParticle().transform.position = itemDropTransform.position;

            savedItems.Clear();
        }
        audioHandler.PlaySound(audioHandler.activatedSound);

        fumes.Stop();

    }

    public override void ItemInteraction(InventoryItemData item)
    {
        ItemConversion ic = item.FetchConversion(ItemConversionMethod.SeedExtract);
        if(ic != null && savedItems.Count == 0)
        {

            savedItems.Add(item);
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(ic.itemsNeeded);
            PlayerInventoryHolder.Instance.UpdateInventory();

            audioHandler.PlaySound(audioHandler.itemInteractSound);

            ParticlePoolManager.Instance.GrabCloudParticle().transform.position = itemInsertPos.position;
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
        if(progress < maxProgress && savedItems.Count > 0)
        {
            if(ignoreNextHour)
            {
                ignoreNextHour = false;
                return;
            }
            progress++;

            if(progress >= maxProgress)
            {
                progress = 0;
                fumes.Stop();
            }
        }
        else 
        {
            fumes.Stop();
        }
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

        if(progress < maxProgress && savedItems.Count > 0) fumes.Play();
    }

    public override void SaveVariables()
    {
        saveInt1 = progress;
    }

    public override List<StructureUIValueGroup> GetStructureUIValues()
    {
        if(!structureUIVariables.enableUI || structureUIVariables.valueGroups.Count == 0) return null;
        structureUIVariables.valueGroups[0].value = health;
        structureUIVariables.valueGroups[0].maxValue = maxHealth;

        structureUIVariables.valueGroups[1].value = progress;
        structureUIVariables.valueGroups[1].maxValue = maxProgress;

        return structureUIVariables.valueGroups;
    }
}
