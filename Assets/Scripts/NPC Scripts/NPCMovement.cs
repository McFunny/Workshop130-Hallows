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

    NPC npcScript;
    bool isTalking = false;
    bool facePlayer = true;

    Vector3 currentDestination;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        TimeManager.OnHourlyUpdate += CheckDestination;
        npcMovementManager = FindObjectOfType<NPCMovementManager>();
        npcScript = GetComponent<NPC>();
        CheckDestination();
    }

    void OnDisable()
    {
        TimeManager.OnHourlyUpdate -= CheckDestination;
    }

    public void CheckDestination()
    {
        foreach (Schedule schedule in scheduleList)
        {
            if (schedule.time == TimeManager.Instance.currentHour)
            {
                currentSchedule = schedule;

                // Release the current sublocation if occupied
                if (currentSublocation != null && !currentSublocation.isAtHome)
                {
                    ReleaseSublocation(currentSublocation);
                }

                // Determine if NPC is working
                bool isWorker = (schedule.Action == Action.Working);

                // Handle AtHome separately
                if (schedule.Action == Action.AtHome)
                {
                    currentSublocation = npcMovementManager.GetRandomSublocation(schedule.Destination, false, true);
                }
                else
                {
                    // Standard behavior for other actions
                    currentSublocation = npcMovementManager.GetRandomSublocation(schedule.Destination, isWorker);
                }

                // Claim the sublocation and move to it
                if (currentSublocation != null)
                {
                    if (!currentSublocation.isAtHome) // Only claim if it's not a home sublocation
                    {
                        currentSublocation.isOccupied = true;
                        currentSublocation.occupant = this;
                    }

                    StartCoroutine(MoveToDestination(currentSublocation.transform));
                }
                else
                {
                    // Fallback to main destination if no sublocations available
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
                break;
            }
        }
    }

    IEnumerator MoveToDestination(Transform destination)
    {
        agent.destination = destination.position;
        currentDestination = destination.position;

        while (agent.pathPending)
        {
            yield return null;
        }

        while(agent.isStopped)
        {
            yield return null;
        }
        if(isWorking)
        {
            npcScript.StopWorking();
            isWorking = false;
        }

        while (agent.remainingDistance > agent.stoppingDistance)
        {
            if(!agent.isStopped)
            {
                npcScript.anim.SetBool("IsMoving", true);
            }
            else
            {
                npcScript.anim.SetBool("IsMoving", false);
            }
            yield return null;
        }

        if (currentSublocation.lookAtPoint != null)
        {
            transform.LookAt(currentSublocation.lookAtPoint);
        }
        PerformAction(); 
    }

    void PerformAction()
    {
        npcScript.anim.SetBool("IsMoving", false);
        switch (currentSchedule.Action)
        {
            case Action.Working:
                isWorking = true; // Set isWorking to true for working action
                npcScript.BeginWorking();
             
                break;

            case Action.Walking:
                isWorking = false;
              
                break;

            case Action.Idle:
                isWorking = false;
             
                break;

            case Action.AtHome:
                isWorking = false;
               
                break;
        }
    }

    void ReleaseSublocation(Sublocation sublocation)
    {
        sublocation.isOccupied = false;
        sublocation.occupant = null;
    }

    public void TalkToPlayer()
    {
        agent.Stop();
        if(facePlayer) npcScript.faceCamera.enabled = true;
        StartCoroutine(ReturnToSchedule());
    }

    IEnumerator ReturnToSchedule()
    {
        isTalking = true;
        do
        {
            yield return new WaitForSeconds(5);
        }
        while(npcScript.dialogueController.IsTalking() && npcScript.dialogueController.currentTalker == npcScript);
        isTalking = false;
        npcScript.faceCamera.enabled = false;
        agent.Resume();
        //agent.destination = currentDestination;
    }
}

