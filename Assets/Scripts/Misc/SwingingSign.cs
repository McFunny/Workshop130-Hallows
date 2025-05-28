using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingingSign : MonoBehaviour
{
    public Rigidbody rb;

    public AudioSource source;

    // Update is called once per frame
    void Update()
    {
        if(rb && source && !source.isPlaying && rb.velocity.magnitude > 2)
        {
            source.Play();
        }
    }
}
