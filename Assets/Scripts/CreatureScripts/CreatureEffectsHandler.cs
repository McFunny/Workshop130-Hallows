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
    public AudioClip[] idleSounds;
    public AudioClip[] hitSounds;
    public AudioClip[] footSteps;
    public AudioClip deathSound;
    public AudioClip miscSound;
    public AudioClip miscSound2;



    float r;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        originalPitch = source.pitch;
    }

    public void OnMove(float _volume)
    {
        r = Random.Range(pitchMin,pitchMax);
        source.PlayOneShot(moveSound, volume);
    }

    public void Idle1()
    {
        r = Random.Range(pitchMin,pitchMax);
        //source.pitch = originalPitch + r;
        source.PlayOneShot(idleSound1, volume);
    }

    public void Idle2()
    {
        r = Random.Range(pitchMin,pitchMax);
        //source.pitch = originalPitch + r;
        source.PlayOneShot(idleSound2, volume);
    }

    public void RandomIdle()
    {
        r = Random.Range(pitchMin,pitchMax);
        int i = Random.Range(0, idleSounds.Length);
        source.PlayOneShot(idleSounds[i], volume);
    }

    public void OnHit()
    {
        source.Stop();
        if(hitSounds.Length == 0)
        {
            r = Random.Range(0,1);
            if(r > 0.5f) Idle2();
            else Idle1();
        }
        else
        {
            r = Random.Range(pitchMin,pitchMax);
            //source.pitch = originalPitch + r;
            int i = Random.Range(0, hitSounds.Length);
            source.PlayOneShot(hitSounds[i], volume);
        }
        
    }

    public void OnDeath()
    {
        r = Random.Range(pitchMin,pitchMax);
        if (!source) return;
        //source.pitch = originalPitch + r;
        source.PlayOneShot(deathSound, volume);
    }

    public void MiscSound()
    {
        if(miscSound != null) source.PlayOneShot(miscSound, volume);
    }

    public void MiscSound2()
    {
        if(miscSound2 != null) source.PlayOneShot(miscSound2, volume);
    }
}
