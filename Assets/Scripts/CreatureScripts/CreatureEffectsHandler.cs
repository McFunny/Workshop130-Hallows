using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureEffectsHandler : MonoBehaviour
{
    //This script handles the audio and particle triggers for the enemies
    public float volume = 1f;
    public float pitchMin = 0;
    public float pitchMax = 0;
    float originalPitch;
    [HideInInspector]
    public AudioSource source;
    public AudioClip moveSound;
    public AudioClip idleSound1;
    public AudioClip idleSound2;
    public AudioClip hitSound;
    public AudioClip deathSound;
    public AudioClip miscSound;


    public ParticleSystem hitParticles;
    public GameObject deathParticles;


    float r;

    void Start()
    {
        source = GetComponent<AudioSource>();
        originalPitch = source.pitch;
    }

    public void OnMove(float _volume)
    {
        source.PlayOneShot(moveSound, _volume);
    }

    public void Idle1()
    {
        r = Random.Range(pitchMin,pitchMax);
        source.pitch = originalPitch + r;
        source.PlayOneShot(idleSound1, volume);
    }

    public void Idle2()
    {
        r = Random.Range(pitchMin,pitchMax);
        source.pitch = originalPitch + r;
        source.PlayOneShot(idleSound2, volume);
    }

    public void OnHit()
    {
        if(hitSound == null)
        {
            r = Random.Range(0,1);
            if(r > 0.5f) Idle2();
            else Idle1();
        }
        else
        {
            r = Random.Range(pitchMin,pitchMax);
            source.pitch = originalPitch + r;
            source.PlayOneShot(hitSound, volume);
        }
        if(hitParticles != null) hitParticles.Play();
        
    }

    public void OnDeath()
    {
        r = Random.Range(pitchMin,pitchMax);
        if (!source) return;
        source.pitch = originalPitch + r;
        source.PlayOneShot(deathSound, volume);
        if(hitParticles != null) hitParticles.Play();
    }

    public void MiscSound()
    {
        if(miscSound != null) source.PlayOneShot(miscSound, volume);
    }

    public void DeathParticles()
    {
        Instantiate(deathParticles, transform.position, transform.rotation);
    }
}
