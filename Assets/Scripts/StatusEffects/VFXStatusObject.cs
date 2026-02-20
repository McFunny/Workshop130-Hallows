using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VFXStatusObject : MonoBehaviour
{
    public CreatureBehaviorScript afflictedCreature;
    public StatusEffectName name;
    public StatusEffectObject statusObject;

    public List<ParticleSystem> allParticles = new List<ParticleSystem>();

    bool onPlayer = false;
    bool burningCorpse = false; //Specifically to track Carrion Cooker trinket

    FireObject fireObject;

    public Transform followTransform;

    public AudioClip appliedSFX, removedSFX;

    public Volume statusVolume;


    void OnEnable()
    {
        if(statusVolume)
        {
            statusVolume.weight = 0;
            StartCoroutine(VolumeSmoothing());
        }
        StartCoroutine(CheckForStatus());
        //if(transform.parent != null) transform.parent = null;
        followTransform = null;
        if(name == StatusEffectName.Fire && !fireObject) fireObject = GetComponent<FireObject>();

        foreach(ParticleSystem p in allParticles)
        {
            p.Play();
        }

        if(appliedSFX && AudioPoolManager.Instance) AudioPoolManager.Instance.PlayClipAtPosition(appliedSFX, transform.position);
    }

    void OnDisable()
    {
        if(!gameObject.scene.isLoaded) return;
        StopCoroutine(CheckForStatus());
        onPlayer = false;
        followTransform = null;

        if(removedSFX) AudioPoolManager.Instance.PlayClipAtPosition(removedSFX, transform.position);

        if(burningCorpse)
        {
            AchievementManager.Instance.AddProgressWithEnum(ACHKey.Corpse_Burn, 1);
            if(TrinketInventoryHandler.Instance.CheckForTrinket(TrinketKey.CarrionCooker)) TrinketInventoryHandler.Instance.ApplyTrinketDamage(TrinketKey.CarrionCooker);
        }

        afflictedCreature = null;
        burningCorpse = false;
    }

    void Update()
    {
        if(followTransform) transform.position = followTransform.position;
    }

    IEnumerator VolumeSmoothing()
    {
        while(statusVolume.weight < 1)
        {
            yield return new WaitForSeconds(0.1f);
            statusVolume.weight += 0.1f;
        }
        if(!onPlayer) statusVolume.weight = 0;
    }

    IEnumerator VolumeRemovalSmoothing()
    {
        while(statusVolume.weight > 0)
        {
            yield return new WaitForSeconds(0.1f);
            statusVolume.weight -= 0.1f;
        }
    }

    IEnumerator CheckForStatus()
    {
        yield return new WaitForSeconds(0.1f);
        bool clearThis = false;
        if(afflictedCreature == null) onPlayer = true;
        while(!clearThis)
        {
            yield return new WaitForSeconds(1);

            if(onPlayer)
            {
                if(PlayerInteraction.Instance.currentEffects.Count == 0) clearThis = true;
                else
                {
                    for(int x = 0; x < PlayerInteraction.Instance.currentEffects.Count; x++)
                    {
                        //do the effects referencing the scriptable object here
                        if(PlayerInteraction.Instance.currentEffects[x].effect.name == name)
                        {
                            if(PlayerInteraction.Instance.currentEffects[x].remainingDuration == 0) clearThis = true;
                            break;
                        }
                    }
                }
            }
            else
            {
                if(!afflictedCreature || afflictedCreature.currentEffects.Count == 0) clearThis = true;
                else
                {
                    for(int x = 0; x < afflictedCreature.currentEffects.Count; x++)
                    {
                        //do the effects referencing the scriptable object here
                        if(afflictedCreature.currentEffects[x].effect.name == name)
                        {
                            afflictedCreature.currentEffects[x].effect.TimedEffect(afflictedCreature);

                            if(fireObject)
                            {
                                if(TrinketInventoryHandler.Instance.CheckForTrinket(TrinketKey.CarrionCooker)) yield return new WaitForSeconds(1); //Makes it burn for longer
                                if(afflictedCreature.health < 0) 
                                {
                                    burningCorpse = true;
                                    break; //Makes it burn forever until death
                                }
                            }

                            if(afflictedCreature.currentEffects[x].remainingDuration > 0) afflictedCreature.currentEffects[x].remainingDuration -= 1;
                            if(afflictedCreature.currentEffects[x].remainingDuration == 0)
                            {
                                afflictedCreature.currentEffects[x].effect.OnEffectRemoved(afflictedCreature);
                                afflictedCreature.currentEffects.RemoveAt(x);
                                clearThis = true;
                                break;
                            }

                            break;
                        }
                    }
                }
            }
        }

        foreach(ParticleSystem p in allParticles)
        {
           p.Stop();
        }

        if(fireObject) 
        {
            fireObject.Extinguished();
        }

        if(statusVolume) StartCoroutine(VolumeRemovalSmoothing());

        yield return new WaitForSeconds(2);

        if(clearThis)
        {
            gameObject.SetActive(false);
        }
    }
}
