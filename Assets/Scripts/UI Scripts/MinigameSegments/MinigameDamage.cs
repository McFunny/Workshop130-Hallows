using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinigameDamage : MinigameFunctionality
{
    [SerializeField] private float damage = 50f;
    private PlayerInteraction playerInteraction;
    private void Start()
    {
        playerInteraction = FindObjectOfType<PlayerInteraction>();
        size = this.transform.localScale.x / 2;
    }
    public override void MinigameFunction()
    {
        playerInteraction.StaminaChange(-damage);
        Debug.Log("Minigame: Damage Applied.");
    }
}
