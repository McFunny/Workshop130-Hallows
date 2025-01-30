using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;

public class WaterPuzzleTile : StructureBehaviorScript
{
    

    public MeshRenderer meshRenderer;
    public Material dry, wet, barren, barrenWet;

    public float waterLevel;




    public VisualEffect waterSplash;

    public GameObject waterCanvas;
    
    
    //[SerializeField] private CropNeedsUI cropNeedsUI; //make your own
    // Start is called before the first frame update
    void Awake()
    {
        //base.Awake();

    }

    void Start()
    {
        //base.Start();
        //ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);
    
        meshRenderer.material = dry;
        waterSplash.Stop();

    }

    // Update is called once per frame
    void Update()
    {
        //base.Update();

        waterLevel -= Time.deltaTime;

        if (waterLevel <= 0)
        {
            waterCanvas.SetActive(true);
            meshRenderer.material = dry;
        }
        else
        {
            waterCanvas.SetActive(false);
            meshRenderer.material = wet;
        }
    }

    public override void ItemInteraction(InventoryItemData item)
    {
       
    }

    public override void StructureInteraction()
    {
       
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
    }

    public override void HourPassed()
    {
       
       
    }

    void OnDestroy()
    {

    }

    public override void TimeLapse(int hours)
    {
        for (int i = 0; i < hours; i++)
        {
            HourPassed();
        }
    }

    public void WaterCrops()
    {
        meshRenderer.material = wet;
        waterCanvas.SetActive(false);
        waterLevel = 10;
        waterSplash.Play();
        
    }

    public override void HitWithWater()
    {
        WaterCrops();
    }
}
