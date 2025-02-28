using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    public AudioClip hitStruct, hitEnemy, hitGround;

    public float structureDamage, creatureDamage, playerDamage;

    public bool fireBullet;
    public float bulletLifetime = 3;


    void OnTriggerEnter(Collider other)
    {
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
                    ParticlePoolManager.Instance.MoveAndPlayVFX(transform.position, ParticlePoolManager.Instance.hitEffect);
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
                ParticlePoolManager.Instance.MoveAndPlayVFX(transform.position, ParticlePoolManager.Instance.hitEffect);
                gameObject.SetActive(false);
                return;
            }
            
        }

        if(other.gameObject.layer == 9)
        {
            var creature = other.GetComponentInParent<CreatureBehaviorScript>();
            if (creature != null && creature.shovelVulnerable)
            {
                if(fireBullet && !creature.fireVulnerable) return;
                creature.TakeDamage(creatureDamage);
                //playsound
                HandItemManager.Instance.toolSource.PlayOneShot(hitEnemy);
                print("Hit Creature");
                ParticlePoolManager.Instance.MoveAndPlayVFX(transform.position, ParticlePoolManager.Instance.hitEffect);
                creature.PlayHitParticle(new Vector3(transform.position.x, transform.position.y, transform.position.z));
                gameObject.SetActive(false);
                return;
            }
        }

        if(other.gameObject.layer == 0 || other.gameObject.layer == 7)
        {
            HandItemManager.Instance.toolSource.PlayOneShot(hitGround);
            print("Missed");
            ParticlePoolManager.Instance.MoveAndPlayVFX(transform.position, ParticlePoolManager.Instance.hitEffect);
            if(!fireBullet) ParticlePoolManager.Instance.GrabPoofParticle().transform.position = transform.position;
            gameObject.SetActive(false);
            return;
        }

        if(other.gameObject.layer == 10)
        {
            if(playerDamage == 0) return;
            PlayerInteraction.Instance.StaminaChange(-playerDamage);
            ParticlePoolManager.Instance.MoveAndPlayVFX(transform.position, ParticlePoolManager.Instance.hitEffect);
            HandItemManager.Instance.toolSource.PlayOneShot(hitStruct);
            gameObject.SetActive(false);
        }

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
