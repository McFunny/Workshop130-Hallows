using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeedExtractor : StructureBehaviorScript
{
    public Transform itemDropTransform, itemInsertPos;

    public InventoryItemData dullSeedItem;

    public List<InventoryItemData> reducedSeedCrops;

    public int progress = 0;
    int maxProgress = 5;

    bool ignoreNextHour = false;

    public ParticleSystem fumes;

    void Start()
    {
        base.Start();
        if(StructureManager.Instance.ValidateGridType(transform.position, GridType.Farm) == false) maxProgress *= 3; //Takes longer when not on the farm
    }

    public override void StructureInteraction()
    {

        if(progress < maxProgress || savedItems.Count == 0) return;

        InventoryItemData itemToSpawn = savedItems[0].FetchConversion(ItemConversionMethod.SeedExtract).newItem;

        int r = Random.Range(3, 7);
        if(reducedSeedCrops.Contains(savedItems[0])) r = Random.Range(1, 5);
        if(MainMenuScript.currentFileMode == FileMode.Cozy) r += Random.Range(1, 3);


        int dullSeeds = Random.Range(-2, 4);
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

        }
        savedItems.Clear();
        audioHandler.PlaySound(audioHandler.activatedSound);

        fumes.Stop();

        progress = 0;

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

            progress = 0;
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
            if(reducedSeedCrops.Contains(savedItems[0])) progress += 2;

            if(progress >= maxProgress)
            {
                progress = maxProgress;
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
