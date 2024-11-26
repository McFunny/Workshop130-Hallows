using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterBarrel : StructureBehaviorScript
{
    public InventoryItemData recoveredItem;
    public int waterLevel = 3; //max is 3

    public Transform waterTexture;
    public SpriteRenderer renderer;
    public Sprite[] waterSprites;
    // Start is called before the first frame update
    void Awake()
    {
        base.Awake();
        StartCoroutine("AnimateWater");
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
    }

    public override void ItemInteraction(InventoryItemData item)
    {
        ToolItem waterCan = item as ToolItem;
        //
    }

    public override void StructureInteraction()
    {
        
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(type == ToolType.Shovel)
        {
            StartCoroutine(DugUp());
            success = true;
        }
        if(type == ToolType.WateringCan && PlayerInteraction.Instance.waterHeld < PlayerInteraction.Instance.maxWaterHeld && waterLevel > 0)
        {
            PlayerInteraction.Instance.waterHeld += 5;
            waterLevel--;
            WaterLevelChange();
            success = true;
        }
    }

    IEnumerator DugUp()
    {
        yield return  new WaitForSeconds(1);
        GameObject droppedItem = ItemPoolManager.Instance.GrabItem(recoveredItem);
        droppedItem.transform.position = transform.position;
        Destroy(this.gameObject);
    }

    public void WaterLevelChange()
    {
        switch(waterLevel)
        {
            case 0:
                waterTexture.position = new Vector3(waterTexture.position.x, 0.1f, waterTexture.position.z);
                break;
            case 1:
                waterTexture.position = new Vector3(waterTexture.position.x, 0.4f, waterTexture.position.z);
                break;
            case 2:
                waterTexture.position = new Vector3(waterTexture.position.x, 0.8f, waterTexture.position.z);
                break;
            case 3:
                waterTexture.position = new Vector3(waterTexture.position.x, 1, waterTexture.position.z);
                break;
            default:
                waterLevel = 0;
                waterTexture.position = new Vector3(waterTexture.position.x, 0, waterTexture.position.z);
                break;
        }
    }

    IEnumerator AnimateWater()
    {
        int currentSprite = 0;
        do
        {
            currentSprite++;
            if(currentSprite >= waterSprites.Length) currentSprite = 0;
            yield return new WaitForSeconds(0.15f);
            renderer.sprite = waterSprites[currentSprite];
        }
        while(gameObject.activeSelf);
    }

    public override void HourPassed()
    {
        //simulate rain accumulation
        if(Random.Range(0,10) < 8) return;
        if(waterLevel < 3)
        {
            waterLevel++;
            WaterLevelChange();
        }
    }
}
