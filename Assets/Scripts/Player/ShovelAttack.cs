using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShovelAttack : MonoBehaviour
{
    //public LayerMask hitDetection;
    public Collider collider;

    public AudioClip hitStruct, hitHay, hitFlesh, hitDirt;

    CreatureBehaviorScript hitCreature;
    StructureBehaviorScript hitStructure;
    CreatureArmor hitArmor;
    BugBehaviorScript hitBug;

    Vector3 c_Collision, s_Collision, d_Collision;
    GroundType type;

    [HideInInspector] public bool chargedSwing = false;

    void Start()
    {
        collider.enabled = false;
    }
    
    public IEnumerator Swing()
    {
        hitCreature = null;
        hitStructure = null;
        hitArmor = null;
        hitBug = null;
        collider.enabled = true;
        Physics.SyncTransforms();
        d_Collision = new Vector3(0,0,0);
        s_Collision = Vector3.zero;
        c_Collision = Vector3.zero;
        yield return new WaitForSeconds(0.04f);
        Physics.SyncTransforms();
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
        if (structure != null && (s_Collision == Vector3.zero || Vector3.Distance(transform.position, s_Collision) > Vector3.Distance(transform.position, other.ClosestPoint(transform.position))))
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

        var bug = other.GetComponentInParent<BugBehaviorScript>();
        if (bug != null && hitBug == null)
        {
            hitBug = bug;
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
        if(other.GetComponentInParent<NPC>() || other.gameObject.layer == 12 || other.gameObject.layer == 15 || other.GetComponentInParent<PetBehaviorScript>()) return; //Add exception to grub
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
            float damage = 2;
            if(chargedSwing) damage = 5;
            hitArmor.TakeDamage(damage);
            HandItemManager.Instance.toolSource.PlayOneShot(hitStruct);
            //print("Hit Armor");
            //if(PlayerInteraction.Instance.stamina > 50) PlayerInteraction.Instance.StaminaChange(-1);

            //PlayHitParticle(s_Collision);
            return;
        }

        if(hitCreature)
        {
            float damage = 25;
            if(chargedSwing) damage = 50;
            hitCreature.TakeDamage(damage, PlayerInteraction.Instance.transform.position);
            //playsound
            if(hitCreature.corpseType != CorpseParticleType.Metal) HandItemManager.Instance.toolSource.PlayOneShot(hitFlesh);
            //print("Hit Creature");
            //if(PlayerInteraction.Instance.stamina > 50) PlayerInteraction.Instance.StaminaChange(-1);

            //PlayHitParticle(c_Collision);
            ParticlePoolManager.Instance.MoveAndPlayVFX(c_Collision, ParticlePoolManager.Instance.hitEffect);
            hitCreature.PlayHitParticle(c_Collision);
            ParticlePoolManager.Instance.GrabOrangeHitParticle().transform.position = c_Collision;

            if(hitCreature && hitCreature.health > 0) PlayerInteraction.Instance.InvokeEnemyHitEvent(hitCreature);
            return;
        }

        if(hitStructure)
        {
            hitStructure.TakeDamage(2);
            if(hitStructure.structData.structureType == StructureType.Null || hitStructure.structData.structureType == StructureType.Hay || hitStructure.structData.structureType == StructureType.CorruptedFlesh) 
            HandItemManager.Instance.toolSource.PlayOneShot(hitHay);
            else HandItemManager.Instance.toolSource.PlayOneShot(hitStruct);
            //print("Hit Structure");
            //if(PlayerInteraction.Instance.stamina > 50) PlayerInteraction.Instance.StaminaChange(-1);

            PlayHitParticle(s_Collision);
            return;
        }

        if(hitBug)
        {
            hitBug.Struck();
            return; //To stop hitting the floor after striking bug
        }

        if(d_Collision != new Vector3(0,0,0))
        {
            //print("Hit default");

            PlayHitParticle(d_Collision);
            //HandItemManager.Instance.toolSource.PlayOneShot(hitStruct);
            //return;

            if(type == GroundType.Dirt)
            {
                //print("Hit dirt");
                ParticlePoolManager.Instance.MoveAndPlayParticle(d_Collision, ParticlePoolManager.Instance.dirtParticle);
                HandItemManager.Instance.toolSource.PlayOneShot(hitDirt);
            }
            else
            {
                //print("Hit default");
                PlayHitParticle(d_Collision);
                HandItemManager.Instance.toolSource.PlayOneShot(hitStruct);
            }
        }
    }


    void PlayHitParticle(Vector3 hitPoint)
    {
        //print("Played");
        ParticlePoolManager.Instance.GrabImpactParticle().transform.position = hitPoint;
        ParticlePoolManager.Instance.MoveAndPlayVFX(hitPoint, ParticlePoolManager.Instance.hitEffect);
        ParticlePoolManager.Instance.GrabOrangeHitParticle().transform.position = hitPoint;
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
