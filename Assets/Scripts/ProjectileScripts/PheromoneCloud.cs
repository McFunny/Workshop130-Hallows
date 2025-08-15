using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PheromoneCloud : MonoBehaviour
{
    public float duration = 7;
    public ParticleSystem effectParticles;

    public LayerMask mask;

    void Start()
    {
        StartCoroutine(LifeTime());
    }

    IEnumerator LifeTime()
    {
        float timeAlive = 0;
        yield return new WaitForSeconds(2);
        while(timeAlive < duration)
        {
            timeAlive++;
            Collider[] hitEnemies = Physics.OverlapSphere(transform.position, 10f, mask);
            foreach(Collider collider in hitEnemies)
            {
                var creature = collider.GetComponentInParent<CreatureBehaviorScript>();
                if (creature != null && creature.shovelVulnerable)
                {
                    creature.ApplyStatusEffect(StatusDatabase.Instance.GetStatus(StatusEffectName.MimicScent), 15);
                    continue;
                }

                if(collider.gameObject.layer == 10) PlayerInteraction.Instance.ApplyStatusEffect(StatusDatabase.Instance.GetStatus(StatusEffectName.MimicScent), 15);
            }
            yield return new WaitForSeconds(1);
        }
        effectParticles.Stop();
        yield return new WaitForSeconds(5);
        Destroy(gameObject);
    }
}
