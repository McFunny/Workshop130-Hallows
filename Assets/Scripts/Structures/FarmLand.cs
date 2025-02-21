using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using TMPro;

public class FarmLand : StructureBehaviorScript
{
    public CropDatabase cropDatabase;

    public CropData crop; //The current crop planted here //MUST BE SAVED
    public InventoryItemData terraFert, gloamFert, ichorFert, compost;
    public SpriteRenderer cropRenderer;
    public Transform itemDropTransform;
    public Collider finishedGrowingCollider;

    public MeshRenderer meshRenderer;
    public Material dry, wet, barren, barrenWet;

    //public float nutrients.waterLevel; //How much has this crop been watered
    public int growthStage = -1; //-1 means there is no crop //MUST BE SAVED
    public int hoursSpent = 0; //how long has the plant been in this growth stage for?
    public int plantStress = 0; //how much stress the plant has, gained from lack of nutrients/water. If 0 stress, the plant can produce seeds

    public bool harvestable = false; //true if growth stage matches crop data growth stages
    public bool rotted = false; //MUST BE SAVED
    public bool isWeed = false; //MUST BE SAVED
    public bool isFrosted = false;
    bool forceDig = false;

    private bool ignoreNextGrowthMoment = false; //tick this if crop was just planted

    PlayerInventoryHolder playerInventoryHolder;

    private NutrientStorage nutrients;

    public VisualEffect growth, growthComplete, growthImpeded, waterSplash, ichorSplash;
    public GameObject frostParticles;
    public GameObject light;

    public TextMeshProUGUI harvestText;
    [SerializeField] private CropNeedsUI cropNeedsUI;
    // Start is called before the first frame update
    void Awake()
    {
        base.Awake();
        if(growth) growth.Stop();
        if(growthComplete) growthComplete.Stop();
        if(growthImpeded) growthImpeded.Stop();

        if(!crop) wealthValue = 0;
    }

    void Start()
    {
        base.Start();
        ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);
        if (!crop) ignoreNextGrowthMoment = true;
        else if(crop.harvestableGrowthStages.Contains(growthStage) && !rotted)
        {
            harvestable = true;
            if(growthComplete) growthComplete.Play();
        }
        if(harvestText)
        {
            if(harvestable) harvestText.text = "Interact To Harvest";
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

    }

    // Update is called once per frame
    void Update()
    {
        base.Update();

        if(((crop && growthStage >= crop.growthStages) || isWeed || onFire) && !finishedGrowingCollider.enabled) finishedGrowingCollider.enabled = true;

        if((!crop || growthStage < crop.growthStages) && !isWeed && !onFire && finishedGrowingCollider.enabled) finishedGrowingCollider.enabled = false;

        if(!crop && growthComplete) growthComplete.Stop();
    }

    public override void ItemInteraction(InventoryItemData item)
    {
        if(item == terraFert && nutrients.terraLevel < 10)
        {
            nutrients.terraLevel = 10;
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            playerInventoryHolder.UpdateInventory();
            return;
        }
        if(item == gloamFert && nutrients.gloamLevel < 10)
        {
            nutrients.gloamLevel = 10;
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            playerInventoryHolder.UpdateInventory();
            return;
        }
        if(item == ichorFert && nutrients.ichorLevel < 10)
        {
            nutrients.ichorLevel = 10;
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            playerInventoryHolder.UpdateInventory();
            return;
        }
        if(item == compost && (nutrients.gloamLevel < 10 || nutrients.terraLevel < 10))
        {
            nutrients.gloamLevel += 5;
            nutrients.terraLevel += 5;
            if(nutrients.gloamLevel > 10) nutrients.gloamLevel = 10;
            if(nutrients.terraLevel > 10) nutrients.terraLevel = 10;
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            playerInventoryHolder.UpdateInventory();
            return;
        }
        StructureManager.Instance.UpdateStorage(transform.position, nutrients);

        if(crop) return;
        CropItem newCrop = item as CropItem;
        if(newCrop && newCrop.plantable)
        {
            InsertCrop(newCrop.cropData);
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            playerInventoryHolder.UpdateInventory();

        }
    }

    public override void StructureInteraction()
    {
        if(harvestable || forceDig || rotted)
        {
            if((isWeed && !forceDig) || (rotted && !forceDig)) return; //Forces the player to dig the weeds and rotted plants using the shovel
            if(isWeed || forceDig) audioHandler.PlaySoundAtPoint(audioHandler.interactSound, transform.position);
            else audioHandler.PlaySound(audioHandler.interactSound);
            if((rotted == false && harvestable) || isWeed)
            {
                if (crop.creaturePrefab)
                {
                    Instantiate(crop.creaturePrefab, transform.position, transform.rotation); //Code needs work once mandrake crop is added
                }
                else
                {
                    GameObject droppedItem;
                    Rigidbody itemRB;

                    int totalCropYield = 0;

                    if(crop.behavior)
                    {
                        crop.behavior.CropBonusYield(this, out int bonusYield);
                        totalCropYield += bonusYield;
                        print(bonusYield);
                    }

                    int r = Random.Range(crop.cropYieldAmount - crop.cropYieldVariance, crop.cropYieldAmount + crop.cropYieldVariance);
                    if(totalCropYield == 0) r = 1;
                    totalCropYield += r;
                    for (int i = 0; i < totalCropYield; i++)
                    {
                        droppedItem = ItemPoolManager.Instance.GrabItem(crop.cropYield);
                        droppedItem.transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);

                        Vector3 dir3 = Random.onUnitSphere;
                        dir3 = new Vector3(dir3.x, droppedItem.transform.position.y, dir3.z);
                        itemRB = droppedItem.GetComponent<Rigidbody>();
                        itemRB.AddForce(dir3 * 20);
                        itemRB.AddForce(Vector3.up * 50);
                    }

                    r = Random.Range(crop.seedYieldAmount - crop.seedYieldVariance, crop.seedYieldAmount + crop.seedYieldVariance + 1);
                    if(r == 0 && Random.Range(0,10) >= 6) r = 1;
                    for (int i = 0; i < r; i++)
                    {
                        if(crop.cropSeed && plantStress == 0 && crop.seedYieldAmount > 0)
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
            }

            if(rotted)
            {
                ReturnNutrientsFromDeadPlant();
            }
            
            if(crop.behavior && crop.behavior.DestroyOnHarvest() == false && !rotted && harvestable)
            {
                growthStage -= 3;
            }
            else
            {
                crop = null;
                wealthValue = 0;
                ParticlePoolManager.Instance.GrabPoofParticle().transform.position = transform.position;
                ParticlePoolManager.Instance.GrabDirtPixelParticle().transform.position = transform.position;
            } 
            harvestable = false;
            if(forceDig || isWeed) Destroy(this.gameObject);
            forceDig = false;
            hoursSpent = 0;
            SpriteChange();
            if(growthComplete) growthComplete.Stop();
        }
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(type == ToolType.Shovel && !forceDig)
        {
            StartCoroutine(DigPlant());
            success = true;
        }
        if(type == ToolType.WateringCan && PlayerInteraction.Instance.waterHeld > 0 && nutrients.waterLevel < 10)
        {
            WaterCrops();
            success = true;

            PlayerInteraction.Instance.waterHeld--;
        }
    }

    public override void HourPassed()
    {
        if(isWeed && !TimeManager.Instance.isDay) StructureManager.Instance.WeedSpread(transform.position);
        //print(cropNeedsUI);
        if(ignoreNextGrowthMoment || rotted || TimeManager.Instance.isDay)
        {
            ignoreNextGrowthMoment = false;
            if(!rotted && crop && crop.behavior) crop.behavior.OnHour(this);
            return;
        }
        if(!crop)
        {
            float r = Random.Range(0, 10);
            if(r > 6f) Destroy(this.gameObject);
            return;
        }
        hoursSpent++;
        if(crop.behavior) crop.behavior.OnHour(this);

        if(hoursSpent >= crop.hoursPerStage || StructureManager.Instance.ignoreCropGrowthTime)
        {
            if(growthStage >= crop.growthStages && !isWeed)
            {
                return;
                //IT HAS REACHED MAX GROWTH STATE

                //if(hoursSpent < crop.hoursPerStage * 3) return;
                //plant rots
                //CropDied();
            }
            else
            {
                hoursSpent = 0;
                health += 5;
                if(health > maxHealth) health = maxHealth;
                DrainNutrients(out bool gainedStress);
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
        wealthValue = 5;
        ignoreNextGrowthMoment = true;
    }

    /*public void InsertCreature(CropData _data, int _growthStage)
    {
        //the mimic will use this function to "plant" itself
        isWeed = true;
        crop = _data;
        growthStage = _growthStage;
        SpriteChange();
    } */

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

        if(nutrients.ichorLevel <= 1 || nutrients.terraLevel <= 1 || nutrients.gloamLevel <= 1)
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
            if(harvestable) harvestText.text = "Interact To Harvest";
            else harvestText.text = "";
        }
    }

    void DrainNutrients(out bool gainedStress)
    {
        //PLANTS DRAIN PER GROWTH STAGE, AND THE PLAYER SHOULD HAVE TO WATER ROUGHLY EVERY STAGE/EVERY OTHER STAGE
        gainedStress = false;
        if(!crop || nutrients == null)
        {
            return;
        }

        //Check if it can properly grow before draining
        if(nutrients.ichorLevel - crop.ichorIntake < 0) gainedStress = true;
        if(nutrients.terraLevel - crop.terraIntake < 0) gainedStress = true;
        if(nutrients.gloamLevel - crop.gloamIntake < 0) gainedStress = true;
        if(nutrients.waterLevel - crop.waterIntake < 0) gainedStress = true;

        nutrients.waterLevel -= crop.waterIntake;
        if(nutrients.waterLevel < 0) nutrients.waterLevel = 0;

        if(!gainedStress)
        {
            nutrients.ichorLevel -= crop.ichorIntake;
            if(nutrients.ichorLevel > 10) nutrients.ichorLevel = 10;

            nutrients.terraLevel -= crop.terraIntake;
            if(nutrients.terraLevel > 10) nutrients.terraLevel = 10;

            nutrients.gloamLevel -= crop.gloamIntake;
            if(nutrients.gloamLevel > 10) nutrients.gloamLevel = 10;

        }
        else plantStress++;

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
    }

    public void CropDestroyed()
    {
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

    IEnumerator DigPlant()
    {
        forceDig = true;
        yield return new WaitForSeconds(1f);
        if(crop) StructureInteraction();
        else
        {
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
        if (crop != null && crop.creaturePrefab)
        {
            Instantiate(crop.creaturePrefab, transform.position, transform.rotation); //Code needs work once Plant Mimic is added
        }
        if(health <= 0) ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);
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
        SpriteChange();
        if(isFrosted) FrostDamage();
        if(onFire) Extinguish();

        StructureManager.Instance.UpdateStorage(transform.position, nutrients);
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
        if((crop || isWeed) && !onFire) return true;
        else return false;
    }

    void Damaged()
    {
        ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);
    }

    void FrostDamage() //When watering a frosted crop
    {
        //particle
        TakeDamage(5);
        ParticlePoolManager.Instance.GrabFrostBurstParticle().transform.position = transform.position;
        if(isWeed) return;
        plantStress++;
        growthImpeded.Play();

        if(plantStress > crop.stressLimit && !isWeed)
        {
            CropDied();
        }
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

    bool CheckForWeeds()
    {
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
        hoursSpent = saveInt2;
        plantStress = saveInt3;
        if(saveString2 == "true") rotted = true;
        else rotted = false;

        //SpriteChange();
        if(crop) wealthValue = 5;
        else wealthValue = 0;

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
        }

    }
}


