using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NauseaFungus : StructureBehaviorScript
{
    public ParticleSystem sporeParticles;
    public GameObject deathSporeParticles; // unparent this and enable it, have timer on it to destroy it
    //Chance to spawn on a corpse on a tile when over 5 corpses are present. Passively gives nausea, on hit gives blindness. No ichor or item drop
    void Start()
    {
        OnDamage += Damaged;
    }

    void Damaged()
    {
        sporeParticles.Play();
        if(Vector3.Distance(transform.position, PlayerInteraction.Instance.transform.position) < 8.1f)
        {
            PlayerInteraction.Instance.ApplyStatusEffect(StatusDatabase.Instance.GetStatus(StatusEffectName.Blindness), Random.Range(8, 18));
        }
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, 8f, 1 << 9);
        foreach(Collider collider in hitEnemies)
        {
            var creature = collider.GetComponentInParent<CreatureBehaviorScript>();
            if (creature != null && creature.shovelVulnerable)
            {
                if(creature.fireVulnerable) creature.ApplyStatusEffect(StatusDatabase.Instance.GetStatus(StatusEffectName.Blindness), Random.Range(5, 15));
            }
        }
    }

    void OnDestroy()
    {
        OnDamage -= Damaged;
        base.OnDestroy();
        if(!gameObject.scene.isLoaded) return;
        deathSporeParticles.SetActive(true);
        deathSporeParticles.transform.parent = null;
    }
}
