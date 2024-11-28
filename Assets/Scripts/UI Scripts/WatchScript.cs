using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class WatchScript : MonoBehaviour
{
    public GameObject watchHand, amImage, pmImage;
    public Animator animator;
    ControlManager controlManager;
    bool isClockAnimating = false;


    void Awake()
    {
        controlManager = FindFirstObjectByType<ControlManager>();
    }

    void Update()
    {
        UpdateWatch();

        if(!isClockAnimating)
        {
            if(controlManager.moreInfo.action.WasPressedThisFrame()){animator.SetBool("isClockRaised", true);}
            if(controlManager.moreInfo.action.WasReleasedThisFrame()){animator.SetBool("isClockRaised", false);}
        }
        
    }

    void UpdateWatch() //Cam don't look at this again it's been a long week // :)
    {
       watchHand.transform.rotation = Quaternion.Euler(0,0,TimeManager.Instance.currentHour * 30 * -1);
       if(TimeManager.Instance.currentHour >= 12)
       {
            amImage.SetActive(false);
            pmImage.SetActive(true);
       }
       else
       {
            amImage.SetActive(true);
            pmImage.SetActive(false);
       }
    }

    void ClockIsAnimating()
    {
        isClockAnimating = true;
    }

    void ClockIsNotAnimating()
    {
        isClockAnimating = false;
    }
}
