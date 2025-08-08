using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILerpHandler : UILerp
{
    public enum LogicType
    {
        OR,
        AND
    }
    [Header("AND is currently unused")]
    public LogicType logicType;
    public bool[] lerpToStartArray; // Array of booleans to determine if each lerp should go to start or end

    private void Awake()
    {
        base.Awake();
    }
    private void Update()
    {
        base.Update();

        switch (logicType)
        {
            case LogicType.OR:
                LerpOrGate();
                break;
            case LogicType.AND:
                // Unused
                break;
        }
        lerpToStart = LerpOrGate();
    }

    private bool LerpOrGate()
    {
        var trueCount = 0;
        for (int i = 0; i < lerpToStartArray.Length; i++)
        {
            if (lerpToStartArray[i]) trueCount++;
        }
        
        if(trueCount > 0)
        {
            return true;
        }
        else return false;
            
    }
}
