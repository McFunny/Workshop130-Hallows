using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEffectsHandler : MonoBehaviour
{
    //HANDLES THE AUDIO AND EFFECTS THAT COME FROM THE PLAYER
    public float volume = 1f;
    public AudioSource source, footStepSource;
    public AudioClip itemPickup;
    //public AudioClip footSteps;
    Rigidbody rb;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        StartCoroutine("FootStepsPitchChanger");
    }

    // Update is called once per frame
    void Update()
    {
        if(rb.velocity.magnitude > 0.2f) footStepSource.volume = 0.1f;
        else footStepSource.volume = 0f;
        
    }

    IEnumerator FootStepsPitchChanger()
    {
        do
        {
            yield return new WaitForSeconds(0.5f);
            footStepSource.pitch = Random.Range(0.7f, 1.3f);
        }
        while(gameObject.activeSelf);
    }

    public void ItemCollectSFX()
    {
        source.PlayOneShot(itemPickup);
    }

}
