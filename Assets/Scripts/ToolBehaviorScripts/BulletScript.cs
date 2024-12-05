using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    [HideInInspector]
    public float damage = 25;

    public AudioClip hitStruct, hitEnemy, hitGround;


    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == 6)
        {
            //break
            var structure = other.GetComponent<StructureBehaviorScript>();
            if (structure != null)
            {
                structure.TakeDamage(2);
                HandItemManager.Instance.toolSource.PlayOneShot(hitStruct);
                print("Hit Structure");
                ParticlePoolManager.Instance.MoveAndPlayVFX(transform.position, ParticlePoolManager.Instance.hitEffect);
                gameObject.SetActive(false);
                return;
            }

            var npc = other.GetComponent<NPC>();
            if (npc != null)
            {
                npc.ShotAt();
                HandItemManager.Instance.toolSource.PlayOneShot(hitStruct);
                print("Hit Structure");
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
                creature.TakeDamage(25);
                //playsound
                HandItemManager.Instance.toolSource.PlayOneShot(hitEnemy);
                print("Hit Creature");
                ParticlePoolManager.Instance.MoveAndPlayVFX(transform.position, ParticlePoolManager.Instance.hitEffect);
                gameObject.SetActive(false);
                return;
            }
        }

        if(other.gameObject.layer == 0 || other.gameObject.layer == 7)
        {
            HandItemManager.Instance.toolSource.PlayOneShot(hitGround);
            print("Missed");
            ParticlePoolManager.Instance.MoveAndPlayVFX(transform.position, ParticlePoolManager.Instance.hitEffect);
            gameObject.SetActive(false);
            return;
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
        yield return new WaitForSeconds(3);
        gameObject.SetActive(false);
    }

}
