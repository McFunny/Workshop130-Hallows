using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    public AudioClip hitStruct, hitEnemy, hitGround;

    public float structureDamage, creatureDamage, playerDamage;
    float baseCreatureDamage;
    public float armorDamage = 2;

    public bool fireBullet, piercing, cannonBall, energyBullet;
    public float bulletLifetime = 3;

    private Rigidbody bulletRigidbody;

    public StructureType particleType = StructureType.Null;

    bool initialDisable = true;

    private void Awake()
    {
        bulletRigidbody = GetComponent<Rigidbody>();
        baseCreatureDamage = creatureDamage;
    }


    void OnTriggerEnter(Collider other)
    {
        Vector3 hitPoint = other.ClosestPoint(transform.position);

        if(other.gameObject.layer == 18)
        {
            var armor = other.GetComponent<CreatureArmor>();
            if(armor)
            {
                armor.TakeDamage(armorDamage);
                HandItemManager.Instance.toolSource.PlayOneShot(hitStruct);
                print("Hit Armor");
                ParticlePoolManager.Instance.GrabImpactParticle().transform.position = hitPoint;
                ParticlePoolManager.Instance.GrabOrangeHitParticle().transform.position = hitPoint;

                GameObject particles = ParticlePoolManager.Instance.GrabDestructionParticle(particleType);
                if(particles) particles.transform.position = hitPoint;

                gameObject.SetActive(false);
                return;
            }
        }

        if(other.gameObject.layer == 6)
        {
            var structure = other.GetComponent<StructureBehaviorScript>();
            if (structure != null && (structureDamage > 0 || fireBullet))
            {
                if(energyBullet)
                {
                    FarmLand tile = structure as FarmLand;
                    if(tile) return;
                }

                if(structureDamage > 0)
                {
                    structure.TakeDamage(structureDamage);
                    HandItemManager.Instance.toolSource.PlayOneShot(hitStruct);
                    print("Hit Structure");
                    ParticlePoolManager.Instance.GrabImpactParticle().transform.position = hitPoint;
                    ParticlePoolManager.Instance.GrabOrangeHitParticle().transform.position = hitPoint;

                    GameObject particles = ParticlePoolManager.Instance.GrabDestructionParticle(particleType);
                    if(particles) particles.transform.position = hitPoint;

                    gameObject.SetActive(false);
                    if(fireBullet && structure.IsFlammable()) structure.LitOnFire(); 
                    return;
                }
                else if(fireBullet && structure.IsFlammable())
                {
                    HandItemManager.Instance.toolSource.PlayOneShot(hitStruct);
                    print("Hit Structure");
                    structure.LitOnFire(); 
                    return;
                }
            }

            var npc = other.GetComponent<NPC>();
            if (npc != null)
            {
                npc.ShotAt();
                HandItemManager.Instance.toolSource.PlayOneShot(hitStruct);
                print("Hit Person");
                ParticlePoolManager.Instance.GrabImpactParticle().transform.position = hitPoint;
                gameObject.SetActive(false);
                return;
            }
        }

        if (other.gameObject.layer == 17)
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 impactDirection = bulletRigidbody.velocity.normalized;
                float bulletSpeed = bulletRigidbody.velocity.magnitude;
                float forceMultiplier = 0.1f;
                rb.AddForce(impactDirection * bulletSpeed * forceMultiplier, ForceMode.Impulse);
            }
        }

        if (other.gameObject.layer == 9)
        {
            var creature = other.GetComponentInParent<CreatureBehaviorScript>();
            if (creature != null && creature.shovelVulnerable)
            {
                if(fireBullet && !creature.fireVulnerable) return;

                if(fireBullet) creature.ApplyStatusEffect(StatusDatabase.Instance.GetStatus(StatusEffectName.Fire), Random.Range(3, 7));

                if(cannonBall) creature.lastDamageTypeTaken = DamageType.Cannonball;
                creature.TakeDamage(creatureDamage);
                HandItemManager.Instance.toolSource.PlayOneShot(hitEnemy);

                GameObject particles = ParticlePoolManager.Instance.GrabDestructionParticle(particleType);
                if(particles) particles.transform.position = hitPoint;

                print("Hit Creature");
                ParticlePoolManager.Instance.GrabImpactParticle().transform.position = hitPoint;
                ParticlePoolManager.Instance.GrabOrangeHitParticle().transform.position = hitPoint;
                creature.PlayHitParticle(new Vector3(hitPoint.x, hitPoint.y, hitPoint.z));
                if(creature.health + creatureDamage > 0 && !piercing) gameObject.SetActive(false);
                return;
            }
        }

        if(other.gameObject.layer == 0 || other.gameObject.layer == 7 || other.gameObject.layer == 19)
        {
            HandItemManager.Instance.toolSource.PlayOneShot(hitGround);
            print("Missed");
            ParticlePoolManager.Instance.GrabImpactParticle().transform.position = hitPoint;

            GameObject particles = ParticlePoolManager.Instance.GrabDestructionParticle(particleType);
            if(particles) particles.transform.position = hitPoint;

            if(!fireBullet) ParticlePoolManager.Instance.GrabPoofParticle().transform.position = hitPoint;
            else ParticlePoolManager.Instance.GrabExtinguishParticle().transform.position = hitPoint;
            gameObject.SetActive(false);
            return;
        }

        if(other.gameObject.layer == 10)
        {
            if(playerDamage == 0) return;
            PlayerInteraction.Instance.StaminaChange(-playerDamage);
            if(fireBullet) PlayerInteraction.Instance.ApplyStatusEffect(StatusDatabase.Instance.GetStatus(StatusEffectName.Fire), Random.Range(3, 5));
            ParticlePoolManager.Instance.GrabImpactParticle().transform.position = hitPoint;
            HandItemManager.Instance.toolSource.PlayOneShot(hitStruct);
            gameObject.SetActive(false);
        }

        var bug = other.GetComponent<BugBehaviorScript>();
        if(bug) bug.Struck();
    }

    void OnEnable()
    {
        StartCoroutine(LifeTime());
    }

    void OnDisable()
    {
        if(energyBullet && !initialDisable) ParticlePoolManager.Instance.GrabElecZapParticle().transform.position = transform.position; 
        StopCoroutine(LifeTime());
        initialDisable = false;
        bulletRigidbody.velocity = Vector3.zero;
        bulletRigidbody.angularVelocity = Vector3.zero;
        creatureDamage = baseCreatureDamage;
    }

    IEnumerator LifeTime()
    {
        yield return new WaitForSeconds(bulletLifetime);
        gameObject.SetActive(false);
    }

}

public enum BulletType
{
    Bullet,
    TimberEar
}
