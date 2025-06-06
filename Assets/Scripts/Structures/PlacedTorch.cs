using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacedTorch : StructureBehaviorScript
{

    //public FireFearTrigger fireTrigger;
    public GameObject fire;

    bool currentlyLit;

    public LightController lightScript;

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

    IEnumerator FireDrain()
    {
        currentlyLit = true;
        float r = Random.Range(50, 70);
        lightScript.flickerSpeed = 0.1f;
        lightScript.intensityVariation = 0.2f;
        yield return new WaitForSeconds(r * 0.7f);
        lightScript.flickerSpeed = 0.9f;
        lightScript.intensityVariation = 1f;
        yield return new WaitForSeconds(r * 0.3f);
        ExtinguishFlame();
    }

    void ExtinguishFlame()
    {
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
}
