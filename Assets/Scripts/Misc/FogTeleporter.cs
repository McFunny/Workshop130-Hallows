using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogTeleporter : MonoBehaviour
{
    public Transform otherEnd;

    public Transform enemyTeleport; //To prevent enemies from spawning behind the cabin and getting stuck

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == 10)
        {
            if(otherEnd) other.transform.position = otherEnd.position;
        }
        else if(other.gameObject.layer == 9)
        {
            if(TimeManager.Instance.isDay)
            {
                var creature = other.gameObject.GetComponentInParent<CreatureBehaviorScript>();
                Destroy(creature.gameObject);
            }
            else if(enemyTeleport) other.transform.position = enemyTeleport.position;
            else if(otherEnd) other.transform.position = otherEnd.position;
        }


    }
}
