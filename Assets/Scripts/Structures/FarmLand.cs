using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using TMPro;

public class FarmLand : StructureBehaviorScript
{
    public CropDatabase cropDatabase;

    public CropData crop; //The current crop planted here //MUST BE SAVED
    public InventoryItemData terraFert, gloamFert, ichorFert, compost, rocks, mulch, nectar, trellis, plantFiber;
    public SpriteRenderer cropRenderer;
    public Transform itemDropTransform;
    public Collider finishedGrowingCollider;

    public MeshRenderer meshRenderer;
    public Material dry, wet, barren, barrenWet;

    [Header("Crop Stats")]
    public int growthStage = -1; //-1 means there is no crop //MUST BE SAVED
    public int hoursSpent = 0; //how long has the plant been in this growth stage for?
    public int plantStress = 0; //how much stress the plant has, gained from lack of nutrients/water. If 0 stress, the plant can produce seeds

    public bool harvestable = false; //true if growth stage matches crop data growth stages
    public bool rotted = false; //MUST BE SAVED
    public bool isWeed = false; //MUST BE SAVED
    public bool isFrosted = false;
    public bool isPollinated = false; //MUST BE SAVED
    bool forceDig = false;
    bool harvestedByScythe = false;

    public bool ignoreNextGrowthMoment = false; //tick this if crop was just planted

    PlayerInventoryHolder playerInventoryHolder;

    private NutrientStorage nutrients;
    [Header("VFX and Extra References")]
    public GameObject light;
    public VisualEffect growth, growthComplete, growthImpeded, waterSplash, ichorSplash;
    public GameObject splashObject; //extra particles
    public TextMeshProUGUI supportText;

    public TextMeshProUGUI harvestText;
    [SerializeField] private CropNeedsUI cropNeedsUI;

    float oldMaxHealth;

    [HideInInspector] public FarmTileUpgrade currentUpgrade;
    public GameObject[] upgradeObjects;

    public PopupScript needTrellis, removeTrellis;

    public enum FarmTileUpgrade
    {
        None,
        Stone,
        Mulch,
        Trellis,
        MiniWeeds
    }
    // Start is called before the first frame update
    void Awake()
    {
        base.Awake();
        if(growth) growth.Stop();
        if(growthComplete) growthComplete.Stop();
        if(growthImpeded) growthImpeded.Stop();

        if(!crop) wealthValue = 0;
        else wealthValue = crop.wealthValue;

        //tutorial
        if(Tutorial.Instance && !isWeed) Tutorial.Instance.TilledGround();

        oldMaxHealth = maxHealth;
    }

    void Start()
    {
        base.Start();
        if(supportText != null) supportText.gameObject.SetActive(false);
        ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);
        if (!crop) ignoreNextGrowthMoment = true;
        else if(crop.harvestableGrowthStages.Contains(growthStage) && !rotted)
        {
            harvestable = true;
            if(growthComplete) growthComplete.Play();
        }
        if(harvestText)
        {
            if(harvestable)
            {
                if(crop.requireScythe) harvestText.text = "Use Tool to Harvest";
                else harvestText.text = "Interact To Harvest";
            }
            else harvestText.text = "";
        }
        playerInventoryHolder = PlayerInventoryHolder.Instance;

        nutrients = StructureManager.Instance.FetchNutrient(transform.position);

        if(isWeed)
        {
            growthStage = Random.Range(0, crop.growthStages);
            growthStage++;
        }

        waterSplash.Stop();
        ichorSplash.Stop();

        SpriteChange();

        OnDamage += Damaged;

        if(!isWeed) StartCoroutine(BehaviorTimer());

    }

    // Update is called once per frame
    void Update()
    {
        base.Update();

        if(((crop && growthStage >= crop.growthStages) || isWeed || onFire) && !finishedGrowingCollider.enabled) finishedGrowingCollider.enabled = true;

        if((!crop || growthStage < crop.growthStages) && !isWeed && !onFire && finishedGrowingCollider.enabled) finishedGrowingCollider.enabled = false;

        if(!crop && growthComplete && growthComplete.HasAnySystemAwake()) growthComplete.Stop();

        if(supportText != null && !highlight[0].activeSelf)
        {
            if(structureUI) supportText.gameObject.SetActive(structureUI.activeSelf);
            if(crop != null) supportText.text = "";
        }
        
        
        
    }

    IEnumerator BehaviorTimer()
    {
        while(health > 0)
        {
            if(crop && crop.behavior && crop.behavior.behaviorUpdateTime > 0)
            {
                yield return new WaitForSeconds(crop.behavior.behaviorUpdateTime);
                crop.behavior.BehaviorUpdate(this);
            }
            else yield return new WaitForSeconds(1);
        }
    }

    public override void ItemInteraction(InventoryItemData item)
    {
        bool consumeItem = false;
        if(item == terraFert/* && nutrients.terraLevel < 10*/)
        {
            StructureManager.Instance.NutrientRefill(transform.position, 4.5f, 0, 10, 0);
            consumeItem = true;
        }
        else if(item == gloamFert/* && nutrients.gloamLevel < 10*/)
        {
            StructureManager.Instance.NutrientRefill(transform.position, 4.5f, 0, 0, 10);
            consumeItem = true;
        }
        else if(item == ichorFert && nutrients.ichorLevel < 10)
        {
            StructureManager.Instance.NutrientRefill(transform.position, 4.5f, 10, 0, 0);
            consumeItem = true;
        }
        else if(item == compost && (nutrients.gloamLevel < 10 || nutrients.terraLevel < 10))
        {
            nutrients.gloamLevel += 2;
            nutrients.terraLevel += 2;
            if(nutrients.gloamLevel > 10) nutrients.gloamLevel = 10;
            if(nutrients.terraLevel > 10) nutrients.terraLevel = 10;
            consumeItem = true;
        }
        //StructureManager.Instance.UpdateStorage(transform.position, nutrients);

        else if(!isWeed && item == rocks && currentUpgrade == FarmTileUpgrade.None)
        {
            consumeItem = true;
            ApplyNewUpgrade(FarmTileUpgrade.Stone);
            ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);
            if(audioHandler != null) audioHandler.PlayRandomSound(audioHandler.miscSounds1);
        }
        else if(!isWeed && item == mulch && currentUpgrade == FarmTileUpgrade.None)
        {
            consumeItem = true;
            ApplyNewUpgrade(FarmTileUpgrade.Mulch);
        }
        else if(!isWeed && item == trellis && currentUpgrade == FarmTileUpgrade.None && !crop)
        {
            consumeItem = true;
            ApplyNewUpgrade(FarmTileUpgrade.Trellis);
        }

        else if(item == nectar && NeedsPollination())
        {
            consumeItem = true;
            isPollinated = true;
        }
        
        if(consumeItem)
        {
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            playerInventoryHolder.UpdateInventory();
            SpriteChange();
            return;
        }

        if(crop) return;
        CropItem newCrop = item as CropItem;
        if(newCrop && newCrop.plantable)
        {
            if(newCrop.requireTrellis && currentUpgrade != FarmTileUpgrade.Trellis)
            {
                PopupHandler.Instance.AddToQueue(needTrellis);
                return;
            }
            if(!newCrop.requireTrellis && currentUpgrade == FarmTileUpgrade.Trellis)
            {
                PopupHandler.Instance.AddToQueue(removeTrellis);
                return;
            }
            InsertCrop(newCrop.cropData);
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            playerInventoryHolder.UpdateInventory();

        }
    }

    public override void StructureInteraction()
    {
        if(currentUpgrade == FarmTileUpgrade.MiniWeeds && !forceDig && !harvestedByScythe) //To remove the weeds
        {
            audioHandler.PlaySound(audioHandler.interactSound);
            ApplyNewUpgrade(FarmTileUpgrade.None);
            if(Random.Range(0, 10) > 7) ItemPoolManager.Instance.GrabItem(plantFiber).transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
            ParticlePoolManager.Instance.GrabDirtPixelParticle().transform.position = transform.position;
            return;
        }
        if(harvestable || forceDig || rotted || harvestedByScythe)
        {
            if((isWeed || rotted) && !forceDig && !harvestedByScythe) return; //Forces the player to dig the weeds and rotted plants using the shovel
            if(crop && crop.requireScythe && !forceDig && !harvestedByScythe) return; //Forces player to either use scythe or shovel for scyth crops
            if(isWeed || forceDig) audioHandler.PlaySoundAtPoint(audioHandler.interactSound, transform.position);
            else audioHandler.PlaySound(audioHandler.interactSound);

            if((rotted == false && harvestable) || isWeed)
            {
                if (crop && crop.creaturePrefab)
                {
                    Instantiate(crop.creaturePrefab, transform.position, transform.rotation); //Outdated code
                }
                else
                {
                    GameObject droppedItem;
                    Rigidbody itemRB;

                    int totalCropYield = 0;

                    if(crop.behavior)
                    {
                        crop.behavior.CropBonusYield(this, out int bonusYield, out int secondaryBonusYield);
                        totalCropYield += bonusYield;
                        print(bonusYield);

                        for (int i = 0; i < secondaryBonusYield; i++) //Secondary crop yield
                        {
                            if(!crop.cropSecondaryYield) continue;
                            droppedItem = ItemPoolManager.Instance.GrabItem(crop.cropSecondaryYield);
                            droppedItem.transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);

                            Vector3 dir3 = Random.onUnitSphere;
                            dir3 = new Vector3(dir3.x, droppedItem.transform.position.y, dir3.z);
                            itemRB = droppedItem.GetComponent<Rigidbody>();
                            itemRB.AddForce(dir3 * 20);
                            itemRB.AddForce(Vector3.up * 50);
                        }
                    }

                    int r = Random.Range(1, crop.cropYieldAmount + crop.cropYieldVariance + 1); //Adding 1 due to it being non inclusive
                    totalCropYield += r;
                    //if (totalCropYield <= 0) totalCropYield = 1;
                    for (int i = 0; i < totalCropYield; i++) //Primary crop yield
                    {
                        droppedItem = ItemPoolManager.Instance.GrabItem(crop.cropYield);
                        droppedItem.transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);

                        Vector3 dir3 = Random.onUnitSphere;
                        dir3 = new Vector3(dir3.x, droppedItem.transform.position.y, dir3.z);
                        itemRB = droppedItem.GetComponent<Rigidbody>();
                        itemRB.AddForce(dir3 * 20);
                        itemRB.AddForce(Vector3.up * 50);

                        QuestManager.Instance.CropHarvested(crop);//Increase progress per crop yield
                    }


                    r = Random.Range(0, crop.seedYieldAmount + crop.seedYieldVariance + 1); //Adding 1 due to it being non inclusive
                    for (int i = 0; i < r; i++) //Seed yield
                    {
                        if(crop.cropSeed && plantStress == 0)
                        {
                            droppedItem = ItemPoolManager.Instance.GrabItem(crop.cropSeed);
                            droppedItem.transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);

                            Vector3 dir3 = Random.onUnitSphere;
                            dir3 = new Vector3(dir3.x, droppedItem.transform.position.y, dir3.z);
                            itemRB = droppedItem.GetComponent<Rigidbody>();
                            itemRB.AddForce(dir3 * 20);
                            itemRB.AddForce(Vector3.up * 50);
                        }
                    }
                }
                crop.amountHarvested++;

                if(crop && crop.behavior) crop.behavior.OnHarvest(this, forceDig, harvestedByScythe);
            }
            
            if(crop.behavior && crop.cropSecondaryYield) //For a bonus yield at any point like cactus seeds
            {
                GameObject bonusDroppedItem;
                Rigidbody bonusItemRB;
                crop.behavior.CropRemovalBonusYield(this, out int secondaryCropBonus2);
                for (int i = 0; i < secondaryCropBonus2; i++) //Secondary crop yield
                {
                    bonusDroppedItem = ItemPoolManager.Instance.GrabItem(crop.cropSecondaryYield);
                    bonusDroppedItem.transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);

                    Vector3 dir3 = Random.onUnitSphere;
                    dir3 = new Vector3(dir3.x, bonusDroppedItem.transform.position.y, dir3.z);
                    bonusItemRB = bonusDroppedItem.GetComponent<Rigidbody>();
                    bonusItemRB.AddForce(dir3 * 20);
                    bonusItemRB.AddForce(Vector3.up * 50);
                }
            }

            if(rotted)
            {
                ReturnNutrientsFromDeadPlant();
                ItemPoolManager.Instance.GrabItem(plantFiber).transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
            }

            if(crop.behavior && crop.behavior.DestroyOnHarvest(this) == false && !rotted && harvestable)
            {
                growthStage -= 3;
            }
            else
            {
                if(crop && crop.behavior)
                {
                    crop.behavior.OnCropDestroyed(this);
                }

                if(growthStage == 1 && crop && crop.cropSeed) //drop a seed if dug up in seed stage
                {
                    ItemPoolManager.Instance.GrabItem(crop.cropSeed).transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
                }

                crop = null;
                wealthValue = 0;
                ParticlePoolManager.Instance.GrabPoofParticle().transform.position = transform.position;
                ParticlePoolManager.Instance.GrabDirtPixelParticle().transform.position = transform.position;
            } 
            harvestable = false;
            if(forceDig || isWeed)
            {
                if(currentUpgrade == FarmTileUpgrade.Trellis) ItemPoolManager.Instance.GrabItem(trellis).transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
                //if(currentUpgrade == FarmTileUpgrade.Stone) ItemPoolManager.Instance.GrabItem(rocks).transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);

                Destroy(this.gameObject);
            }
            
            forceDig = false;
            harvestedByScythe = false;
            hoursSpent = 0;
            SpriteChange();
            if(growthComplete) growthComplete.Stop();
            ignoreNextGrowthMoment = true;
            
        }
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(type == ToolType.Shovel && !forceDig)
        {
            //StartCoroutine(DigPlant());
            success = true;
        }
        if(type == ToolType.WateringCan && PlayerInteraction.Instance.waterHeld > 0 && (nutrients.waterLevel < 10 || onFire))
        {
            WaterCrops();
            success = true;

            PlayerInteraction.Instance.waterHeld--;
        }
        if(type == ToolType.Scythe && !harvestedByScythe && (isWeed || harvestable) && currentUpgrade != FarmTileUpgrade.Trellis)
        {
            harvestedByScythe = true;
            StructureInteraction();
            success = true;
        }
    }

    public override void HourPassed()
    {
        if(isWeed && !TimeManager.Instance.isDay)
        {
            StructureManager.Instance.WeedSpread(transform.position, out bool becomeThorn);
            if(becomeThorn)
            {
                growthStage = 5;
                SpriteChange();
            }
        }
        //print(cropNeedsUI);
        if(ignoreNextGrowthMoment || rotted || TimeManager.Instance.isDay || isFrosted)
        {
            ignoreNextGrowthMoment = false;
            if(!rotted && crop && crop.behavior) crop.behavior.OnHour(this);
            return;
        }
        if(!crop && !isWeed)
        {
            if(Random.Range(0, 10) > 6f && currentUpgrade == FarmTileUpgrade.None) Destroy(this.gameObject);
            return;
        }
        if(!isWeed && (nutrients.waterLevel - crop.waterIntake) < 0 && MainMenuScript.currentFileMode == FileMode.Cozy) //Behavior for when a crop is not watered enough to advance a stage
        {
            return;
        }
        hoursSpent++;
        if(crop && crop.behavior) crop.behavior.OnHour(this);

        if((crop && hoursSpent >= crop.hoursPerStage) || StructureManager.Instance.ignoreCropGrowthTime)
        {
            if(crop && crop.behavior && !crop.behavior.CanGrow(this)) return;

            if(growthStage >= crop.growthStages && !isWeed || NeedsPollination())
            {
                if(NeedsPollination()) return;

                //Reduce only water while fully grown
                hoursSpent = 0;
                health += 5;
                if(health > maxHealth) health = maxHealth;
                DrainNutrients(out bool gainedStress, true);
                if(gainedStress && growthImpeded) growthImpeded.Play();
                return;
            }
            else
            {
                hoursSpent = 0;
                DrainNutrients(out bool gainedStress, false);
                if(!isWeed)
                {
                    if(gainedStress)
                    {
                        if(growthImpeded) growthImpeded.Play();
                    } 
                    else
                    {
                        growthStage++;
                        if(growth) growth.Play();
                        health += 5;
                        if(health > maxHealth) health = maxHealth;
                    }
                }
                if(crop.harvestableGrowthStages.Contains(growthStage) && !rotted)
                {
                    harvestable = true;
                    if(growth) growth.Stop();
                    if(growthComplete)
                    {
                        growthComplete.Stop();
                        growthComplete.Play();
                    }

                    if(crop.behavior)
                    {
                        print("Call Behavior");
                        crop.behavior.OnFullyGrown(this);
                    } 
                }
                else harvestable = false;
                SpriteChange();
            }
            
        }
        else return;
    }

    public void InsertCrop(CropData _crop)
    {
        crop = _crop;
        growthStage = 1;
        hoursSpent = 0;
        plantStress = 0;
        if(nutrients != null) SpriteChange();
        ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);
        if(audioHandler != null) audioHandler.PlayRandomSound(audioHandler.miscSounds1);
        wealthValue = crop.wealthValue;
        ignoreNextGrowthMoment = true;
        maxHealth = oldMaxHealth;

        if(crop.behavior) crop.behavior.OnPlanted(this);

        if(Tutorial.Instance) Tutorial.Instance.PlantedSeed();
    }

    public void ForceChangeGrowthStage(int newStage)
    {
        growthStage = newStage;
        if(crop && crop.growthStages < growthStage) growthStage =  crop.growthStages;
        if(crop.harvestableGrowthStages.Contains(growthStage) && !rotted) harvestable = true;
        else harvestable = false;
        SpriteChange();
    }

    public void SpriteChange()
    {
        if(crop) 
        {
            if(rotted) cropRenderer.sprite = crop.rottedImage;
            else cropRenderer.sprite = crop.cropSprites[(growthStage - 1)];

            if(light)
            {
                if(crop.emitsGlow && !rotted) light.SetActive(true);
                else light.SetActive(false);
            }
        }
        else
        {
            cropRenderer.sprite = null;
            if(light) light.SetActive(false);
        }

        if(nutrients == null)
        {
            nutrients = StructureManager.Instance.FetchNutrient(transform.position);
            if(nutrients == null)
            {
                print("Nutrients are null. They Should not be");
                return;
            }
        }

        if(/*nutrients.ichorLevel <= 1 ||*/ nutrients.terraLevel <= 1 || nutrients.gloamLevel <= 1)
        {
            meshRenderer.material = barren;
        }
        else meshRenderer.material = dry;

        if(nutrients.waterLevel > 5)
        {
            if(meshRenderer.material == barren) meshRenderer.material = barrenWet;
            else meshRenderer.material = wet;
        }

        if(harvestText)
        {
            growthComplete.Stop();
            if(currentUpgrade == FarmTileUpgrade.MiniWeeds) harvestText.text = "Interact To Remove Weeds";
            else if(harvestable && !rotted)
            {
                if(crop.requireScythe) harvestText.text = "Use Tool to Harvest";
                else harvestText.text = "Interact To Harvest";
                growthComplete.Play();
            } 
            else harvestText.text = "";
        }

        //For Updating the upgrades
        if(upgradeObjects.Length == 0) return;
        for(int i = 0; i < upgradeObjects.Length; i++)
        {
            upgradeObjects[i].SetActive(false);
        }
        switch (currentUpgrade)
        {
            case FarmTileUpgrade.None:
            break;
            case FarmTileUpgrade.Stone:
            upgradeObjects[0].SetActive(true);
            break;
            case FarmTileUpgrade.Mulch:
            upgradeObjects[1].SetActive(true);
            break;
            case FarmTileUpgrade.Trellis:
            upgradeObjects[2].SetActive(true);
            break;
            case FarmTileUpgrade.MiniWeeds:
            upgradeObjects[3].SetActive(true);
            break;
        }
    }

    public void DrainNutrients(out bool gainedStress, bool waterOnly) //Drains nutrients according to crop
    {
        //PLANTS DRAIN PER GROWTH STAGE, AND THE PLAYER SHOULD HAVE TO WATER ROUGHLY EVERY STAGE/EVERY OTHER STAGE
        gainedStress = false;
        if(!crop || nutrients == null)
        {
            return;
        }

        bool ignoreWaterConsumption = false;
        if(currentUpgrade == FarmTileUpgrade.Mulch && Random.Range(0, 10) > 6) ignoreWaterConsumption = true;

        //Check if it can properly grow before draining
        if(nutrients.ichorLevel - crop.ichorIntake < 0 && !waterOnly) gainedStress = true;
        if(nutrients.terraLevel - crop.terraIntake < 0 && !waterOnly) gainedStress = true;
        if(nutrients.gloamLevel - crop.gloamIntake < 0 && !waterOnly) gainedStress = true;
        if(nutrients.waterLevel - crop.waterIntake < 0 && !isWeed && !ignoreWaterConsumption) gainedStress = true;

        if(!ignoreWaterConsumption) 
        {
            nutrients.waterLevel -= crop.waterIntake;
            if(currentUpgrade == FarmTileUpgrade.MiniWeeds) nutrients.waterLevel -= 3f; //MiniWeeds
        }
        if(nutrients.waterLevel < 0) nutrients.waterLevel = 0;

        if(!gainedStress && !waterOnly)
        {
            nutrients.ichorLevel -= crop.ichorIntake;
            if(currentUpgrade == FarmTileUpgrade.MiniWeeds) nutrients.ichorLevel -= 0.5f; //MiniWeeds
            if(nutrients.ichorLevel > 10) nutrients.ichorLevel = 10;
            if(nutrients.ichorLevel < 0) nutrients.ichorLevel = 0;

            nutrients.terraLevel -= crop.terraIntake;
            if(currentUpgrade == FarmTileUpgrade.MiniWeeds) nutrients.terraLevel -= 0.25f; //MiniWeeds
            if(nutrients.terraLevel > 10) nutrients.terraLevel = 10;
            if(nutrients.terraLevel < 0) nutrients.terraLevel = 0;

            nutrients.gloamLevel -= crop.gloamIntake;
            if(currentUpgrade == FarmTileUpgrade.MiniWeeds) nutrients.gloamLevel -= 0.25f; //MiniWeeds
            if(nutrients.gloamLevel > 10) nutrients.gloamLevel = 10;
            if(nutrients.gloamLevel < 0) nutrients.gloamLevel = 0;

        }
        else if(gainedStress) plantStress++;

        StructureManager.Instance.UpdateStorage(transform.position, nutrients);

        if(!isWeed && CheckForWeeds())
        {
            plantStress++;
            gainedStress = true;
        }

        if(plantStress > crop.stressLimit && !isWeed)
        {
            CropDied();
        }
        else SpriteChange();
    }

    public void CropDied()
    {
        rotted = true;
        harvestable = true;
        growthStage = crop.growthStages;
        SpriteChange();
        crop.amountKilled++;

        if(crop && crop.behavior)
        {
            crop.behavior.OnCropDestroyed(this);
        }
    }

    public void CropDestroyed()
    {
        if(crop && crop.behavior)
        {
            crop.behavior.OnCropDestroyed(this);
        }

        if(crop && !rotted) crop.amountKilled++;

        crop = null;
        harvestable = false;
        SpriteChange();
        ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);
    }

    void ReturnNutrientsFromDeadPlant()
    {
        //Returns half the nutrients it consumed to the soil if a plant rots and is dug out
        if(crop.ichorIntake > 0)
        {
            nutrients.ichorLevel += growthStage * crop.ichorIntake * 0.5f;
        }
        if(crop.terraIntake > 0)
        {
            nutrients.terraLevel += growthStage * crop.terraIntake * 0.5f;
        }
        if(crop.gloamIntake > 0)
        {
            nutrients.gloamLevel += growthStage * crop.gloamIntake * 0.5f;
        }

        StructureManager.Instance.UpdateStorage(transform.position, nutrients);
    }

    public override void DigAction()
    {
        forceDig = true;
        if(Tutorial.Instance && isWeed) Tutorial.Instance.WeedDug();
        else if(Tutorial.Instance && crop) Tutorial.Instance.LostSeed();

        if(crop) StructureInteraction();
        else
        {
            if(currentUpgrade == FarmTileUpgrade.Trellis) ItemPoolManager.Instance.GrabItem(trellis).transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
            if(currentUpgrade == FarmTileUpgrade.Stone) ItemPoolManager.Instance.GrabItem(rocks).transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);

            audioHandler.PlaySoundAtPoint(audioHandler.interactSound, transform.position);
            ParticlePoolManager.Instance.GrabPoofParticle().transform.position = transform.position;
            ParticlePoolManager.Instance.GrabDirtPixelParticle().transform.position = transform.position;
            Destroy(this.gameObject);
        }
    }

    void OnDestroy()
    {
        OnDamage -= Damaged;
        base.OnDestroy();
        if (!gameObject.scene.isLoaded) return; 

        if(forceDig || harvestedByScythe) CallDestroyedEvent();

        if(health <= 0)
        {
            ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);
            if(currentUpgrade == FarmTileUpgrade.Trellis) ParticlePoolManager.Instance.GrabDestructionParticle(StructureType.Wood).transform.position = transform.position;
        }
        else if(currentUpgrade == FarmTileUpgrade.Stone) ItemPoolManager.Instance.GrabItem(rocks).transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
        if(crop && !rotted) crop.amountKilled++;

        if(crop && crop.behavior)
        {
            crop.behavior.OnCropDestroyed(this);
        }

        if(Tutorial.Instance && isWeed) Tutorial.Instance.WeedDestroyed();
        if(Tutorial.Instance && crop) Tutorial.Instance.LostSeed();
    }

    public override void TimeLapse(int hours)
    {
        for(int i = 0; i < hours; i++)
        {
            HourPassed();
        }
    }

    public void WaterCrops()
    {
        //for sprinkler and gun
        nutrients.waterLevel = 10;
        waterSplash.Play();
        if(splashObject && !splashObject.activeSelf) splashObject.SetActive(true);
        SpriteChange();
        if(isFrosted) FrostDamage();
        if(onFire) Extinguish();

        if(crop && crop.behavior) crop.behavior.OnWatered(this);

        StructureManager.Instance.UpdateStorage(transform.position, nutrients);

        if(Tutorial.Instance && !isWeed) Tutorial.Instance.WateredSeed();
    }

    public void IchorRefill()
    {
        ichorSplash.Play();
        if(crop && crop.behavior) crop.behavior.OnIchorRefill(this);
        StructureManager.Instance.UpdateStorage(transform.position, nutrients);
    }

    public NutrientStorage GetCropStats() //For the UI
    {
        return nutrients;
    }

    public bool ShouldIgnoreNextGrowth()
    {
        return ignoreNextGrowthMoment;
    }

    public override bool IsFlammable()
    {
        if(crop && crop.behavior && !crop.behavior.IsFlammable()) return false;

        if((crop || isWeed) && !onFire) return true;
        else return false;
    }

    void Damaged()
    {
        ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);
        if(currentUpgrade == FarmTileUpgrade.Stone && health < 10) ApplyNewUpgrade(FarmTileUpgrade.None);
    }

    void FrostDamage() //When watering a frosted crop
    {
        //particle
        TakeDamage(5);
        ParticlePoolManager.Instance.GrabFrostBurstParticle().transform.position = transform.position;
        if(isWeed) return;
        TakeStressDamage(1);
    }

    public void RecieveFrost()
    {
        print("Recieved Frost");
        if(nearbyFires.Count == 0 && !isFrosted && (isWeed || crop))
        {
            print("Afflicted");
            isFrosted = true;
            GameObject frost = ParticlePoolManager.Instance.GrabFrostParticle();
            frost.transform.position = transform.position;
            frost.GetComponent<CropFrost>().afflictedTile = this;
            //spawn frost particle and assign it to this
        }
    }

    public void TakeStressDamage(int amount)
    {
        if(!crop) return;
        plantStress += amount;
        growthImpeded.Play();

        if(plantStress > crop.stressLimit && !isWeed)
        {
            CropDied();
        }
    }

    bool CheckForWeeds()
    {
        //save this later when able to explain this mechanic
        return false;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 3f);
        foreach(Collider collider in hitColliders)
        {
            FarmLand tile = collider.gameObject.GetComponentInParent<FarmLand>();
            if(tile && tile.isWeed)
            {
                return true;
                break;
            }
        }
        return false;
    }

    public void DrainNutrientsByMimic()
    {
        if(!crop || nutrients == null)
        {
            return;
        }


        nutrients.waterLevel -= 5;
        if(nutrients.waterLevel < 0) nutrients.waterLevel = 0;

        nutrients.ichorLevel -= .5f;
        if(nutrients.ichorLevel < 0) nutrients.ichorLevel = 0;

        nutrients.terraLevel -= .5f;
        if(nutrients.terraLevel < 0) nutrients.terraLevel = 0;

        nutrients.gloamLevel -= .5f;
        if(nutrients.gloamLevel < 0) nutrients.gloamLevel = 0;

        StructureManager.Instance.UpdateStorage(transform.position, nutrients);

        SpriteChange();

    }

    public void RefreshNutrients()
    {
        nutrients = StructureManager.Instance.FetchNutrient(transform.position);
        SpriteChange();
    }

    //[ContextMenu("PollenCheck")]
    public bool NeedsPollination()
    {
        if(crop && crop.requirePollination && !isPollinated && growthStage == crop.growthStages - 1)
        {
            //print("true");
            return true;
        }
        return false;
        
    }

    public void ApplyNewUpgrade(FarmTileUpgrade newUpgrade)
    {
        if(currentUpgrade == newUpgrade || isWeed) return;

        //Removing current effects
        switch (currentUpgrade)
        {
            case FarmTileUpgrade.Stone:
            maxHealth -= 10;
            break;
            case FarmTileUpgrade.Trellis:
            maxHealth -= 5;
            isObstacle = false;
            break;
            default:
            break;
        }

        currentUpgrade = newUpgrade;

        //Applying New effects
        switch (currentUpgrade)
        {
            case FarmTileUpgrade.Stone:
            maxHealth += 10;
            health += 10;
            break;
            case FarmTileUpgrade.Trellis:
            maxHealth += 5;
            health += 5;
            isObstacle = true;
            break;
            default:
            break;
        }

        SpriteChange();
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == 10 && isWeed && growthStage == 5)
        {
            PlayerInteraction.Instance.StaminaChange(-5); //Hit by a thorn
        }

        if(crop && crop.behavior) crop.behavior.OnContact(this, other.gameObject);
    }

    public override void LoadVariables() //Issues: Does not currently save the crop that is on it
    {
        nutrients = StructureManager.Instance.FetchNutrient(transform.position);
        if(isWeed) return;
        //print(saveString1);
        if(saveString1 != "")
        {
            crop = cropDatabase.GetCropByName(saveString1);
            print("Checked For Crop");
            print(crop);
        }
        growthStage = saveInt1;
        if(crop && growthStage > crop.growthStages) growthStage = crop.growthStages;
        hoursSpent = saveInt2;
        plantStress = saveInt3;
        isPollinated = saveBool1;
        if(saveString2 == "true") rotted = true;
        else rotted = false;

        //SpriteChange();
        if(crop) wealthValue = crop.wealthValue;
        else wealthValue = 0;

        switch(saveFloat1)
        {
            case 0:
            ApplyNewUpgrade(FarmTileUpgrade.None);
            break;
            case 1:
            ApplyNewUpgrade(FarmTileUpgrade.Stone);
            break;
            case 2:
            ApplyNewUpgrade(FarmTileUpgrade.Mulch);
            break;
            case 3:
            ApplyNewUpgrade(FarmTileUpgrade.Trellis);
            break;
        }

        GetCropStats();
    }

    public override void SaveVariables()
    {
        if(!isWeed)
        {
            if(crop) saveString1 = crop.name;
            else saveString1 = "";
            saveInt1 = growthStage;
            saveInt2 = hoursSpent;
            saveInt3 = plantStress;
            if(rotted) saveString2 = "true";
            else saveString2 = "false";

            switch (currentUpgrade)
            {
                case FarmTileUpgrade.None:
                saveFloat1 = 0;
                break;
                case FarmTileUpgrade.Stone:
                saveFloat1 = 1;
                break;
                case FarmTileUpgrade.Mulch:
                saveFloat1 = 2;
                break;
                case FarmTileUpgrade.Trellis:
                saveFloat1 = 3;
                break;
            }

            saveBool1 = isPollinated;
        }

    }
}


