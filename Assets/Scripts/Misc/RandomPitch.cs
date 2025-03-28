using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomPitch : MonoBehaviour
{

    public float lowest;
    public float highest;
    private AudioSource audioSource;
    public bool useRandomClip;
    public List<AudioClip> clips = new List<AudioClip>();
   
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.pitch = Random.Range(lowest, highest);
        if (useRandomClip) audioSource.clip = clips[Random.Range(0,clips.Count)]; audioSource.Play();
    }

   
}
