using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacedTorch : StructureBehaviorScript
{
    public InventoryItemData recoveredItem;

    //public FireFearTrigger fireTrigger;
    public GameObject fire;

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
    }

    public override void StructureInteraction()
    {
        bool addedSuccessfully = PlayerInventoryHolder.Instance.AddToInventory(recoveredItem, 1);
        if (addedSuccessfully)
        {
            Destroy(this.gameObject);
        }
    }

    IEnumerator FireDrain()
    {
        float r = Random.Range(5, 10);
        yield return new WaitForSeconds(r);
        ExtinguishFlame();
    }

    void ExtinguishFlame()
    {
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
