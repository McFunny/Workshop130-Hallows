using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    public AudioClip hitStruct, hitEnemy, hitGround;

    public float structureDamage, creatureDamage, playerDamage;
    public float armorDamage = 2;

    public bool fireBullet, piercing;
    public float bulletLifetime = 3;

    private Rigidbody bulletRigidbody;

    private void Start()
    {
        bulletRigidbody = GetComponent<Rigidbody>();
    }


    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == 18)
        {
            var armor = other.GetComponent<CreatureArmor>();
            if(armor)
            {
                armor.TakeDamage(armorDamage);
                HandItemManager.Instance.toolSource.PlayOneShot(hitStruct);
                print("Hit Armor");
                ParticlePoolManager.Instance.GrabImpactParticle().transform.position = transform.position;
                gameObject.SetActive(false);
                return;
            }
        }

        if(other.gameObject.layer == 6)
        {
            //break
            var structure = other.GetComponent<StructureBehaviorScript>();
            if (structure != null && (structureDamage > 0 || fireBullet))
            {
                if(structureDamage > 0)
                {
                    structure.TakeDamage(structureDamage);
                    HandItemManager.Instance.toolSource.PlayOneShot(hitStruct);
                    print("Hit Structure");
                    ParticlePoolManager.Instance.GrabImpactParticle().transform.position = transform.position;
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
                ParticlePoolManager.Instance.GrabImpactParticle().transform.position = transform.position;
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
                creature.TakeDamage(creatureDamage);
                //playsound
                HandItemManager.Instance.toolSource.PlayOneShot(hitEnemy);
                print("Hit Creature");
                ParticlePoolManager.Instance.GrabImpactParticle().transform.position = transform.position;
                creature.PlayHitParticle(new Vector3(transform.position.x, transform.position.y, transform.position.z));
                if(creature.health + creatureDamage > 0 && !piercing) gameObject.SetActive(false);
                return;
            }
        }

        if(other.gameObject.layer == 0 || other.gameObject.layer == 7)
        {
            HandItemManager.Instance.toolSource.PlayOneShot(hitGround);
            print("Missed");
            ParticlePoolManager.Instance.GrabImpactParticle().transform.position = transform.position;
            if(!fireBullet) ParticlePoolManager.Instance.GrabPoofParticle().transform.position = transform.position;
            gameObject.SetActive(false);
            return;
        }

        if(other.gameObject.layer == 10)
        {
            if(playerDamage == 0) return;
            PlayerInteraction.Instance.StaminaChange(-playerDamage);
            PlayerInteraction.Instance.ApplyStatusEffect(StatusDatabase.Instance.GetStatus(StatusEffectName.Fire), Random.Range(3, 5));
            ParticlePoolManager.Instance.GrabImpactParticle().transform.position = transform.position;
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
        StopCoroutine(LifeTime());
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
