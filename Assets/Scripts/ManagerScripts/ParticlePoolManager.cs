using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class ParticlePoolManager : MonoBehaviour
{
    public static ParticlePoolManager Instance;

    public ParticleSystem dirtParticle;

    public VisualEffect hitEffect;

    public GameObject corpseParticle, corpseParticleYellow, poofParticle, bloodDropletParticle;

    List<GameObject> corpsePool = new List<GameObject>();
    List<GameObject> corpsePoolYellow = new List<GameObject>();
    List<GameObject> poofPool = new List<GameObject>();
    List<GameObject> bloodDropPool = new List<GameObject>();

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
        GameObject newParticle;

        for(int i = 0; i < 5; i++)
        {
            newParticle = Instantiate(corpseParticle);
            corpsePool.Add(newParticle);
            newParticle.SetActive(false);
        }

        for(int i = 0; i < 5; i++)
        {
            newParticle = Instantiate(corpseParticleYellow);
            corpsePoolYellow.Add(newParticle);
            newParticle.SetActive(false);
        }

        for(int i = 0; i < 5; i++)
        {
            newParticle = Instantiate(poofParticle);
            poofPool.Add(newParticle);
            newParticle.SetActive(false);
        }

        for(int i = 0; i < 5; i++)
        {
            newParticle = Instantiate(bloodDropletParticle);
            bloodDropPool.Add(newParticle);
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
        else if(type == CorpseParticleType.Yellow)
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
        else return null;
    }

    public GameObject GrabPoofParticle()
    {
        foreach (GameObject particle in poofPool)
        {
            if(!particle.activeSelf)
            {
                particle.SetActive(true);
                return particle;
            }
        }

        //No available particles, must make a new one
        GameObject newParticle = Instantiate(poofParticle);
        poofPool.Add(newParticle);
        return newParticle;
    }

    public GameObject GrabBloodDropParticle()
    {
        foreach (GameObject particle in bloodDropPool)
        {
            if(!particle.activeSelf)
            {
                particle.SetActive(true);
                return particle;
            }
        }

        //No available particles, must make a new one
        GameObject newParticle = Instantiate(bloodDropletParticle);
        bloodDropPool.Add(newParticle);
        return newParticle;
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
    Yellow,
    Null
}
