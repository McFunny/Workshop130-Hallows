using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIButtonSound : MonoBehaviour
{
    [SerializeField] private AudioClip selectSound;
    [SerializeField] private float selectVolume;

    public void PlaySelectSound()
    {
        AudioPoolManager.Instance.PlayClip(selectSound, selectVolume);
    }
}
