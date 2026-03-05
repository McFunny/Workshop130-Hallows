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
        print("Collided with: " + other.gameObject + ". Am I already exploding? " + exploding);
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
                creature.lastDamageTypeTaken = DamageType.Mine;
                if(creature.health - creatureDamage <= 0) AchievementManager.Instance.CompleteProgressWithEnum(ACHKey.Return_To_Sender);
                creature.TakeDamage(creatureDamage);
                if(creature.fireVulnerable) creature.ApplyStatusEffect(StatusDatabase.Instance.GetStatus(StatusEffectName.Fire), Random.Range(6, 10));
                creature.PlayHitParticle(new Vector3(transform.position.x, transform.position.y, transform.position.z));
            }
            else if(creature)
            {
                MurderMancer mancer = collider.gameObject.GetComponentInParent<MurderMancer>();
                if(mancer) mancer.IgnitedByOther();
            }
        }

        Collider[] hitBugs = Physics.OverlapSphere(transform.position, 6f, 1 << 18);
        foreach(Collider collider in hitBugs)
        {
            var bug = collider.GetComponentInParent<BugBehaviorScript>();
            if(bug) bug.Struck();
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
        bulletRigidbody.velocity = Vector3.zero;
        bulletRigidbody.angularVelocity = Vector3.zero;
    }

    IEnumerator LifeTime()
    {
        yield return new WaitForSeconds(bulletLifetime);
        if(gameObject.activeSelf && !exploding) Explode();
    }

}
