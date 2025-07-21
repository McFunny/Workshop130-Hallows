using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MiniSprinkler : StructureBehaviorScript
{
    public int waterLevel = 0; 
    int maxWaterLevel = 3;
    public GameObject water;
    public GameObject waterVFX;

    public TextMeshProUGUI waterText, modeText;

    public SprinklerMode mode;
    public Collider c_stream, c_cone;

    bool watering = false;

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
        waterVFX.SetActive(false);
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
        if(waterLevel > 0 && !TimeManager.Instance.isDay)
        {
            waterLevel--;
            StartCoroutine(WaterTiles());
        }
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
        if(type == ToolType.Shovel)
        {
            //StartCoroutine(DugUpForItem());
            success = true;
        }
        if(type == ToolType.WateringCan && PlayerInteraction.Instance.waterHeld >= (maxWaterLevel - waterLevel) && waterLevel < maxWaterLevel)
        {
            PlayerInteraction.Instance.waterHeld -= maxWaterLevel - waterLevel;
            waterLevel = maxWaterLevel;
            StartCoroutine(WaterTiles());
            success = true;
        }
    }

    public override void HitWithWater()
    {
        if(waterLevel < maxWaterLevel) waterLevel++;
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

            if(structsInRange[index].isObstacle && mode == SprinklerMode.Stream)
            {
                structsInRange.Clear();
            }

            structsInRange.RemoveAt(index);
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        StructureBehaviorScript structure = collider.gameObject.GetComponentInParent<StructureBehaviorScript>();
        if(structure && !structsInRange.Contains(structure))
        {
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
        waterVFX.SetActive(true);
        audioHandler.PlaySound(audioHandler.activatedSound);
        yield return new WaitForSeconds(5);
        watering = false;
        waterVFX.SetActive(false);
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
