using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupEvents : MonoBehaviour
{
    public static PopupEvents current;

    private void Awake()
    {
        current = this;
    }

    public event System.Action OnTillGround, OnShovelSwing; 

    public void TillGround()
    {
        if (OnTillGround != null)
        {
            OnTillGround(); 
        }
    }

    public void ShovelSwing()
    {
        if (OnShovelSwing != null)
        {
            OnShovelSwing(); 
        }
    }
}
