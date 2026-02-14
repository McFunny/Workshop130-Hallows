using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Geyser : StructureBehaviorScript
{
    float timeLeft;
    public List<ParticleSystem> particles;

    public LayerMask mask;
    void Start()
    {
        base.Start();
        ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);

        StartCoroutine(LifeTime());
    }

    IEnumerator LifeTime()
    {
        timeLeft = Random.Range(120, 180);
        audioHandler.GetSource().Play();
        while(timeLeft > 10)
        {
            yield return new WaitForSeconds(0.5f);
            timeLeft -= 0.5f;

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, 3f, mask);
            foreach(Collider collider in hitColliders)
            {
                StructureBehaviorScript structure = collider.gameObject.GetComponentInParent<StructureBehaviorScript>();
                if(structure)
                {
                    FarmLand tile = structure as FarmLand;
                    if(tile) tile.WaterCrops();
                    if(structure && structure.onFire) structure.Extinguish();
                    else structure.HitWithWater();

                    continue;
                }

                CreatureBehaviorScript creature = collider.gameObject.GetComponentInParent<CreatureBehaviorScript>();
                if(creature)
                {
                    creature.HitWithWater();
                    continue;
                }

                if(collider.gameObject.layer == 10 && PlayerInteraction.Instance.waterHeld < PlayerInteraction.Instance.maxWaterHeld)
                {
                    PlayerInteraction.Instance.waterHeld++;
                    AudioPoolManager.Instance.PlayClip(audioHandler.interactSound, 0.5f);
                }

            }
        }

        foreach(ParticleSystem p in particles) p.Stop();
        audioHandler.GetSource().Stop();

        while(timeLeft > 0)
        {
            yield return new WaitForSeconds(1);
            timeLeft--;
        }
        ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);
        Destroy(gameObject);
    }

}
