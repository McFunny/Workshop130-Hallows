using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireFearTrigger : MonoBehaviour
{
    public float fleeRange = 15; //how far the distance between this and the target fleeing must be for the fleeing to stop
    public delegate void ScaredCreature();
    [HideInInspector] public event ScaredCreature OnScare;

    List<StructureBehaviorScript> affectedStructures = new List<StructureBehaviorScript>();

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
        if(structure && !structure.nearbyFires.Contains(this))
        {
            structure.nearbyFires.Add(this);
            affectedStructures.Add(structure);
            return;
        }
    }

    void OnTriggerExit(Collider other)
    {
        StructureBehaviorScript structure = other.gameObject.GetComponentInParent<StructureBehaviorScript>();
        if(structure && structure.nearbyFires.Contains(this))
        {
            structure.nearbyFires.Remove(this);
            affectedStructures.Remove(structure);
            return;
        }
    }

    void OnDisable()
    {
        foreach(StructureBehaviorScript structure in affectedStructures) structure.nearbyFires.Remove(this);
        affectedStructures.Clear();
    }
}
