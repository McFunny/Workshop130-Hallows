using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXStatusObject : MonoBehaviour
{
    public CreatureBehaviorScript afflictedCreature;
    public StatusEffectName name;
    public StatusEffectObject statusObject;

    public List<ParticleSystem> allParticles = new List<ParticleSystem>();

    bool onPlayer = false;

    FireObject fireObject;

    public Transform followTransform;

    public AudioClip appliedSFX, removedSFX;


    void OnEnable()
    {
        StartCoroutine(CheckForStatus());
        //if(transform.parent != null) transform.parent = null;
        followTransform = null;
        if(name == StatusEffectName.Fire && !fireObject) fireObject = GetComponent<FireObject>();

        foreach(ParticleSystem p in allParticles)
        {
            p.Play();
        }

        if(appliedSFX) AudioPoolManager.Instance.PlayClipAtPosition(appliedSFX, transform.position);
    }

    void OnDisable()
    {
        if(!gameObject.scene.isLoaded) return;
        StopCoroutine(CheckForStatus());
        onPlayer = false;
        followTransform = null;

        if(removedSFX) AudioPoolManager.Instance.PlayClipAtPosition(removedSFX, transform.position);
    }

    void Update()
    {
        if(followTransform) transform.position = followTransform.position;
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

        if(fireObject) fireObject.Extinguished();

        yield return new WaitForSeconds(2);

        if(clearThis)
        {
            gameObject.SetActive(false);
        }
    }
}
