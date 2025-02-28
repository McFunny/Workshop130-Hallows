using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterProjectileScript : MonoBehaviour
{
    public AudioClip hitStruct, hitEnemy, hitGround, hitIce;

    public bool homing = false;
    public Vector3 target;

    Rigidbody rb;

    public GameObject[] thingsToTurnOff;
    bool canCollide = true;
    public TrailRenderer trail;

    public bool isFrozen = false;
    public GameObject iceObject;


    void OnTriggerEnter(Collider other)
    {
        if(!canCollide) return;
        if(other.gameObject.layer == 6)
        {
            //break
            var structure = other.GetComponentInParent<StructureBehaviorScript>();
            if (structure != null)
            {
                if(isFrozen)
                {
                    structure.TakeDamage(2);
                    ParticlePoolManager.Instance.MoveAndPlayVFX(transform.position, ParticlePoolManager.Instance.hitEffect);
                    ParticlePoolManager.Instance.GrabFrostBurstParticle().transform.position = transform.position;
                    StartCoroutine(TurnOff());
                    return;
                }

                if(structure.onFire) structure.Extinguish();

                FarmLand farmTile = structure as FarmLand;
                if(farmTile)
                {
                    NutrientStorage nutrients = farmTile.GetCropStats();
                    if(nutrients.waterLevel != 10)
                    {
                        farmTile.WaterCrops();
                    }
                    else return;
                }
                structure.HitWithWater();
                HandItemManager.Instance.toolSource.PlayOneShot(hitStruct);
                print("Hit Structure: " + structure);
                ParticlePoolManager.Instance.MoveAndPlayVFX(transform.position, ParticlePoolManager.Instance.hitEffect);
                ParticlePoolManager.Instance.GrabSplashParticle().transform.position = transform.position;
                //gameObject.SetActive(false);
                StartCoroutine(TurnOff());
                return;
            }

            var npc = other.GetComponent<NPC>();
            if (npc != null)
            {
                npc.ShotAt();
                HandItemManager.Instance.toolSource.PlayOneShot(hitStruct);
                //print("Hit Person");
                ParticlePoolManager.Instance.MoveAndPlayVFX(transform.position, ParticlePoolManager.Instance.hitEffect);
                ParticlePoolManager.Instance.GrabSplashParticle().transform.position = transform.position;
                //gameObject.SetActive(false);
                StartCoroutine(TurnOff());
                return;
            }
            
        }

        if(other.gameObject.layer == 9)
        {
            var creature = other.GetComponentInParent<CreatureBehaviorScript>();
            if (creature != null && creature.shovelVulnerable)
            {
                if(isFrozen)
                {
                    creature.TakeDamage(50);
                    ParticlePoolManager.Instance.MoveAndPlayVFX(transform.position, ParticlePoolManager.Instance.hitEffect);
                    ParticlePoolManager.Instance.GrabFrostBurstParticle().transform.position = transform.position;
                    StartCoroutine(TurnOff());
                    return;
                }

                creature.TakeDamage(0);
                creature.HitWithWater();
                HandItemManager.Instance.toolSource.PlayOneShot(hitEnemy);
                //print("Hit Creature");
                ParticlePoolManager.Instance.MoveAndPlayVFX(transform.position, ParticlePoolManager.Instance.hitEffect);
                ParticlePoolManager.Instance.GrabSplashParticle().transform.position = transform.position;
                //gameObject.SetActive(false);
                StartCoroutine(TurnOff());
                return;
            }

            if(creature)
            {
                Wraith wraith = creature as Wraith;
                if(wraith && !isFrozen)
                {
                    FreezeShot();
                }
            }
        }

        if(other.gameObject.layer == 0 || other.gameObject.layer == 7)
        {

            if(isFrozen)
            {
                ParticlePoolManager.Instance.MoveAndPlayVFX(transform.position, ParticlePoolManager.Instance.hitEffect);
                ParticlePoolManager.Instance.GrabFrostBurstParticle().transform.position = transform.position;
                StartCoroutine(TurnOff());
                return;
            }
            HandItemManager.Instance.toolSource.PlayOneShot(hitGround);
            print("Missed");
            ParticlePoolManager.Instance.MoveAndPlayVFX(transform.position, ParticlePoolManager.Instance.hitEffect);
            ParticlePoolManager.Instance.GrabSplashParticle().transform.position = transform.position;
            //gameObject.SetActive(false);
            StartCoroutine(TurnOff());
            return;
        }

    }

    void FreezeShot()
    {
        if(isFrozen) return;
        isFrozen = true;
        foreach(GameObject thing in thingsToTurnOff)
        {
            thing.SetActive(false);
        }
        iceObject.SetActive(true);
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

        isFrozen = false;
        iceObject.SetActive(false);
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
        iceObject.SetActive(false);
        yield return new WaitForSeconds(1.5f);
        gameObject.SetActive(false);
    }

    IEnumerator LifeTime()
    {
        yield return new WaitForSeconds(0.02f);
        trail.emitting = true;
        yield return new WaitForSeconds(0.3f);
        if(homing && target != new Vector3(0,0,0))
        {
            rb.velocity = new Vector3(0,0,0);
            Vector3 dir = (transform.position - target).normalized;
            dir *= -1f;
            rb.AddForce(dir * 100);
            //Debug.Log("ZOOM");
        }
        yield return new WaitForSeconds(20);
        gameObject.SetActive(false);
    }

}
