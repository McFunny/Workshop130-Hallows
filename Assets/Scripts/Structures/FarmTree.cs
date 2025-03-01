using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmTree : StructureBehaviorScript
{
    public InventoryItemData treePapers;

    public bool taggedForCutting = false;

    public GameObject papers;

    public Transform itemDrop;
    void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        base.Start();
        if(taggedForCutting) papers.SetActive(true);
    }

    public override void StructureInteraction()
    {
        if(taggedForCutting)
        {
            taggedForCutting = false;
            papers.SetActive(false);

            GameObject droppedItem = ItemPoolManager.Instance.GrabItem(treePapers);
            droppedItem.transform.position = itemDrop.position;
        }
    }

    public override void ItemInteraction(InventoryItemData item)
    {
        if(item == treePapers && !taggedForCutting)
        {
            taggedForCutting = true;
            papers.SetActive(true);
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            PlayerInventoryHolder.Instance.UpdateInventory();
        }
    }

    public override void HourPassed()
    {
        if(taggedForCutting && TimeManager.Instance.currentHour == 8)
        {
            Destroy(this.gameObject);
        }
    }
}
