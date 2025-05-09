using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectManager : MonoBehaviour
{
    public static StatusEffectManager Instance;

    public List<AfflictedObjects> effectedThings = new List<AfflictedObjects>();

    //Should probably handle this stuff using scriptable objects tbh
    //Also handle the pooling of effects objects (Fire particles, dare particles, ect)

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

        //StartCoroutine(OneSecondTimer());
    }

    /*public void ApplyStatusToCreature(CreatureBehaviorScript c)
    {
        //
    }

    public void ApplyStatusToPlayer()
    {
        //
    }

    public void RemoveStatusFromCreature(CreatureBehaviorScript c)
    {
        //
    }

    public void RemoveStatusFromPlayer()
    {
        //
    }

    public bool CheckStatus(CreatureBehaviorScript c, StatusEffectName e)
    {
        //
    }

    public bool CheckStatus(StatusEffectName e)
    {
        //
    }

    IEnumerator OneSecondTimer()
    {
        while(true)
        {
            yield return new WaitForSeconds(1);
            //cycle through afflicted
            for(int i = 0; i < effectedThings.Count; i++)
            {
                if(effectedThings[i] == null || effectedThings[i].currentEffects.Count == 0)
                {
                    effectedThings.RemoveAt(i);
                    i--;
                    continue;
                }

                //effectedThings
                //for loop for every status effect in the effect things object. Call the functions in the scriptable objects to do the effect, and remove it if duration is under 0
            }
        }
    }*/
}

public enum StatusEffectName
{
    Fire, //DOT
    Frosted, //Slow movespeed, cannot use water
    Dare //1.25 speed increase, 1.5 oncoming damage
}

public class StatusEffect
{
    public StatusEffectName effect;
    public float remainingDuration;
}

public class AfflictedObjects
{
    public bool playerAfflicted; //Marked yes if afflicted thing is the player
    public CreatureBehaviorScript creature; //Holds reference of afflicted creature
    public List<StatusEffect> currentEffects = new List<StatusEffect>();
}
