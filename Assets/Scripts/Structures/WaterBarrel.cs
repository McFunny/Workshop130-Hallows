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
        WaterLevelChange();
    }

    void Start()
    {
        base.Start();
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
        if((type == ToolType.WateringCan || type == ToolType.WaterGun) && PlayerInteraction.Instance.waterHeld < PlayerInteraction.Instance.maxWaterHeld && waterLevel > 0)
        {
            PlayerInteraction.Instance.waterHeld += 5;
            waterLevel--;
            WaterLevelChange();
            success = true;
        }
    }

    public void ManualFill(out bool success)
    {
        if(PlayerInteraction.Instance.waterHeld >= 5 && waterLevel < 3)
        {
            PlayerInteraction.Instance.waterHeld -= 5;
            waterLevel++;
            WaterLevelChange();
            success = true;
        }
        else success = false;
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
        if(waterLevel > 0) renderer.enabled = true;
        else renderer.enabled = false;
        switch(waterLevel)
        {
            case 0:
                waterTexture.position = new Vector3(waterTexture.position.x, 0.2f, waterTexture.position.z);
                break;
            case 1:
                waterTexture.position = new Vector3(waterTexture.position.x, 0.45f, waterTexture.position.z);
                break;
            case 2:
                waterTexture.position = new Vector3(waterTexture.position.x, 0.8f, waterTexture.position.z);
                break;
            case 3:
                waterTexture.position = new Vector3(waterTexture.position.x, 1.1f, waterTexture.position.z);
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
        return; //You can fill it manually so this is obsolete
        if(Random.Range(0,10) < 8) return;
        if(waterLevel < 3)
        {
            waterLevel++;
            WaterLevelChange();
        }
    }

    public override bool IsFlammable()
    {
        if(waterLevel > 0) return false;
        else return true;
    }

    public override void LoadVariables()
    {
        saveInt1 = waterLevel;
        //WaterLevelChange();
    }

    public override void SaveVariables()
    {
        waterLevel = saveInt1;
    }
}
