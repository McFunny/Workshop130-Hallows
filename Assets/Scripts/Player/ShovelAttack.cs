using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShovelAttack : MonoBehaviour
{
    public LayerMask hitDetection;
    public Collider collider;

    public AudioClip hitStruct, hitFlesh;

    CreatureBehaviorScript hitCreature;
    StructureBehaviorScript hitStructure;

    Vector3 c_Collision, s_Collision;

    void Start()
    {
        collider.enabled = false;
    }
    
    public IEnumerator Swing()
    {
        hitCreature = null;
        hitStructure = null;
        collider.enabled = true;
        yield return new WaitForSeconds(0.04f);
        collider.enabled = false;
        HitObject();
    }

    void OnTriggerEnter(Collider other)
    {
        //Vector3 collisionPoint;

        var structure = other.GetComponent<StructureBehaviorScript>();
        if (structure != null && hitStructure == null)
        {
            hitStructure = structure;
            s_Collision = other.ClosestPoint(transform.position);
        }

        var creature = other.GetComponentInParent<CreatureBehaviorScript>();
        if (creature != null && creature.shovelVulnerable && hitCreature == null)
        {
            hitCreature = creature;
            c_Collision = other.ClosestPoint(transform.position);
        }

        //Something to hit corpses

        
    }

    void HitObject()
    {
        if(hitCreature)
        {
            hitCreature.TakeDamage(25);
            //playsound
            HandItemManager.Instance.toolSource.PlayOneShot(hitFlesh);
            print("Hit Creature");
            if(PlayerInteraction.Instance.stamina > 50) PlayerInteraction.Instance.StaminaChange(-1);
            //collider.enabled = false;
            //collisionPoint = other.ClosestPoint(transform.position);
            PlayHitParticle(c_Collision);
            hitCreature.PlayHitParticle(new Vector3(transform.position.x, transform.position.y, transform.position.z));
            return;
        }

        if(hitStructure)
        {
            hitStructure.TakeDamage(2);
            HandItemManager.Instance.toolSource.PlayOneShot(hitStruct);
            print("Hit Structure");
            if(PlayerInteraction.Instance.stamina > 50) PlayerInteraction.Instance.StaminaChange(-1);
            //collider.enabled = false;
            //collisionPoint = other.ClosestPoint(transform.position);
            PlayHitParticle(s_Collision);
        }
    }


    void PlayHitParticle(Vector3 hitPoint)
    {
        print("Played");
        ParticlePoolManager.Instance.MoveAndPlayVFX(hitPoint, ParticlePoolManager.Instance.hitEffect);
        return;
        /*
        Vector3 direction = (transform.position - hitPoint).normalized;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, 20, hitDetection))
        {
            ParticlePoolManager.Instance.MoveAndPlayVFX(hit.point, ParticlePoolManager.Instance.hitEffect);
            print("Played Success");
        }
        */
    }
}
