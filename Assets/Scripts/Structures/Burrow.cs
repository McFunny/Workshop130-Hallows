using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Burrow : StructureBehaviorScript, IWaterHolder
{
    [HideInInspector] public Transform ObjectTransform => transform; // For the Interface

    bool isDigging;

    public BugObject termite;

    public GameObject eggObject;

    public bool containsEgg;

    public InventoryItemData eggItem, rockItem;
    
    void Awake()
    {
        base.Awake();
    }
    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);

        OnDamage += Damaged;

        if(/*!TimeManager.Instance.isDay &&*/ Random.Range(0,10) > 7)
        {
            InsertItem(rockItem);
        }
        StartCoroutine(LateStart());
    }

    IEnumerator LateStart()
    {
        yield return new WaitForSeconds(1);

        //Grab the top structure
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, 1, 1 << 6);
        foreach(Collider collider in nearbyColliders)
        {
            StructureBehaviorScript structure = collider.gameObject.GetComponentInParent<StructureBehaviorScript>();

            if(structure && structure != this)
            {
                if(structure.structData == structData)
                {
                    savedItems.Clear();
                    Destroy(gameObject); //Duplicate tile
                    yield break;
                }
            }
        }
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(type == ToolType.Shovel)
        {
            //StartCoroutine(Dig());
            success = true;
        }
        if(type == ToolType.WateringCan && PlayerInteraction.Instance.waterHeld > 0)
        {
            ParticlePoolManager.Instance.GrabSplashParticle().transform.position = transform.position;
            PlayerInteraction.Instance.WaterChange(-1);
            success = true;
            Destroy(gameObject);
        }
    }

    public override void ItemInteraction(InventoryItemData item)
    {
        if(item == eggItem)
        {
            eggObject.SetActive(true);
            containsEgg = true;
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            PlayerInventoryHolder.Instance.UpdateInventory();
            ParticlePoolManager.Instance.GrabPoofParticle().transform.position = transform.position;
        }
    }

    public override void DigAction()
    {
        audioHandler.PlaySoundAtPoint(audioHandler.interactSound, transform.position);
        ParticlePoolManager.Instance.GrabPoofParticle().transform.position = transform.position;
        ParticlePoolManager.Instance.GrabDirtPixelParticle().transform.position = transform.position;
        Destroy(this.gameObject);
    }

    public override void HourPassed()
    {
        if(Random.Range(0,30) > 29) StartCoroutine(SpawnBug());
    }

    public void InsertItem(InventoryItemData item)
    {
        savedItems.Add(item);
    }

    IEnumerator SpawnBug()
    {
        yield return new WaitForSeconds(Random.Range(2, 15));
        BugSpawningManager.Instance.SpawnBug(transform.position, termite);
    }

    void OnDestroy()
    {
        OnDamage -= Damaged;
        base.OnDestroy();
        if (!gameObject.scene.isLoaded) return; 
        //drop items
        GameObject droppedItem;
        foreach(InventoryItemData item in savedItems)
        {
            droppedItem = ItemPoolManager.Instance.GrabItem(item);
            droppedItem.transform.position = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);
        }

        if(containsEgg)
        {
            droppedItem = ItemPoolManager.Instance.GrabItem(eggItem);
            droppedItem.transform.position = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);
        }
    }

    void Damaged()
    {
        ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);
        if(containsEgg) Explode();
    }

    public override void HitWithWater()
    {
        if(IsFrozen()) return;
        ParticlePoolManager.Instance.GrabPoofParticle().transform.position = transform.position;
        ParticlePoolManager.Instance.GrabDirtPixelParticle().transform.position = transform.position;
        Destroy(gameObject);
    }

    public bool CanBeWatered()
    {
        if(savedItems.Count > 0 && !IsFrozen()) return false;
        return true;
    }

    public void GivenWater()
    {
        HitWithWater();
    }

    public void EmptyWater(){}

    public void ManualFill(out bool success)
    {
        if(PlayerInteraction.Instance.waterHeld > 0)
        {
            ParticlePoolManager.Instance.GrabSplashParticle().transform.position = transform.position;
            PlayerInteraction.Instance.waterHeld--;
            success = true;
            Destroy(gameObject);
        }
        else success = false;
    }

    public void UseBurrow() //creatures call this when using it
    {
        if(containsEgg)
        {
            Explode();

            Destroy(gameObject);
        }
    }

    void Explode()
    {
        containsEgg = false;
        ParticlePoolManager.Instance.GrabExplosionParticle().transform.position = transform.position;
        if(Vector3.Distance(transform.position, PlayerInteraction.Instance.transform.position) < 3f)
        {
            PlayerInteraction.Instance.ApplyStatusEffect(StatusDatabase.Instance.GetStatus(StatusEffectName.Fire), 8);
            PlayerInteraction.Instance.StaminaChange(-15);
            PlayerInteraction.Instance.PlayerTrip();
        }
        Collider[] hitStructures = Physics.OverlapSphere(transform.position, 3f, 1 << 6);
        foreach(Collider collider in hitStructures)
        {
            StructureBehaviorScript structure = collider.gameObject.GetComponentInParent<StructureBehaviorScript>();
            if(structure && structure != this)
            {
                if(structure.IsFlammable()) structure.LitOnFire();
                else structure.TakeDamage(10);
            }
        }

        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, 4f, 1 << 9);
        foreach(Collider collider in hitEnemies)
        {
            var creature = collider.GetComponentInParent<CreatureBehaviorScript>();
            if (creature != null && creature.shovelVulnerable)
            {
                creature.lastDamageTypeTaken = DamageType.Mine;
                creature.TakeDamage(40);
                if(creature.fireVulnerable) creature.ApplyStatusEffect(StatusDatabase.Instance.GetStatus(StatusEffectName.Fire), Random.Range(5, 15));
                creature.PlayHitParticle(creature.transform.position);
            }
        }
    }

    public override void SaveVariables()
    {
        saveBool1 = containsEgg;
    }

    public override void LoadVariables()
    {
        containsEgg = saveBool1;
        if(containsEgg == true) eggObject.SetActive(true);
    }
}
