using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientAudioManager : MonoBehaviour
{
    public static AmbientAudioManager Instance;

    public AudioSource ambienceSource, musicSource;
    public AudioClip[] biomeAmbience;
    public AudioClip[] nightAmbience;
    public AudioClip[] windAmbience;
    public AudioClip[] wildernessAmbience;
    public AudioClip[] musicAmbience;
    public AudioClip[] musicNightAmbience;
    public AudioClip[] wildernessMusicAmbience;
    public AudioClip[] catacombMusicAmbience;

    public AudioClip finaleTheme, finaleIntro, finaleLose, finaleWin;

    public AudioClip bellTower;

    private Coroutine ambientMusicCoroutine;

    public delegate void BlowWind(Vector3 dir);
    public static event BlowWind OnWindBlow;

    bool firstTrackPlayed = false;
    [HideInInspector] public bool playMusicAtStart = true;

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            print("Destroyed Copy");
            return;
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        StartCoroutine(PlayAmbientTrack());

        StartCoroutine(PlayMusicCheck());

        TimeManager.OnHourlyUpdate += HourUpdate;
    }

    void Update()
    {
        if(Time.timeScale == 0 && musicSource.isPlaying)
        {
            musicSource.Pause();
        }

        if(Time.timeScale != 0 && !musicSource.isPlaying)
        {
            musicSource.UnPause();
        }
    }

    public void BeginPlayingMusic()
    {
        if (ambientMusicCoroutine != null)
        {
            StopCoroutine(ambientMusicCoroutine); // Stop the current music coroutine
        }
        ambientMusicCoroutine = StartCoroutine(PlayAmbientMusic()); //Making it trackable
    }

    IEnumerator PlayMusicCheck()
    {
        yield return new WaitForSeconds(7);
        if(playMusicAtStart) BeginPlayingMusic();
    }

    void OnDisable()
    {
        TimeManager.OnHourlyUpdate -= HourUpdate;
    }

    IEnumerator PlayAmbientTrack()
    {
        while (gameObject.activeSelf)
        {
            float trackCooldown = Random.Range(5f, 15f);
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
            else if(TownGate.Instance.location == PlayerLocation.InWilderness)
            {
                ambienceSource.clip = wildernessAmbience[Random.Range(0, wildernessAmbience.Length)];
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
        float musicCooldown = 0;
        while (gameObject.activeSelf)
        {
            if(NightSpawningManager.Instance.finaleActivated) musicCooldown = 0;
            else if(!firstTrackPlayed)
            {
                firstTrackPlayed = true;
                musicCooldown = 5;
            }
            else musicCooldown = Random.Range(5, 10);

            yield return new WaitForSeconds(musicCooldown);
            Debug.Log("CoolDown Done picking song");
            if(NightSpawningManager.Instance.finaleActivated)
            {
                musicSource.clip = finaleTheme;
            }
            else if (TimeManager.Instance.isDay)
            {
                if(TownGate.Instance.location == PlayerLocation.InWilderness) musicSource.clip = wildernessMusicAmbience[Random.Range(0, wildernessMusicAmbience.Length)];
                else if(TownGate.Instance.location == PlayerLocation.InCrypt) musicSource.clip = catacombMusicAmbience[Random.Range(0, catacombMusicAmbience.Length)];
                else musicSource.clip = musicAmbience[Random.Range(0, musicAmbience.Length)];
            }
            else
                musicSource.clip = musicNightAmbience[Random.Range(0, musicNightAmbience.Length)];

            float musicRuntime = musicSource.clip.length;
            musicSource.Play();
            Debug.Log("Playing MUSIC");
            yield return new WaitForSeconds(musicRuntime);
            Debug.Log("Song ended"); 
        }
    }

    void HourUpdate()
    {
        if (TimeManager.Instance.currentHour == 6 || TimeManager.Instance.currentHour == 20 && !NightSpawningManager.Instance.finaleActivated)
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

        if (ambientMusicCoroutine != null)
        {
            StopCoroutine(ambientMusicCoroutine); // Stop the current music coroutine
        }
        StopCoroutine(FinaleTheme());
        ambientMusicCoroutine = StartCoroutine(PlayAmbientMusic()); //restarts coroutine
    }

    IEnumerator FadeBell()
    {
        yield return new WaitForSeconds(2.5f);
        ambienceSource.PlayOneShot(bellTower);
    }

    public void ChangeMusic() //Call this when the player changes location or dies
    {
        if (ambientMusicCoroutine != null)
        {
            StopCoroutine(ambientMusicCoroutine); // Stop the current music coroutine
            //musicSource.Stop(); // Stop current music
        }
        StopCoroutine(FinaleTheme());
        StartCoroutine(FadeAudio()); 
    }

    public void StartFinaleTheme()
    {
        StartCoroutine(FinaleTheme());
    }

    public void EndFinaleTheme()
    {
        StartCoroutine(LoseFinale());
    }

    public void WinFinaleTheme()
    {
        //
    }

    IEnumerator FinaleTheme()
    {
        if (ambientMusicCoroutine != null)
        {
            StopCoroutine(ambientMusicCoroutine); // Stop the current music coroutine
        }
        //
        musicSource.clip = finaleIntro;
        float musicRuntime = musicSource.clip.length;
        musicSource.Play();

        yield return new WaitForSecondsRealtime(musicRuntime);
        ambientMusicCoroutine = StartCoroutine(PlayAmbientMusic());
    }

    IEnumerator LoseFinale()
    {
        if (ambientMusicCoroutine != null)
        {
            StopCoroutine(ambientMusicCoroutine); // Stop the current music coroutine
        }
        musicSource.clip = finaleLose;
        float musicRuntime = musicSource.clip.length;
        musicSource.Play();

        yield return new WaitForSecondsRealtime(musicRuntime);
        ambientMusicCoroutine = StartCoroutine(PlayAmbientMusic());
    }
    
}
