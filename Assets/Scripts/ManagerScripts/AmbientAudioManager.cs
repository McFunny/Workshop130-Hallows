using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientAudioManager : MonoBehaviour
{
    public AudioSource ambienceSource, musicSource;
    public AudioClip[] biomeAmbience;
    public AudioClip[] nightAmbience;
    public AudioClip[] musicAmbience;

    public AudioClip bellTower;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("PlayAmbientTrack");
        StartCoroutine("PlayAmbientMusic");

        TimeManager.OnHourlyUpdate += HourUpdate;
    }

    IEnumerator PlayAmbientTrack()
    {
        while(gameObject.activeSelf)
        {
            float trackCooldown = Random.Range(2f, 15f);
            yield return new WaitForSecondsRealtime(trackCooldown);
            float r = Random.Range(0,2f);
            if(TimeManager.currentHour < 6 && TimeManager.currentHour > 20)
            {
                ambienceSource.clip = nightAmbience[Random.Range(0, nightAmbience.Length)];
            }
            else
            {
                ambienceSource.clip = biomeAmbience[Random.Range(0, biomeAmbience.Length)];
            }
            float trackRuntime = ambienceSource.clip.length;
            ambienceSource.Play();
            yield return new WaitForSecondsRealtime(trackRuntime);
        }
    }
    IEnumerator PlayAmbientMusic()
    {
        while(gameObject.activeSelf)
        {
            float musicCooldown = Random.Range(10, 30);
            yield return new WaitForSecondsRealtime(musicCooldown);
            musicSource.clip = musicAmbience[Random.Range(0, musicAmbience.Length)];
            float musicRuntime = musicSource.clip.length;
            musicSource.Play();
            yield return new WaitForSecondsRealtime(musicRuntime);
        }
    }

    void HourUpdate()
    {
        if(TimeManager.currentHour == 8)
        {
            ambienceSource.PlayOneShot(bellTower);
        }
    }
}
