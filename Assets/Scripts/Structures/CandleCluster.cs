using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandleCluster : StructureBehaviorScript, IFireHolder
{
    public FireFearTrigger fireTrigger;
    public GameObject fire;

    [HideInInspector] public bool burning = false;

    float chanceForDrain = 35;

    public CandleType type;

    public CreatureObject mothData;

    public List<RepairItem> repairItems;

    //Candles are crafted from 1 silk, 3-5 combs, and 1 nectar OR bug meat. Probably made in bulk

    void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        base.Start();
        fire.SetActive(false);
        if(TimeManager.Instance.isDay) chanceForDrain = 0;

        if(type == CandleType.Aroma) StartCoroutine(AttractMoths());
    }

    void Update()
    {
        base.Update();
    }

    public override void ItemInteraction(InventoryItemData item)
    {
        if(health >= maxHealth) return;
        foreach(RepairItem r in repairItems)
        {
            if(r.item == item)
            {
                if(maxHealth <= r.repairAmount + health) health = maxHealth;
                else health += r.repairAmount;
                HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
                PlayerInventoryHolder.Instance.UpdateInventory();

                PlayHitEffect();
                return;
            }
        }
    }


    public override void ToolInteraction(ToolType type, out bool success)
    {
        print("Interacted");
        if(type == ToolType.Torch)
        {
            print("Torch");
            if(PlayerInteraction.Instance.torchLit && !burning)
            {
                fire.SetActive(true);
                audioHandler.PlaySound(audioHandler.activatedSound);
                burning = true;
                success = true;
            }
            else if(burning && !PlayerInteraction.Instance.torchLit)
            {
                HandItemManager.Instance.TorchFlameToggle(true);
                success = true;
            }
            else success = false;
            return;
        }
        else if(type == ToolType.Shovel)
        {
            success = true;
        }
        else if(type == ToolType.WateringCan && PlayerInteraction.Instance.waterHeld > 0 && burning)
        {
            PlayerInteraction.Instance.waterHeld--;
            HitWithWater();
            success = true;
        }
        else if (type == ToolType.Pyrefly && burning && !PlayerInteraction.Instance.pyreflyLit)
        {
            HandItemManager.Instance.PyreflyFlameToggle(true);
            success = true;
        }
        else success = false;
        
    }

    public override void HourPassed()
    {
        if(!burning) return;
        if(Random.Range(0,100) < chanceForDrain)
        {
            chanceForDrain = 45;
            health--;

            if(Random.Range(0,10) > 6) ExtinguishFlame();
        }
        else chanceForDrain += 45;
    }

    public override void HitWithWater()
    {
        ExtinguishFlame();
    }

    IEnumerator AttractMoths()
    {
        while(health > 0)
        {
            yield return new WaitForSeconds(10);
            if(burning == false) continue;

            Collider[] hitEnemies = Physics.OverlapSphere(transform.position, 50, 1 << 9);
            foreach(Collider collider in hitEnemies)
            {
                var creature = collider.GetComponentInParent<CreatureBehaviorScript>();
                if (creature != null && mothData == creature.creatureData)
                {
                    creature.NewPriorityTarget(this);
                }
            }
        }
    }


    void ExtinguishFlame()
    {
        if(!burning) return;
        ParticlePoolManager.Instance.GrabExtinguishParticle().transform.position = fire.transform.position;
        fire.SetActive(false);
        audioHandler.PlaySound(audioHandler.miscSounds1[0]);
        burning = false;
        chanceForDrain = 25;
    }

    public bool CanBeExtinguished()
    {
        if(!burning) return false;
        else return true;
    }

    public void ExternalExtinguish()
    {
        ExtinguishFlame();
    }
}

public enum CandleType
{
    Default,
    Aroma
}
