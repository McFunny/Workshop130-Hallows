using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JagBeetle : BugBehaviorScript
{
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            PlayerInteraction.Instance.StaminaChange(-7);
        }
    }
}
