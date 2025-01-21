using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;
public class PlantMimic : CreatureBehaviorScript
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


    public enum CreatureState
    {
        WakeUp,
        Run,
        Wander,
        FindAvaliableLand,
        Dig,
        Replant,
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
        currentState = CreatureState.Wander;
        tileMap = FindObjectOfType<Tilemap>();

    }

    private void Update()
    {
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

            case CreatureState.FindAvaliableLand:
                FindAvaliableLand();
                break;

            case CreatureState.Dig:
                Dig();
                break;

            case CreatureState.Replant:
                Replant();
                break;

            case CreatureState.Die:
                //OnDeath();
                break;

            case CreatureState.Trapped:
                Trapped();
                break;
        }
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


                float randomAngle = Random.Range(-30f, 30f); //random offset for random movement

                fleeDirection = Quaternion.Euler(0, randomAngle, 0) * fleeDirection;

                Vector3 newDestination = transform.position + fleeDirection * fleeDistance;


                agent.SetDestination(newDestination);
            }

        }
        else { currentState = CreatureState.Wander; }
    }

    private void Wander()
    {
        agent.speed = 3.5f;
        agent.angularSpeed = 120f;
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

        // Agent has reached the destination, now decide the next action (50/50 chance)
        int randomChoice = Random.Range(0, 3);
        if (randomChoice == 0)
        {
            currentState = CreatureState.Wander;
        }
        else
        {
            currentState = CreatureState.FindAvaliableLand;
        }

        isMoving = false;
    }

    private void FindAvaliableLand()
    {

        Vector3? availableLand = GetClosestAvailableLand(transform.position);
        if (availableLand.HasValue)
        {

            Vector3Int cellPosition = tileMap.WorldToCell(availableLand.Value);  // Convert availableLand world position to a cell position


            spot = tileMap.GetCellCenterWorld(cellPosition); // Get the center of the cell in world space


            agent.SetDestination(spot);

            if (!agent.pathPending && agent.remainingDistance < agent.stoppingDistance + 0.1f)
            {
                currentState = CreatureState.Dig;
            }
        }
        else
        {
            Debug.Log("No available land found!");
        }
    }


    private Vector3? GetClosestAvailableLand(Vector3 startPosition)
    {
        List<(Vector3 tilePosition, float distance)> tileDistances = new List<(Vector3, float)>();


        foreach (var position in tileMap.cellBounds.allPositionsWithin)
        {

            Vector3 worldPosition = tileMap.CellToWorld(position);


            if (tileMap.HasTile(position))
            {

                float distance = Vector3.Distance(startPosition, worldPosition);


                tileDistances.Add((worldPosition, distance));
            }
        }


        tileDistances.Sort((a, b) => a.distance.CompareTo(b.distance));

        // Find the closest available tile that is not taken
        foreach (var (tilePosition, _) in tileDistances)
        {
            if (!IsTileTaken(tilePosition))
            {
                return tilePosition; // Return the first available tile
            }
        }

        return null; // No available land found
    }

    private bool IsTileTaken(Vector3 tilePosition)
    {
        foreach (StructureBehaviorScript structure in structManager.allStructs)
        {

            if (Vector3.Distance(structure.transform.position, tilePosition) < 0.1f)
            {
                return true; // Tile is taken
            }
        }
        return false; // Tile is available
    }

    private void Dig()
    {
        if (!coroutineRunning)
        {
            StartCoroutine(Digging());
        }
    }

    IEnumerator Digging()
    {
        coroutineRunning = true;

        // Play dig animation
        float digDuration = 5f;
        float elapsedTime = 0f;

        // Check if the player is nearby when digging; if so, break the coroutine
        while (elapsedTime < digDuration)
        {
            if (playerInSightRange)
            {
                Debug.Log("Player is too close! Cancelling dig.");
                coroutineRunning = false;
                yield break;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Spawn the farm tile
        spawnedFarmTile = structManager.SpawnStructureWithInstance(farmTile, spot);

        // Wait for a frame to ensure the farm tile is instantiated before searching

        if (spawnedFarmTile == null)
        {
            Debug.LogWarning("No farm tile found at the location!");
        }

        currentState = CreatureState.Replant;
        coroutineRunning = false;

        // Pass the reference to the Replant coroutine
        StartCoroutine(Replanting(spawnedFarmTile));
    }




    private void Replant()
    {
        if (!coroutineRunning)
        {
            StartCoroutine(Replanting(spawnedFarmTile));
        }
    }

    IEnumerator Replanting(GameObject spawnedFarmTile)
    {
        coroutineRunning = true;

        // Play replant animation
        float replantDuration = 5f;
        float elapsedTime = 0f;

        // Check if the player is nearby when replanting; if so, break the coroutine
        while (elapsedTime < replantDuration)
        {
            if (playerInSightRange)
            {
                Debug.Log("Player is too close! Cancelling replant.");
                coroutineRunning = false;
                yield break;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (spawnedFarmTile != null)
        {
            //Destroy(spawnedFarmTile);

            //spawnedFarmTile.GetComponent<FarmLand>().InsertCreature(data, 4); //this is mimic behavior btw, the mandrake should not replant itself
        }

        // Spawn the mandrake tile
        //structManager.SpawnStructure(mandrakeTile, spot);
        coroutineRunning = false;
        Destroy(this.gameObject);
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
