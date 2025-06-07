using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinigameDamage : MinigameFunctionality
{
    [SerializeField] private float damage = 50f;
    private PlayerInteraction playerInteraction;
    public override void Start()
    {
        base.Start();
        playerInteraction = FindObjectOfType<PlayerInteraction>();
    }
    public override void MinigameFunction()
    {
        playerInteraction.StaminaChange(-damage);
        Debug.Log("Minigame: Damage Applied.");
    }
}
