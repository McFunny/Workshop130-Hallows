using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeatPile : StructureBehaviorScript
{
    public List<CreatureObject> attractableCreatures;
    public List<GameObject> stageObjects;

    public int stage;

    public List<RepairItem> repairItems;

    float range;

    //public ParticleSystem meatDamageParticle;

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        StartCoroutine(AttractCreatures());
        OnDamage += PileHit;
        PileHit();
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
                PileHit();
                audioHandler.PlaySound(audioHandler.interactSound);
                return;
            }
        }
    }

    void PileHit()
    {
        int newStage = 0;
        if(health > maxHealth * .75f) newStage = 4;
        else if(health > maxHealth * 0.50f) newStage = 3;
        else if(health > maxHealth * 0.25f) newStage = 2;
        else newStage = 1;

        switch(newStage)
        {
            case 4:
            for(int i = 0; i < stageObjects.Count; i++) stageObjects[i].SetActive(true);
            break;
            case 3:
            for(int i = 0; i < stageObjects.Count; i++)
            {
                if(i < 3) stageObjects[i].SetActive(true);
                else stageObjects[i].SetActive(false);
            }
            break;
            case 2:
            for(int i = 0; i < stageObjects.Count; i++)
            {
                if(i < 2) stageObjects[i].SetActive(true);
                else stageObjects[i].SetActive(false);
            }
            break;
            case 1:
            for(int i = 0; i < stageObjects.Count; i++)
            {
                if(i < 1) stageObjects[i].SetActive(true);
                else stageObjects[i].SetActive(false);
            }
            break;
        }

        if(stage > newStage)
        {
            stage = newStage;
            audioHandler.PlaySound(audioHandler.breakSound);
        }

    }

    protected override float ApplyDamageModifier(float damage)
    {
        if(damage < 20) damage = 2;
        else damage = damage / 3; //This averages out how long the meat decoy keeps enemies distracted

        return damage;
    }

    IEnumerator AttractCreatures()
    {
        while(gameObject.activeSelf)
        {
            yield return new WaitForSeconds(Random.Range(5, 10));
            Collider[] nearbyCreatures = Physics.OverlapSphere(transform.position, range, 1 << 9);
            foreach(Collider collider in nearbyCreatures)
            {
                CreatureBehaviorScript c = collider.gameObject.GetComponentInParent<CreatureBehaviorScript>();

                if(c && attractableCreatures.Contains(c.creatureData) && Random.Range(0,10) > 5) c.NewPriorityTarget(this);
            }
        }
    }

    void OnDestroy()
    {
        OnDamage -= PileHit;
        //if(health)
        base.OnDestroy();
    }
}
