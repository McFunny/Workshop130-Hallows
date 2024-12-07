using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class ParticlePoolManager : MonoBehaviour
{
    public static ParticlePoolManager Instance;

    public ParticleSystem dirtParticle;

    public VisualEffect hitEffect;

    public GameObject corpseParticle, corpseParticleYellow;

    List<GameObject> corpsePool = new List<GameObject>();
    List<GameObject> corpsePoolYellow = new List<GameObject>();

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else Instance = this;

        PopulateParticlePools();
    }

    void PopulateParticlePools()
    {
        //

        for(int i = 0; i < 5; i++)
        {
            GameObject newParticle = Instantiate(corpseParticle);
            corpsePool.Add(newParticle);
            newParticle.SetActive(false);
        }

        for(int i = 0; i < 5; i++)
        {
            GameObject newParticle = Instantiate(corpseParticleYellow);
            corpsePoolYellow.Add(newParticle);
            newParticle.SetActive(false);
        }
    }

    public GameObject GrabCorpseParticle(CorpseParticleType type)
    {
        if(type == CorpseParticleType.Red)
        {
            foreach (GameObject particle in corpsePool)
            {
                if(!particle.activeSelf)
                {
                    particle.SetActive(true);
                    return particle;
                }
            }

            //No available particles, must make a new one
            GameObject newParticle = Instantiate(corpseParticle);
            corpsePool.Add(newParticle);
            return newParticle;
        }
        else
        {
            foreach (GameObject particle in corpsePoolYellow)
            {
                if(!particle.activeSelf)
                {
                    particle.SetActive(true);
                    return particle;
                }
            }

            //No available particles, must make a new one
            GameObject newParticle = Instantiate(corpseParticleYellow);
            corpsePoolYellow.Add(newParticle);
            return newParticle;
        }
    }

    public void MoveAndPlayParticle(Vector3 pos, ParticleSystem p)
    {
        p.transform.position = pos;
        p.Play();
    }

    public void MoveAndPlayVFX(Vector3 pos, VisualEffect v)
    {
        v.transform.position = pos;
        v.Play();
    }
}

public enum CorpseParticleType
{
    Red,
    Yellow
}
