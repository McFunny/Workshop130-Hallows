using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterProjectileScript : MonoBehaviour
{
    public AudioClip hitStruct, hitEnemy, hitGround;

    public bool homing = false;
    public Vector3 target;

    Rigidbody rb;

    public GameObject[] thingsToTurnOff;
    bool canCollide = true;
    public TrailRenderer trail;


    void OnTriggerEnter(Collider other)
    {
        if(!canCollide) return;
        if(other.gameObject.layer == 6)
        {
            //break
            var structure = other.GetComponent<StructureBehaviorScript>();
            if (structure != null)
            {
                if(structure.onFire) structure.Extinguish();

                FarmLand farmTile = structure as FarmLand;
                if(farmTile)
                {
                    NutrientStorage nutrients = farmTile.GetCropStats();
                    if(nutrients.waterLevel != 10) farmTile.WaterCrops();
                    else return;
                }
                structure.HitWithWater();
                HandItemManager.Instance.toolSource.PlayOneShot(hitStruct);
                print("Hit Structure");
                ParticlePoolManager.Instance.MoveAndPlayVFX(transform.position, ParticlePoolManager.Instance.hitEffect);
                //gameObject.SetActive(false);
                StartCoroutine(TurnOff());
                return;
            }

            var npc = other.GetComponent<NPC>();
            if (npc != null)
            {
                npc.ShotAt();
                HandItemManager.Instance.toolSource.PlayOneShot(hitStruct);
                print("Hit Person");
                ParticlePoolManager.Instance.MoveAndPlayVFX(transform.position, ParticlePoolManager.Instance.hitEffect);
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
                creature.TakeDamage(0);
                creature.HitWithWater();
                HandItemManager.Instance.toolSource.PlayOneShot(hitEnemy);
                print("Hit Creature");
                ParticlePoolManager.Instance.MoveAndPlayVFX(transform.position, ParticlePoolManager.Instance.hitEffect);
                //gameObject.SetActive(false);
                StartCoroutine(TurnOff());
                return;
            }
        }

        if(other.gameObject.layer == 0 || other.gameObject.layer == 7)
        {
            HandItemManager.Instance.toolSource.PlayOneShot(hitGround);
            print("Missed");
            ParticlePoolManager.Instance.MoveAndPlayVFX(transform.position, ParticlePoolManager.Instance.hitEffect);
            ParticlePoolManager.Instance.GrabPoofParticle().transform.position = transform.position;
            //gameObject.SetActive(false);
            StartCoroutine(TurnOff());
            return;
        }

    }

    void OnEnable()
    {
        canCollide = true;
        trail.emitting = false;
        StartCoroutine(LifeTime());
        if(!rb) rb = GetComponent<Rigidbody>();
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
    }

    IEnumerator TurnOff()
    {
        canCollide = false;
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
        yield return new WaitForSeconds(0.2f);
        if(homing && target != new Vector3(0,0,0))
        {
            rb.velocity = new Vector3(0,0,0);
            Vector3 dir = (transform.position - target).normalized;
            dir *= -1f;
            rb.AddForce(dir * 150);
            Debug.Log("ZOOM");
        }
        yield return new WaitForSeconds(20);
        gameObject.SetActive(false);
    }

}
