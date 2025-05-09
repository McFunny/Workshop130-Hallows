using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShovelAttack : MonoBehaviour
{
    public LayerMask hitDetection;
    public Collider collider;

    public AudioClip hitStruct, hitFlesh, hitDirt;

    CreatureBehaviorScript hitCreature;
    StructureBehaviorScript hitStructure;
    CreatureArmor hitArmor;

    Vector3 c_Collision, s_Collision, d_Collision;
    GroundType type;

    void Start()
    {
        collider.enabled = false;
    }
    
    public IEnumerator Swing()
    {
        hitCreature = null;
        hitStructure = null;
        hitArmor = null;
        collider.enabled = true;
        d_Collision = new Vector3(0,0,0);
        yield return new WaitForSeconds(0.04f);
        collider.enabled = false;
        HitObject();

        //yield return new WaitForSeconds(0.1f);
        //PlayerMovement.limitMaxVelocity = true;
        //PlayerMovement.ignoreMovementInputs = false;
    }

    void OnTriggerEnter(Collider other)
    {
        //Vector3 collisionPoint;

        var structure = other.GetComponentInParent<StructureBehaviorScript>();
        if (structure != null && hitStructure == null && (s_Collision == Vector3.zero || Vector3.Distance(transform.position, s_Collision) > Vector3.Distance(transform.position, other.ClosestPoint(transform.position))))
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

        var creatureArmor = other.GetComponentInParent<CreatureArmor>();
        if (creatureArmor != null && hitArmor == null)
        {
            hitArmor = creatureArmor;
        }

        if (other.gameObject.layer == 17)
        {
            Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
            Vector3 contactPoint = other.ClosestPoint(transform.position);
            Vector3 rawDirection = other.transform.position - PlayerInteraction.Instance.transform.position;
            rawDirection.y = 0;
            Vector3 forceDir = rawDirection.normalized;

            float forceStrength = 25f;
            rb.AddForceAtPosition(forceDir * forceStrength, contactPoint, ForceMode.Impulse);
        }

        //it hit default collider
        if(other.GetComponentInParent<NPC>()) return;
        if(d_Collision == new Vector3(0,0,0))
        {
            d_Collision = other.ClosestPoint(transform.position);
            if(other.gameObject.tag == "Grass_FootStepSurface") type = GroundType.Dirt;
            else type = GroundType.Other;
        }

        //Something to hit corpses

        
    }

    void HitObject()
    {
        if(hitArmor)
        {
            hitArmor.TakeDamage(2);
            HandItemManager.Instance.toolSource.PlayOneShot(hitStruct);
            print("Hit Armor");
            //if(PlayerInteraction.Instance.stamina > 50) PlayerInteraction.Instance.StaminaChange(-1);

            //PlayHitParticle(s_Collision);
            return;
        }

        if(hitCreature)
        {
            hitCreature.TakeDamage(25);
            //playsound
            HandItemManager.Instance.toolSource.PlayOneShot(hitFlesh);
            print("Hit Creature");
            //if(PlayerInteraction.Instance.stamina > 50) PlayerInteraction.Instance.StaminaChange(-1);

            //PlayHitParticle(c_Collision);
            ParticlePoolManager.Instance.MoveAndPlayVFX(c_Collision, ParticlePoolManager.Instance.hitEffect);
            hitCreature.PlayHitParticle(c_Collision);
            return;
        }

        if(hitStructure)
        {
            hitStructure.TakeDamage(2);
            HandItemManager.Instance.toolSource.PlayOneShot(hitStruct);
            print("Hit Structure");
            if(PlayerInteraction.Instance.stamina > 50) PlayerInteraction.Instance.StaminaChange(-1);

            PlayHitParticle(s_Collision);
        }

        if(d_Collision != new Vector3(0,0,0))
        {
            print("Hit default");

            PlayHitParticle(d_Collision);
            HandItemManager.Instance.toolSource.PlayOneShot(hitStruct);
            return;

            if(type == GroundType.Dirt)
            {
                ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);
                HandItemManager.Instance.toolSource.PlayOneShot(hitStruct);
            }
            else
            {
                PlayHitParticle(d_Collision);
                HandItemManager.Instance.toolSource.PlayOneShot(hitStruct);
            }
        }
    }


    void PlayHitParticle(Vector3 hitPoint)
    {
        print("Played");
        ParticlePoolManager.Instance.GrabImpactParticle().transform.position = transform.position;
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

public enum GroundType
{
    Other,
    Dirt
}
