using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WaterBarrel : StructureBehaviorScript, IWaterHolder
{
    [HideInInspector] public Transform ObjectTransform => transform; // For the Interface

    public int waterLevel = 0; //max is maxWaterLevel
    int maxWaterLevel = 10;
    int oldLevel;

    public Transform waterTexture;
    public SpriteRenderer renderer;
    public Sprite[] waterSprites;

    public TextMeshProUGUI waterText;

    public ParticleSystem splash;

    bool showSplash = false;
    bool waterCooldown = false;

    public GameObject waterExplosionPrefab;
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

        //waterText.text = waterLevel + "/" + maxWaterLevel;

        if(oldLevel != waterLevel)
        {
            //print("Old level was " + oldLevel +". New level is " + waterLevel);
            oldLevel = waterLevel;
        }

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
                    PlayerInteraction.Instance.WaterChange(1);
                    //PlayerInteraction.Instance.waterHeld++;
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
                    PlayerInteraction.Instance.WaterChange(-1);
                    //PlayerInteraction.Instance.waterHeld--;
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
        base.HitWithWater();
        if(waterLevel < maxWaterLevel && !waterCooldown && !IsFrozen()) 
        {
            waterLevel++;
            WaterLevelChange();
            //StartCoroutine(WaterCooldown()); //Keep disabled if the watergun costs 1 per multi shot
        }
    }

    IEnumerator WaterCooldown()
    {
        waterCooldown = true;
        yield return new WaitForSeconds(.9f);
        waterCooldown = false;
    }

    public void WaterLevelChange()
    {
        if(waterLevel > 0) waterTexture.gameObject.SetActive(true);
        else waterTexture.gameObject.SetActive(false);

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

    public bool CanBeWatered()
    {
        if(waterLevel < maxWaterLevel && !IsFrozen()) return true;
        else return false;
    }

    public void GivenWater()
    {
        HitWithWater();
    }

    public void EmptyWater()
    {
        waterLevel = 0;
        showSplash = false;
        WaterLevelChange();
    }

    public void OnDestroy()
    {
        base.OnDestroy();
        if(!gameObject.scene.isLoaded) return;
        if(waterLevel >= 3) 
        {
            Instantiate(waterExplosionPrefab, particleCenter.position, Quaternion.identity);
            Collider[] hitStructures = Physics.OverlapSphere(transform.position, 3f, 1 << 6);
            foreach(Collider collider in hitStructures)
            {
                StructureBehaviorScript structure = collider.gameObject.GetComponentInParent<StructureBehaviorScript>();
                if(structure)
                {
                    structure.HitWithWater();
                }
            }

            Collider[] hitEnemies = Physics.OverlapSphere(transform.position, 4.5f, 1 << 9);
            foreach(Collider collider in hitEnemies)
            {
                var creature = collider.GetComponentInParent<CreatureBehaviorScript>();
                if (creature != null)
                {
                    creature.HitWithWater();
                }
            }
        }
    }

    public override List<StructureUIValueGroup> GetStructureUIValues()
    {
        if(!structureUIVariables.enableUI || structureUIVariables.valueGroups.Count == 0) return null;
        structureUIVariables.valueGroups[0].value = health;
        structureUIVariables.valueGroups[0].maxValue = maxHealth;

        structureUIVariables.valueGroups[1].value = waterLevel;
        structureUIVariables.valueGroups[1].maxValue = maxWaterLevel;
        return structureUIVariables.valueGroups;
    }
}
