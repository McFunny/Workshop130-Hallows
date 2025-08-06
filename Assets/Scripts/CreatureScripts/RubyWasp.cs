using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RubyWasp : CreatureBehaviorScript
{
    public RubyWaspSwarm homeSwarm;

    public float accelerationRate, maxVelocity;

    public Vector3 targetPos;
    public Vector3 parentPos;

    public List<FireFearTrigger> fireSources; //find out which one is the player torch; they will prioritize following this one
    int currentFirePriority = 0;

    public GameObject fearObject; //The particle system
    Vector3 fearedObjectPosition; //Where the lavent leaf is
    Vector3 fleeToPos;

    public enum CreatureState
    {
        Wander,
        Chase,
        Attack,
        Stuck,
        Flee
    }

    public CreatureState currentState;

    void Start()
    {
        base.Start();

        accelerationRate += Random.Range(-3, 3);
        transform.position = new Vector3(transform.position.x, transform.position.y + Random.Range(-.4f, .4f), transform.position.z);
        StartCoroutine(RefreshDestination());

        targetPos = transform.position;
    }

    void FixedUpdate()
    {
        base.Update();
        if(!homeSwarm) Destroy(gameObject); //Should never happen unless morning hit

        CheckState(currentState);
    }

    public void CheckState(CreatureState currentState)
    {
        switch (currentState)
        {
            case CreatureState.Wander:
                Wander();
                break;
            case CreatureState.Chase:
                //Chase();
                break;
            case CreatureState.Attack:
                //Attack();
                break;
            case CreatureState.Stuck:
                //Stuck();
                break;
            case CreatureState.Flee:
                //Flee();
                break;

            default:
                Debug.LogError("Unknown state: " + currentState);
                break;
        }
    }

    void Wander()
    {
        Vector3 dir = (transform.position - targetPos).normalized;
        dir *= -1f;
        rb.AddForce(dir * (accelerationRate));

        LimitVelocity();

        //transform.LookAt(new Vector3(homeSwarm.transform.position.x, transform.position.y, homeSwarm.transform.position.z));
        SmoothLookAt(targetPos);
    }

    IEnumerator RefreshDestination()
    {
        while(health > 0)
        {
            yield return new WaitForSeconds(Random.Range(0.5f, 1f));
            if(currentState == CreatureState.Wander)
            {
                if(homeSwarm) targetPos = GetRandomPointNearby(homeSwarm.transform.position);
                else targetPos = StructureManager.Instance.GetRandomNearbyTile(GridType.Farm, 25, transform.position);
            }
        }
    }

    Vector3 GetRandomPointNearby(Vector3 target)
    {
        float x = Random.Range(-1f, 1f);
        float z = Random.Range(-1f, 1f);
        return new Vector3(target.x + x, target.y, target.z + z);
    }

    void SmoothLookAt(Vector3 targetPos)
    {
        targetPos.y = transform.position.y;
        // Calculate the direction vector to the target
        Vector3 direction = targetPos - transform.position;

        // Create a Quaternion representing the target rotation
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // Smoothly interpolate towards the target rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5 * Time.deltaTime);
    }

    void LimitVelocity()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // Limit velocity if needed
        if (flatVel.magnitude > maxVelocity)
        {
            Vector3 limitedVel = flatVel.normalized * maxVelocity;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    void OnDestroy()
    {
        base.OnDestroy();
        if(homeSwarm) homeSwarm.wasps.Remove(gameObject);
    }
}
