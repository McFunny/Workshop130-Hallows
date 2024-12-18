using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShovelAttack : MonoBehaviour
{
    public LayerMask hitDetection;
    public Collider collider;

    public AudioClip hitStruct, hitFlesh;

    void Start()
    {
        collider.enabled = false;
    }
    
    public IEnumerator Swing()
    {
        collider.enabled = true;
        yield return new WaitForSeconds(0.1f);
        collider.enabled = false;
    }

    void OnTriggerEnter(Collider other)
    {
        Vector3 collisionPoint;

        var structure = other.GetComponent<StructureBehaviorScript>();
        if (structure != null)
        {
            structure.TakeDamage(2);
            HandItemManager.Instance.toolSource.PlayOneShot(hitStruct);
            print("Hit Structure");
            PlayerInteraction.Instance.StaminaChange(-1);
            collider.enabled = false;
            collisionPoint = other.ClosestPoint(transform.position);
            PlayHitParticle(collisionPoint);
        }

        var creature = other.GetComponentInParent<CreatureBehaviorScript>();
        if (creature != null && creature.shovelVulnerable)
        {
            creature.TakeDamage(25);
            //playsound
            HandItemManager.Instance.toolSource.PlayOneShot(hitFlesh);
            print("Hit Creature");
            PlayerInteraction.Instance.StaminaChange(-1);
            collider.enabled = false;
            collisionPoint = other.ClosestPoint(transform.position);
            PlayHitParticle(collisionPoint);
            if(creature.corpseType == CorpseParticleType.Red) 
            {
                GameObject bloodParticle = ParticlePoolManager.Instance.GrabBloodDropParticle();
                bloodParticle.transform.position = transform.position;
            }
        }

        //Something to hit corpses

        
    }

    void PlayHitParticle(Vector3 hitPoint)
    {
        print("Played");
        ParticlePoolManager.Instance.MoveAndPlayVFX(hitPoint, ParticlePoolManager.Instance.hitEffect);
        return;
        Vector3 direction = (transform.position - hitPoint).normalized;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, 10, hitDetection))
        {
            ParticlePoolManager.Instance.MoveAndPlayVFX(hit.point, ParticlePoolManager.Instance.hitEffect);
            print("Played");
        }
    }
}
