using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


public class PlayerEffectsHandler : MonoBehaviour
{
    //HANDLES THE AUDIO AND EFFECTS THAT COME FROM THE PLAYER
    public float volume = 1f;
    public AudioSource source, footStepSource;
    public AudioClip itemPickup, itemEat, playerDie, playerDamage, footstep;
    //public AudioClip footSteps;

    Volume globalVolume;
    public Color damageColor, focusColor;

    Rigidbody rb;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        StartCoroutine("FootStepsPitchChanger");

        globalVolume = FindObjectOfType<Volume>();

        PlayerInteraction p = PlayerInteraction.Instance;

        ResetVignette();
    }

    // Update is called once per frame
    void Update()
    {
        if(rb.velocity.magnitude > 0.2f) footStepSource.volume = 0.025f;
        else footStepSource.volume = 0f;
        
    }

    IEnumerator FootStepsPitchChanger()
    {
        do
        {
            yield return new WaitForSeconds(0.5f);
            footStepSource.pitch = Random.Range(0.7f, 1.3f);
        }
        while(gameObject.activeSelf);
    }

    public void ItemCollectSFX()
    {
        source.PlayOneShot(itemPickup);
    }

    public void PlayerDamage()
    {
        StopCoroutine(DamageFlash());
        StartCoroutine(DamageFlash());
            
        
    }

    IEnumerator DamageFlash()
    {
        if(globalVolume.profile.TryGet(out Vignette vignette))
        {
            vignette.color.Override(damageColor);
            //source.PlayOneShot(playerDamage);
            vignette.intensity.value = 0;
            do
            {
                yield return new WaitForSeconds(0.1f);
                vignette.intensity.value += 0.25f;
            }
            while(vignette.intensity.value < 0.5f);
            yield return new WaitForSeconds(1);
            do
            {
                yield return new WaitForSeconds(0.1f);
                vignette.intensity.value -= 0.05f;
            }
            while(vignette.intensity.value > 0);
            ResetVignette();
        }
        
    }

    public IEnumerator Focus()
    {
        if(globalVolume.profile.TryGet(out Vignette vignette))
        {
            vignette.color.Override(focusColor);
            vignette.intensity.value = 0;
            do
            {
                yield return new WaitForSecondsRealtime(0.1f);
                vignette.intensity.value += 0.25f;
            }
            while(vignette.intensity.value < .5f);

            yield return new WaitForSeconds(0.1f);

            while(PlayerMovement.restrictMovementTokens > 0)
            {
                yield return null;
            }

            do
            {
                yield return new WaitForSecondsRealtime(0.1f);
                vignette.intensity.value -= 0.05f;
            }
            while(vignette.intensity.value > 0);
            ResetVignette();
        }
    }

    void ResetVignette()
    {
        if(globalVolume.profile.TryGet(out Vignette vignette))
        {
            vignette.color.Override(focusColor);
            vignette.intensity.value = 0.25f;
        }
    }

    public void PlayClip(AudioClip clip)
    {
        source.PlayOneShot(clip);
    }

    public void PlayClip(AudioClip clip, float volume)
    {
        source.PlayOneShot(clip, volume);
    }

    public void PlayFootstepSound()
    {
        footStepSource.pitch = Random.Range(0.7f, 1.3f);
        footStepSource.Play();
    }

}
