using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brazier : StructureBehaviorScript
{
    //public InventoryItemData recoveredItem;

    public FireFearTrigger fireTrigger;
    public GameObject fire;

    public float flameLeft; //if 0, fire is gone
    float maxFlame = 20;

    //Rework to incorporate a fuel based system rather than static time.

    void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        base.Start();
        fireTrigger.OnScare += EnemyScaredByFire;
        //StartCoroutine(FireDrain()); //Gonna see how this is without the passive fire drain
        flameLeft = 0;
        fire.SetActive(false);
    }

    void Update()
    {
        base.Update();
    }

    public override void StructureInteraction()
    {
        return;
        if(flameLeft == 0)
        {
            flameLeft = maxFlame;
            fire.SetActive(true);
            audioHandler.PlaySound(audioHandler.activatedSound);
        }
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        print("Interacted");
        if(type == ToolType.Torch)
        {
            print("Torch");
            if(PlayerInteraction.Instance.torchLit && flameLeft <= 0)
            {
                flameLeft = maxFlame;
                fire.SetActive(true);
                audioHandler.PlaySound(audioHandler.activatedSound);
                success = true;
            }
            else if(flameLeft > 0 && !PlayerInteraction.Instance.torchLit)
            {
                HandItemManager.Instance.TorchFlameToggle(true);
                success = true;
            }
            else success = false;
            return;
        }
        else if(type == ToolType.Shovel)
        {
            //StartCoroutine(DugUpForItem());
            success = true;
        }
        else if(type == ToolType.WateringCan && PlayerInteraction.Instance.waterHeld > 0 && flameLeft > 0)
        {
            PlayerInteraction.Instance.waterHeld--;
            HitWithWater();
            success = true;
        }
        else success = false;
        
    }

    public override void HitWithWater()
    {
        if(flameLeft <= 0) return;
        flameLeft = 0;
        ExtinguishFlame();
    }

    /*IEnumerator DugUp()
    {
        yield return  new WaitForSeconds(1);
        GameObject droppedItem = ItemPoolManager.Instance.GrabItem(recoveredItem);
        droppedItem.transform.position = transform.position;
        Destroy(this.gameObject);
        
    }*/

    IEnumerator FireDrain()
    {
        int r;
        while(gameObject.activeSelf)
        {
            r = Random.Range(10, 20);
            yield return new WaitForSeconds(r);
            flameLeft -= 1;
            if(flameLeft < 0) flameLeft = 0;
            if(flameLeft == 0 && fire.activeSelf)
            {
                ExtinguishFlame();
            }
        }
    }

    void ExtinguishFlame()
    {
        ParticlePoolManager.Instance.GrabExtinguishParticle().transform.position = fire.transform.position;
        fire.SetActive(false);
        audioHandler.PlaySound(audioHandler.miscSounds1[0]);
    }

    void OnDestroy()
    {
        fireTrigger.OnScare -= EnemyScaredByFire;
        base.OnDestroy();
        //if (!gameObject.scene.isLoaded) return; 
    }

    void EnemyScaredByFire(bool successful)
    {
        if(flameLeft <= 0 || !successful) return;
        flameLeft -= Random.Range(1,3);
        if(flameLeft <= 0) ExtinguishFlame();
    }
}
