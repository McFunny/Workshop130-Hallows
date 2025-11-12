using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MiniSprinkler : StructureBehaviorScript, IWaterHolder
{
    [HideInInspector] public Transform ObjectTransform => transform; // For the Interface

    public bool smartSprinkler;

    public int waterLevel = 0; 
    int maxWaterLevel = 3;
    public GameObject water;
    //public GameObject waterVFX;

    public TextMeshProUGUI waterText, modeText;

    public SprinklerMode mode;
    public Collider c_stream, c_cone;
    public GameObject streamWater, coneWater;
    public ParticleSystem splash;

    bool watering = false;
    bool waterCooldown = false;
    bool wateredThisHour = false; //To make sure it doesnt water twice in the same hour

    List<StructureBehaviorScript> structsInRange = new List<StructureBehaviorScript>();

    public enum SprinklerMode
    {
        Stream,
        Cone
    }

    //extinguish fire check

    // Start is called before the first frame update
    void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        base.Start();
        if(smartSprinkler) StartCoroutine(ScanTiles());
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
        if(water.activeSelf && waterLevel == 0)
        {
            water.SetActive(false);
        }

        if(!water.activeSelf && waterLevel != 0)
        {
            water.SetActive(true);
        }

        //waterText.text = waterLevel + "/" + maxWaterLevel;
        modeText.text = mode.ToString();

    }

    public override void HourPassed()
    {
        if(smartSprinkler) return;

        if(waterLevel > 0 && !TimeManager.Instance.isDay && !watering)
        {
            waterLevel--;
            StartCoroutine(WaterTiles());
            wateredThisHour = true;
        }
        else wateredThisHour = false;
    }

    public override void StructureInteraction()
    {
        if(watering) return;
        if(mode == SprinklerMode.Stream) mode = SprinklerMode.Cone;
        else mode = SprinklerMode.Stream;
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(watering) return;
        if(type == ToolType.Shovel)
        {
            //StartCoroutine(DugUpForItem());
            success = true;
        }
        if(type == ToolType.WateringCan && PlayerInteraction.Instance.waterHeld >= (maxWaterLevel - waterLevel) && waterLevel < maxWaterLevel)
        {
            PlayerInteraction.Instance.waterHeld -= maxWaterLevel - waterLevel;
            waterLevel = maxWaterLevel;
            if(!wateredThisHour && !smartSprinkler) 
            {
                StartCoroutine(WaterTiles());
                waterLevel--;
                wateredThisHour = true;
            }
            splash.Play();
            success = true;
        }
    }

    public override void HitWithWater()
    {
        if(waterLevel < maxWaterLevel && !waterCooldown) 
        {
            waterLevel++;
            splash.Play();
            //StartCoroutine(WaterCooldown()); //Keep disabled if the watergun costs 1 per multi shot
        }
    }

    IEnumerator WaterCooldown()
    {
        waterCooldown = true;
        yield return new WaitForSeconds(.9f);
        waterCooldown = false;
    }

    public override void TimeLapse(int hours)
    {
        for(int i = 0; i < hours; i++)
        {
            HourPassed();
        }
    }

    IEnumerator WaterTiles()
    {
        StartCoroutine(SprinkleAnimation());
        yield return new WaitForSeconds(1);

        if(mode == SprinklerMode.Stream) c_stream.enabled = true;
        else c_cone.enabled = true;

        yield return new WaitForSeconds(0.2f);

        if(mode == SprinklerMode.Stream) c_stream.enabled = false;
        else c_cone.enabled = false;

        while(structsInRange.Count > 0) //To prevent watering crops behind an obstacle
        {
            float minDist = 100;
            float dist;
            int index = 0;
            for(int i = 0; i < structsInRange.Count; i++) //Checks which one is the closest
            {
                dist = Vector3.Distance(transform.position, structsInRange[i].transform.position);
                if(dist < minDist)
                {
                    minDist = dist;
                    index = i;
                }
            }


            if(structsInRange[index].onFire) structsInRange[index].Extinguish();
            
            MiniSprinkler sprinkler = structsInRange[index] as MiniSprinkler;
            if(sprinkler && sprinkler.transform.rotation != transform.rotation) ;//Makes sure that u can only water sprinklers from behind
            else structsInRange[index].HitWithWater();

            FarmLand tile = structsInRange[index] as FarmLand;
            if(tile) tile.WaterCrops();

            if(structsInRange[index].isObstacle && mode == SprinklerMode.Stream)
            {
                structsInRange.Clear();
                yield break;
            }

            structsInRange.RemoveAt(index);
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        StructureBehaviorScript structure = collider.gameObject.GetComponentInParent<StructureBehaviorScript>();
        if(structure && !structsInRange.Contains(structure))
        {
            //print("Found a structure");
            structsInRange.Add(structure);
        }
        /*
        if(collider.gameObject.GetComponentInParent<FarmLand>())
        {
            FarmLand tile = collider.gameObject.GetComponentInParent<FarmLand>();
            tile.WaterCrops();
        }
        else
        {
            StructureBehaviorScript structure = collider.gameObject.GetComponentInParent<StructureBehaviorScript>();
            if(structure && structure.onFire) structure.Extinguish();
        }
        */
    }

    IEnumerator SprinkleAnimation()
    {
        watering = true;
        if(mode == SprinklerMode.Stream) streamWater.SetActive(true);
        else coneWater.SetActive(true);
        audioHandler.PlaySound(audioHandler.activatedSound);
        yield return new WaitForSeconds(5);
        streamWater.SetActive(false);
        coneWater.SetActive(false);
        watering = false;
    }

    public override void LoadVariables()
    {
        if(saveInt2 == 0) mode = SprinklerMode.Stream;
        else mode = SprinklerMode.Cone;
        waterLevel = saveInt1;
    }

    public override void SaveVariables()
    {
        saveInt1 = waterLevel;
        if(mode == SprinklerMode.Stream) saveInt2 = 0;
        else saveInt2 = 1;
    }

    public bool CanBeWatered()
    {
        if(waterLevel < maxWaterLevel) return true;
        else return false;
    }

    public void GivenWater()
    {
        HitWithWater();
    }

    IEnumerator ScanTiles()
    {
        while(health > 0)
        {
            yield return new WaitForSeconds(3);

            if(waterLevel == 0) continue;

            if(mode == SprinklerMode.Stream) c_stream.enabled = true;
            else c_cone.enabled = true;

            yield return new WaitForSeconds(0.5f);

            c_stream.enabled = false;
            c_cone.enabled = false;

            bool activate = false;

            for(int i = 0; i < structsInRange.Count; i++)
            {
                if(structsInRange[i].onFire)
                {
                    activate = true;
                    break;
                }

                FarmLand tile = structsInRange[i] as FarmLand;
                if(tile && tile.crop && tile.GetCropStats().waterLevel < tile.crop.waterIntake)
                {
                    activate = true;
                    break;
                }
            }

            if(activate)
            {
                structsInRange.Clear();
                waterLevel--;
                StartCoroutine(WaterTiles());
                yield return new WaitForSeconds(10);
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
