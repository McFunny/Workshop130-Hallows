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

    void ExtinguishFlame()
    {
        flameLeft = 0;
        
        currentlyLit = false;
        //ParticlePoolManager.Instance.GrabExtinguishParticle().transform.position = fire.transform.position;
        fire.SetActive(false);
        //audioHandler.PlaySound(audioHandler.miscSounds1[0]);
        HandItemManager.Instance.TorchFlameToggle(false);
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
