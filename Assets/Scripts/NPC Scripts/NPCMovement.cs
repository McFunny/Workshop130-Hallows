using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.AI;



[System.Serializable]
public class Schedule
{
    public int time;
    public Destination Destination;
    public Action Action;
}

public class NPCMovement : MonoBehaviour
{
    NPCMovementManager npcMovementManager;

    [SerializeField] private Transform currentDestination;
    private NavMeshAgent agent;

    private Schedule currentSchedule;

    public bool isWorking;

    public List<Schedule> scheduleList = new List<Schedule>();
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        TimeManager.OnHourlyUpdate += CheckDestination;
        npcMovementManager = FindObjectOfType<NPCMovementManager>();
    }


    public void CheckDestination()
    {
        foreach (Schedule schedule in scheduleList)
        {
            if (schedule.time == TimeManager.currentHour)
            {
                Debug.Log("This Ran");
                currentSchedule = schedule;
                currentDestination = npcMovementManager.GetDestination(schedule.Destination);
                StartCoroutine(MoveToDestination(currentDestination));
            }
        }
    }

    IEnumerator MoveToDestination(Transform destination)
    {
        agent.destination = destination.position;

        while (agent.pathPending)
        {
            yield return null;
        }

        while (agent.remainingDistance > agent.stoppingDistance)
        {
            yield return null;
        }

        if (currentSchedule.Action == Action.Working)
        {
            isWorking = true;
        }
        else
        {
            isWorking = false;
        }
    }

    void Update()
    {
       /* if (currentDestination != null)
        {
            agent.destination = currentDestination.position;

            if (currentSchedule.Action == Action.Working)
            {
                isWorking = true;
            }
            else
            {
                isWorking = false;
            }
        }*/
    }
}
 


