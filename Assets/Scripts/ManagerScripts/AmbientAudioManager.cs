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
    public AudioClip[] lightningAmbience;
    public AudioClip[] siegeMusicAmbience;

    public AudioClip finaleTheme, finaleIntro, finaleLose, finaleWin;

    public AudioClip bellTower;

    private Coroutine ambientMusicCoroutine;

    public delegate void BlowWind(Vector3 dir);
    public static event BlowWind OnWindBlow;

    bool firstTrackPlayed = false;
    [HideInInspector] public bool playMusicAtStart = true;
    bool fadingMusic;

    [HideInInspector] public Gramophone playingGramophone;
    AudioClip gramoPhoneTrack;

    public LightningEffect lightingScript;

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
        playingGramophone = null;
        gramoPhoneTrack = null;
    }

    void Start()
    {
        StartCoroutine(PlayAmbientTrack());

        StartCoroutine(PlayMusicCheck());

        TimeManager.OnHourlyUpdate += HourUpdate;
    }

    void Update()
    {
        /*if(Time.timeScale == 0 && musicSource.isPlaying)
        {
            musicSource.Pause();
        }

        if(Time.timeScale != 0 && !musicSource.isPlaying)
        {
            musicSource.UnPause();
        }*/

        //print(musicSource.isPlaying);

        if(playingGramophone)
        {
            if(Vector3.Distance(PlayerInteraction.Instance.transform.position, playingGramophone.transform.position) > 100)
            {
                EndGramophone();
            }
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
            if(r > .65f) //effects
            {
                if(NightSpawningManager.Instance.finaleActivated && lightingScript)
                {
                    StartCoroutine(lightingScript.PlayLightning());
                    ambienceSource.clip = lightningAmbience[Random.Range(0, lightningAmbience.Length)];
                }
                else //blow wind
                {
                    ambienceSource.clip = windAmbience[Random.Range(0, windAmbience.Length)];
                    Vector3 windDirection = new Vector3(Random.Range(-1, 1f), 0, 0);
                    OnWindBlow?.Invoke(windDirection);
                }
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

            Debug.Log("CoolDown for song begun");
            yield return new WaitForSecondsRealtime(musicCooldown);
            Debug.Log("CoolDown Done picking song");
            if(NightSpawningManager.Instance.finaleActivated)
            {
                musicSource.clip = finaleTheme;
            }
            else if(gramoPhoneTrack != null) musicSource.clip = gramoPhoneTrack;
            else if(TownGate.Instance.location == PlayerLocation.InWilderness) musicSource.clip = wildernessMusicAmbience[Random.Range(0, wildernessMusicAmbience.Length)];
            else if(TownGate.Instance.location == PlayerLocation.InCrypt) musicSource.clip = catacombMusicAmbience[Random.Range(0, catacombMusicAmbience.Length)];
            else if (TimeManager.Instance.isDay)
            {
                yield return new WaitForSecondsRealtime(Random.Range(8, 20));
                musicSource.clip = musicAmbience[Random.Range(0, musicAmbience.Length)];
            }
            else
            {
                if(SiegeManager.Instance.siegeCropOnFarm) musicSource.clip = siegeMusicAmbience[Random.Range(0, siegeMusicAmbience.Length)];
                else musicSource.clip = musicNightAmbience[Random.Range(0, musicNightAmbience.Length)];
            } 

            float musicRuntime = musicSource.clip.length;
            if(!playingGramophone) musicSource.Play();
            Debug.Log("Playing MUSIC");
            yield return new WaitForSecondsRealtime(musicRuntime);
            Debug.Log("Song ended"); 
        }
    }

    void HourUpdate()
    {
        if ((TimeManager.Instance.currentHour == 6 || TimeManager.Instance.currentHour == 20) && !NightSpawningManager.Instance.finaleActivated)
        {
            print(NightSpawningManager.Instance.finaleActivated);
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
            StartCoroutine(FadeAudio(0)); 
        }
    }

    public IEnumerator FadeAudio(float time)
    {
        if(fadingMusic) yield break;
        fadingMusic = true;
        float oldVolume = musicSource.volume;
        float currentVolume = oldVolume;

        yield return new WaitForSeconds(0.1f);
        if(NightSpawningManager.Instance.finaleActivated) yield break;

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

        playingGramophone = null;
        gramoPhoneTrack = null;

        yield return new WaitForSeconds(time); //Time it takes to restart the coroutine

        ambientMusicCoroutine = StartCoroutine(PlayAmbientMusic()); //restarts coroutine
        fadingMusic = false;
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
        StartCoroutine(FadeAudio(0)); 
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
        if (ambientMusicCoroutine != null)
        {
            StopCoroutine(ambientMusicCoroutine); // Stop the current music coroutine
        }
        musicSource.clip = finaleWin;
        musicSource.Play();
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

    public void StartGramophone(Gramophone g, AudioClip c)
    {
        if(playingGramophone) playingGramophone.source.Stop();
        playingGramophone = g;
        gramoPhoneTrack = c;
        ChangeMusic();
    }

    public void EndGramophone()
    {
        if(playingGramophone) playingGramophone.source.Stop();
        playingGramophone = null;
        gramoPhoneTrack = null;

        ChangeMusic();
    }
    
}
