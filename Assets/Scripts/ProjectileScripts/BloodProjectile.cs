using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodProjectile : MonoBehaviour
{
    public AudioClip hitStruct, hitEnemy, hitGround;

    public bool homing = false;
    public Vector3 target;

    Rigidbody rb;

    public GameObject[] thingsToTurnOff;
    bool canCollide = true;
    public TrailRenderer trail;

    public float ichorGain = 1f;

    public CreatureBehaviorScript sourceCreature;


    void OnTriggerEnter(Collider other)
    {
        if(!canCollide) return;

        if (other.gameObject.layer == 17)
        {
            Rigidbody signRB = other.GetComponent<Rigidbody>();
            if (signRB != null)
            {
                Vector3 impactDirection = rb.velocity.normalized;
                float bulletSpeed = rb.velocity.magnitude;
                float forceMultiplier = 0.1f;
                signRB.AddForce(impactDirection * bulletSpeed * forceMultiplier, ForceMode.Impulse);
            }
        }

        if (other.gameObject.layer == 6)
        {
            //break
            var structure = other.GetComponentInParent<StructureBehaviorScript>();
            if (structure != null)
            {
                bool deleteDrop = false;
                if(structure.onFire) 
                {
                    structure.Extinguish();
                    deleteDrop = true;
                }

                FarmLand farmTile = structure as FarmLand;
                if(farmTile)
                {
                    NutrientStorage nutrients = farmTile.GetCropStats();
                    if(nutrients.ichorLevel < 10)
                    {
                        nutrients.ichorLevel += ichorGain;
                        farmTile.IchorRefill();
                        deleteDrop = true;
                    }
                    else return;
                }
                if(!deleteDrop) return;
                //HandItemManager.Instance.toolSource.PlayOneShot(hitStruct);
                AudioPoolManager.Instance.PlayClipAtPosition(hitStruct, transform.position, 0.1f, 30);
                ParticlePoolManager.Instance.MoveAndPlayVFX(transform.position, ParticlePoolManager.Instance.hitEffect);
                ParticlePoolManager.Instance.GrabBloodSplashParticle().transform.position = transform.position;
                StartCoroutine(TurnOff());

                return;
            }
            
        }

        if(other.gameObject.layer == 9)
        {
            var creature = other.GetComponentInParent<CreatureBehaviorScript>();
            if (creature != null && creature.shovelVulnerable && creature.health > 0 && (!sourceCreature || sourceCreature != creature))
            {
                creature.HitWithWater();
                //HandItemManager.Instance.toolSource.PlayOneShot(hitEnemy);
                AudioPoolManager.Instance.PlayClipAtPosition(hitEnemy, transform.position, 0.1f, 30);
                //print("Hit Creature");
                ParticlePoolManager.Instance.MoveAndPlayVFX(transform.position, ParticlePoolManager.Instance.hitEffect);
                ParticlePoolManager.Instance.GrabBloodSplashParticle().transform.position = transform.position;
                //gameObject.SetActive(false);
                //StartCoroutine(TurnOff());
                return;
            }
        }

        if(other.gameObject.layer == 0 || other.gameObject.layer == 7)
        {
            //HandItemManager.Instance.toolSource.PlayOneShot(hitGround);
            AudioPoolManager.Instance.PlayClipAtPosition(hitGround, transform.position, 0.1f, 30);
            print("Missed");
            ParticlePoolManager.Instance.MoveAndPlayVFX(transform.position, ParticlePoolManager.Instance.hitEffect);
            ParticlePoolManager.Instance.GrabBloodSplashParticle().transform.position = transform.position;
            //gameObject.SetActive(false);
            StartCoroutine(TurnOff());
            GroundImpact(transform.position);
            //NutrientRefill
            return;
        }

        var bug = other.GetComponent<BugBehaviorScript>();
        if(bug) bug.Struck();

    }

    void GroundImpact(Vector3 pos)
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 3f);
        foreach(Collider collider in hitColliders)
        {
            StructureBehaviorScript structure = collider.gameObject.GetComponentInParent<StructureBehaviorScript>();
            if(structure && structure.onFire) structure.Extinguish();
            if(structure) return;

            var creature = collider.gameObject.GetComponentInParent<CreatureBehaviorScript>();
            if (creature != null && creature.shovelVulnerable && creature.health > 0 && (!sourceCreature || sourceCreature != creature))
            {
                creature.TakeDamage(5);
                creature.HitWithWater();
                AudioPoolManager.Instance.PlayClipAtPosition(hitEnemy, pos, 0.1f, 30);
                ParticlePoolManager.Instance.GrabBloodSplashParticle().transform.position = creature.transform.position;
                return;
            }
        }
    }

    void OnEnable()
    {
        canCollide = true;
        trail.emitting = false;
        StartCoroutine(LifeTime());
        if(!rb) rb = GetComponent<Rigidbody>();
        //rb.isKinematic = false;
        foreach(GameObject thing in thingsToTurnOff)
        {
            thing.SetActive(true);
        }
    }

    void OnDisable()
    {
        StopCoroutine(LifeTime());
        homing = false;
        canCollide = false;
        target = new Vector3(0,0,0);
        sourceCreature = null;
    }

    IEnumerator TurnOff()
    {
        canCollide = false;
        //rb.isKinematic = true;
        rb.velocity = new Vector3(0,0,0);
        foreach(GameObject thing in thingsToTurnOff)
        {
            thing.SetActive(false);
        }
        yield return new WaitForSeconds(1.5f);
        gameObject.SetActive(false);
    }

    IEnumerator LifeTime()
    {
        yield return new WaitForSeconds(0.02f);
        trail.emitting = true;
        yield return new WaitForSeconds(20);
        gameObject.SetActive(false);
    }
}
