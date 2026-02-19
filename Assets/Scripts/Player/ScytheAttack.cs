using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScytheAttack : MonoBehaviour
{
    //public LayerMask hitDetection;
    public Collider collider;

    public AudioClip hitPlant, hitFlesh, hitGround, hitHardObject;

    bool cancelSwing; //Happens when the player hits a hard thing
    [HideInInspector] public bool upgradedSwing;


    List<CreatureBehaviorScript> hitCreatures = new List<CreatureBehaviorScript>();
    List<FarmLand> hitCrops = new List<FarmLand>();
    List<BugBehaviorScript> hitBugs = new List<BugBehaviorScript>();

    void Start()
    {
        collider.enabled = false;
    }
    
    public IEnumerator Swing()
    {
        cancelSwing = false;

        hitCreatures.Clear();
        hitCrops.Clear();
        hitBugs.Clear();
        collider.enabled = true;
        yield return new WaitForSeconds(0.08f);
        collider.enabled = false;
        HitObjects();
        print("SwungScythe");

        yield return new WaitForSeconds(0.25f);
        PlayerMovement.limitMaxVelocity = true;
        PlayerMovement.ignoreMovementInputs = false;
        if(HandItemManager.Instance.scytheTrail) HandItemManager.Instance.scytheTrail.emitting = false;
        upgradedSwing = false;
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
            if(crop && crop.currentUpgrade != FarmLand.FarmTileUpgrade.Trellis)
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
            if(creature.corpseType == CorpseParticleType.Stone || creature.corpseType == CorpseParticleType.Metal)
            {
                cancelSwing = true;
                HandItemManager.Instance.toolSource.PlayOneShot(hitHardObject);
                ParticlePoolManager.Instance.MoveAndPlayVFX(other.ClosestPoint(transform.position), ParticlePoolManager.Instance.hitEffect);
                creature.TakeDamage(35, PlayerInteraction.Instance.transform.position);
                return;
            }

            if(hitCreatures.Contains(creature)) return;
            hitCreatures.Add(creature);
        }

        var creatureArmor = other.GetComponentInParent<CreatureArmor>();
        if (creatureArmor != null)
        {
            cancelSwing = true;
            creatureArmor.TakeDamage(2);
            HandItemManager.Instance.toolSource.PlayOneShot(hitHardObject);
            ParticlePoolManager.Instance.MoveAndPlayVFX(other.ClosestPoint(transform.position), ParticlePoolManager.Instance.hitEffect);
            return;
        }

        var bug = other.GetComponentInParent<BugBehaviorScript>();
        if (bug != null)
        {
            if(hitBugs.Contains(bug)) return;
            hitBugs.Add(bug);
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
        if(other.GetComponentInParent<NPC>() || other.gameObject.layer == 12 || other.gameObject.layer == 15 || other.GetComponentInParent<PetBehaviorScript>()) return;
        
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
            HandItemManager.Instance.toolSource.PlayOneShot(hitHardObject);
            return;
        }

        if(hitCreatures.Count == 0 && hitCrops.Count == 0) return;

        if(PlayerInteraction.Instance.stamina > 50) PlayerInteraction.Instance.StaminaChange(-2);

        for(int i = 0; i < hitCreatures.Count; i++)
        {
            if(hitCreatures[i] == null) continue;
            HandItemManager.Instance.toolSource.PlayOneShot(hitFlesh);

            ParticlePoolManager.Instance.MoveAndPlayVFX(hitCreatures[i].GetComponentInChildren<Collider>().ClosestPoint(transform.position), ParticlePoolManager.Instance.hitEffect);
            hitCreatures[i].PlayHitParticle(hitCreatures[i].GetComponentInChildren<Collider>().ClosestPoint(transform.position));

            float damage = 40;
            if(upgradedSwing) damage += 15;
            if(upgradedSwing && (hitCreatures[i].corpseType == CorpseParticleType.Red || hitCreatures[i].corpseType == CorpseParticleType.Corrupted) && hitCreatures[i].health > 0) 
            SummonBloodParticles(hitCreatures[i]);

            hitCreatures[i].TakeDamage(damage, PlayerInteraction.Instance.transform.position);

            if(hitCreatures[i] && hitCreatures[i].health > 0) PlayerInteraction.Instance.InvokeEnemyHitEvent(hitCreatures[i]);

            if(TrinketInventoryHandler.Instance.CheckForTrinket(TrinketKey.MimicNose) && Random.Range(0, 20) == 1)
            {
                hitCreatures[i].ApplyStatusEffect(StatusDatabase.Instance.GetStatus(StatusEffectName.MimicScent), 15);
                TrinketInventoryHandler.Instance.ApplyTrinketDamage(TrinketKey.MimicNose);
            }
        }

        for(int i = 0; i < hitCrops.Count; i++)
        {
            if(hitCrops[i] == null) continue;
            //Harvest grown
            hitCrops[i].ToolInteraction(ToolType.Scythe, out bool success);
            if(success) HandItemManager.Instance.toolSource.PlayOneShot(hitPlant);
        }

        for(int i = 0; i < hitBugs.Count; i++)
        {
            if(hitBugs[i] == null) continue;
            hitBugs[i].Struck();
        }
    }

    void SummonBloodParticles(CreatureBehaviorScript c)
    {
        float bulletsToSpawn = (c.ichorWorth * 2) + 4;

        for(int i = 0; i < bulletsToSpawn; i++)
        {
            GameObject newBullet = ProjectilePoolManager.Instance.GrabBlood();
            newBullet.GetComponent<BloodProjectile>().sourceCreature = c;
            Vector3 dropPos = c.corpseParticleTransform.position;
            dropPos.y += 1;
            newBullet.transform.position = dropPos;
            newBullet.transform.rotation = c.corpseParticleTransform.rotation;
            //newBullet.GetComponent<WaterProjectileScript>().homing = true;
            //newBullet.GetComponent<WaterProjectileScript>().target = highlights[i].transform.position;
            Vector3 dir = new Vector3(Random.Range(-1,1), 0, Random.Range(-1,1));//+ new Vector3(Random.Range(-bulletSpread,bulletSpread), Random.Range(-bulletSpread,bulletSpread), Random.Range(-bulletSpread,bulletSpread));
            newBullet.GetComponent<Rigidbody>().AddForce(Vector3.up * Random.Range(50,90));
            newBullet.GetComponent<Rigidbody>().AddForce(dir * Random.Range(20,40));
        }
    }


    void PlayHitParticle(Vector3 hitPoint)
    {
        print("Played");
        ParticlePoolManager.Instance.GrabImpactParticle().transform.position = transform.position;
        return;
    }
}
