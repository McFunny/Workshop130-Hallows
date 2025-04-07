using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureArmor : MonoBehaviour
{
    //Treat health values as if you are hitting a structure
    public float health = 6;
    public float maxHealth = 6;

    public GameObject damageParticlesObject;
    List<ParticleSystem> damageParticles = new List<ParticleSystem>();
    public DestructionType destructionType;
    public GameObject gibs;

    public Transform particleCenter; //for particles

    public AudioSource source;
    public AudioClip damagedSFX, destroyedSFX;

    CreatureBehaviorScript parentCreature;

    void Awake()
    {
        if(damageParticlesObject)
        {
            foreach(Transform child in damageParticlesObject.transform)
            {
                damageParticles.Add(child.GetComponent<ParticleSystem>());
            }
        }
    }

    void Start()
    {
        parentCreature = GetComponentInParent<CreatureBehaviorScript>();
    }

    public void Update()
    {
        if(health <= 0) Destroy(this.gameObject);
        if(parentCreature && parentCreature.health <= 0) health = 0;
    }

    public void TakeDamage(float damage)
    {
        if(health <= 0) return;
        health -= damage;
        for(int i = 0; i < damageParticles.Count; i++)
        {
            damageParticles[i].Play();
        }

        if(source) source.PlayOneShot(damagedSFX);
    }

    public void OnDestroy()
    {
        if(!gameObject.scene.isLoaded) return;
        if(health <= 0)
        {
            GameObject p = ParticlePoolManager.Instance.GrabDestructionParticle(destructionType);
            if(p)
            {
                if(particleCenter) p.transform.position = particleCenter.position;
                else p.transform.position = transform.position;
            }

            if(gibs)
            {
                if(particleCenter) Instantiate(gibs, particleCenter.position, Quaternion.identity);
                else Instantiate(gibs, transform.position, Quaternion.identity);
            }
        }
        if(source) source.PlayOneShot(destroyedSFX);

    }
}
[System.Serializable]
public class EquipEnemyArmor
{
    public GameObject armorObject;
    public float chanceToEquip = 0;
}
