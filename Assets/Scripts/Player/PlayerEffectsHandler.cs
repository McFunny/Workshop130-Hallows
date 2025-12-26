using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Cinemachine;


public class PlayerEffectsHandler : MonoBehaviour
{
    //HANDLES THE AUDIO AND EFFECTS THAT COME FROM THE PLAYER
    public float volume = 1f;
    float originalPitch;
    public AudioSource source, footStepSource;
    public AudioClip itemPickup, itemEat, playerDie, playerDamage, waterJet, trip, playerHeal, heartBeat;
    public AudioClip grassFootsteps, stoneFootsteps, woodFootsteps;
    public AudioClip[] fleshFootsteps;
    AudioClip lastPlayedSteps;

    public LayerMask groundLayers;

    public float shakeIntensity;
    //public AudioClip footSteps;

    public CinemachineImpulseSource damageImpulse;
    //public CinemachineImpulseSource shakeImpulse;

    Volume globalVolume;
    public Volume lowHealthVolume;
    public Color damageColor, focusColor;
    Coroutine damageFlashCoroutine, lowHealthCoroutine;

    Rigidbody rb;

    public Material pixelRenderer;
    float pixelation, originalPixelation;
    public float pixelationFloor = 400;
    public float pixelationStep = 25;
    Coroutine pixelCoroutine;

    public bool onItemSoundCooldown = false;
    bool isFocusing = false;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        StartCoroutine("FootStepsPitchChanger");

        globalVolume = GameObject.Find("Global Volume").GetComponent<Volume>();

        PlayerInteraction p = PlayerInteraction.Instance;

        ResetVignette();

        originalPitch = source.pitch;
        lastPlayedSteps = grassFootsteps;

        originalPixelation = pixelRenderer.GetFloat("_pixelization");
        lowHealthCoroutine = null;
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
        if(onItemSoundCooldown) return;
        onItemSoundCooldown = true;
        StartCoroutine(ItemCollectCooldown());
        source.pitch = Random.Range(0.95f, 1.05f);
        source.PlayOneShot(itemPickup);
    }

    IEnumerator ItemCollectCooldown()
    {
        yield return new WaitForSeconds(0.1f);
        onItemSoundCooldown = false;
    }

    public void PlayerDamage()
    {
        if(damageFlashCoroutine != null) StopCoroutine(DamageFlash());
        //ResetVignette();
        damageFlashCoroutine = StartCoroutine(DamageFlash());
        damageImpulse.GenerateImpulseWithForce(shakeIntensity);

        if(pixelCoroutine != null) 
        {
            StopCoroutine(pixelCoroutine);
        }
        pixelCoroutine = StartCoroutine(DamagePixelization());

        if(lowHealthCoroutine == null) lowHealthCoroutine = StartCoroutine(LowHealthPulse());

        if(playerDamage)
        {
            source.pitch = Random.Range(0.8f, 1.2f);
            source.PlayOneShot(playerDamage);
        }

    }



    IEnumerator DamageFlash()
    {
        if(lowHealthVolume.profile.TryGet(out Vignette vignette2) != null) vignette2.color.Override(damageColor);
        if(globalVolume.profile.TryGet(out Vignette vignette))
        {
            vignette.color.Override(damageColor);
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
            damageFlashCoroutine = null;
        }

        if(vignette2) vignette2.color.Override(focusColor);
        
    }

    IEnumerator DamagePixelization()
    {
        pixelation = pixelRenderer.GetFloat("_pixelization");
        do
        {
            pixelation -= pixelationStep;
            if(pixelation < pixelationFloor) pixelation = pixelationFloor;
            pixelRenderer.SetFloat("_pixelization", pixelation); 
            yield return new WaitForSeconds(0.1f);
        }
        while(pixelation > pixelationFloor);
        yield return new WaitForSeconds(0.4f);
        do
        {
            yield return new WaitForSeconds(0.1f);
            pixelation += pixelationStep;
            pixelRenderer.SetFloat("_pixelization", pixelation); 
        }
        while(pixelation < originalPixelation);
        pixelation = originalPixelation;
        pixelRenderer.SetFloat("_pixelization", pixelation); 

        pixelCoroutine = null;
        
    }

    IEnumerator LowHealthPulse()
    {
        if(lowHealthVolume.profile.TryGet(out Vignette vignette) == null) yield break;

        while(PlayerInteraction.Instance.stamina <= 50)
        {
            print("Pulsing");
            do
            {
                yield return new WaitForSeconds(0.1f);
                vignette.intensity.value += 0.02f;
            }
            while(vignette.intensity.value < 0.7f);
            yield return new WaitForSeconds(0.1f);
            do
            {
                yield return new WaitForSeconds(0.1f);
                vignette.intensity.value -= 0.02f;
            }
            while(vignette.intensity.value > 0.45f);
            source.PlayOneShot(heartBeat);
        }

        do
        {
            yield return new WaitForSeconds(0.1f);
            vignette.intensity.value -= 0.05f;
        }
        while(vignette.intensity.value > 0);
        vignette.intensity.value = 0;

        lowHealthCoroutine = null;
        
    }

    public IEnumerator Focus()
    {
        if(isFocusing) yield break;
        isFocusing = true;
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
        isFocusing = false;
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
        source.pitch = originalPitch;
        source.PlayOneShot(clip, volume);
    }

    public void PlayFootstepSound()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -Vector3.up, out hit, 3, groundLayers))
        {
            if(hit.collider.gameObject.tag == "Stone_FootStepSurface")
            {
                footStepSource.clip = stoneFootsteps;
            }
            else if(hit.collider.gameObject.tag == "Wood_FootStepSurface")
            {
                footStepSource.clip = woodFootsteps;
            }
            else if(hit.collider.gameObject.tag == "Flesh_FootStepSurface")
            {
                footStepSource.clip = fleshFootsteps[Random.Range(0, fleshFootsteps.Length)];
            }
            else
            {
                footStepSource.clip = grassFootsteps;
            }

            lastPlayedSteps = footStepSource.clip;
        }
        else footStepSource.clip = lastPlayedSteps;
        footStepSource.pitch = Random.Range(0.7f, 1.3f);
        footStepSource.Play();
    }

    void OnDestroy()
    {
        pixelRenderer.SetFloat("_pixelization", originalPixelation); 
    }

}
