using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static FeralHareTest;

public class MistWalker : CreatureBehaviorScript
{
    StructureBehaviorScript foundStructure;
    public List<StructureBehaviorScript> availableStructure = new List<StructureBehaviorScript>();

    bool isMoving = false;
    bool isBeingAttacked = false; //mainly for use for priority target tracking
    bool isRunningCoroutine = false;
    private Transform target;

    [HideInInspector] public NavMeshAgent agent;
    public enum WalkerStates
    {
        Idle,
        Wander,
        WalkTowardsClosestStructure,
        WalkTowardsPriorityStructure,
        WalkTowardsPlayer,
        AttackStructure,
        AttackPlayer,
        Stun,
        Die
    }

    public WalkerStates currentState;

    void Start()
    {
        base.Start();
        agent = GetComponent<NavMeshAgent>();
        StartCoroutine(StructureCheck());
        currentState = WalkerStates.Idle;
    }

    // Update is called once per frame
    void Update()
    {

        float distance = Vector3.Distance(player.position, transform.position);
        playerInSightRange = distance <= sightRange;
        if (playerInSightRange) { currentState = WalkerStates.WalkTowardsPlayer; }
        CheckState(currentState);
    }

    public void CheckState(WalkerStates currentState)
    {
        switch (currentState)
        {
            case WalkerStates.Idle:
                Idle();
                break;
            case WalkerStates.Wander:
                Wander();
                break;

            case WalkerStates.WalkTowardsClosestStructure:
                WalkTowardsClosestStructure();
                break;

            case WalkerStates.WalkTowardsPriorityStructure:
                WalkTowardsPriorityStructure();
                break;

            case WalkerStates.WalkTowardsPlayer:
                WalkTowardsPlayer();
                break;

            case WalkerStates.AttackStructure:
                AttackStructure();
                break;

            case WalkerStates.AttackPlayer:
                AttackPlayer();
                break;

            case WalkerStates.Stun:
                Stun();
                break;

            case WalkerStates.Die:
                Die();
                break;

            default:
                Debug.LogError("Unknown state: " + currentState);
                break;
        }
    }

    private void Idle()
    {
        if (!isRunningCoroutine)
        {
            int r = Random.Range(0, 6);
            if (r == 0) 
            {
                if (availableStructure.Count > 0) 
                {
                    currentState = WalkerStates.WalkTowardsClosestStructure;
                }
            }
            else if (r < 3 && r >= 1) StartCoroutine(WaitAround());
            else if (r >= 3) currentState = WalkerStates.Wander;
        }
    }

    // Implement each method to define behavior
    public void Wander()
    {
        if (!isMoving)
        {
            Vector3 randomPoint = GetRandomPointAround(transform.position, 5f); //gets a random point within a 5 unit radius of itself
            StartCoroutine(MoveToPoint(randomPoint));
        }
    }

   
    private Vector3 GetRandomPointAround(Vector3 origin, float radius)
    {
       
        Vector2 randomDirection = Random.insideUnitCircle * radius;

     
        Vector3 randomPoint = new Vector3(randomDirection.x, origin.y, randomDirection.y) + origin;

        return randomPoint;
    }

    private IEnumerator WaitAround()
    {
        isRunningCoroutine = true;
        float r = Random.Range(1, 4.5f);
        yield return new WaitForSeconds(r);
        isRunningCoroutine = false;
    }

    private IEnumerator MoveToPoint(Vector3 destination)
    {
        isMoving = true;

        
        agent.destination = destination;

        
        while (!agent.pathPending && agent.remainingDistance > agent.stoppingDistance + 1f && !isBeingAttacked && !playerInSightRange)
        {
            yield return null;
        }

        // Agent has reached the destination, now decide the next action (50/50 chance)
        int randomChoice = Random.Range(0, 2);  
        if (randomChoice == 0)
        {
            currentState = WalkerStates.Idle;   
        }
        else
        {
            currentState = WalkerStates.Wander;
        }

        isMoving = false;
    }




    private void WalkTowardsClosestStructure()
    {
        if (target == null)
        {
            target = FindClosestStructure();
            agent.destination = target.position;
        }
        else if (!agent.pathPending && agent.remainingDistance < agent.stoppingDistance + 1f)
        {
            target = null;
            currentState = WalkerStates.AttackStructure;
        }

    }

    private Transform FindClosestStructure()
    {
        Transform closestStructure = null;
        for (int i = 0; i < availableStructure.Count; i++)
        {
            if (i == 0)
            {
                closestStructure = availableStructure[i].transform;
            }
            else
            {
                if (Vector3.Distance(agent.transform.position, availableStructure[i].transform.position) < Vector3.Distance(agent.transform.position, availableStructure[i-1].transform.position))
                {
                    closestStructure = availableStructure[i].transform;
                }
            }
        }

        return closestStructure;
    }

    private void WalkTowardsPriorityStructure()
    {
        // Implementation for walking towards a priority structure
    }

    private void WalkTowardsPlayer()
    {
        if(playerInSightRange)
        {
            agent.destination = player.transform.position;
        }
        else if (!playerInSightRange)
        {
            currentState = WalkerStates.Wander;
        }
    }

    private void AttackStructure()
    {
        print("Attacking");
    }

    private void AttackPlayer()
    {
        // Implementation for attacking the player
    }

    private void Stun()
    {
        // Implementation for stun behavior
    }

    private void Die()
    {
        // Implementation for death behavior
    }




    IEnumerator StructureCheck()
    {
        yield return new WaitForSeconds(2);
        do
        {
            yield return new WaitForSeconds(10);
            if (foundStructure || (structManager.allStructs.Count == 0))
            {
                yield return new WaitForSeconds(5);
            }
            else
            {
                foreach (StructureBehaviorScript structure in structManager.allStructs)
                {
                    availableStructure.Add(structure);
                    if (structure != availableStructure[availableStructure.Count-1]) availableStructure.Remove(structure);


                }
                if (availableStructure.Count > 0)
                {
                    int r = Random.Range(0, availableStructure.Count);
                    foundStructure = availableStructure[r];
                }
            }
        } while (gameObject.activeSelf);
    }
}
