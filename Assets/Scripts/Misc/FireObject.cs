using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireObject : MonoBehaviour
{
    //public Collider burnBox; //collider that damages enemies and players that wander into it

    public AudioClip extinguishedSFX;

    bool active = false;

    //float playerDamage = 6;
    //float creatureDamage = 5;

    void OnEnable()
    {
        StartCoroutine(FireSpread());
        active = true;
    }

    void OnDisable()
    {
        if(!gameObject.scene.isLoaded || !active) return;
        Extinguished();
    }

    public void Extinguished()
    {
        ParticlePoolManager.Instance.GrabExtinguishParticle().transform.position = transform.position;
        AudioPoolManager.Instance.PlayClipAtPosition(extinguishedSFX, transform.position);
        StopAllCoroutines();
        active = false;
    }

    /*void OnTriggerEnter(Collider other) //Burn things that come into contact
    {
        var player = other.GetComponent<PlayerInteraction>();
        if (player != null)
        {
            player.StaminaChange(-playerDamage);
            if(Random.Range(0,4) > 0) player.ApplyStatusEffect(StatusDatabase.Instance.GetStatus(StatusEffectName.Fire), 4);
        }

        var creature = other.GetComponentInParent<CreatureBehaviorScript>();
        if (creature != null && creature.shovelVulnerable && creature.fireVulnerable)
        {
            creature.TakeDamage(creatureDamage);
            if(Random.Range(0,4) > 0) creature.ApplyStatusEffect(StatusDatabase.Instance.GetStatus(StatusEffectName.Fire), 5);

            creature.PlayHitParticle(new Vector3(0, 0, 0));
        }
    }*/

    IEnumerator FireSpread()
    {
        int burnTimer = 0;
        List<StructureBehaviorScript> nearbyStructs = new List<StructureBehaviorScript>();
        while(gameObject.activeSelf)
        {
            yield return new WaitForSeconds(1f);
            burnTimer++;

            if(burnTimer >= 3)
            {
                Collider[] hitColliders = Physics.OverlapSphere(transform.position, 3f);
                foreach(Collider collider in hitColliders)
                {
                    StructureBehaviorScript newStruct = collider.gameObject.GetComponentInParent<StructureBehaviorScript>();
                    if(newStruct && !nearbyStructs.Contains(newStruct))
                    {
                        nearbyStructs.Add(newStruct);
                        continue;
                    }
                    
                    MurderMancer mancer = collider.gameObject.GetComponentInParent<MurderMancer>();
                    if(mancer)
                    {
                        mancer.IgnitedByOther();
                        continue;
                    }

                    CreatureBehaviorScript creature = collider.gameObject.GetComponentInParent<CreatureBehaviorScript>();
                    if(creature && creature.fireVulnerable && TrinketInventoryHandler.Instance.CheckForTrinket(TrinketKey.CarrionCooker) && !StatusEffectManager.Instance.FindStatusOnCreature(StatusEffectName.Fire, creature))
                    {
                        int r = Random.Range(0,10);
                        if(r > 2) creature.ApplyStatusEffect(StatusDatabase.Instance.GetStatus(StatusEffectName.Fire), Random.Range(5, 10));
                    }
                }

                foreach(StructureBehaviorScript structure in nearbyStructs)
                {
                    if(structure && structure.IsFlammable() && !structure.onFire)
                    {
                        int r = Random.Range(0,10);
                        if(r > 3) structure.LitOnFire();
                        break;
                    }
                }
                burnTimer = 0;
                nearbyStructs.Clear();
            }
        }
    }
}
