using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HogClock : FurnitureBehaviorScript
{
    public Animator anim;
    public AudioSource source, gearSoundSource;
    public AudioClip hogSound, flySound;

    public GameObject hog, fly;

    public bool forceFly = false;


    void Start()
    {
        base.Start();
        FurnitureStart();
        
    }

    [ContextMenu("Hour Passed")]
    public override void HourPassed()
    {
        //if(TimeManager.Instance.currentHour == 6 || TimeManager.Instance.currentHour == 18)
        //{
            //
        //}
        StartCoroutine(PlayAnimation());
    }

    IEnumerator PlayAnimation()
    {
        yield return new WaitForSeconds(Random.Range(0.2f, 0.8f));
        bool isHog = true;
        if(Random.Range(0, 10) >= 9 || forceFly) isHog = false;

        hog.SetActive(isHog);
        fly.SetActive(!isHog);

        AudioClip soundToPlay = hogSound;
        if(!isHog) soundToPlay = flySound;

        anim.SetTrigger("Play");
        gearSoundSource.Play();
        yield return new WaitForSeconds(1.5f);
        source.PlayOneShot(soundToPlay);
        yield return new WaitForSeconds(0.7f);
        source.Stop();
        source.PlayOneShot(soundToPlay);
        yield return new WaitForSeconds(0.7f);
        source.Stop();
        yield return new WaitForSeconds(0.5f);
        gearSoundSource.Stop();
    }
}
