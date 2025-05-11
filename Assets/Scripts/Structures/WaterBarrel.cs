using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WaterBarrel : StructureBehaviorScript
{
    public int waterLevel = 0; //max is maxWaterLevel
    int maxWaterLevel = 10;
    int oldLevel;

    public Transform waterTexture;
    public SpriteRenderer renderer;
    public Sprite[] waterSprites;

    public TextMeshProUGUI waterText;

    public ParticleSystem splash;

    bool showSplash = false;
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

        waterText.text = waterLevel + "/" + maxWaterLevel;

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
            //StartCoroutine(DugUpForItem());
            success = true;
        }
        if((type == ToolType.WateringCan || type == ToolType.WaterGun) && PlayerInteraction.Instance.waterHeld < PlayerInteraction.Instance.maxWaterHeld && waterLevel > 0)
        {
            /*if(waterLevel < 5)
            {
                PlayerInteraction.Instance.waterHeld += waterLevel;
                waterLevel = 0;
            }
            else
            {
                PlayerInteraction.Instance.waterHeld += 5;
                waterLevel -= 5;
            }*/
            for(int i = 0; i < PlayerInteraction.Instance.maxWaterHeld; i++)
            {
                if(PlayerInteraction.Instance.waterHeld < PlayerInteraction.Instance.maxWaterHeld && waterLevel > 0)
                {
                    PlayerInteraction.Instance.waterHeld++;
                    waterLevel--;
                }
            }

            WaterLevelChange();
            success = true;
        }
    }

    public void ManualFill(out bool success)
    {
        if(PlayerInteraction.Instance.waterHeld > 0 && waterLevel < maxWaterLevel)
        {
            for(int i = 0; i < PlayerInteraction.Instance.maxWaterHeld; i++)
            {
                if(PlayerInteraction.Instance.waterHeld > 0 && waterLevel < maxWaterLevel)
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

    public override void HitWithWater()
    {
        if(waterLevel < maxWaterLevel) 
        {
            waterLevel++;
            WaterLevelChange();
        }
    }

    public void WaterLevelChange()
    {
        if(waterLevel > 0) renderer.enabled = true;
        else renderer.enabled = false;

        if(waterLevel >= 8) waterTexture.position = new Vector3(waterTexture.position.x, 1.6f, waterTexture.position.z);
        else if(waterLevel >= 5) waterTexture.position = new Vector3(waterTexture.position.x, 1f, waterTexture.position.z);
        else if(waterLevel > 0) waterTexture.position = new Vector3(waterTexture.position.x, 0.5f, waterTexture.position.z);
        else waterTexture.position = new Vector3(waterTexture.position.x, 0.2f, waterTexture.position.z);

        if(showSplash)
        {
            splash.Play();
            audioHandler.PlaySound(audioHandler.interactSound);
        }
        else showSplash = true;
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
