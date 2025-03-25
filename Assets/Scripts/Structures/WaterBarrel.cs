using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WaterBarrel : StructureBehaviorScript
{
    public InventoryItemData recoveredItem;
    public int waterLevel = 0; //max is 15
    int oldLevel;

    public Transform waterTexture;
    public SpriteRenderer renderer;
    public Sprite[] waterSprites;

    public TextMeshProUGUI waterText;
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

        waterText.text = waterLevel + "/" + 15;

        if(oldLevel != waterLevel)
        {
            //print("Old level was " + oldLevel +". New level is " + waterLevel);
            oldLevel = waterLevel;
        }

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
            if(waterLevel < 5)
            {
                PlayerInteraction.Instance.waterHeld += waterLevel;
                waterLevel = 0;
            }
            else
            {
                PlayerInteraction.Instance.waterHeld += 5;
                waterLevel -= 5;
            }
            WaterLevelChange();
            success = true;
        }
    }

    public void ManualFill(out bool success)
    {
        if(PlayerInteraction.Instance.waterHeld > 0 && waterLevel < 15)
        {
            for(int i = 0; i < 5; i++)
            {
                if(PlayerInteraction.Instance.waterHeld > 0 && waterLevel < 15)
                {
                    PlayerInteraction.Instance.waterHeld--;
                    waterLevel++;
                }
            }

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

        if(waterLevel > 10) waterTexture.position = new Vector3(waterTexture.position.x, 1.3f, waterTexture.position.z);
        else if(waterLevel > 5) waterTexture.position = new Vector3(waterTexture.position.x, 0.8f, waterTexture.position.z);
        else if(waterLevel > 0) waterTexture.position = new Vector3(waterTexture.position.x, 0.45f, waterTexture.position.z);
        else waterTexture.position = new Vector3(waterTexture.position.x, 0.2f, waterTexture.position.z);
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
        //
    }

    public override bool IsFlammable()
    {
        if(waterLevel > 0) return false;
        else return true;
    }

    public override void LoadVariables()
    {
        waterLevel = saveInt1;
        WaterLevelChange();
    }

    public override void SaveVariables()
    {
        saveInt1 = waterLevel;
    }
}
