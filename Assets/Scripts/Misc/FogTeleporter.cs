using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FogTeleporter : MonoBehaviour
{
    public Transform otherEnd;

    public Transform enemyTeleport; //To prevent enemies from spawning behind the cabin and getting stuck

    public bool overrideRotation = false;

    private void OnTriggerEnter(Collider other)
    {
        bool teleportSuccessful = false;
        if(other.gameObject.layer == 10) //player
        {
            if(otherEnd) 
            {
                other.transform.position = new Vector3(otherEnd.position.x, otherEnd.position.y + 1.23f, otherEnd.position.z); //To account for misalignment of player (thx Abner)
                if(overrideRotation)
                {
                    PlayerCam.Instance.ForceChangeRotation(otherEnd.parent.eulerAngles.y);
                }
            }
        }
        else if(other.gameObject.layer == 9) //creature
        {
            if(TimeManager.Instance.stopTime) return;
            if(TimeManager.Instance.isDay)
            {
                var creature = other.gameObject.GetComponentInParent<CreatureBehaviorScript>();
                if(creature && !creature.persistAfterNewDay) Destroy(creature.gameObject);
            }
            else if(enemyTeleport)
            {
                NavMeshAgent agent = other.gameObject.GetComponentInParent<NavMeshAgent>();
                if(agent)
                {
                    agent.Warp(enemyTeleport.position);
                    teleportSuccessful = true;
                }
                var creature = other.gameObject.GetComponentInParent<CreatureBehaviorScript>();
                if(creature)
                {
                    if(!teleportSuccessful) creature.transform.position = enemyTeleport.position;
                    creature.FogTeleport();
                }
            } 
            else if(otherEnd) 
            {
                NavMeshAgent agent = other.gameObject.GetComponentInParent<NavMeshAgent>();
                if(agent)
                {
                    agent.Warp(otherEnd.position);
                    teleportSuccessful = true;
                }
                var creature = other.gameObject.GetComponentInParent<CreatureBehaviorScript>();
                if(creature)
                {
                    if(!teleportSuccessful) creature.transform.position = otherEnd.position;
                    creature.FogTeleport();
                }
            }
        }


    }

}
