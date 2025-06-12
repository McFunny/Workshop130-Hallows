using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScytheAttack : MonoBehaviour
{
    public LayerMask hitDetection;
    public Collider collider;

    public AudioClip hitPlant, hitFlesh, hitGround, hitHardObject;

    bool cancelSwing; //Happens when the player hits a hard thing


    List<CreatureBehaviorScript> hitCreatures = new List<CreatureBehaviorScript>();
    List<FarmLand> hitCrops = new List<FarmLand>();
    //List<StructureBehaviorScript> hitStructures = new List<StructureBehaviorScript>();
    CreatureArmor hitArmor;

    void Start()
    {
        collider.enabled = false;
    }
    
    public IEnumerator Swing()
    {
        cancelSwing = false;

        hitCreatures.Clear();
        hitCrops.Clear();
        hitArmor = null;
        collider.enabled = true;
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
            FarmLand crop = structure as FarmLand;
            //if not farmland, hand it recoil
            if(crop)
            {
                if(hitCrops.Contains(crop)) return;
                hitCrops.Add(crop);
                return;
            }

            structure.TakeDamage(1);
            ParticlePoolManager.Instance.MoveAndPlayVFX(other.ClosestPoint(transform.position), ParticlePoolManager.Instance.hitEffect);
            cancelSwing = true;
            return;
        }

        var creature = other.GetComponentInParent<CreatureBehaviorScript>();
        if (creature != null && creature.shovelVulnerable)
        {
            if(hitCreatures.Contains(creature)) return;
            hitCreatures.Add(creature);
        }

        var creatureArmor = other.GetComponentInParent<CreatureArmor>();
        if (creatureArmor != null)
        {
            cancelSwing = true;
            hitArmor.TakeDamage(2);
            HandItemManager.Instance.toolSource.PlayOneShot(hitHardObject);
            ParticlePoolManager.Instance.MoveAndPlayVFX(other.ClosestPoint(transform.position), ParticlePoolManager.Instance.hitEffect);
            return;
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
        
        /*
        cancelSwing = true;
        return;
        */
        
    }

    void HitObjects()
    {
        if(cancelSwing)
        {
            HandItemManager.Instance.PlaySecondaryAnimation(); //PlayRecoil
            return;
        }

        if(hitCreatures.Count == 0 && hitCrops.Count == 0) return;

        if(PlayerInteraction.Instance.stamina > 50) PlayerInteraction.Instance.StaminaChange(-2);

        for(int i = 0; i < hitCreatures.Count; i++)
        {
            if(hitCreatures[i] == null) continue;
            hitCreatures[i].TakeDamage(35);
            HandItemManager.Instance.toolSource.PlayOneShot(hitFlesh);

            ParticlePoolManager.Instance.MoveAndPlayVFX(hitCreatures[i].GetComponent<Collider>().ClosestPoint(transform.position), ParticlePoolManager.Instance.hitEffect);
            hitCreatures[i].PlayHitParticle(hitCreatures[i].GetComponent<Collider>().ClosestPoint(transform.position));
        }

        for(int i = 0; i < hitCrops.Count; i++)
        {
            if(hitCrops[i] == null) continue;
            //Harvest grown
            hitCrops[i].ToolInteraction(ToolType.Scythe, out bool success);
            if(success) HandItemManager.Instance.toolSource.PlayOneShot(hitPlant);
        }
    }


    void PlayHitParticle(Vector3 hitPoint)
    {
        print("Played");
        ParticlePoolManager.Instance.GrabImpactParticle().transform.position = transform.position;
        return;
    }
}
