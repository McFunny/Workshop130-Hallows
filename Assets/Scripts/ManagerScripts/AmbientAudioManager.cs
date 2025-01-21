using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientAudioManager : MonoBehaviour
{
    public AudioSource ambienceSource, musicSource;
    public AudioClip[] biomeAmbience;
    public AudioClip[] nightAmbience;
    public AudioClip[] windAmbience;
    public AudioClip[] musicAmbience;
    public AudioClip[] musicNightAmbience;

    public AudioClip bellTower;

    private Coroutine ambientMusicCoroutine;

    public delegate void BlowWind(Vector3 dir);
    public static event BlowWind OnWindBlow;

    void Start()
    {
        StartCoroutine(PlayAmbientTrack());
        ambientMusicCoroutine = StartCoroutine(PlayAmbientMusic()); //Making it trackable

        TimeManager.OnHourlyUpdate += HourUpdate;
    }

    void OnDisable()
    {
        TimeManager.OnHourlyUpdate -= HourUpdate;
    }

    IEnumerator PlayAmbientTrack()
    {
        while (gameObject.activeSelf)
        {
            float trackCooldown = Random.Range(2f, 15f);
            yield return new WaitForSeconds(trackCooldown);
            float r = Random.Range(0, 1f);
            if(r > .65f) //blow wind
            {
                ambienceSource.clip = windAmbience[Random.Range(0, windAmbience.Length)];
                Vector3 windDirection = new Vector3(Random.Range(-1, 1f), 0, 0);
                OnWindBlow?.Invoke(windDirection);
                print("Wind");
            }
            else if (TimeManager.Instance.currentHour < 6 || TimeManager.Instance.currentHour > 20)
            {
                ambienceSource.clip = nightAmbience[Random.Range(0, nightAmbience.Length)];
            }
            else
            {
                ambienceSource.clip = biomeAmbience[Random.Range(0, biomeAmbience.Length)];
            }
            float trackRuntime = ambienceSource.clip.length;
            ambienceSource.Play();
            yield return new WaitForSeconds(trackRuntime);
        }
    }

    IEnumerator PlayAmbientMusic()
    {
        while (gameObject.activeSelf)
        {
            float musicCooldown = Random.Range(5, 10);
            yield return new WaitForSecondsRealtime(musicCooldown);
            Debug.Log("CoolDown Done picking song");
            if (TimeManager.Instance.isDay)
                musicSource.clip = musicAmbience[Random.Range(0, musicAmbience.Length)];
            else
                musicSource.clip = musicNightAmbience[Random.Range(0, musicNightAmbience.Length)];
            float musicRuntime = musicSource.clip.length;
            musicSource.Play();
            Debug.Log("Playing MUSIC");
            yield return new WaitForSecondsRealtime(musicRuntime);
            Debug.Log("Song ended"); 
        }
    }

    void HourUpdate()
    {
        if (TimeManager.Instance.currentHour == 6 || TimeManager.Instance.currentHour == 20)
        {
            StartCoroutine(FadeBell());
            //StopCoroutine(PlayAmbientMusic());
            //StartCoroutine(FadeAudio());
            //StartCoroutine(PlayAmbientMusic());

            //ambienceSource.PlayOneShot(bellTower);
            //This commented out bit of code would stop the current coroutine and then run it again

            if (ambientMusicCoroutine != null)
            {
                StopCoroutine(ambientMusicCoroutine); // Stop the current music coroutine
                //musicSource.Stop(); // Stop current music
            }
            Debug.Log("It's either 6 or 20 music time");
            StartCoroutine(FadeAudio()); 
        }
    }

    IEnumerator FadeAudio()
    {
        float oldVolume = musicSource.volume;
        float currentVolume = oldVolume;

        while (currentVolume > 0)
        {
            yield return new WaitForSeconds(0.2f);
            currentVolume -= 0.01f;
            musicSource.volume = currentVolume;
        }

        musicSource.Stop();
        musicSource.volume = oldVolume;

        ambientMusicCoroutine = StartCoroutine(PlayAmbientMusic()); //restarts coroutine
    }

    IEnumerator FadeBell()
    {
        yield return new WaitForSeconds(2.5f);
        ambienceSource.PlayOneShot(bellTower);
    }
}
