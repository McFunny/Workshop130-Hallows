using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPoolManager : MonoBehaviour
{
    public static AudioPoolManager Instance;

    public List<GameObject> audioPool = new List<GameObject>();
    public GameObject audioPrefab;

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
                audio.GetComponent<AudioSource>().PlayOneShot(clip);
                audio.transform.position = pos;
                return;
            }
        }

        //No available items, must make a new one
        GameObject newAudio = Instantiate(audioPrefab);
        audioPool.Add(newAudio);
        newAudio.GetComponent<AudioSource>().PlayOneShot(clip);
        newAudio.transform.position = pos;
    }
}
