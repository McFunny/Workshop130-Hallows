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
    public Sublocation currentSublocation;

    public bool isWorking;
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
                switch (schedule.actionCheck)
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

                if (!passedCheck) continue;

                if (schedule.Destination == Destination.RandomLocation)
                {
                    StartCoroutine(DelayedRandomDestination(schedule));
                }
                else
                {
                    AssignSublocation(schedule);
                }
                return;
            }
        }
    }

    IEnumerator DelayedRandomDestination(Schedule schedule)
    {
        float initialDelay = Random.Range(2.5f, 5f);
        yield return new WaitForSeconds(initialDelay);

        int attempts = 0;
        while (attempts < 5)
        {
            bool success = AssignSublocation(schedule);
            if (success)
                yield break;

            float retryDelay = Random.Range(1f, 3f);
            yield return new WaitForSeconds(retryDelay);
            attempts++;
        }

        Debug.LogWarning($"{gameObject.name} could not find available sublocation after 5 attempts for RandomLocation.");
    }

    bool AssignSublocation(Schedule schedule)
    {
        currentSchedule = schedule;

        if (currentSublocation != null)
        {
            ReleaseSublocation(currentSublocation);
        }

        bool isWorker = (schedule.Action == Action.Working);

        if (schedule.Action == Action.AtHome)
        {
            currentSublocation = npcMovementManager.GetRandomSublocation(schedule.Destination, false, true);
        }
        else if (isWorker)
        {
            currentSublocation = npcMovementManager.GetRandomSublocation(schedule.Destination, true, false);
        }
        else
        {
            currentSublocation = npcMovementManager.GetRandomSublocation(schedule.Destination, false, false);
        }

        if (currentSublocation != null)
        {
            if (!currentSublocation.isAtHome)
            {
                currentSublocation.isOccupied = true;
                currentSublocation.occupant = this;
            }

            StartCoroutine(MoveToDestination(currentSublocation.transform));
            return true;
        }
        else
        {
            if (schedule.Destination == Destination.RandomLocation)
            {
                return false;
            }

            Transform mainDestination = npcMovementManager.GetDestination(schedule.Destination);

            if (mainDestination != null)
            {
                Debug.Log($"NPC {gameObject.name} moving to main destination {schedule.Destination}");
                StartCoroutine(MoveToDestination(mainDestination));
            }
            else
            {
                Debug.LogWarning($"No available destination or sublocation for {schedule.Destination}");
            }

            return true;
        }
    }

    IEnumerator MoveToDestination(Transform destination)
    {
        if(destination == null) yield break;
        npcScript.anim.SetBool("IsLeaning", false);
        yield return new WaitForSeconds(1.5f);

        actionToPlay = currentSchedule.actionAnim;

        agent.destination = destination.position;
        currentDestination = destination.position;

        while (agent.pathPending)
        {
            yield return null;
        }

        while (agent.isStopped)
        {
            yield return null;
        }

        if (isWorking)
        {
            npcScript.StopWorking();
            isWorking = false;
        }

        while (agent.remainingDistance > agent.stoppingDistance)
        {
            npcScript.anim.SetBool("IsMoving", !agent.isStopped);
            yield return null;
        }

        if (currentSublocation != null && currentSublocation.lookAtPoint != null)
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
                isWorking = true;
                npcScript.BeginWorking();
                break;
            case Action.Walking:
            case Action.Idle:
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
        //agent.Stop();
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.Stop();
        if (actionToPlay == ActionAnim.Stand) npcScript.faceCamera.enabled = true;
        StartCoroutine(ReturnToSchedule());
    }

    IEnumerator ReturnToSchedule()
    {
        isTalking = true;
        do
        {
            yield return new WaitForSeconds(5);
        }
        while (npcScript.dialogueController.IsTalking() && npcScript.dialogueController.currentTalker == npcScript);
        isTalking = false;
        npcScript.faceCamera.enabled = false;
        agent.Resume();
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
