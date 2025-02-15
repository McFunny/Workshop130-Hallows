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
    public ActionCheck actionCheck;
    public ActionAnim actionAnim;
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

    ActionAnim actionToPlay = ActionAnim.Stand;

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
                bool passedCheck = true;
                switch(schedule.actionCheck)
                {
                    case ActionCheck.Check1:
                    passedCheck = npcScript.ActionCheck1();
                    break;
                    case ActionCheck.Check2:
                    passedCheck = npcScript.ActionCheck2();
                    break;
                    case ActionCheck.Check3:
                    passedCheck = npcScript.ActionCheck3();
                    break;
                }

                if(!passedCheck) break;
                
                

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
                else if (isWorker)
                {
                    // Standard behavior for other actions

                    currentSublocation = npcMovementManager.GetRandomSublocation(schedule.Destination, isWorker);
                }
                else
                {
                    currentSublocation = npcMovementManager.GetRandomSublocation(schedule.Destination, false);
                   
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
        npcScript.anim.SetBool("IsLeaning", false);
        yield return new WaitForSeconds(1.5f);

        actionToPlay = currentSchedule.actionAnim;

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
        Debug.Log("We made it here");
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

        switch (actionToPlay)
        {
            case ActionAnim.Stand:
            break;
            case ActionAnim.Lean:
            npcScript.anim.SetBool("IsLeaning", true);
            break;
            case ActionAnim.Sit:
            break;
        }
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
        if(actionToPlay == ActionAnim.Stand) npcScript.faceCamera.enabled = true;
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

public enum ActionCheck
{
    None,
    Check1,
    Check2,
    Check3
}

public enum ActionAnim
{
    Stand,
    Lean,
    Sit
}

