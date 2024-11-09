using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class Mandrake : CreatureBehaviorScript
{
    [HideInInspector] public NavMeshAgent agent;
    private bool coroutineRunning = false;
    public float fleeDistance = 3f;
    public Tilemap tileMap; // Reference to your tilemap

    public GameObject farmTile;
    public CropData data;
    //public GameObject mandrakeTile;
    private GameObject spawnedFarmTile;
    private Vector3 spot;
    public float timeBeforeLeavingFarm;
    private float savedTime;


    public enum CreatureState
    {
        WakeUp,
        Run,
        Wander,
        Idle,
        LeaveFarm,
        Die,
        Trapped
    }

    public CreatureState currentState;
    private bool hasTarget = false;
    private bool isMoving;

    void Start()
    {
        base.Start();
        agent = GetComponent<NavMeshAgent>();
        currentState = CreatureState.WakeUp;
        tileMap = FindObjectOfType<Tilemap>();
        savedTime = timeBeforeLeavingFarm;

    }

    private void Update()
    {
        if (timeBeforeLeavingFarm < 0)
        {
            if(TimeManager.isDay) currentState = CreatureState.LeaveFarm;
        }
        else timeBeforeLeavingFarm -= Time.deltaTime;
        float distance = Vector3.Distance(player.position, transform.position);
        playerInSightRange = distance <= sightRange;
        if (isTrapped) { currentState = CreatureState.Trapped; }
        if (playerInSightRange && !isTrapped && !coroutineRunning && currentState != CreatureState.WakeUp) { currentState = CreatureState.Run; }
        CheckState(currentState);
    }

    public void CheckState(CreatureState currentState)
    {
        switch (currentState)
        {
            case CreatureState.WakeUp:
                WakeUp();
                break;

            case CreatureState.Run:
                Run();
                break;

            case CreatureState.Wander:
                Wander();
                break;

            case CreatureState.Idle:
                Idle();
                break;

            case CreatureState.LeaveFarm:
                LeaveFarm();
                break;

            case CreatureState.Die:
                //OnDeath();
                break;

            case CreatureState.Trapped:
                Trapped();
                break;
        }
    }

    private void Idle()
    {
       
            if (!coroutineRunning)
            {
                int r = Random.Range(1, 6);
               
                 if (r < 4 && r >= 1) StartCoroutine(WaitAround());
                 if (r >= 4) currentState = CreatureState.Wander;
            }
    }

    private IEnumerator WaitAround()
    {
        coroutineRunning = true;
        float r = Random.Range(1, 4.5f);
        yield return new WaitForSeconds(r);
        coroutineRunning = false;
    } 

    private void WakeUp()
    {
        if (!coroutineRunning)
        {
            StartCoroutine(FreakOut());
        }
    }

    IEnumerator FreakOut()
    {
        //Play freak out Animation
        if (playerInSightRange)
        {
            transform.LookAt(player);
        }
        yield return new WaitForSeconds(1); //Adjust this based off of animation time
        currentState = CreatureState.Run;
    }

    private void Run()
    {
        timeBeforeLeavingFarm = savedTime;
        agent.speed = 10;
        agent.angularSpeed = 150;
        if (playerInSightRange)
        {
            if (hasTarget && !agent.pathPending && agent.remainingDistance < agent.stoppingDistance + 1f)
            {
                hasTarget = false;
            }
            else if (!hasTarget)
            {
                hasTarget = true;
                Vector3 fleeDirection = (transform.position - player.position).normalized;

               
                float randomAngle = Random.Range(-90f, 90f); //random offset for random movement

                fleeDirection = Quaternion.Euler(0, randomAngle, 0) * fleeDirection;

                Vector3 newDestination = transform.position + fleeDirection * fleeDistance;

           
                agent.SetDestination(newDestination);
            }

        }
        else { currentState = CreatureState.Wander; }
    }

    private void Wander()
    {
        agent.speed = 2f;
        agent.angularSpeed = 80f;
        if (!isMoving)
        {
            Vector3 randomPoint = GetRandomPointAround(transform.position, 15f); //gets a random point within a 5 unit radius of itself
            StartCoroutine(MoveToPoint(randomPoint));
        }
    }

    private Vector3 GetRandomPointAround(Vector3 origin, float radius)
    {

        Vector2 randomDirection = Random.insideUnitCircle * radius;


        Vector3 randomPoint = new Vector3(randomDirection.x, origin.y, randomDirection.y) + origin;

        return randomPoint;
    }

    private IEnumerator MoveToPoint(Vector3 destination)
    {
        isMoving = true;


        agent.destination = destination;


        while (!agent.pathPending && agent.remainingDistance > agent.stoppingDistance + 1f && !playerInSightRange)
        {
            yield return null;
        }

        int r = Random.Range(0, 2);
        if (r == 0) { currentState = CreatureState.Wander; }
        else if (r == 1) { currentState = CreatureState.Idle; }

        isMoving = false;
    }

    private void LeaveFarm()
    {
        //if (hasTarget && !agent.pathPending && agent.remainingDistance < agent.stoppingDistance + 1f)
        //{
        //    Destroy(this.gameObject);
        //}
        if (!hasTarget)
        {
            hasTarget = true;
            Vector3 fleeDirection = (transform.position - player.position).normalized;
            Vector3 newDestination = transform.position + fleeDirection * fleeDistance *5;

            agent.SetDestination(newDestination);
        }
    }

    public override void OnDeath()
    {
        base.OnDeath();
    }

    private void Trapped()
    {
        rb.isKinematic = true;
    }
}
