using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BucketStructure : StructureBehaviorScript, IWaterHolder
{
    [HideInInspector] public Transform ObjectTransform => transform; // For the Interface

    public int waterLevel = 0; //max is maxWaterLevel
    int maxWaterLevel = 3;
    int oldLevel;

    public Transform waterTexture, splashPosition, splashPositionL, splashPositionR;
    public SpriteRenderer renderer;
    public Sprite[] waterSprites;

    public ParticleSystem splash, spillSplash, largeSpillSplash;

    bool showSplash = false;
    bool waterCooldown = false;
    bool spilled = false;

    public Animator anim;
    // Start is called before the first frame update
    void Awake()
    {
        base.Awake();
        StartCoroutine("AnimateWater");
        WaterLevelChange();

        //reset rotation
    }

    void Start()
    {
        transform.rotation = Quaternion.Euler(new Vector3(0,0,0));
        base.Start();
        OnDamageWithValue += Damaged;
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();

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
        if(waterLevel > 0) return;
        bool addedSuccessfully = PlayerInventoryHolder.Instance.AddToInventory(itemForm, 1);
        if (addedSuccessfully)
        {
            AudioPoolManager.Instance.PlayClipAtPosition(audioHandler.itemInteractSound, transform.position, 0.4f, 10);
            Destroy(this.gameObject);
        }
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(type == ToolType.Shovel)
        {
            //StartCoroutine(DugUpForItem());
            success = true;
        }
        if((type == ToolType.WateringCan || type == ToolType.WaterGun) && PlayerInteraction.Instance.waterHeld < PlayerInteraction.Instance.maxWaterHeld && waterLevel > 0 && !spilled)
        {
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
        if(PlayerInteraction.Instance.waterHeld > 0 && waterLevel < maxWaterLevel && !spilled)
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
        if(waterLevel < maxWaterLevel && !waterCooldown && !spilled && !IsFrozen()) 
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

        if(showSplash)
        {
            splash.Play();
            audioHandler.PlaySound(audioHandler.miscSounds1[0]);
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

    public void ForceSpillBucket()
    {
        if(!spilled) SpillBucket(Direction.South);
    }

    void SpillBucket(Direction dir)
    {
        spilled = true;

        switch(dir)
        {
            case Direction.North:
            transform.Rotate(0, 180, 0);
            break;
            case Direction.East:
            transform.Rotate(0, 270, 0);
            break;
            case Direction.West:
            transform.Rotate(0, 90, 0);
            break;
            default:
            break;
        }
        anim.SetTrigger("Spill");
        audioHandler.PlaySound(audioHandler.activatedSound);

        if(waterLevel == 0) return;
        int waterSpilled = waterLevel;
        waterLevel = 0;
        WaterLevelChange();

        splash.Play();
        spillSplash.Play();

        if(waterSpilled == 3) largeSpillSplash.Play();
        audioHandler.PlaySound(audioHandler.interactSound);

        Vector3 splashPos;
        float range = 1;
        for(int i = 0; i < 2; ++i)
        {
            if(i == 0) splashPos = transform.position;
            else 
            {
                splashPos = splashPosition.position;
                range = 1f;
            }

            if(Vector3.Distance(splashPos, PlayerInteraction.Instance.transform.position) < 3f)
            {
                StatusEffectManager.Instance.RemoveStatusOnPlayer(StatusEffectName.Fire);
            }
            Collider[] hitStructures = Physics.OverlapSphere(splashPos, range, 1 << 6);
            foreach(Collider collider in hitStructures)
            {
                if(i == 0) break;
                StructureBehaviorScript structure = collider.gameObject.GetComponentInParent<StructureBehaviorScript>();
                if(structure && structure != this)
                {
                    IWaterHolder wHolder = structure as IWaterHolder;
                    if(wHolder != null && waterSpilled < 3) for(int x = 0; x < waterSpilled; ++x) wHolder.GivenWater();
                    else structure.HitWithWater();
                    break;
                }
            }

            Collider[] hitEnemies = Physics.OverlapSphere(splashPos, range * 2.5f, 1 << 9);
            foreach(Collider collider in hitEnemies)
            {
                var creature = collider.GetComponentInParent<CreatureBehaviorScript>();
                if (creature != null)
                {
                    creature.HitWithWater();
                    if(creature.isDead || creature.health <= 0)
                    {
                        AchievementManager.Instance.NotifyKickedBucket();
                    }
                }
            }
        }

        if(waterSpilled == 3) SpillExtraWater();
    }


    void SpillExtraWater()
    {
        Vector3 splashPos;
        float range = 1;
        for(int i = 0; i < 2; ++i)
        {
            if(i == 0) splashPos = splashPositionL.position;
            else splashPos = splashPositionR.position;

            if(Vector3.Distance(splashPos, PlayerInteraction.Instance.transform.position) < 3f)
            {
                StatusEffectManager.Instance.RemoveStatusOnPlayer(StatusEffectName.Fire);
            }
            Collider[] hitStructures = Physics.OverlapSphere(splashPos, range, 1 << 6);
            foreach(Collider collider in hitStructures)
            {
                StructureBehaviorScript structure = collider.gameObject.GetComponentInParent<StructureBehaviorScript>();
                if(structure && structure != this)
                {
                    structure.HitWithWater();
                    break;
                }
            }

            Collider[] hitEnemies = Physics.OverlapSphere(splashPos, range * 2.5f, 1 << 9);
            foreach(Collider collider in hitEnemies)
            {
                var creature = collider.GetComponentInParent<CreatureBehaviorScript>();
                if (creature != null)
                {
                    creature.HitWithWater();
                    if(creature.isDead || creature.health <= 0)
                    {
                        AchievementManager.Instance.NotifyKickedBucket();
                    }
                }
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(spilled || (other.gameObject.layer != 9 && other.gameObject.layer != 10) || IsFrozen()) return;
        Vector3 localPos = transform.InverseTransformPoint(other.transform.position);

        // Determine which axis is dominant (x = left/right, z = forward/back)
        if (Mathf.Abs(localPos.x) > Mathf.Abs(localPos.z))
        {
            if (localPos.x > 0) SpillBucket(Direction.East);
                //Debug.Log(other.name + " entered from the RIGHT (local +X)");
            else SpillBucket(Direction.West);
                //Debug.Log(other.name + " entered from the LEFT (local -X)");
        }
        else
        {
            if (localPos.z > 0) SpillBucket(Direction.North);
                //Debug.Log(other.name + " entered from the FORWARD side (local +Z)");
            else SpillBucket(Direction.South);
                //Debug.Log(other.name + " entered from the BACK side (local -Z)");
        }
    }

    void Damaged(float damage)
    {
        if(damage >= maxHealth)
        {
            health = 0;
            Destroy(gameObject);
        }
        else
        {
            health = maxHealth;
        }
    }


    void OnDestroy()
    {
        OnDamageWithValue -= Damaged;
        base.OnDestroy();
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
        if(waterLevel < maxWaterLevel && !spilled && !IsFrozen()) return true;
        else return false;
    }

    public void GivenWater()
    {
        HitWithWater();
    }

    public void EmptyWater()
    {
        waterLevel = 0;
        WaterLevelChange();
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
