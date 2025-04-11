using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HogClock : FurnitureBehaviorScript
{
    public Animator anim;
    public AudioSource source;
    public AudioClip hogSound, flySound;


    void Start()
    {
        base.Start();
        FurnitureStart();
        
    }

    public override void HourPassed()
    {
        //
    }
}
