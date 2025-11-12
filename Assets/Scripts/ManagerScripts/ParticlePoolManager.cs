using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class ParticlePoolManager : MonoBehaviour
{
    public static ParticlePoolManager Instance;

    public ParticleSystem dirtParticle;

    public VisualEffect hitEffect;

    public GameObject corpseParticle, corpseParticleYellow, corruptedCorpseParticle, poofParticle, extinguishParticle, bloodDropletParticle, corruptBloodDropletParticle, sparksParticle, flameEffect, dirtPixelParticle, 
    explosionParticle, cloudParticle, frostParticle, thawParticle, frostBurstParticle, splashParticle, impactParticle, bugSplatParticle, elecZapParticle, heartParticles, slimeSplash, 
    slimeSplashLarge, orangeHitParticle, cleanseParticle, whiteHitParticle;

    public GameObject woodDestructionP, metalDestructionP, gloomDestructionP, stoneDestructionP, robotDestructionP, c_fleshDestructionP;

    List<GameObject> corpsePool = new List<GameObject>();
    List<GameObject> corpsePoolYellow = new List<GameObject>();
    List<GameObject> corruptedCorpsePool = new List<GameObject>();
    List<GameObject> poofPool = new List<GameObject>();
    List<GameObject> extinguishPool = new List<GameObject>();
    List<GameObject> bloodDropPool = new List<GameObject>();
    List<GameObject> corruptBloodDropPool = new List<GameObject>();
    List<GameObject> sparkPool = new List<GameObject>();
    List<GameObject> flamePool = new List<GameObject>();
    List<GameObject> dirtPixelPool = new List<GameObject>();
    List<GameObject> explosionPool = new List<GameObject>();
    List<GameObject> cloudPool = new List<GameObject>();
    List<GameObject> frostPool = new List<GameObject>();
    List<GameObject> thawPool = new List<GameObject>();
    List<GameObject> frostBurstPool = new List<GameObject>();
    List<GameObject> splashPool = new List<GameObject>();
    List<GameObject> impactPool = new List<GameObject>();
    List<GameObject> bugSplatPool = new List<GameObject>();
    List<GameObject> elecZapPool = new List<GameObject>();
    List<GameObject> heartPool = new List<GameObject>();
    List<GameObject> slimeSplashPool = new List<GameObject>();
    List<GameObject> slimeSplashLargePool = new List<GameObject>();
    List<GameObject> orangeHitPool = new List<GameObject>();
    List<GameObject> cleansePool = new List<GameObject>();
    List<GameObject> whiteHitPool = new List<GameObject>();

    //Destruction
    List<GameObject> woodPool = new List<GameObject>();
    List<GameObject> metalPool = new List<GameObject>();
    List<GameObject> gloomPool = new List<GameObject>();
    List<GameObject> stonePool = new List<GameObject>();
    List<GameObject> robotPool = new List<GameObject>();
    List<GameObject> c_fleshPool = new List<GameObject>();

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
            newParticle = Instantiate(extinguishParticle);
            extinguishPool.Add(newParticle);
            newParticle.SetActive(false);
        }

        for(int i = 0; i < 5; i++)
        {
            newParticle = Instantiate(bloodDropletParticle);
            bloodDropPool.Add(newParticle);
            newParticle.SetActive(false);
        }

        for(int i = 0; i < 5; i++)
        {
            newParticle = Instantiate(sparksParticle);
            sparkPool.Add(newParticle);
            newParticle.SetActive(false);
        }

        for(int i = 0; i < 5; i++)
        {
            newParticle = Instantiate(flameEffect);
            flamePool.Add(newParticle);
            newParticle.SetActive(false);
        }

        for(int i = 0; i < 3; i++)
        {
            newParticle = Instantiate(explosionParticle);
            explosionPool.Add(newParticle);
            newParticle.SetActive(false);
        }

        for(int i = 0; i < 5; i++)
        {
            newParticle = Instantiate(cloudParticle);
            cloudPool.Add(newParticle);
            newParticle.SetActive(false);
        }

        for(int i = 0; i < 5; i++)
        {
            newParticle = Instantiate(frostParticle);
            frostPool.Add(newParticle);
            newParticle.SetActive(false);
        }

        for(int i = 0; i < 5; i++)
        {
            newParticle = Instantiate(thawParticle);
            thawPool.Add(newParticle);
            newParticle.SetActive(false);
        }

        for(int i = 0; i < 5; i++)
        {
            newParticle = Instantiate(frostBurstParticle);
            frostBurstPool.Add(newParticle);
            newParticle.SetActive(false);
        }

        for(int i = 0; i < 3; i++)
        {
            newParticle = Instantiate(splashParticle);
            splashPool.Add(newParticle);
            newParticle.SetActive(false);
        }

        for(int i = 0; i < 5; i++)
        {
            newParticle = Instantiate(impactParticle);
            impactPool.Add(newParticle);
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
        else if(type == CorpseParticleType.Metal)
        {
            foreach (GameObject particle in robotPool)
            {
                if(!particle.activeSelf)
                {
                    particle.SetActive(true);
                    return particle;
                }
            }

            //No available particles, must make a new one
            GameObject newParticle = Instantiate(robotDestructionP);
            robotPool.Add(newParticle);
            return newParticle;
        }
        else if(type == CorpseParticleType.Slime)
        {
            foreach (GameObject particle in slimeSplashLargePool)
            {
                if(!particle.activeSelf)
                {
                    particle.SetActive(true);
                    return particle;
                }
            }

            //No available particles, must make a new one
            GameObject newParticle = Instantiate(slimeSplashLarge);
            slimeSplashLargePool.Add(newParticle);
            return newParticle;
        }
        else if(type == CorpseParticleType.Corrupted)
        {
            foreach (GameObject particle in corruptedCorpsePool)
            {
                if(!particle.activeSelf)
                {
                    particle.SetActive(true);
                    return particle;
                }
            }

            //No available particles, must make a new one
            GameObject newParticle = Instantiate(corruptedCorpseParticle);
            corruptedCorpsePool.Add(newParticle);
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

    public GameObject GrabExtinguishParticle()
    {
        foreach (GameObject particle in extinguishPool)
        {
            if(!particle.activeSelf)
            {
                particle.SetActive(true);
                return particle;
            }
        }

        //No available particles, must make a new one
        GameObject newParticle = Instantiate(extinguishParticle);
        extinguishPool.Add(newParticle);
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

    public GameObject GrabCorruptBloodDropParticle()
    {
        foreach (GameObject particle in corruptBloodDropPool)
        {
            if(!particle.activeSelf)
            {
                particle.SetActive(true);
                return particle;
            }
        }

        //No available particles, must make a new one
        GameObject newParticle = Instantiate(corruptBloodDropletParticle);
        corruptBloodDropPool.Add(newParticle);
        return newParticle;
    }
    public GameObject GrabSparkParticle()
    {
        foreach (GameObject particle in sparkPool)
        {
            if(!particle.activeSelf)
            {
                particle.SetActive(true);
                return particle;
            }
        }

        //No available particles, must make a new one
        GameObject newParticle = Instantiate(sparksParticle);
        sparkPool.Add(newParticle);
        return newParticle;
    }

    public GameObject GrabFlameEffect()
    {
        foreach (GameObject effect in flamePool)
        {
            if(!effect.activeSelf)
            {
                effect.SetActive(true);
                return effect;
            }
        }

        //No available particles, must make a new one
        GameObject newEffect = Instantiate(flameEffect);
        flamePool.Add(newEffect);
        return newEffect;
    }

    public GameObject GrabDirtPixelParticle()
    {
        foreach (GameObject particle in dirtPixelPool)
        {
            if(!particle.activeSelf)
            {
                particle.SetActive(true);
                return particle;
            }
        }

        //No available particles, must make a new one
        GameObject newParticle = Instantiate(dirtPixelParticle);
        dirtPixelPool.Add(newParticle);
        return newParticle;
    }

    public GameObject GrabExplosionParticle()
    {
        foreach (GameObject particle in explosionPool)
        {
            if(!particle.activeSelf)
            {
                particle.SetActive(true);
                return particle;
            }
        }

        //No available particles, must make a new one
        GameObject newParticle = Instantiate(explosionParticle);
        explosionPool.Add(newParticle);
        return newParticle;
    }

    public GameObject GrabCloudParticle()
    {
        foreach (GameObject particle in cloudPool)
        {
            if(!particle.activeSelf)
            {
                particle.SetActive(true);
                return particle;
            }
        }

        //No available particles, must make a new one
        GameObject newParticle = Instantiate(cloudParticle);
        cloudPool.Add(newParticle);
        return newParticle;
    }

    public GameObject GrabFrostParticle()
    {
        foreach (GameObject particle in frostPool)
        {
            if(!particle.activeSelf)
            {
                particle.SetActive(true);
                return particle;
            }
        }

        //No available particles, must make a new one
        GameObject newParticle = Instantiate(frostParticle);
        frostPool.Add(newParticle);
        return newParticle;
    }

    public GameObject GrabThawParticle()
    {
        foreach (GameObject particle in thawPool)
        {
            if(!particle.activeSelf)
            {
                particle.SetActive(true);
                return particle;
            }
        }

        //No available particles, must make a new one
        GameObject newParticle = Instantiate(thawParticle);
        thawPool.Add(newParticle);
        return newParticle;
    }

    public GameObject GrabFrostBurstParticle()
    {
        foreach (GameObject particle in frostBurstPool)
        {
            if(!particle.activeSelf)
            {
                particle.SetActive(true);
                return particle;
            }
        }

        //No available particles, must make a new one
        GameObject newParticle = Instantiate(frostBurstParticle);
        frostBurstPool.Add(newParticle);
        return newParticle;
    }

    public GameObject GrabSplashParticle()
    {
        foreach (GameObject particle in splashPool)
        {
            if(!particle.activeSelf)
            {
                particle.SetActive(true);
                return particle;
            }
        }

        //No available particles, must make a new one
        GameObject newParticle = Instantiate(splashParticle);
        splashPool.Add(newParticle);
        return newParticle;
    }

    public GameObject GrabDestructionParticle(StructureType type)
    {
        if(type == StructureType.Null) return null;
        if(type == StructureType.Wood)
        {
            foreach (GameObject particle in woodPool)
            {
                if(!particle.activeSelf)
                {
                    particle.SetActive(true);
                    return particle;
                }
            }

            //No available particles, must make a new one
            GameObject newParticle = Instantiate(woodDestructionP);
            woodPool.Add(newParticle);
            return newParticle;
        }
        else if(type == StructureType.Metal)
        {
            foreach (GameObject particle in metalPool)
            {
                if(!particle.activeSelf)
                {
                    particle.SetActive(true);
                    return particle;
                }
            }

            //No available particles, must make a new one
            GameObject newParticle = Instantiate(metalDestructionP);
            metalPool.Add(newParticle);
            return newParticle;
        }
        else if(type == StructureType.Hay)
        {
            foreach (GameObject particle in gloomPool)
            {
                if(!particle.activeSelf)
                {
                    particle.SetActive(true);
                    return particle;
                }
            }

            //No available particles, must make a new one
            GameObject newParticle = Instantiate(gloomDestructionP);
            gloomPool.Add(newParticle);
            return newParticle;
        }
        else if(type == StructureType.Stone)
        {
            foreach (GameObject particle in stonePool)
            {
                if(!particle.activeSelf)
                {
                    particle.SetActive(true);
                    return particle;
                }
            }

            //No available particles, must make a new one
            GameObject newParticle = Instantiate(stoneDestructionP);
            stonePool.Add(newParticle);
            return newParticle;
        }
        else if(type == StructureType.CorruptedFlesh)
        {
            foreach (GameObject particle in c_fleshPool)
            {
                if(!particle.activeSelf)
                {
                    particle.SetActive(true);
                    return particle;
                }
            }

            //No available particles, must make a new one
            GameObject newParticle = Instantiate(c_fleshDestructionP);
            c_fleshPool.Add(newParticle);
            return newParticle;
        }
        else return null;
    }

    public GameObject GrabImpactParticle()
    {
        foreach (GameObject particle in impactPool)
        {
            if(!particle.activeSelf)
            {
                particle.SetActive(true);
                return particle;
            }
        }

        //No available particles, must make a new one
        GameObject newParticle = Instantiate(impactParticle);
        impactPool.Add(newParticle);
        return newParticle;
    }

    public GameObject GrabBugSplatParticle()
    {
        foreach (GameObject particle in bugSplatPool)
        {
            if(!particle.activeSelf)
            {
                particle.SetActive(true);
                return particle;
            }
        }

        //No available particles, must make a new one
        GameObject newParticle = Instantiate(bugSplatParticle);
        bugSplatPool.Add(newParticle);
        return newParticle;
    }

    public GameObject GrabElecZapParticle()
    {
        foreach (GameObject particle in elecZapPool)
        {
            if(!particle.activeSelf)
            {
                particle.SetActive(true);
                return particle;
            }
        }

        //No available particles, must make a new one
        GameObject newParticle = Instantiate(elecZapParticle);
        elecZapPool.Add(newParticle);
        return newParticle;
    }

    public GameObject GrabHeartParticle()
    {
        foreach (GameObject particle in heartPool)
        {
            if(!particle.activeSelf)
            {
                particle.SetActive(true);
                return particle;
            }
        }

        //No available particles, must make a new one
        GameObject newParticle = Instantiate(heartParticles);
        heartPool.Add(newParticle);
        return newParticle;
    }

    public GameObject GrabSlimeSplashParticle()
    {
        foreach (GameObject particle in slimeSplashPool)
        {
            if(!particle.activeSelf)
            {
                particle.SetActive(true);
                return particle;
            }
        }

        //No available particles, must make a new one
        GameObject newParticle = Instantiate(slimeSplash);
        slimeSplashPool.Add(newParticle);
        return newParticle;
    }

    public GameObject GrabOrangeHitParticle()
    {
        foreach (GameObject particle in orangeHitPool)
        {
            if(!particle.activeSelf)
            {
                particle.SetActive(true);
                return particle;
            }
        }

        //No available particles, must make a new one
        GameObject newParticle = Instantiate(orangeHitParticle);
        orangeHitPool.Add(newParticle);
        return newParticle;
    }

    public GameObject GrabWhiteHitParticle()
    {
        foreach (GameObject particle in whiteHitPool)
        {
            if(!particle.activeSelf)
            {
                particle.SetActive(true);
                return particle;
            }
        }

        //No available particles, must make a new one
        GameObject newParticle = Instantiate(whiteHitParticle);
        whiteHitPool.Add(newParticle);
        return newParticle;
    }

    public GameObject GrabCleanseParticle()
    {
        foreach (GameObject particle in cleansePool)
        {
            if(!particle.activeSelf)
            {
                particle.SetActive(true);
                return particle;
            }
        }

        //No available particles, must make a new one
        GameObject newParticle = Instantiate(cleanseParticle);
        cleansePool.Add(newParticle);
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
    Null,
    Metal,
    Slime,
    Corrupted
}

public enum StructureType
{
    Null,
    Wood,
    Metal,
    Hay,
    Stone,
    CorruptedFlesh
}
