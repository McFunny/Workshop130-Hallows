using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureAudioHandler : MonoBehaviour
{
    public AudioClip[] hitSounds;
    public AudioClip[] miscSounds1;
    public AudioClip[] miscSounds2;
    public AudioClip interactSound, itemInteractSound, breakSound, activatedSound;

    AudioSource source;
    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();
    }

    public void PlayRandomSound(AudioClip[] clips)
    {
        int r = Random.Range(0, clips.Length);
        source.PlayOneShot(clips[r]);
    }

    public void PlaySound(AudioClip clip)
    {
        source.PlayOneShot(clip);
    }

    public void PlaySoundAtPoint(AudioClip clip, Vector3 pos)
    {
        AudioSource.PlayClipAtPoint(clip, pos);
    }
}
