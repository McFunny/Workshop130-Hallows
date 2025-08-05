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

        CreatureBehaviorScript creature = other.gameObject.GetComponentInParent<CreatureBehaviorScript>();
        if(creature && creature.health > 0 && creature.shovelVulnerable)
        {
            creature.TakeDamage(5);
            creature.PlayHitParticle(creature.transform.position);
        }
    }
}
