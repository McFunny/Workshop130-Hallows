using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootWhip : MonoBehaviour
{
    public bool corrupted = false;
    public float speed = 10;
    public Vector3 target;

    public Animator anim;

    public float creatureDamage, playerDamage;

    bool attacking = false;

    public LayerMask mask;

    public AudioClip whip;
    public AudioSource burrowLoop;

    void Start()
    {
        if(corrupted) anim.SetBool("IsCorrupted", true);
    }

    void Update()
    {
        MovingToTarget();
    }

    void MovingToTarget()
    {
        if(target == Vector3.zero || attacking) return;

        //move to position
        Vector3 direction = target - transform.position;
        float distance = direction.magnitude;

        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (Vector3.Distance(target, transform.position) < 0.1f)
        {
            attacking = true;
            burrowLoop.Stop();
            StartCoroutine(Attack());
        }
    }

    IEnumerator Attack()
    {
        anim.SetBool("Burrowing", false);
        yield return new WaitForSeconds(0.4f);
        AudioPoolManager.Instance.PlayClipAtPosition(whip, transform.position);
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, 2, mask);
        foreach(Collider collider in hitEnemies)
        {
            var c = collider.GetComponentInParent<CreatureBehaviorScript>();
            var player = collider.GetComponentInParent<PlayerInteraction>();
            if (c != null && c.health > 0 && c.shovelVulnerable && c.bearTrapVulnerable)
            {
                c.TakeDamage(creatureDamage);
                ParticlePoolManager.Instance.GrabOrangeHitParticle().transform.position = c.transform.position;
                c.PlayHitParticle(new Vector3(c.transform.position.x, c.transform.position.y, c.transform.position.z));
                continue;
            }

            if(player != null)
            {
                PlayerInteraction.Instance.StaminaChange(-playerDamage);
            }
        }
        yield return new WaitForSeconds(0.8f);
        Destroy(gameObject);
    }
}
