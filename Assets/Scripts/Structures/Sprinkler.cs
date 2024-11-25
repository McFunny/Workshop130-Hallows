using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sprinkler : StructureBehaviorScript
{
    public InventoryItemData recoveredItem;
    public int waterLevel = 0; //max is 24
    public GameObject water;
    // Start is called before the first frame update
    void Awake()
    {
        base.Awake();
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
        if(water.activeSelf && waterLevel == 0)
        {
            water.SetActive(false);
        }

        if(!water.activeSelf && waterLevel != 0)
        {
            water.SetActive(true);
        }

    }

    public override void HourPassed()
    {
        if(waterLevel > 0)
        {
            waterLevel--;
            StartCoroutine(WaterTiles());
        }
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(type == ToolType.Shovel)
        {
            StartCoroutine(DugUp());
            success = true;
        }
        if(type == ToolType.WateringCan && PlayerInteraction.Instance.waterHeld > 0 && waterLevel < 24)
        {
            PlayerInteraction.Instance.waterHeld--;
            waterLevel = 24;
            StartCoroutine(WaterTiles());
            success = true;
        }
    }

    public override void TimeLapse(int hours)
    {
        for(int i = 0; i < hours; i++)
        {
            HourPassed();
        }
    }

    IEnumerator WaterTiles()
    {
        yield return new WaitForSeconds(1);
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 3f);
        foreach(Collider collider in hitColliders)
        {
            if(collider.gameObject.GetComponentInParent<FarmLand>())
            {
                FarmLand tile = collider.gameObject.GetComponentInParent<FarmLand>();
                tile.WaterCrops();
            }
        }
    }

    IEnumerator DugUp()
    {
        yield return  new WaitForSeconds(1);
        GameObject droppedItem = ItemPoolManager.Instance.GrabItem(recoveredItem);
        droppedItem.transform.position = transform.position;
        Destroy(this.gameObject);
    }

    void OnDestroy()
    {
        base.OnDestroy();
    }
}
