using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KukriAttack : MonoBehaviour
{
    //public LayerMask hitDetection;
    public Collider collider;

    public AudioClip hitStruct, hitHay, hitFlesh, hitDirt;

    CreatureBehaviorScript hitCreature;
    StructureBehaviorScript hitStructure;
    CreatureArmor hitArmor;
    BugBehaviorScript hitBug;
    NPC hitNPC;

    Vector3 c_Collision, s_Collision, d_Collision, n_Collision;
    GroundType type;

    float creatureDamage = 10;

    [HideInInspector] public bool chargedSwing = false;
    bool cancelSwing = false;

    void Start()
    {
        collider.enabled = false;
    }
    
    public IEnumerator Swing(int swingCount)
    {
        hitCreature = null;
        hitStructure = null;
        hitArmor = null;
        hitBug = null;
        hitNPC = null;
        cancelSwing = false;
        collider.enabled = true;
        Physics.SyncTransforms();
        d_Collision = new Vector3(0,0,0);
        s_Collision = Vector3.zero;
        c_Collision = Vector3.zero;
        n_Collision = Vector3.zero;
        yield return new WaitForSeconds(0.03f);
        Physics.SyncTransforms();
        collider.enabled = false;
        if(swingCount >= 3) creatureDamage = 20;
        else creatureDamage = 10;
        HitObject();
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
        if(other.gameObject.layer == 12 || other.gameObject.layer == 15) return; 

        NPC npc = other.GetComponent<NPC>();
        if(npc != null && hitNPC == null && !npc.cannotBeStruck)
        {
            hitNPC = npc;
            n_Collision = other.ClosestPoint(transform.position);
            return;
        }

        
        PetBehaviorScript petHit = other.GetComponentInParent<PetBehaviorScript>();
        if(petHit)
        {
            PyreGrub grub = petHit as PyreGrub;
            if(grub)
            {
                grub.ApplyForce(other.ClosestPoint(transform.position), 80);
                ParticlePoolManager.Instance.GrabWhiteHitParticle().transform.position = other.ClosestPoint(transform.position);
                HandItemManager.Instance.toolSource.PlayOneShot(hitStruct);
                cancelSwing = true;
            }
            return;
        }

        if(d_Collision == new Vector3(0,0,0))
        {
            Vector3 fwd = PlayerInteraction.Instance.mainCam.transform.TransformDirection(Vector3.forward);
            RaycastHit hit;

            if (Physics.Raycast(PlayerInteraction.Instance.mainCam.transform.position, fwd, out hit, 8)) 
            {
                d_Collision = hit.point;
                if(other.gameObject.tag == "Grass_FootStepSurface") type = GroundType.Dirt;
                else type = GroundType.Other;
            }
            else return;

            /*d_Collision = other.ClosestPoint(transform.position);
            if(other.gameObject.tag == "Grass_FootStepSurface") type = GroundType.Dirt;
            else type = GroundType.Other;*/
        }

        //Something to hit corpses

        
    }

    void HitObject()
    {
        if(cancelSwing) return;

        if(hitArmor)
        {
            float damage = 1;
            hitArmor.TakeDamage(damage);
            HandItemManager.Instance.toolSource.PlayOneShot(hitStruct);
            print("Hit Armor");
            //if(PlayerInteraction.Instance.stamina > 50) PlayerInteraction.Instance.StaminaChange(-1);

            //PlayHitParticle(s_Collision);
            ParticlePoolManager.Instance.GrabWhiteHitParticle().transform.position = hitArmor.transform.position;
            return;
        }

        if(hitCreature)
        {
            float damage = creatureDamage;
            hitCreature.TakeDamage(damage, PlayerInteraction.Instance.transform.position);
            //playsound
            if(hitCreature.corpseType != CorpseParticleType.Metal && hitCreature.corpseType != CorpseParticleType.Stone) HandItemManager.Instance.toolSource.PlayOneShot(hitFlesh);
            else HandItemManager.Instance.toolSource.PlayOneShot(hitStruct);
            print("Hit Creature");
            //if(PlayerInteraction.Instance.stamina > 50) PlayerInteraction.Instance.StaminaChange(-1);

            //PlayHitParticle(c_Collision);
            ParticlePoolManager.Instance.MoveAndPlayVFX(c_Collision, ParticlePoolManager.Instance.hitEffect);
            ParticlePoolManager.Instance.GrabOrangeHitParticle().transform.position = c_Collision;
            hitCreature.PlayHitParticle(c_Collision);

            if(hitCreature && hitCreature.health > 0) PlayerInteraction.Instance.InvokeEnemyHitEvent(hitCreature);

            if(TrinketInventoryHandler.Instance.CheckForTrinket(TrinketKey.MimicNose) && Random.Range(0, 20) == 1)
            {
                hitCreature.ApplyStatusEffect(StatusDatabase.Instance.GetStatus(StatusEffectName.MimicScent), 15);
                TrinketInventoryHandler.Instance.ApplyTrinketDamage(TrinketKey.MimicNose);
            }
            return;
        }

        if(hitStructure)
        {
            hitStructure.TakeDamage(1f);
            if(hitStructure.structData.structureType == StructureType.Null || hitStructure.structData.structureType == StructureType.Hay || hitStructure.structData.structureType == StructureType.CorruptedFlesh) 
            HandItemManager.Instance.toolSource.PlayOneShot(hitHay);
            else HandItemManager.Instance.toolSource.PlayOneShot(hitStruct);
            print("Hit Structure");

            PlayHitParticle(s_Collision);
            ParticlePoolManager.Instance.GrabWhiteHitParticle().transform.position = s_Collision;
            return;
        }

        if(hitBug)
        {
            hitBug.Struck();
            return; //To stop hitting the floor after striking bug
        }

        if(hitNPC)
        {
            hitNPC.Struck(n_Collision);
            HandItemManager.Instance.toolSource.PlayOneShot(hitFlesh);
            ParticlePoolManager.Instance.GrabOrangeHitParticle().transform.position = n_Collision;
            return;
        }

        if(d_Collision != new Vector3(0,0,0))
        {
            PlayHitParticle(d_Collision);
            ParticlePoolManager.Instance.GrabWhiteHitParticle().transform.position = d_Collision;
            if(type == GroundType.Dirt)
            {
                print("Hit dirt");
                ParticlePoolManager.Instance.MoveAndPlayParticle(d_Collision, ParticlePoolManager.Instance.dirtParticle);
                HandItemManager.Instance.toolSource.PlayOneShot(hitDirt);
            }
            else
            {
                print("Hit default");
                PlayHitParticle(d_Collision);
                HandItemManager.Instance.toolSource.PlayOneShot(hitStruct);
            }
        }
    }


    void PlayHitParticle(Vector3 hitPoint)
    {
        print("Played");
        ParticlePoolManager.Instance.GrabImpactParticle().transform.position = hitPoint;
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
