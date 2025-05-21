using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinigameFunctionality : MonoBehaviour
{
    [Header("Ignore this. It uses the X scale of the object to determine size")]
    public float size;

    [Header("Actual variables that matter")]
    public int hitCount = 1; // Number of "hits" this segment counts as
    public bool isHit; // Determines if hitting this segment counts as a hit or a miss

    public virtual void MinigameFunction()
    {
        // This function is meant to be overridden by subclasses
        Debug.Log("Minigame: Miss!!!");
    }
}
