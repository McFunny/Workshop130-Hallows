using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectManager : MonoBehaviour
{
    public static StatusEffectManager Instance;

    public GameObject dareVFX, burnVFX, frostVFX, mimicScentVFX;

    List<GameObject> darePool = new List<GameObject>();
    List<GameObject> burnPool = new List<GameObject>();
    List<GameObject> frostPool = new List<GameObject>();
    List<GameObject> mimicScentPool = new List<GameObject>();

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }

        //PopulateVFXPools();

    }

    void Start()
    {
        StartCoroutine(OneSecondTimer());
    }

    public bool FindStatusOnPlayer(StatusEffectName s)
    {
        if(PlayerInteraction.Instance.currentEffects.Count == 0)
        {
            return false;
        }
        for(int x = 0; x < PlayerInteraction.Instance.currentEffects.Count; x++)
        {
            //do the effects referencing the scriptable object here
            if(PlayerInteraction.Instance.currentEffects[x].effect.name == s)
            {
                return true;
            }
        }

        return false;
    }

    public bool FindStatusOnCreature(StatusEffectName s, CreatureBehaviorScript c)
    {
        if(c.currentEffects.Count == 0)
        {
            return false;
        }
        for(int x = 0; x < c.currentEffects.Count; x++)
        {
            //do the effects referencing the scriptable object here
            if(c.currentEffects[x].effect.name == s)
            {
                return true;
            }
        }

        return false;
    }

    public bool RemoveStatusOnPlayer(StatusEffectName s)
    {
        if(PlayerInteraction.Instance.currentEffects.Count == 0)
        {
            return false;
        }
        for(int x = 0; x < PlayerInteraction.Instance.currentEffects.Count; x++)
        {
            //do the effects referencing the scriptable object here
            if(PlayerInteraction.Instance.currentEffects[x].effect.name == s)
            {
                PlayerInteraction.Instance.currentEffects[x].remainingDuration = 0;
                return true;
            }
        }

        return false;
    }

    public bool RemoveStatusOnCreature(StatusEffectName s, CreatureBehaviorScript c)
    {
        if(c.currentEffects.Count == 0)
        {
            return false;
        }
        for(int x = 0; x < c.currentEffects.Count; x++)
        {
            //do the effects referencing the scriptable object here
            if(c.currentEffects[x].effect.name == s)
            {
                c.currentEffects[x].remainingDuration = 0;
                return true;
            }
        }

        return false;
    }

    IEnumerator OneSecondTimer()
    {
        PlayerInteraction p = PlayerInteraction.Instance;
        while(true)
        {
            yield return new WaitForSeconds(1);
            //cycle through afflicted

            if(p.currentEffects.Count > 0)
            {
                for(int x = 0; x < p.currentEffects.Count; x++)
                {
                    if(p.currentEffects[x].remainingDuration > 0) p.currentEffects[x].remainingDuration -= 1;
                    if(p.currentEffects[x].remainingDuration == 0)
                    {
                        p.currentEffects[x].effect.OnEffectRemoved();
                        p.currentEffects.RemoveAt(x);
                        x--;
                        continue;
                    }

                    //do the effects referencing the scriptable object here
                    if(p.currentEffects[x].effect)
                    {
                        p.currentEffects[x].effect.TimedEffect();
                    }
                    else
                    {
                        p.currentEffects.RemoveAt(x);
                        x--;
                        continue;
                    }
                }
            }

            /*for(int i = 0; i < NightSpawningManager.Instance.allCreatures.Count; i++)
            {
                if(NightSpawningManager.Instance.allCreatures[i] == null || NightSpawningManager.Instance.allCreatures[i].currentEffects.Count == 0)
                {
                    continue;
                }

                for(int x = 0; x < NightSpawningManager.Instance.allCreatures[i].currentEffects.Count; x++)
                {
                    StatusEffect currentEffect = NightSpawningManager.Instance.allCreatures[i].currentEffects[x];
                    //do the effects referencing the scriptable object here
                    if(currentEffect.effect)
                    {
                        currentEffect.effect.TimedEffect(NightSpawningManager.Instance.allCreatures[i]);
                    }

                    if(currentEffect.remainingDuration > 0) currentEffect.remainingDuration -= 1;
                    if(currentEffect.remainingDuration == 0)
                    {
                        currentEffect.effect.OnEffectRemoved(NightSpawningManager.Instance.allCreatures[i]);
                        NightSpawningManager.Instance.allCreatures[i].currentEffects.RemoveAt(x);
                        x--;
                    }
                }
            }*/
            
        }
    }

    void PopulateVFXPools()
    {
        GameObject newParticle;
        Vector3 origin = new Vector3(500,500,500);

        for(int i = 0; i < 5; i++)
        {
            newParticle = Instantiate(dareVFX, origin, Quaternion.identity);
            darePool.Add(newParticle);
            newParticle.SetActive(false);
        }

        for(int i = 0; i < 5; i++)
        {
            newParticle = Instantiate(burnVFX, origin, Quaternion.identity);
            burnPool.Add(newParticle);
            newParticle.SetActive(false);
        }

        for(int i = 0; i < 5; i++)
        {
            newParticle = Instantiate(frostVFX, origin, Quaternion.identity);
            frostPool.Add(newParticle);
            newParticle.SetActive(false);
        }

        for(int i = 0; i < 5; i++)
        {
            newParticle = Instantiate(mimicScentVFX, origin, Quaternion.identity);
            mimicScentPool.Add(newParticle);
            newParticle.SetActive(false);
        }
    }

    public GameObject GrabStatusVFX(StatusEffectName name)
    {
        if(name == StatusEffectName.Dare)
        {
            foreach (GameObject particle in darePool)
            {
                if(!particle.activeSelf)
                {
                    particle.SetActive(true);
                    return particle;
                }
            }

            //No available particles, must make a new one
            GameObject newParticle = Instantiate(dareVFX);
            darePool.Add(newParticle);
            return newParticle;
        }

        if(name == StatusEffectName.Fire)
        {
            foreach (GameObject particle in burnPool)
            {
                if(!particle.activeSelf)
                {
                    particle.SetActive(true);
                    return particle;
                }
            }

            //No available particles, must make a new one
            GameObject newParticle = Instantiate(burnVFX);
            burnPool.Add(newParticle);
            return newParticle;
        }

        if(name == StatusEffectName.Frost)
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
            GameObject newParticle = Instantiate(frostVFX);
            frostPool.Add(newParticle);
            return newParticle;
        }

        if(name == StatusEffectName.MimicScent)
        {
            foreach (GameObject particle in mimicScentPool)
            {
                if(!particle.activeSelf)
                {
                    particle.SetActive(true);
                    return particle;
                }
            }

            //No available particles, must make a new one
            GameObject newParticle = Instantiate(mimicScentVFX);
            mimicScentPool.Add(newParticle);
            return newParticle;
        }

        return null;
    }
}

public enum StatusEffectName
{
    Fire, //DOT
    Frost, //Slow movespeed, cannot use water
    Dare, //1.25 speed increase, 1.5 oncoming damage
    MimicScent //Mimics attack the host
}

[System.Serializable]
public class StatusEffect
{
    public StatusEffectObject effect;
    public int remainingDuration;

    public StatusEffect(StatusEffectObject _effect, int _duration)
    {
        effect = _effect;
        remainingDuration = _duration;
    }
}
