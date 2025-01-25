using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireFearTrigger : MonoBehaviour
{
    public float fleeRange = 15; //how far the distance between this and the target fleeing must be for the fleeing to stop
    public delegate void ScaredCreature(bool successful);
    [HideInInspector] public event ScaredCreature OnScare;

    void OnTriggerEnter(Collider other)
    {
        CreatureBehaviorScript creature = other.gameObject.GetComponentInParent<CreatureBehaviorScript>();
        if(creature)
        {
            creature.EnteredFireRadius(this, out bool successful);
            OnScare?.Invoke(successful);
            return;
        }

        StructureBehaviorScript structure = other.gameObject.GetComponentInParent<StructureBehaviorScript>();
        if(structure)
        {
            //structure.EnteredFireRadius(this, out bool successful);
            //OnScare?.Invoke(successful);
            return;
        }
    }
}
