using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveBarrel : StructureBehaviorScript
{
    ///
    /// 
    void ExplosionLogic()
    {
        ParticlePoolManager.Instance.GrabExplosionParticle().transform.position = particleCenter.position;

        ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);
        ParticlePoolManager.Instance.GrabDirtPixelParticle().transform.position = transform.position;
        
        if(Vector3.Distance(transform.position, PlayerInteraction.Instance.transform.position) < 5f)
        {
            PlayerInteraction.Instance.StaminaChange(-65);
            PlayerInteraction.Instance.PlayerTrip();
        }
        Collider[] hitStructures = Physics.OverlapSphere(transform.position, 5, 1 << 6);
        foreach(Collider collider in hitStructures)
        {
            StructureBehaviorScript structure = collider.gameObject.GetComponentInParent<StructureBehaviorScript>();
            if(structure && structure != this)
            {
                structure.TakeDamage(20);
            }
        }

        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, 5.5f, 1 << 9);
        foreach(Collider collider in hitEnemies)
        {
            var creature = collider.GetComponentInParent<CreatureBehaviorScript>();
            if (creature != null && creature.shovelVulnerable)
            {
                creature.lastDamageTypeTaken = DamageType.Mine;
                creature.TakeDamage(100);
                creature.PlayHitParticle(new Vector3(transform.position.x, transform.position.y, transform.position.z));
            }
        }
    }

    public void OnDestroy()
    {
        base.OnDestroy();
        if(!gameObject.scene.isLoaded) return;
        ExplosionLogic();
    }
}
