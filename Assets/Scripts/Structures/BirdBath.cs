using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdBath : StructureBehaviorScript
{
    public InventoryItemData recoveredItem;
    public int waterLevel = 3; //max is 3

    public bool inWilderness = false;

    public SpriteRenderer renderer;
    public Sprite[] waterSprites;

    public GameObject crow;
    // Start is called before the first frame update
    void Awake()
    {
        if(!inWilderness) base.Awake();
        StartCoroutine("AnimateWater");
        WaterLevelChange();
    }

    void Start()
    {
        if(!inWilderness) base.Start();
        if(crow)
        {
            if(Random.Range(0,10) > 3) Destroy(crow);
        }
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
    }

    public override void StructureInteraction()
    {
        
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(type == ToolType.Shovel && !inWilderness)
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

    /*public void ManualFill(out bool success)
    {
        if(PlayerInteraction.Instance.waterHeld >= 5 && waterLevel < 3)
        {
            PlayerInteraction.Instance.waterHeld -= 5;
            waterLevel++;
            WaterLevelChange();
            success = true;
        }
        else success = false;
    }*/

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
        if(inWilderness) return;
        if(Random.Range(0,10) < 8) return;
        if(waterLevel < 3)
        {
            waterLevel++;
            WaterLevelChange();
        }
    }

    public override void LoadVariables()
    {
        saveInt1 = waterLevel;
    }

    public override void SaveVariables()
    {
        waterLevel = saveInt1;
    }
}
