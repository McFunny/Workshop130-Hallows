using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectManager : MonoBehaviour
{
    public static StatusEffectManager Instance;

    public List<CreatureBehaviorScript> effectedCreatures = new List<CreatureBehaviorScript>();
    //public bool isPlayerAfflicted = false;

    //Should probably handle this stuff using scriptable objects tbh
    //Also handle the pooling of effects objects (Fire particles, dare particles, ect)
    //Have the effect object hold the reference to the creature in conjunction with the list here. Once the creature does not have the effect, it removes itself and the reference here

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

                    if(p.currentEffects[x].remainingDuration > 0) p.currentEffects[x].remainingDuration -= 1;
                    if(p.currentEffects[x].remainingDuration == 0)
                    {
                        p.currentEffects.RemoveAt(x);
                        x--;
                    }
                }
            }

            /*for(int i = 0; i < effectedCreatures.Count; i++)
            {
                if(effectedCreatures[i] == null || effectedCreatures[i].currentEffects.Count == 0)
                {
                    effectedCreatures.RemoveAt(i);
                    i--;
                    continue;
                }

                //effectedCreatures
                //for loop for every status effect in the effect things object. Call the functions in the scriptable objects to do the effect, and remove it if duration is under 0
            }*/
        }
    }
}

public enum StatusEffectName
{
    Fire, //DOT
    Frosted, //Slow movespeed, cannot use water
    Dare //1.25 speed increase, 1.5 oncoming damage
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
