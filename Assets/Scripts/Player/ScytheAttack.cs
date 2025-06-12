using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScytheAttack : MonoBehaviour
{
    /*public LayerMask hitDetection;
    public Collider collider;

    public AudioClip hitPlant, hitFlesh, hitGround;

    bool cancelSwing; //Happens when the player hits a hard thing


    List<CreatureBehaviorScript> hitCreatures = new List<CreatureBehaviorScript>();
    List<FarmLand> hitCrops = new List<FarmLand>();
    //List<StructureBehaviorScript> hitStructures = new List<StructureBehaviorScript>();
    CreatureArmor hitArmor;

    Vector3 c_Collision, s_Collision, d_Collision;

    void Start()
    {
        collider.enabled = false;
    }
    
    public IEnumerator Swing()
    {
        cancelSwing = false;

        hitCreatures.Clear();
        hitStructures.Clear();
        hitArmor = null;
        collider.enabled = true;
        d_Collision = new Vector3(0,0,0);
        yield return new WaitForSeconds(0.02f);
        collider.enabled = false;
        HitObjects();

        //yield return new WaitForSeconds(0.1f);
        //PlayerMovement.limitMaxVelocity = true;
        //PlayerMovement.ignoreMovementInputs = false;
    }

    void OnTriggerEnter(Collider other)
    {
        //Vector3 collisionPoint;

        if(cancelSwing) return;

        var structure = other.GetComponentInParent<StructureBehaviorScript>();
        if (structure != null)
        {
            //if not farmland, hand it recoil
            hitStructures.Add(structure);
            s_Collision = other.ClosestPoint(transform.position);
        }

        var creature = other.GetComponentInParent<CreatureBehaviorScript>();
        if (creature != null && creature.shovelVulnerable)
        {
            hitCreatures.Add(creature);
            c_Collision = other.ClosestPoint(transform.position);
        }

        var creatureArmor = other.GetComponentInParent<CreatureArmor>();
        if (creatureArmor != null && hitArmor == null)
        {
            //Cancel the swing
            hitArmor = creatureArmor;
        }

        if (other.gameObject.layer == 17)
        {
            //Cancel the swing

            Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
            Vector3 contactPoint = other.ClosestPoint(transform.position);
            Vector3 rawDirection = other.transform.position - PlayerInteraction.Instance.transform.position;
            rawDirection.y = 0;
            Vector3 forceDir = rawDirection.normalized;

            float forceStrength = 25f;
            rb.AddForceAtPosition(forceDir * forceStrength, contactPoint, ForceMode.Impulse);
        }

        //it hit default collider
        if(other.GetComponentInParent<NPC>() || other.gameObject.layer == 12 || other.gameObject.layer == 15) return;
        if(d_Collision == new Vector3(0,0,0))
        {
            //Cancel the swing

            d_Collision = other.ClosestPoint(transform.position);
            if(other.gameObject.tag == "Grass_FootStepSurface") type = GroundType.Dirt;
            else type = GroundType.Other;
        }

        
    }

    void HitObjects()
    {
        if(cancelSwing) return;

        for(int i = 0; i < hitCreatures.Count; i++)
        {
            if(hitCreatures[i] == null) continue;
            hitCreatures[i].TakeDamage(25);
            HandItemManager.Instance.toolSource.PlayOneShot(hitFlesh);

            ParticlePoolManager.Instance.MoveAndPlayVFX(hitCreatures[i].ClosestPoint(transform.position), ParticlePoolManager.Instance.hitEffect);
            hitCreatures[i].PlayHitParticle(hitCreatures[i].ClosestPoint(transform.position));
        }

        for(int i = 0; i < hitCrops.Count; i++)
        {
            if(hitCrops[i] == null) continue;
            //Harvest grown
            HandItemManager.Instance.toolSource.PlayOneShot(hitPlant);
        }


        if(hitArmor)
        {
            hitArmor.TakeDamage(2);
            HandItemManager.Instance.toolSource.PlayOneShot(hitPlant);
            print("Hit Armor");
            //if(PlayerInteraction.Instance.stamina > 50) PlayerInteraction.Instance.StaminaChange(-1);

            //PlayHitParticle(s_Collision);
            return;
        }

        if(d_Collision != new Vector3(0,0,0))
        {
            print("Hit default");

            PlayHitParticle(d_Collision);
            HandItemManager.Instance.toolSource.PlayOneShot(hitPlant);
        }
    }


    void PlayHitParticle(Vector3 hitPoint)
    {
        print("Played");
        ParticlePoolManager.Instance.GrabImpactParticle().transform.position = transform.position;
        return;
    }
    */
}
