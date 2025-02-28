using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class FakeFarmLand : StructureBehaviorScript
{
    public PlantMimic mimic;

    public CropData crop; //Use a new fake mimic crop

    public SpriteRenderer cropRenderer;

    private NutrientStorage nutrients;

    PlayerInventoryHolder playerInventoryHolder;

    public VisualEffect waterSplash, ichorSplash;

    bool isDigging;

    void Awake()
    {
        base.Awake();
    }
    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);

        playerInventoryHolder = PlayerInventoryHolder.Instance;

        nutrients = StructureManager.Instance.FetchNutrient(transform.position);

        waterSplash.Stop();
        ichorSplash.Stop();

        OnDamage += Damaged;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void CopyNearbyPlant()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 3f);
        foreach(Collider collider in hitColliders)
        {
            FarmLand tile = collider.gameObject.GetComponentInParent<FarmLand>();
            if(tile && tile.crop)
            {
                cropRenderer.sprite = tile.cropRenderer.sprite;
                return;
            }
        }
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(type == ToolType.Shovel && !isDigging)
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
        if(TimeManager.Instance.isDay)
        {
            ParticlePoolManager.Instance.GrabPoofParticle().transform.position = transform.position;
            ParticlePoolManager.Instance.GrabDirtPixelParticle().transform.position = transform.position;
            Destroy(this.gameObject);
            return;
        }

        StartCoroutine(DrainNutrients());
    }

    IEnumerator DrainNutrients()
    {
        if(!crop || nutrients == null)
        {
            yield break;
        }

        nutrients.waterLevel -= 5;
        if(nutrients.waterLevel < 0) nutrients.waterLevel = 0;

        nutrients.ichorLevel -= 1;
        if(nutrients.ichorLevel < 0) nutrients.ichorLevel = 0;

        nutrients.terraLevel -= 1;
        if(nutrients.terraLevel < 0) nutrients.terraLevel = 0;

        nutrients.gloamLevel -= 1;
        if(nutrients.gloamLevel < 0) nutrients.gloamLevel = 0;

        StructureManager.Instance.UpdateStorage(transform.position, nutrients);

        yield return new WaitForSeconds(3);
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 3f);
        foreach(Collider collider in hitColliders)
        {
            FarmLand tile = collider.gameObject.GetComponentInParent<FarmLand>();
            if(tile && tile.crop)
            {
                tile.DrainNutrientsByMimic();
            }
        }
    }

    IEnumerator DigPlant()
    {
        isDigging = true;
        yield return new WaitForSeconds(1f);
        audioHandler.PlaySoundAtPoint(audioHandler.interactSound, transform.position);
        ParticlePoolManager.Instance.GrabPoofParticle().transform.position = transform.position;
        ParticlePoolManager.Instance.GrabDirtPixelParticle().transform.position = transform.position;
        Destroy(this.gameObject);
    }

    public void WaterCrops()
    {
        //for sprinkler and gun
        nutrients.waterLevel = 10;
        waterSplash.Play();
        if(onFire) Extinguish();

        StructureManager.Instance.UpdateStorage(transform.position, nutrients);
    }

    void OnDestroy()
    {
        OnDamage -= Damaged;
        base.OnDestroy();
        if (!gameObject.scene.isLoaded) return; 
        if (mimic)
        {
            //moveit and the mimic will make the burrow
        }
        if(health <= 0) ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);
    }

    void Damaged()
    {
        ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);
    }
}
