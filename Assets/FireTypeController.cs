using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireTypeController : MonoBehaviour
{
    [SerializeField] private GameObject fire;
    [SerializeField] private GameObject gloam;
    [SerializeField] private GameObject terra;
    [SerializeField] private GameObject ichor;
    private AudioSource audioSource;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        DoFire();
    }

    public void DoFire()
    {
        if (fire) fire.SetActive(true);
        if (gloam) gloam.SetActive(false);
        if (terra) terra.SetActive(false);
        if (ichor) ichor.SetActive(false);
        if (audioSource) audioSource.Play();
    }

    public void DoGloam()
    {
        if (fire) fire.SetActive(false);
        if (gloam) gloam.SetActive(true);
        if (terra) terra.SetActive(false);
        if (ichor) ichor.SetActive(false);
        if(audioSource) audioSource.Play();
    }
    public void DoTerra()
    {
        if (fire) fire.SetActive(false);
        if (gloam) gloam.SetActive(false);
        if (terra) terra.SetActive(true);
        if (ichor) ichor.SetActive(false);
        if (audioSource) audioSource.Play();
    }

    public void DoIchor()
    {
        if (ichor) ichor.SetActive(true);
        if (gloam) gloam.SetActive(false);
        if (terra) terra.SetActive(false);
        if (fire) fire.SetActive(false);
        if (audioSource) audioSource.Play();
    }

    public void DoTypeBasedOnNumber(int number)
    {
        switch (number)
        {
            case 0:
                DoFire();
                break;
            case 1:
                DoGloam();
                break;
            case 2:
                DoTerra();
                break;
            case 3:
                DoIchor();
                break;
        }
    }

}