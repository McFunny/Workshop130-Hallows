using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogTeleporter : MonoBehaviour
{
    public Transform otherEnd;

    public Transform enemyTeleport; //To prevent enemies from spawning behind the cabin and getting stuck

    public bool overrideRotation = false;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == 10)
        {
            if(otherEnd) 
            {
                other.transform.position = new Vector3(otherEnd.position.x, otherEnd.position.y + 1.23f, otherEnd.position.z); //To account for misalignment of player (thx Abner)
                if(overrideRotation) other.transform.rotation = otherEnd.parent.transform.rotation;
            }
        }
        else if(other.gameObject.layer == 9)
        {
            if(TimeManager.Instance.isDay)
            {
                var creature = other.gameObject.GetComponentInParent<CreatureBehaviorScript>();
                Destroy(creature.gameObject);
            }
            else if(enemyTeleport)
            {
                var creature = other.gameObject.GetComponentInParent<CreatureBehaviorScript>();
                if(creature) creature.transform.position = enemyTeleport.position;
            } 
            else if(otherEnd) 
            {
                var creature = other.gameObject.GetComponentInParent<CreatureBehaviorScript>();
                if(creature) creature.transform.position = otherEnd.position;
            }
        }


    }
}
