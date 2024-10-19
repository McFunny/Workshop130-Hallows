using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmLand : StructureBehaviorScript
{
    public CropData crop; //The current crop planted here
    public SpriteRenderer cropRenderer;
    public Transform itemDropTransform;

    public MeshRenderer meshRenderer;
    public Material dry, wet, barren, barrenWet;

    //public float nutrients.waterLevel; //How much has this crop been watered
    public int growthStage = -1; //-1 means there is no crop
    public int hoursSpent = 0; //how long has the plant been in this growth stage for?

    public bool harvestable = false; //true if growth stage matches crop data growth stages
    public bool rotted = false;
    public bool isWeed = false;

    private bool ignoreNextGrowthMoment = false; //tick this if crop was just planted

    PlayerInventoryHolder playerInventoryHolder;

    private NutrientStorage nutrients;
    public NutrientStorage cropStats;
    // Start is called before the first frame update
    void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);
        if (!crop) ignoreNextGrowthMoment = true;
        else if(crop.harvestableGrowthStages.Contains(growthStage))
        {
            harvestable = true;
        }
        playerInventoryHolder = FindObjectOfType<PlayerInventoryHolder>();

        nutrients = StructureManager.Instance.FetchNutrient(transform.position);

        if(isWeed)
        {
            growthStage = Random.Range(0, crop.growthStages);
            growthStage++;
        }

        SpriteChange();
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
    }

    public override void ItemInteraction(InventoryItemData item)
    {
        if(crop) return;
        CropItem newCrop = item as CropItem;
        if(newCrop && newCrop.plantable)
        {
            crop = newCrop.cropData;
            growthStage = 1;
            SpriteChange();
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            playerInventoryHolder.UpdateInventory();
            ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);

            audioHandler.PlayRandomSound(audioHandler.miscSounds1);

        }
    }

    public override void StructureInteraction()
    {
        if(harvestable)
        {
            audioHandler.PlaySound(audioHandler.interactSound);
            harvestable = false;
            if(rotted == false)
            {
                if (crop.creaturePrefab)
                {
                    Instantiate(crop.creaturePrefab, transform.position, transform.rotation); //Code needs work once mandrake crop is added
                }
                else
                {
                    GameObject droppedItem;
                    for (int i = 0; i < crop.cropYieldAmount; i++)
                    {
                        droppedItem = ItemPoolManager.Instance.GrabItem(crop.cropYield);
                        droppedItem.transform.position = transform.position;
                    }
                    int r = Random.Range(crop.seedYieldAmount - crop.seedYieldVariance, crop.seedYieldAmount + crop.seedYieldVariance + 1);
                    for (int i = 0; i < r; i++)
                    {
                        droppedItem = ItemPoolManager.Instance.GrabItem(crop.cropSeed);
                        droppedItem.transform.position = transform.position;
                    }
                }
            }

            crop = null;
            isWeed = false;
            SpriteChange();
        }
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(type == ToolType.Shovel)
        {
            //Harvest
        }
        if(type == ToolType.WateringCan && PlayerInteraction.Instance.waterHeld > 0 && nutrients.waterLevel < 10)
        {
            PlayerInteraction.Instance.waterHeld--;
            nutrients.waterLevel = 10;
            SpriteChange();
            success = true;
        }
    }

    public override void HourPassed()
    {
        if(ignoreNextGrowthMoment || rotted)
        {
            ignoreNextGrowthMoment = false;
            return;
        }
        if(!crop)
        {
            Destroy(this.gameObject);
            return;
        }
        hoursSpent++;
        if(hoursSpent >= crop.hoursPerStage)
        {
            if(growthStage >= crop.growthStages - 1)
            {
                //IT HAS REACHED MAX GROWTH STATE

                //if(hoursSpent < crop.hoursPerStage * 3) return;
                //plant rots
                //CropDied();
            }
            else
            {
                hoursSpent = 0;
                if(!isWeed) growthStage++;
                if(crop.harvestableGrowthStages.Contains(growthStage)) harvestable = true;
                else harvestable = false;
                SpriteChange();
            }
            
        }
        else return;

        DrainNutrients();
    }

    public void InsertCreature(CropData _data, int _growthStage)
    {
        //the mimic will use this function to "plant" itself
        isWeed = true;
        crop = _data;
        growthStage = _growthStage;
        SpriteChange();
    }

    public void SpriteChange()
    {
        print(growthStage);
        if(crop) cropRenderer.sprite = crop.cropSprites[(growthStage - 1)];
        else cropRenderer.sprite = null;

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
    }

    void DrainNutrients()
    {
        //PLANTS DRAIN PER GROWTH STAGE, AND THE PLAYER SHOULD HAVE TO WATER ROUGHLY EVERY STAGE/EVERY OTHER STAGE
        bool plantDied = false;
        nutrients.ichorLevel -= crop.ichorIntake;
        if(nutrients.ichorLevel < 0)
        {
            nutrients.ichorLevel = 0;
            plantDied = true;
        }
        nutrients.terraLevel -= crop.terraIntake;
        if(nutrients.terraLevel < 0)
        {
            nutrients.terraLevel = 0;
            plantDied = true;
        }
        nutrients.gloamLevel -= crop.gloamIntake;
        if(nutrients.gloamLevel < 0)
        {
            nutrients.gloamLevel = 0;
            plantDied = true;
        }
        nutrients.waterLevel -= crop.waterIntake;
        if(nutrients.waterLevel < 0)
        {
            nutrients.waterLevel = 0;
            plantDied = true;
        }
        StructureManager.Instance.UpdateStorage(transform.position, nutrients);

        if(plantDied && !isWeed)
        {
            CropDied();
        }
        else SpriteChange();
    }

    void CropDied()
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

    void OnDestroy()
    {
        if (!gameObject.scene.isLoaded) return; 
        if (crop != null && crop.creaturePrefab)
        {
            Instantiate(crop.creaturePrefab, transform.position, transform.rotation); //Code needs work once mandrake crop is added
        }
        ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);
        base.OnDestroy();
    }

    public NutrientStorage GetCropStats()
    {
        cropStats = StructureManager.Instance.FetchNutrient(transform.position);
        return cropStats;
    }
}
