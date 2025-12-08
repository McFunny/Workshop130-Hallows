using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPoolManager : MonoBehaviour
{
    public static AudioPoolManager Instance;

    public List<GameObject> audioPool = new List<GameObject>();
    public GameObject audioPrefab;

    public AudioClip digUpSound;

    float defaultVolume, defaultDistance;

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }

        PopulateAudioPool();
    }

    void PopulateAudioPool()
    {
        //

        for(int i = 0; i < 5; i++)
        {
            GameObject newAudio = Instantiate(audioPrefab);
            if(i == 0)
            {
                defaultVolume = newAudio.GetComponent<AudioSource>().volume;
                defaultDistance = newAudio.GetComponent<AudioSource>().maxDistance;
            }
            audioPool.Add(newAudio);
            newAudio.SetActive(false);
        }
    }

    public void PlayClipAtPosition(AudioClip clip, Vector3 pos) //make one for volume too
    {
        foreach (GameObject audio in audioPool)
        {
            if(!audio.activeSelf)
            {
                audio.SetActive(true);
                audio.transform.position = pos;
                AudioSource oldSource = audio.GetComponent<AudioSource>();
                oldSource.volume = defaultVolume;
                oldSource.maxDistance = defaultDistance;
                oldSource.PlayOneShot(clip);
                return;
            }
        }

        //No available items, must make a new one
        GameObject newAudio = Instantiate(audioPrefab);
        audioPool.Add(newAudio);
        
        newAudio.transform.position = pos;
        AudioSource newSource = newAudio.GetComponent<AudioSource>();
        newSource.volume = defaultVolume;
        newSource.maxDistance = defaultDistance;
        newSource.PlayOneShot(clip);
    }

    public void PlayClipAtPosition(AudioClip clip, Vector3 pos, float volume, float maxDistance) //make one for volume too
    {
        foreach (GameObject audio in audioPool)
        {
            if(!audio.activeSelf)
            {
                audio.SetActive(true);
                audio.transform.position = pos;
                AudioSource oldSource = audio.GetComponent<AudioSource>();
                oldSource.volume = volume;
                oldSource.maxDistance = maxDistance;
                oldSource.PlayOneShot(clip);
                return;
            }
        }

        //No available items, must make a new one
        GameObject newAudio = Instantiate(audioPrefab);
        audioPool.Add(newAudio);

        newAudio.transform.position = pos;
        AudioSource newSource = newAudio.GetComponent<AudioSource>();
        newSource.volume = volume;
        newSource.maxDistance = maxDistance;
        newSource.PlayOneShot(clip);
    }
}
