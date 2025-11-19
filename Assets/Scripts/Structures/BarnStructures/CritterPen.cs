using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CritterPen : StructureBehaviorScript
{
    //Should probably have text to show which creatures reside here

    public PenType type;
    //public int currentOccupents = 0;
    public List<CritterBehaviorScript> housedCritters;
    public int maxOccupency = 2;

    public int durability = 100; //Max is 100; Drains by 13 per critter

    public InventoryItemData combItem;

    bool dropItems;

    public List<ItemWithPrefab> critterItemPairs;

    void Start()
    {
        base.Start();
        StartCoroutine(DelayedStart());

        if(TutorialMiller.Instance) TutorialMiller.Instance.SpawnHog();
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(type == ToolType.Shovel)
        {
            success = true;
        }
    }

    public override void ItemInteraction(InventoryItemData item)
    {
        //if comb, then repair durability
        if(type == PenType.Hive && item == combItem)
        {
            durability += 30;
            if(durability > 100) durability = 100;
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            PlayerInventoryHolder.Instance.UpdateInventory();
        }

        foreach (ItemWithPrefab pair in critterItemPairs)
        {
            if(item == pair.item && housedCritters.Count < maxOccupency)
            {
                HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
                PlayerInventoryHolder.Instance.UpdateInventory();
                CritterBehaviorScript newCritter = Instantiate(pair.prefab, transform.position, Quaternion.identity).GetComponent<CritterBehaviorScript>();
                newCritter.homePen = this;
                housedCritters.Add(newCritter);
                return;
            }
        }
    }

    public override void HourPassed()
    {
        if(type == PenType.Hive && TimeManager.Instance.currentHour == 8)
        {
            StartCoroutine(ReduceDurability());
        }
        //If hive, lower durability. If durabiliy is 0, then Remove this home from the critters. Do not allow this to be found by critters looking for a home. Remove this home from the critters on a delay
    }

    void OnDestroy()
    {
        base.OnDestroy();
        if (!gameObject.scene.isLoaded || !dropItems) return; 
        //drop items
        return; // Removing the process of needing to repair the item
        GameObject droppedItem;
        foreach(ItemWithAmount repairItem in structData.repairItems)
        {
            for(int i = 0; i < repairItem.amount; i++)
            {
                droppedItem = ItemPoolManager.Instance.GrabItem(repairItem.item);
                droppedItem.transform.position = transform.position;
            }
        }
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(0.5f);
        dropItems = true;
    }

    IEnumerator ReduceDurability()
    {
        yield return new WaitForSeconds(6);
        durability -= 13 * housedCritters.Count;
        if(durability <= 0)
        {
            durability = 0;
            foreach(CritterBehaviorScript critter in housedCritters)
            {
                critter.homePen = null;
            }
            housedCritters.Clear();
        }
    }

    public override void SaveVariables()
    {
        saveInt1 = durability;
    }

    public override void LoadVariables()
    {
        durability = saveInt1;
    }

    public override List<StructureUIValueGroup> GetStructureUIValues()
    {
        if(!structureUIVariables.enableUI || structureUIVariables.valueGroups.Count == 0) return null;

        structureUIVariables.valueGroups[0].value = housedCritters.Count;
        structureUIVariables.valueGroups[0].maxValue = maxOccupency;

        if(type == PenType.Hive)
        {
            structureUIVariables.valueGroups[1].value = durability;
            structureUIVariables.valueGroups[1].maxValue = 100;
        }
        return structureUIVariables.valueGroups;
    }
}

public enum PenType
{
    Pen,
    Coop,
    Hive
}

[System.Serializable]
public class ItemWithPrefab
{
    public InventoryItemData item;
    public GameObject prefab;
}
