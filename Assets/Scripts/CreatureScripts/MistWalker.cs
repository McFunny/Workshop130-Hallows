using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;

public class MistWalker : CreatureBehaviorScript
{
    public List<StructureObject> targettableStructures;

    StructureBehaviorScript targetStructure;
    public List<StructureBehaviorScript> availableStructure = new List<StructureBehaviorScript>();

    bool isMoving = false;
    bool isBeingAttacked = false; //mainly for use for priority target tracking (seed shooter)
    public bool coroutineRunning = false;
    private Transform target;
    Tilemap tileMap;
    public bool attackingPlayer = false;

    [HideInInspector] public NavMeshAgent agent;
    public AnimEvents animEvents;
    public Collider lungeAttackHitbox;
    private float lungeCooldown = 2f; // time in between lunges
    private float lastLungeTime = 0f; // tracks when the last lunge occurred


    public enum CreatureState
    {
        SpawnIn,
        Idle,
        Wander,
        WalkTowardsClosestStructure,
        WalkTowardsPriorityStructure,
        WalkTowardsPlayer,
        AttackStructure,
        AttackPlayer,
        Stun,
        Die,
        Trapped
    }

    public CreatureState currentState;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if(animEvents) animEvents.OnFloatChange += WalkSpeedToggle;
        if (animEvents) animEvents.OnColliderChange += ColliderChange;
    }

    void Start()
    {
        base.Start();
        //StartCoroutine(StructureCheck());
        //currentState = CreatureState.SpawnIn;
        lungeAttackHitbox.enabled = false;
        StructureBehaviorScript.OnStructuresUpdated += UpdateStructureList; //if a structure is placed or destroyed, this will update the list of available structures
        ImbuedScarecrow.OnScarecrowAttract += TargetImbuedScarecrow;
        UpdateStructureList();
        tileMap = StructureManager.Instance.tileMap;
        agent.enabled = false;
        agent.enabled = true;
    }
    void OnDestroy()
    {
        StructureBehaviorScript.OnStructuresUpdated -= UpdateStructureList; //unsubscribe to prevent memory leaks
        animEvents.OnColliderChange -= ColliderChange;
        animEvents.OnFloatChange -= WalkSpeedToggle;
    }

    public override void OnSpawn()
    {
        if (!isMoving)
        {
            Vector3 randomPoint = StructureManager.Instance.GetRandomTile();
            StartCoroutine(MoveToPoint(randomPoint));
        }
    }

        private void TargetImbuedScarecrow(GameObject structure)
    {
        if (currentState == CreatureState.AttackStructure)
        {
            if (targetStructure == structure.GetComponent<StructureBehaviorScript>())
            {
                return;
            }
        }
            float distance = Vector3.Distance(transform.position, structure.transform.position);
        if (distance < 25f)
        {
            targetStructure = structure.GetComponent<StructureBehaviorScript>();
            target = structure.transform;
            currentState = CreatureState.WalkTowardsPriorityStructure;
        }
    }

    private void UpdateStructureList()
    {
        availableStructure.Clear();
        foreach (var structure in structManager.allStructs)
        {
            if(targettableStructures.Contains(structure.structData)) availableStructure.Add(structure);
        }

        if (availableStructure.Count > 0)
        {
            int r = Random.Range(0, availableStructure.Count);
            targetStructure = availableStructure[r];
        }
    }



    void Update()
    {
        if (health <= 0) isDead = true;

        if (!isDead)
        {
            float distance = Vector3.Distance(player.position, transform.position);
            playerInSightRange = distance <= sightRange;

            if (isTrapped)
            {
                currentState = CreatureState.Trapped;
            }
            else if (playerInSightRange && !isTrapped && currentState != CreatureState.AttackPlayer && !coroutineRunning)
            {
                currentState = CreatureState.WalkTowardsPlayer;
            }

            CheckState(currentState);
            return;
        }

        lungeAttackHitbox.enabled = false;
    }



    private void OnDrawGizmos()
    {
        float attackRange = 9f;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    public void CheckState(CreatureState currentState)
    {
        switch (currentState)
        {
            case CreatureState.AttackPlayer:
                AttackPlayer();
                anim.SetBool("IsWalking", false);
                break;

            case CreatureState.SpawnIn:
                OnSpawn();
                break;

            case CreatureState.Idle:
                Idle();
                anim.SetBool("IsWalking", false);
                break;

            case CreatureState.Wander:
                Wander();
                anim.SetBool("IsWalking", true);
                break;

            case CreatureState.WalkTowardsClosestStructure:
                WalkTowardsClosestStructure();
                anim.SetBool("IsWalking", true);
                break;

            case CreatureState.WalkTowardsPriorityStructure:
                WalkTowardsPriorityStructure();
                anim.SetBool("IsWalking", true);
                break;

            case CreatureState.WalkTowardsPlayer:
                WalkTowardsPlayer();
                anim.SetBool("IsWalking", true);
                break;

            case CreatureState.AttackStructure:
                AttackStructure();
                anim.SetBool("IsWalking", false);
                break;

            case CreatureState.Stun:
                Stun();
                break;

            case CreatureState.Die:
                //OnDeath();
                break;

            case CreatureState.Trapped:
                Trapped();
                break;

            default:
                Debug.LogError("Unknown state: " + currentState);
                break;
        }
    }


    private void Idle()
    {
        if (!coroutineRunning)
        {
            int r = Random.Range(0, 6);
            if (r == 0) 
            {
                if (availableStructure.Count > 0) 
                {
                    currentState = CreatureState.WalkTowardsClosestStructure;
                }
            }
            else if (r < 4 && r >= 1) StartCoroutine(WaitAround());
            else if (r >= 4) currentState = CreatureState.Wander;
        }
    }

   
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
        
        coroutineRunning = true;
        float r = Random.Range(1, 4.5f);
        yield return new WaitForSeconds(r);
        coroutineRunning = false;
    }

    private IEnumerator MoveToPoint(Vector3 destination)
    {
       
        isMoving = true;
        coroutineRunning = true;

        agent.destination = destination;

       
        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
        {
            
            if (isBeingAttacked || playerInSightRange)
            {
                isMoving = false;
                coroutineRunning = false;
                yield break; 
            }

            yield return null;
        }

       
        int randomChoice = Random.Range(0, 3);
        if (randomChoice == 0)
        {
            currentState = CreatureState.Wander;
        }
        else
        {
            currentState = CreatureState.Idle;
        }

        isMoving = false;
        coroutineRunning = false;
    }





    private void WalkTowardsClosestStructure()
    {
        if (target == null || !target.gameObject.activeSelf) 
        {
            target = FindClosestStructure();
            if (target != null)
            {
                agent.destination = target.position;
            }
            else
            {
                currentState = CreatureState.Wander; 
            }
        }
        else if (!agent.pathPending && agent.remainingDistance < agent.stoppingDistance + 3f)
        {
            target = null;
            agent.ResetPath();
            currentState = CreatureState.AttackStructure;
        }
    }


    private Transform FindClosestStructure()
    {
        StructureBehaviorScript structure;
        Transform closestStructure = null;
        float closestDistance = Mathf.Infinity;  
        structure = null;

        for (int i = 0; i < availableStructure.Count; i++)
        {
            if (availableStructure[i] == null) continue;  

            float distanceToStructure = Vector3.Distance(agent.transform.position, availableStructure[i].transform.position);

           
            if (distanceToStructure < closestDistance)
            {
                closestDistance = distanceToStructure;
                closestStructure = availableStructure[i].transform;
                structure = availableStructure[i];
            }
        }
        targetStructure = structure;
        return closestStructure;
    }


    private void WalkTowardsPriorityStructure()
    {
        if (targetStructure == null) return;

        agent.destination = target.position;

       if (!agent.pathPending && agent.remainingDistance < agent.stoppingDistance + 3f)
        {
            agent.ResetPath();
            target = null;
            currentState = CreatureState.AttackStructure;
        }
    }

    private void WalkTowardsPlayer()
    {
       
        if (playerInSightRange)
        {
            agent.destination = player.transform.position;
            float distance = Vector3.Distance(player.position, transform.position);

            if (distance < 9f && Time.time >= lastLungeTime + lungeCooldown)
            {
                currentState = CreatureState.AttackPlayer;
            }
        }
        else if (!playerInSightRange)
        {
            currentState = CreatureState.Wander;
        }
    }


    private void AttackStructure()
    {

        if (targetStructure == null)
        {
            currentState = CreatureState.Wander;
        }
        else if (targetStructure != null && !coroutineRunning)
        {
         
            StartCoroutine(AttackingStructure());
        }
    }

    IEnumerator AttackingStructure()
    {
        //play animation
       
        anim.SetTrigger("IsAttacking");
        float distance = Vector3.Distance(transform.position, targetStructure.transform.position);
        if (distance < 5f)
        {
            coroutineRunning = true;
            targetStructure.health -= damageToStructure;
            transform.LookAt(targetStructure.transform.position);
            if (targetStructure.health <= 0) { targetStructure = null; }
            yield return new WaitForSeconds(3f);
            coroutineRunning = false;
        }
        else
        {
            currentState = CreatureState.WalkTowardsClosestStructure;
        }
        }



    IEnumerator AttackingPlayer()
    {
        attackingPlayer = true;
        coroutineRunning = true;

      
        anim.SetTrigger("IsLunging");

    
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length + 1);

        lastLungeTime = Time.time;
        currentState = CreatureState.WalkTowardsPlayer;

        attackingPlayer = false;
        coroutineRunning = false;
    }




    private void OnTriggerEnter(Collider other)
    {
        if (attackingPlayer && other.CompareTag("Player"))
        {
            PlayerInteraction playerInteraction = other.GetComponent<PlayerInteraction>();
            if (playerInteraction != null)
            {
                playerInteraction.StaminaChange(-25);
            }
        }
    }


    private void AttackPlayer()
    {
        agent.destination = player.position;
        if (!coroutineRunning)
        {
            StartCoroutine(AttackingPlayer());
        }
    }



    private void Stun()
    {
        // Implementation for stun behavior
    }

    public override void OnDeath()
    {
        anim.SetTrigger("IsDead");
        base.OnDeath();
        agent.enabled = false;
        rb.isKinematic = true;
        agent.ResetPath();
        rb.freezeRotation = true;
        //anim.SetTrigger("IsDead");
    }

    public override void OnDamage()
    {
        anim.SetTrigger("IsRecoiling");
    }

    private void Trapped()
    {
        agent.ResetPath();
        rb.isKinematic = true;
    }

    public void WalkSpeedToggle(float _speed)
    {
        agent.speed = _speed;
    }

    public void ColliderChange(bool b)
    {

        lungeAttackHitbox.enabled = b;
    }




    /*IEnumerator StructureCheck()
    {
        yield return new WaitForSeconds(2);
        do
        {
            yield return new WaitForSeconds(10);
            if (foundStructure || (structManager.allStructs.Count == 0))
            {
                for (int i = 0; i < availableStructure.Count; i++)
                {
                    if (availableStructure[i] == null)
                    {
                        availableStructure.RemoveAt(i);
                    }
                }
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
    }*/
}
