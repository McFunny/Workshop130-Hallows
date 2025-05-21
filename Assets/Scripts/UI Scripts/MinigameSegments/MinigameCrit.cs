using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinigameCrit : MinigameFunctionality
{    private void Start()
    {
        size = this.transform.localScale.x / 2;
    }
    public override void MinigameFunction()
    {
        Debug.Log("Minigame: Critical Hit.");
    }
}
