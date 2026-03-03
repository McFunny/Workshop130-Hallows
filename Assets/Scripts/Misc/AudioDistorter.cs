using UnityEngine;
using UnityEngine.Audio;

public class AudioDistorer : MonoBehaviour
{
    public AudioMixer mixer;
    Transform player;

    public float maxDistance = 35f;
    public float maxDistanceForNoMusic = 5f;

    float originalVolume = 0.28f;

    AudioSource source;

    void Start()
    {
        source = AmbientAudioManager.Instance.musicSource;
        originalVolume = source.volume;

        player = PlayerInteraction.Instance.playerFeet;
    }

    void Update()
    {
        float distance = Vector3.Distance(player.position, transform.position);

        // Within the silence zone — stay silent
        if (distance <= maxDistanceForNoMusic)
        {
            source.volume = 0f;
            return;
        }

        // Beyond max distance — full volume
        if (distance >= maxDistance)
        {
            source.volume = originalVolume;
            return;
        }

        // In between — fade from 0 to originalVolume
        // t = 0 when at maxDistanceForNoMusic, t = 1 when at maxDistance
        float t = Mathf.InverseLerp(maxDistanceForNoMusic, maxDistance, distance);
        source.volume = Mathf.Lerp(0f, originalVolume, t);
    }
}