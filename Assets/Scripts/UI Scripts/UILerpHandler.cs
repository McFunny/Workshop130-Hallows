using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILerpHandler : UILerp
{
    public bool[] lerpToStartArray; // Array of booleans to determine if each lerp should go to start or end

    private void Awake()
    {
        base.Awake();   
    }
    private void Update()
    {
        base.Update();
        lerpToStart = LerpToStartOrEnd();
    }

    private bool LerpToStartOrEnd()
    {
        var trueCount = 0;
        for (int i = 0; i < lerpToStartArray.Length; i++)
        {
            if (lerpToStartArray[i]) trueCount++;
        }
        
        if(trueCount == lerpToStartArray.Length)
        {
            return true; // All elements are true, lerp to start
        }
        else return false; // Not all elements are true, lerp to end
            
    }
}
