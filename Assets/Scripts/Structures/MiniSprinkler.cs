using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MiniSprinkler : StructureBehaviorScript
{
    public int waterLevel = 0; 
    int maxWaterLevel = 3;
    public GameObject water;
    //public GameObject waterVFX;

    public TextMeshProUGUI waterText, modeText;

    public SprinklerMode mode;
    public Collider c_stream, c_cone;
    public GameObject streamWater, coneWater;

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

        waterText.text = waterLevel + "/" + maxWaterLevel;
        modeText.text = mode.ToString();

    }

    public override void HourPassed()
    {
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
            if(!wateredThisHour) 
            {
                StartCoroutine(WaterTiles());
                waterLevel--;
                wateredThisHour = true;
            }
            success = true;
        }
    }

    public override void HitWithWater()
    {
        if(waterLevel < maxWaterLevel && !waterCooldown) 
        {
            waterLevel++;
            StartCoroutine(WaterCooldown()); //Keep disabled if the watergun costs 1 per multi shot
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
            structsInRange[index].HitWithWater();
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

}
