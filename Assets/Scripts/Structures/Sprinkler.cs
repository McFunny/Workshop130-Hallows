using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Sprinkler : StructureBehaviorScript
{
    public int waterLevel = 0; //max is 5
    public GameObject water;
    public Transform head;
    public GameObject waterVFX;
    bool rotating = false;

    public TextMeshProUGUI waterText;

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

        if(rotating) head.Rotate(0, Time.deltaTime * 20, 0, Space.Self);

        waterText.text = waterLevel + "/" + 5;

    }

    public override void HourPassed()
    {
        if(waterLevel > 0 && !TimeManager.Instance.isDay)
        {
            waterLevel--;
            StartCoroutine(WaterTiles());
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
        if(type == ToolType.WateringCan && PlayerInteraction.Instance.waterHeld >= (5 - waterLevel) && waterLevel < 5)
        {
            PlayerInteraction.Instance.waterHeld -= 5 - waterLevel;
            waterLevel = 5;
            StartCoroutine(WaterTiles());
            success = true;
        }
    }

    public override void HitWithWater()
    {
        if(waterLevel < 5) waterLevel++;
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
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 3f);
        foreach(Collider collider in hitColliders)
        {
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
        }
    }

    /*IEnumerator WaterAdjacentTiles()
    {
        List<Vector3> nearbyTiles = StructureManager.Instance.GetAdjacentClearTiles(transform.position); //this says get adjacent CLEAR tiles dummy
        StartCoroutine(SprinkleAnimation());
        yield return new WaitForSeconds(1);
        foreach(Vector3 pos in nearbyTiles)
        {
            StructureBehaviorScript structure = StructureManager.Instance.GrabStructureOnTile(pos);
            if(!structure) continue;
            FarmLand tile = structure as FarmLand;
            if(tile)
            {
                tile.WaterCrops();
            }
            else if(structure.onFire) structure.Extinguish();
        }

    }*/

    IEnumerator SprinkleAnimation()
    {
        rotating = true;
        waterVFX.SetActive(true);
        audioHandler.PlaySound(audioHandler.activatedSound);
        yield return new WaitForSeconds(5);
        rotating = false;
        yield return new WaitForSeconds(2);
        waterVFX.SetActive(false);
    }

    public override void LoadVariables()
    {
        waterLevel = saveInt1;
    }

    public override void SaveVariables()
    {
        saveInt1 = waterLevel;
    }

}
