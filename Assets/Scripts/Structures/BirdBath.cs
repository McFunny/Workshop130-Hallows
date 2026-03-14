using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdBath : StructureBehaviorScript
{
    public int waterLevel = 1; //max is 1

    public bool inWilderness = false;

    public SpriteRenderer renderer;
    public Sprite[] waterSprites;

    public GameObject crow;

    public ParticleSystem splash;
    bool showSplash = false;
    // Start is called before the first frame update
    void Awake()
    {
        if(!inWilderness) base.Awake();
        else audioHandler = GetComponent<StructureAudioHandler>();
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

    public override void ItemInteraction(InventoryItemData item)
    {
        if(item.ID == 322 && waterLevel > 0 && Freezable())
        {
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            PlayerInventoryHolder.Instance.UpdateInventory();
            Freeze();
        }
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(type == ToolType.Shovel && !inWilderness)
        {
            //StartCoroutine(DugUp());
            success = true;
        }
        if((type == ToolType.WateringCan || type == ToolType.WaterGun) && PlayerInteraction.Instance.waterHeld < PlayerInteraction.Instance.maxWaterHeld && waterLevel > 0)
        {
            PlayerInteraction.Instance.WaterChange(5);
            //PlayerInteraction.Instance.waterHeld += 5;
            waterLevel--;
            WaterLevelChange();
            success = true;
        }
    }

    public void ManualFill(out bool success)
    {
        if(PlayerInteraction.Instance.waterHeld >= 5 && waterLevel < 1)
        {
            PlayerInteraction.Instance.WaterChange(-5);
            //PlayerInteraction.Instance.waterHeld -= 5;
            waterLevel++;
            WaterLevelChange();
            success = true;
        }
        else success = false;
    }

    public void WaterLevelChange()
    {
        if(waterLevel > 0) renderer.enabled = true;
        else renderer.enabled = false;

        if(showSplash) //Make changes to the wilderness one too
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
        //simulate rain accumulation
        if(inWilderness) return;
        int refillChance = 18;
        if(MainMenuScript.currentFileMode == FileMode.Cozy) refillChance -= 4;
        if(TimeManager.Instance.isDay) refillChance -= 4;
        if(Random.Range(0,20) < refillChance || IsFrozen()) return;
        if(waterLevel < 1)
        {
            waterLevel++;
            WaterLevelChange();
        }
    }

    public override void LoadVariables()
    {
        waterLevel = saveInt1;
        if(waterLevel > 0) renderer.enabled = true;
        else renderer.enabled = false;
    }

    public override void SaveVariables()
    {
        saveInt1 = waterLevel;
    }
}
