using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowProjectile : MonoBehaviour
{
    public AudioClip hitStruct, hitEnemy, hitGround;


    public Rigidbody rb;

    public GameObject[] thingsToTurnOff;
    bool canCollide = true;
    public TrailRenderer trail;

    public float structDamage, playerDamage, creatureDamage;

    public CreatureBehaviorScript sourceCreature;

    bool hitTarget = false;

    public List<CreatureObject> immuneCreatures = new List<CreatureObject>();

    public GameObject explodeParticles;
    public ParticleSystem bounceParticles;

    public Light light;
    


    void OnTriggerEnter(Collider other)
    {
        if(!canCollide) return;

        if (other.gameObject.layer == 17 || other.gameObject.layer == 19)
        {
            Rigidbody signRB = other.GetComponent<Rigidbody>();
            if (signRB != null)
            {
                Vector3 impactDirection = rb.velocity.normalized;
                float bulletSpeed = rb.velocity.magnitude;
                float forceMultiplier = 0.1f;
                signRB.AddForce(impactDirection * bulletSpeed * forceMultiplier, ForceMode.Impulse);
            }

            Vector3 dir = other.gameObject.transform.position - transform.position;
            rb.AddForce(-dir * 0.5f, ForceMode.Impulse);
        }

        if (other.gameObject.layer == 6)
        {
            var structure = other.GetComponentInParent<StructureBehaviorScript>();
            if (structure != null)
            {

                FarmLand farmTile = structure as FarmLand;
                if(farmTile)
                {
                    farmTile.TakeDamage(structDamage);
                    AudioPoolManager.Instance.PlayClipAtPosition(hitStruct, transform.position, 0.1f, 30);
                    ParticlePoolManager.Instance.MoveAndPlayVFX(transform.position, ParticlePoolManager.Instance.hitEffect);
                    StartCoroutine(TurnOff());
                    ParticlePoolManager.Instance.GrabOrangeHitParticle().transform.position = structure.transform.position;
                    return;
                }

                Vector3 dir = structure.transform.position - transform.position;
                rb.AddForce(-dir * 0.5f, ForceMode.Impulse);
                ParticlePoolManager.Instance.GrabOrangeHitParticle().transform.position = transform.position;
                return;
            }
            
        }

        if(other.gameObject.layer == 10)
        {
            PlayerInteraction.Instance.StaminaChange(playerDamage);
            AudioPoolManager.Instance.PlayClipAtPosition(hitStruct, transform.position, 0.1f, 30);
            hitTarget = true;
            PlayerInteraction.Instance.ApplyStatusEffect(StatusDatabase.Instance.GetStatus(StatusEffectName.Blindness), Random.Range(4, 8));
            StartCoroutine(TurnOff());
            ParticlePoolManager.Instance.GrabOrangeHitParticle().transform.position = transform.position;
            return;
        }

        if(other.gameObject.layer == 9 && !hitTarget)
        {
            var creature = other.GetComponentInParent<CreatureBehaviorScript>();
            if (creature != null && creature.shovelVulnerable && creature.health > 0 && (!sourceCreature || sourceCreature != creature) && !immuneCreatures.Contains(creature.creatureData))
            {
                creature.TakeDamage(creatureDamage);
                AudioPoolManager.Instance.PlayClipAtPosition(hitEnemy, transform.position, 0.1f, 30);
                ParticlePoolManager.Instance.MoveAndPlayVFX(transform.position, ParticlePoolManager.Instance.hitEffect);
                
                StartCoroutine(TurnOff());
                hitTarget = true;
                ParticlePoolManager.Instance.GrabOrangeHitParticle().transform.position = creature.corpseParticleTransform.position;
                return;
            }
        }

        if(other.gameObject.layer == 0 || other.gameObject.layer == 7)
        {
            AudioPoolManager.Instance.PlayClipAtPosition(hitGround, transform.position, 0.1f, 30);
            ParticlePoolManager.Instance.MoveAndPlayVFX(transform.position, ParticlePoolManager.Instance.hitEffect);
            bounceParticles.Play();

            rb.AddForce(Vector3.up * 1.3f, ForceMode.Impulse);
            return;
        }

        var bug = other.GetComponent<BugBehaviorScript>();
        if(bug) bug.Struck();

    }

    void OnEnable()
    {
        canCollide = true;
        hitTarget = false;
        trail.emitting = false;
        StartCoroutine(LifeTime());
        if(!rb) rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        foreach(GameObject thing in thingsToTurnOff)
        {
            thing.SetActive(true);
        }
    }

    IEnumerator TurnOff()
    {
        canCollide = false;
        rb.isKinematic = true;
        rb.velocity = new Vector3(0,0,0);
        foreach(GameObject thing in thingsToTurnOff)
        {
            thing.SetActive(false);
        }

        explodeParticles.SetActive(true);
        explodeParticles.transform.parent = null;

        light.enabled = false;
        yield return new WaitForSeconds(6f);
        Destroy(gameObject);
    }

    IEnumerator LifeTime()
    {
        yield return new WaitForSeconds(0.02f);
        trail.emitting = true;
        yield return new WaitForSeconds(4);
        StartCoroutine(TurnOff());
    }
}
