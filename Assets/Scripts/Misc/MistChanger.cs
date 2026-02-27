using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.Events;

public class MistChanger : MonoBehaviour
{
    ///THIS IS FOR TURNING THE MIST WHITE DURING THE ENDING CUTSCENE///
    public GameObject[] extraFog; //To be turned off at ending cutscene

    public VisualEffect effect;

    public VisualEffectAsset newVFXAsset;


    void Start()
    {
        EndingManager.OnEndingStarted += ColorChange;
    }

    void ColorChange()
    {
        effect.visualEffectAsset = newVFXAsset;

        for(int i = 0; i < extraFog.Length; i++)
        {
            extraFog[i].SetActive(false);
        }
    }

    void OnDisable()
    {
        EndingManager.OnEndingStarted -= ColorChange;
    }
}
