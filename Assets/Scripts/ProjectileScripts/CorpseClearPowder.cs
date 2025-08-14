using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorpseClearPowder : MonoBehaviour
{
    public float duration = 15;
    public CreatureObject walkerData;
    public ParticleSystem effectParticles;

    void Start()
    {
        StartCoroutine(LifeTime());
    }

    IEnumerator LifeTime()
    {
        float timeAlive = 0;
        while(timeAlive < duration)
        {
            timeAlive++;
            Collider[] hitEnemies = Physics.OverlapSphere(transform.position, 20f, 1 << 9);
            foreach(Collider collider in hitEnemies)
            {
                var creature = collider.GetComponentInParent<CreatureBehaviorScript>();
                if (creature != null && (creature.health <= 0 || creature.creatureData == walkerData))
                {
                    creature.TakeDamage(5);
                    creature.PlayHitParticle(creature.transform.position);
                }
            }
            yield return new WaitForSeconds(1);
        }
        effectParticles.Stop();
        yield return new WaitForSeconds(5);
        Destroy(gameObject);
    }
}
