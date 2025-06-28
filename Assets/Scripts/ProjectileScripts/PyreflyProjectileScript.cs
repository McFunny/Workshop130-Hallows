using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PyreflyProjectileScript : MonoBehaviour
{
    public AudioClip explodeSFX;

    public float structureDamage, creatureDamage, playerDamage;

    public float bulletLifetime = 3;

    private Rigidbody bulletRigidbody;

    bool exploding = false;

    private void Start()
    {
        bulletRigidbody = GetComponent<Rigidbody>();
    }


    void OnTriggerEnter(Collider other)
    {
        if(exploding) return;
        exploding = true;
        Explode();

    }

    void Explode()
    {
        ParticlePoolManager.Instance.GrabExplosionParticle().transform.position = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);
        AudioPoolManager.Instance.PlayClipAtPosition(explodeSFX, transform.position);
        if(Vector3.Distance(transform.position, PlayerInteraction.Instance.transform.position) < 6f)
        {
            PlayerInteraction.Instance.ApplyStatusEffect(StatusDatabase.Instance.GetStatus(StatusEffectName.Fire), 4);
            PlayerInteraction.Instance.StaminaChange(-playerDamage);
        }
        Collider[] hitStructures = Physics.OverlapSphere(transform.position, 4f, 1 << 6);
        foreach(Collider collider in hitStructures)
        {
            StructureBehaviorScript structure = collider.gameObject.GetComponentInParent<StructureBehaviorScript>();
            if(structure)
            {
                if(structure.IsFlammable()) structure.LitOnFire();
                else structure.TakeDamage(structureDamage);
            }
        }

        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, 6f, 1 << 9);
        foreach(Collider collider in hitEnemies)
        {
            var creature = collider.GetComponentInParent<CreatureBehaviorScript>();
            if (creature != null && creature.shovelVulnerable)
            {
                creature.TakeDamage(creatureDamage);
                if(creature.fireVulnerable) creature.ApplyStatusEffect(StatusDatabase.Instance.GetStatus(StatusEffectName.Fire), Random.Range(4, 15));
                creature.PlayHitParticle(new Vector3(transform.position.x, transform.position.y, transform.position.z));
            }
        }

        gameObject.SetActive(false);
    }

    void OnEnable()
    {
        exploding = false;
        StartCoroutine(LifeTime());
    }

    void OnDisable()
    {
        StopCoroutine(LifeTime());
    }

    IEnumerator LifeTime()
    {
        yield return new WaitForSeconds(bulletLifetime);
        if(gameObject.activeSelf && !exploding) Explode();
    }

}
