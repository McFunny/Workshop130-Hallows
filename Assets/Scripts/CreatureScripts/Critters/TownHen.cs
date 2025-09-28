using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;

public class TownHen : CreatureBehaviorScript
{
    float walkSpeed = 4;
    float runSpeed = 11;

    private bool hasTarget = false; //For fleeing

    public int henID = -1;

    public LayerMask obstacleMask;

    NavMeshAgent agent;

    Coroutine currentRoutine;

    bool isMoving;

    Vector3 target;

    public enum CritterState
    {
        Decide,
        Idle, //Standing still
        Wander, //Moving to random spot
        Flee,
        Dead
    }

    public CritterState currentState;

    void Start()
    {
        base.Start();
        //CritterStart();
        agent = GetComponent<NavMeshAgent>();
        StartCoroutine(IdleSoundTimer());
        StartCoroutine(DelayedStart());
    }

    void OnDestroy()
    {
        base.OnDestroy();
        //OnCritterDestroy();
    }

    ////////Critter Specific Stuff///////////
    /// 
    /// 
    
    public void CheckState(CritterState currentState)
    {
        switch (currentState)
        {
            case CritterState.Decide:
                Decide();
                break;

            case CritterState.Idle:
                Idle();
                break;

            case CritterState.Wander:
                Wander();
                break;
            
            case CritterState.Flee:
                Flee();
                break;

            default:
                Debug.LogError("Unknown state: " + currentState);
                break;
        }
    }

    void Update()
    {
        if(agent.velocity.magnitude < 0.2f)
        {
            anim.SetBool("IsWalking", false);
            anim.SetBool("IsRunning", false);
        }
        else if(agent.velocity.magnitude < walkSpeed + 1)
        {
            anim.SetBool("IsWalking", true);
            anim.SetBool("IsRunning", false);
        }
        else
        {
            anim.SetBool("IsWalking", false);
            anim.SetBool("IsRunning", true);
        }

        float distance = Vector3.Distance(player.position, transform.position);
        playerInSightRange = distance <= sightRange;

        //
        if(!isDead) CheckState(currentState);
    }

    void Decide()
    {
        int r = 0;

        if(playerInSightRange)
        {
            currentState = CritterState.Flee;
            return;
        }

        r = Random.Range(0, 13);
        if (r < 6)
        {
            currentState = CritterState.Idle;
        }
        else
        {
            currentState = CritterState.Wander;
        }
    }

    void Idle()
    {
        if(currentRoutine == null)
        {
            currentRoutine = StartCoroutine(WaitAround());
        }
    }

    protected IEnumerator WaitAround()
    {
        agent.ResetPath();
        float time = Random.Range(1f, 9f);
        if(time > 5)
        {
            anim.SetBool("IsSitting", true);
            anim.Play("HenSit");
            time += 5;
        }
        else
        {
            int r = Random.Range(0,10);
            if(r > 7) anim.Play("HenIdle1");
            else if(r > 5) anim.Play("HenIdle2");
        }
        yield return new WaitForSeconds(time);
        
        if(time > 8)
        {
            anim.SetBool("IsSitting", false);
            yield return new WaitForSeconds(1);
        }

        currentState = CritterState.Decide;
        currentRoutine = null;

    }

    void Wander()
    {
        if (!isMoving && currentRoutine == null)
        {

            agent.speed = walkSpeed;
            target = GetRandomPointAround(transform.position, 7f);

            if(Random.Range(0, 20) == 9 && CanFlyToPoint(target)) currentRoutine = StartCoroutine(FlyToPoint(target));
            else currentRoutine = StartCoroutine(MoveToPoint(target, 7));
        }

        if(playerInSightRange && currentRoutine != null && agent.enabled)
        {
            currentState = CritterState.Flee;
            if(currentRoutine != null) StopCoroutine(currentRoutine);
            currentRoutine = null;
            isMoving = false;
        }
    }

    void Flee()
    {
        if (playerInSightRange)
        {
            agent.speed = runSpeed;
            if (hasTarget && !agent.pathPending && agent.remainingDistance < agent.stoppingDistance + 1.5f)
            {
                hasTarget = false;
            }
            else if (!hasTarget)
            {
                hasTarget = true;
                Vector3 fleeDirection = (transform.position - player.position).normalized;

               
                float randomAngle = Random.Range(-45f, 45f); //random offset for random movement

                fleeDirection = Quaternion.Euler(0, randomAngle, 0) * fleeDirection;

                Vector3 newDestination = transform.position + fleeDirection * 5;

           
                agent.SetDestination(newDestination);
            }

        }
        else 
        { 
            agent.speed = walkSpeed;
            currentState = CritterState.Wander; 
        }
    }

    protected IEnumerator MoveToPoint(Vector3 destination, float duration) //Used just for wandering it seems
    {
        isMoving = true;

        agent.destination = destination;

        float timeSpent = 0; //to make sure it doesnt get stuck

        while ((agent.pathPending || agent.remainingDistance > agent.stoppingDistance) && timeSpent < duration)
        {
            timeSpent += Time.deltaTime;

            yield return null;
        }

        FinishedMoving();

    }

    protected void FinishedMoving()
    {
        //
        if(currentState == CritterState.Wander)
        {
            currentState = CritterState.Idle;
        }
        
        isMoving = false;
        currentRoutine = null;
    }

    bool CanFlyToPoint(Vector3 pos)
    {
        float reach = Vector3.Distance(transform.position, pos);
        if(reach > 12 || reach < 2)
        {
            //print("Cant Fly, target too far");
            return false;
        }
        Vector3 dir = (pos - transform.position).normalized;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, dir, out hit, reach, obstacleMask))
        {
            //print("Cant Fly");
            return false;
        }
        //print("Can Fly");
        return true;
    }

    IEnumerator FlyToPoint(Vector3 pos)
    {
        agent.ResetPath();
        agent.enabled = false;
        Vector3 targetPostition = new Vector3( pos.x, transform.position.y, pos.z );
        transform.LookAt(targetPostition);
        anim.Play("HenJump");

        yield return new WaitForSeconds(0.2f);
        transform.DOJump(pos, 0.7f, 1, 1f);
        yield return new WaitForSeconds(1.4f);
        agent.enabled = true;
        FinishedMoving();
    }

    public override void OnDamage()
    {
        if(health > 0 && effectsHandler.hitSounds.Length > 0) effectsHandler.OnHit();
    }


    public override void OnDeath()
    {
        if (!isDead)
        {
            isDead = true;
            anim.Play("Death");
            base.OnDeath();
            agent.speed = 0;
            agent.ResetPath();
            rb.isKinematic = true;
            rb.freezeRotation = true;
            StopAllCoroutines();
            canCorpseBreak = true;

            GameSaveData.Instance.deadHenIDs.Add(henID);
        }
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(3);
        for(int i = 0; i < GameSaveData.Instance.deadHenIDs.Count; i++)
        {
            if(GameSaveData.Instance.deadHenIDs[i] == henID) Destroy(gameObject);
        }
    }

    protected Vector3 GetRandomPointAround(Vector3 origin, float radius)
    {
        Vector2 randomDirection = Random.insideUnitCircle * radius;
        Vector3 randomPoint = new Vector3(randomDirection.x, origin.y, randomDirection.y) + origin;
        return randomPoint;
    }

    protected IEnumerator IdleSoundTimer()
    {
        while(health > 0)
        {
            int i = Random.Range(3,8);
            yield return new WaitForSeconds(i);
            effectsHandler.RandomIdle();
            if(health > 0) health += 5;
            if(health > maxHealth) health = maxHealth;
        }
    }
}
