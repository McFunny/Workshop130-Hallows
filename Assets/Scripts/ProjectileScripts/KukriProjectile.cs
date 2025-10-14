using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KukriProjectile : MonoBehaviour
{
    public AudioClip hitDull, hitCrit;

    public float hiltDamage, critDamage;

    public float bulletLifetime = 3;

    private Rigidbody bulletRigidbody;

    public GameObject droppedPrefab;

    Transform knifeParent;
    int critChance = 4;

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
                ParticlePoolManager.Instance.GrabImpactParticle().transform.position = transform.position;
                HandItemManager.Instance.toolSource.PlayOneShot(hitDull);

                Destroy(gameObject);
                return;
            }
        }

        if(other.gameObject.layer == 6)
        {
            //break
            var structure = other.GetComponent<StructureBehaviorScript>();
            if (structure != null)
            {
                ParticlePoolManager.Instance.GrabImpactParticle().transform.position = transform.position;
                HandItemManager.Instance.toolSource.PlayOneShot(hitDull);

                Destroy(gameObject);
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
                bool hiltHit = true;
                if(Random.Range(0, 10) < critChance) hiltHit = false;
                knifeParent = creature.GrabKnifeParent();
                if(knifeParent == null || creature.corpseType == CorpseParticleType.Metal) hiltHit = true;

                if(hiltHit)
                {
                    knifeParent = null;
                    creature.TakeDamage(hiltDamage);
                    HandItemManager.Instance.toolSource.PlayOneShot(hitDull);
                }
                else
                {
                    creature.TakeDamage(critDamage);
                    HandItemManager.Instance.toolSource.PlayOneShot(hitCrit);
                    ParticlePoolManager.Instance.GrabOrangeHitParticle().transform.position = transform.position;
                }


                print("Hit Creature");
                ParticlePoolManager.Instance.GrabImpactParticle().transform.position = transform.position;
                creature.PlayHitParticle(new Vector3(transform.position.x, transform.position.y, transform.position.z));
                Destroy(gameObject);
                return;
            }
        }

        if(other.gameObject.layer == 0 || other.gameObject.layer == 7)
        {
            HandItemManager.Instance.toolSource.PlayOneShot(hitDull);
            print("Missed");
            ParticlePoolManager.Instance.GrabImpactParticle().transform.position = transform.position;

            ParticlePoolManager.Instance.GrabPoofParticle().transform.position = transform.position;
            Destroy(gameObject);
            return;
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
        yield return new WaitForSeconds(0.3f);
        critChance += 2;
        yield return new WaitForSeconds(0.3f);
        critChance += 4;
        yield return new WaitForSeconds(bulletLifetime);
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (!gameObject.scene.isLoaded) return; 
        GameObject knife = Instantiate(droppedPrefab, new Vector3(transform.position.x, transform.position.y + 0.8f, transform.position.z), transform.rotation);
        if(knifeParent) knife.GetComponent<DroppedKukri>().StuckInObject(knifeParent);
        else  
        {
            knife.GetComponent<Rigidbody>().AddForce(-knife.transform.forward * 20);
        }
    }
}
