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

    public event System.Action OnTillGround, OnShovelSwing, OnPlant, OnKill, OnWeedDug, OnWateredCrop; 

    public void TillGround()
    {
        if (OnTillGround != null) OnTillGround();
    }

    public void ShovelSwing()
    {
        if (OnShovelSwing != null) OnShovelSwing(); 
    }

    public void PlantSeed()
    {
        if (OnPlant != null) OnPlant();
    }

    public void KillStructure()
    {
        if (OnKill != null) OnKill(); 
    }

    public void WeedDug()
    {
        if (OnWeedDug != null) OnWeedDug(); 
    }

    public void WateredCrop()
    {
        if (OnWateredCrop != null) OnWateredCrop(); 
    }
}
