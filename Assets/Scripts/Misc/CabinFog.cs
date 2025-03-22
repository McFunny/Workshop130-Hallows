using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class CabinFog : MonoBehaviour
{
    public bool isFading = false;

    public VisualEffect[] mists;

    public GameObject background;

    public AudioSource source;
    private NewDayCounter newDayCounter;

    void Start()
    {
        TimeManager.Instance.stopTime = true;
        AmbientAudioManager.Instance.playMusicAtStart = false;
        newDayCounter = FindFirstObjectByType<NewDayCounter>();
    }


    public IEnumerator FadeOut()
    {
        AmbientAudioManager.Instance.BeginPlayingMusic();
        isFading = true;
        TimeManager.Instance.stopTime = false;
        source.Play();
        StartCoroutine(newDayCounter.ForceAnim());
        background.SetActive(false);
        for(int i = 0; i < mists.Length; i++)
        {
            mists[i].SetFloat("Smoke Rate", 0);
            mists[i].SetFloat("SmokeRate2", 0);
        }
        yield return new WaitForSeconds(5);
        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if(!isFading)
        {
            StartCoroutine(FadeOut());
        } 
    }
}
