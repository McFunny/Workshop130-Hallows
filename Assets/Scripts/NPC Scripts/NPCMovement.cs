using System.Collections;
using System.Collections.Generic;
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
    private NPCMovementManager npcMovementManager;
    private NavMeshAgent agent;

    private Schedule currentSchedule;
    private Sublocation currentSublocation; // Track assigned sublocation

    public bool isWorking; // Determines if this NPC is actively working
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
            if (schedule.time == TimeManager.Instance.currentHour)
            {
                currentSchedule = schedule;

                
                if (currentSublocation != null)
                {
                    ReleaseSublocation(currentSublocation); // makes sublocation available for other npcs
                }

               
                bool isWorker = (schedule.Action == Action.Working); // determines if NPC is supposed to be working at that location

                currentSublocation = npcMovementManager.GetRandomSublocation(schedule.Destination, isWorker); // gets sublocation for NPC to be at

                if (currentSublocation != null) //claims sublocation and moves to it
                {
                    currentSublocation.isOccupied = true;
                    currentSublocation.occupant = this;

                    StartCoroutine(MoveToDestination(currentSublocation.transform));
                }
                else
                {
                    // fall back to main destination if no sublocations available
                    Transform mainDestination = npcMovementManager.GetDestination(schedule.Destination);

                    if (mainDestination != null)
                    {
                        StartCoroutine(MoveToDestination(mainDestination));
                    }
                    else
                    {
                        Debug.LogWarning($"No available destination or sublocation for {schedule.Destination}");
                    }
                }
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

        PerformAction(); //when npcs reaches place, their action will be called
    }

    void PerformAction()
    {
        switch (currentSchedule.Action)
        {
            case Action.Working:
                isWorking = true; // Set isWorking to true for working action
                //Debug.Log($"{name} is now working at {currentSublocation?.name ?? currentSchedule.Destination.ToString()}");
                break;

            case Action.Walking:
                isWorking = false;
                //Debug.Log($"{name} is walking at {currentSchedule.Destination}");
                break;

            case Action.Idle:
                isWorking = false;
                //Debug.Log($"{name} is idling at {currentSublocation?.name ?? currentSchedule.Destination.ToString()}");
                break;
        }
    }

    void ReleaseSublocation(Sublocation sublocation)
    {
        sublocation.isOccupied = false;
        sublocation.occupant = null;
    }
}
