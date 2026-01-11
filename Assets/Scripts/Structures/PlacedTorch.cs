using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacedTorch : StructureBehaviorScript, IFireHolder
{

    //public FireFearTrigger fireTrigger;
    public GameObject fire;

    bool currentlyLit;

    public LightController lightScript;

    public ParticleSystem lowFireParticle;

    int flameLeft = 0;
    int maxFlame = 1;

    public bool isUpgraded = false;

    public ParticleSystem flameThrowerParticles;
    public Collider burnCollider;
    bool shootingFire;

    void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        base.Start();
        //fire.SetActive(false);
        if(!PlayerInteraction.Instance.torchLit) ExtinguishFlame();
        else StartCoroutine(FireDrain());
        ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);

        Orient();
    }

    void Orient()
    {
        Direction dir = StructureManager.Instance.GetDirection(PlayerInteraction.Instance.mainCam.transform);
        switch(dir)
        {
            case Direction.North:
            transform.Rotate(0, 180, 0);
            break;
            case Direction.East:
            transform.Rotate(0, 90, 0);
            break;
            case Direction.South:
            transform.Rotate(0, 0, 0);
            break;
            case Direction.West:
            transform.Rotate(0, 270, 0);
            break;
        }
    }

    void Update()
    {
        base.Update();

        if(fire.activeSelf && !PlayerInteraction.Instance.torchLit) ExtinguishFlame();
    }

    public override void StructureInteraction()
    {
        bool addedSuccessfully = PlayerInventoryHolder.Instance.AddToInventory(itemForm, 1);
        if (addedSuccessfully)
        {
            HotbarDisplay display = FindObjectOfType<HotbarDisplay>();
            int i = display.FindItemInHotbar(itemForm);
            if(i != -1)
            {
                display.SelectHotbarSlot(i);
                if(currentlyLit) HandItemManager.Instance.TorchFlameToggle(true);
            }
            ParticlePoolManager.Instance.GrabSparkParticle().transform.position = transform.position;
            Destroy(this.gameObject);
        }
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        if (type == ToolType.Pyrefly && currentlyLit && !PlayerInteraction.Instance.pyreflyLit)
        {
            HandItemManager.Instance.PyreflyFlameToggle(true);
            success = true;
        }
        else success = false;
        
    }

    IEnumerator FireDrain()
    {
        currentlyLit = true;
        maxFlame = Random.Range(110, 140);
        flameLeft = maxFlame;
        if(isUpgraded) StartCoroutine(ShootFire());
        lightScript.flickerSpeed = 0.1f;
        lightScript.intensityVariation = 0.2f;
        while(flameLeft > maxFlame * 0.3f)
        {
            flameLeft--;
            yield return new WaitForSeconds(1);
        }
        //yield return new WaitForSeconds(r * 0.7f);
        lowFireParticle.Play();
        lightScript.flickerSpeed = 0.9f;
        lightScript.intensityVariation = 1f;
        //yield return new WaitForSeconds(r * 0.3f);
        while(flameLeft > 0)
        {
            flameLeft--;
            yield return new WaitForSeconds(1);
        }
        ExtinguishFlame();
    }

    IEnumerator ShootFire()
    {
        while(flameLeft > 20)
        {
            yield return new WaitForSeconds(Random.Range(4f, 12f));
            shootingFire = true;
            burnCollider.enabled = true;
            flameThrowerParticles.Play();
            audioHandler.PlaySound(audioHandler.activatedSound);
            yield return new WaitForSeconds(Random.Range(1.5f, 3f));
            shootingFire = false;
            burnCollider.enabled = false;
            flameThrowerParticles.Stop();
        }
    }

    void ExtinguishFlame()
    {
        flameLeft = 0;
        
        currentlyLit = false;
        //ParticlePoolManager.Instance.GrabExtinguishParticle().transform.position = fire.transform.position;
        fire.SetActive(false);
        //audioHandler.PlaySound(audioHandler.miscSounds1[0]);
        HandItemManager.Instance.TorchFlameToggle(false);

        if(isUpgraded)
        {
            StopCoroutine(ShootFire());
            flameThrowerParticles.Stop();
            burnCollider.enabled = false;
            shootingFire = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(!shootingFire) return;

        if(other.gameObject.layer == 10) //Player
        {
            PlayerInteraction.Instance.ApplyStatusEffect(StatusDatabase.Instance.GetStatus(StatusEffectName.Fire), 6);
            return;
        }

        var enemy = other.GetComponentInParent<CreatureBehaviorScript>();
        if (enemy != null)
        {
            if((enemy.fireVulnerable || (enemy.canCorpseBreak && enemy.health <= 0)))
            {
                int burnDuration = Random.Range(6, 10);
                if(enemy.health <= 0) burnDuration += 20;
                enemy.ApplyStatusEffect(StatusDatabase.Instance.GetStatus(StatusEffectName.Fire), burnDuration);
                return;
            }
        }

        var structure = other.GetComponent<StructureBehaviorScript>();
        if (structure != null)
        {
            if(structure.IsFlammable() && !structure.onFire && PlayerInteraction.Instance.torchLit)
            {
                structure.LitOnFire();
            }
            else if(PlayerInteraction.Instance.torchLit && !structure.IsFlammable()) structure.ToolInteraction(ToolType.Torch, out bool playAnim);
        }
    }

    public override void HitWithWater()
    {
        if(fire.activeSelf == true)
        {
            ExtinguishFlame();
        }
    }

    void OnDestroy()
    {
        base.OnDestroy();
        //if (!gameObject.scene.isLoaded) return; 
    }

    public override List<StructureUIValueGroup> GetStructureUIValues()
    {
        if(!structureUIVariables.enableUI || structureUIVariables.valueGroups.Count == 0) return null;
        structureUIVariables.valueGroups[0].value = health;
        structureUIVariables.valueGroups[0].maxValue = maxHealth;

        structureUIVariables.valueGroups[1].value = flameLeft;
        structureUIVariables.valueGroups[1].maxValue = maxFlame;
        return structureUIVariables.valueGroups;
    }

    public bool CanBeExtinguished()
    {
        if(flameLeft <= 0) return false;
        else return true;
    }

    public void ExternalExtinguish()
    {
        ExtinguishFlame();
    }
}
