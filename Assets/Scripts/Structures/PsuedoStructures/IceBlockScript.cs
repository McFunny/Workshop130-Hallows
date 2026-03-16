using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceBlockScript : MonoBehaviour
{
    public GameObject blockObject;

    public ParticleSystem meltingParticles, iceBreakParticles, hitParticles;

    public float health = 5;
    public float maxHealth = 5;

    public AudioClip iceHit, iceBreak, iceFreeze;
    public AudioSource meltingSource;
    AudioSource source;

    StructureBehaviorScript encasedStructure;

    void Start()
    {
        encasedStructure = GetComponentInParent<StructureBehaviorScript>();
        source = encasedStructure.audioHandler.GetSource();
        StartCoroutine(MeltingCoroutine());
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if(health <= 0)
        {
            health = 0;
            IceBroken();
            hitParticles.Play();
        }
        else source.PlayOneShot(iceHit);
    }

    public void FormIce()
    {
        health = maxHealth;
        blockObject.SetActive(true);
        source.PlayOneShot(iceFreeze);
    }

    public void IceBroken()
    {
        blockObject.SetActive(false);

        source.PlayOneShot(iceBreak);

        IWaterHolder wHolder = encasedStructure as IWaterHolder;
        if(wHolder != null)
        {
            if(encasedStructure.nearbyFires.Count == 0) wHolder.EmptyWater();
            else
            {
                for(int i = 0; i < 3; ++i)
                {
                    wHolder.GivenWater();
                }
            }
        } 
        iceBreakParticles.Play();
    }

    IEnumerator MeltingCoroutine()
    {
        while(true)
        {
            yield return new WaitForSeconds(0.5f);
            if(encasedStructure.nearbyFires.Count == 0 || !blockObject.activeSelf) continue;

            meltingParticles.Play();
            meltingSource.Play();
            while(encasedStructure.nearbyFires.Count > 0 && blockObject.activeSelf)
            {
                yield return new WaitForSeconds(0.5f);
                if(encasedStructure.nearbyFires.Count > 0)
                {
                    TakeDamage(encasedStructure.nearbyFires.Count);
                }
            }
            meltingParticles.Stop();
            meltingSource.Stop();
        }
    }

}
