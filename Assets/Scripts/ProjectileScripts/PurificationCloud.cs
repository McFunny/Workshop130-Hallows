using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PurificationCloud : MonoBehaviour
{
    public LayerMask mask;

    void Start()
    {
        StartCoroutine(LifeTime());
    }

    IEnumerator LifeTime()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 6f, mask);
        foreach(Collider collider in hitColliders)
        {
            var creature = collider.GetComponentInParent<CreatureBehaviorScript>();
            if (creature != null && creature.corpseType == CorpseParticleType.Corrupted)
            {
                creature.TakeDamage(150);
                creature.PlayHitParticle(creature.transform.position);
                continue;
            }

            CorruptedTile c_tile = collider.GetComponentInParent<CorruptedTile>();
            if(c_tile && !c_tile.beingCleansed)
            {
                yield return new WaitForSeconds(Random.Range(0.3f, 0.5f));
                c_tile.StartCoroutine(c_tile.CleanseRoutine());
            }
        }
        yield return new WaitForSeconds(5);
        Destroy(gameObject);
    }
}
