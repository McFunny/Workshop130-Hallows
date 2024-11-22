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

    Vector3 despawnPos;


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

        int r = Random.Range(0, NightSpawningManager.Instance.despawnPositions.Length);
        despawnPos = NightSpawningManager.Instance.despawnPositions[r].position;

    }

    private void Update()
    {
        if(currentState == CreatureState.Die || currentState == CreatureState.Trapped) return;
        if(currentState == CreatureState.WakeUp)
        {
            CheckState(currentState);
            return;
        }
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

        if(agent.velocity.sqrMagnitude > 0) anim.SetBool("IsRunning", true);
        else anim.SetBool("IsRunning", false);
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
                //anim.SetBool("IsRunning", true);
                break;

            case CreatureState.Wander:
                Wander();
                //anim.SetBool("IsRunning", true);
                break;

            case CreatureState.Idle:
                Idle();
                //anim.SetBool("IsRunning", false);
                break;

            case CreatureState.LeaveFarm:
                LeaveFarm();
                //anim.SetBool("IsRunning", true);
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
                if(playerInSightRange)
                {
                    currentState = CreatureState.Run;
                    return;
                }
                int r = Random.Range(1, 7);
               
                if (r < 4) StartCoroutine(WaitAround());
                else if (r < 6) currentState = CreatureState.Wander;
                else StartCoroutine(Scream()); //scream
            }
    }

    private IEnumerator WaitAround()
    {
        coroutineRunning = true;
        float r = Random.Range(0.4f, 1.8f);
        yield return new WaitForSeconds(r);
        coroutineRunning = false;
    } 

    private IEnumerator Scream()
    {
        coroutineRunning = true;
        float r = 1.5f;
        anim.SetTrigger("IsScreaming");
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
        coroutineRunning = true;
        if (playerInSightRange)
        {
            transform.LookAt(player);
        }
        yield return new WaitForSeconds(1.2f); //Adjust this based off of animation time
        coroutineRunning = false;
        currentState = CreatureState.Run;
    }

    private void Run()
    {
        timeBeforeLeavingFarm = savedTime;
        //agent.speed = 10;
        //agent.angularSpeed = 150;
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

               
                float randomAngle = Random.Range(-45f, 45f); //random offset for random movement

                fleeDirection = Quaternion.Euler(0, randomAngle, 0) * fleeDirection;

                Vector3 newDestination = transform.position + fleeDirection * fleeDistance;

           
                agent.SetDestination(newDestination);
            }

        }
        else { currentState = CreatureState.Wander; }
    }

    private void Wander()
    {
        //agent.speed = 2f;
        //agent.angularSpeed = 80f;
        if(playerInSightRange) currentState = CreatureState.Run;
        else if (!isMoving)
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


        while (!agent.pathPending && agent.remainingDistance > agent.stoppingDistance + 0.1f && !playerInSightRange)
        {
            yield return null;
        }

        int r = Random.Range(0, 3);
        if (r == 0) { currentState = CreatureState.Wander; }
        else { currentState = CreatureState.Idle; }

        isMoving = false;
    }

    public override void OnStun(float duration)
    {
        StartCoroutine(Stun(duration));
        agent.destination = transform.position;
        anim.SetBool("IsRunning", false);
    }

    IEnumerator Stun(float duration)
    {
        currentState = CreatureState.Trapped;
        yield return new WaitForSeconds(duration);
        currentState = CreatureState.Wander;
    }

    private void LeaveFarm()
    {
        agent.SetDestination(despawnPos);
    }

    public override void OnDeath()
    {
        StopAllCoroutines();
        currentState = CreatureState.Die;
        anim.SetTrigger("IsDead");
        agent.enabled = false;
        base.OnDeath();
    }

    private void Trapped()
    {
        rb.isKinematic = true;
    }
}
