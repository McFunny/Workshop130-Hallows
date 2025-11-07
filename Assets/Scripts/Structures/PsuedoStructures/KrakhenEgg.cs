using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KrakhenEgg : StructureBehaviorScript
{
    public CreatureObject krakhen;

    void Start()
    {
        audioHandler = GetComponent<StructureAudioHandler>();
        StartCoroutine(HatchEgg());
    }

    public override void HitWithWater()
    {
        TakeDamage(10);
    }

    IEnumerator HatchEgg()
    {
        yield return new WaitForSeconds(Random.Range(20, 40));
        Instantiate(krakhen.objectPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        base.OnDestroy();
        if(!gameObject.scene.isLoaded) return;
        ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);
        audioHandler.PlaySoundAtPoint(audioHandler.breakSound, transform.position);
    }
}
