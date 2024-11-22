using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogTeleporter : MonoBehaviour
{
    public Transform otherEnd;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == 10)
        {
            if(otherEnd) other.transform.position = otherEnd.position;
        }
        else if(other.gameObject.layer == 9)
        {
            if(TimeManager.isDay)
            {
                var creature = other.gameObject.GetComponentInParent<CreatureBehaviorScript>();
                Destroy(creature.gameObject);
            }
            else if(otherEnd) other.transform.position = otherEnd.position;
        }


    }
}
