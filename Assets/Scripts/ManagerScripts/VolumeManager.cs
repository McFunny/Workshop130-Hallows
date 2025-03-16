using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class VolumeManager : MonoBehaviour
{
    [SerializeField] private AudioMixer mixer;

    const string MASTER = "MasterVolume";
    const string MUSIC = "MusicVolume";
    const string SFX = "SFXVolume";
    void Start()
    {
        mixer.SetFloat(MASTER, Mathf.Log10(PlayerPrefs.GetFloat("MasterVolume", 1f)) * 20); //Don't ask me how this math works idk
        mixer.SetFloat(MUSIC, Mathf.Log10(PlayerPrefs.GetFloat("MusicVolume", 1f)) * 20);
        mixer.SetFloat(SFX, Mathf.Log10(PlayerPrefs.GetFloat("SFXVolume", 1f)) * 20);
    }

    public void SettingsChanged()
    {
        mixer.SetFloat(MASTER, Mathf.Log10(PlayerPrefs.GetFloat("MasterVolume", 1f)) * 20);
        mixer.SetFloat(MUSIC, Mathf.Log10(PlayerPrefs.GetFloat("MusicVolume", 1f)) * 20);
        mixer.SetFloat(SFX, Mathf.Log10(PlayerPrefs.GetFloat("SFXVolume", 1f)) * 20);
    }
}
