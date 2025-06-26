using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;

public class BugBehaviorScript : MonoBehaviour
{
    //Ideally, this would be pooled and updated with a bugdata object. But to avoid the inumerous edge cases and for ease of development short term, they will be made like the creatures, as prefabs

    //Needs despawning behavior for incorrect time of day
    //Def potential here for a leaf cutter bug that the player can automate their defense by catching enough or foxglove pesticide

    public InventoryItemData bugItem;

    public SpriteRenderer r;

    public Transform colliderObject;
    Vector3 startPos;

    public Sprite[] movingSprites;
    public float animSpeed = 0.3f;

    public ParticleSystem burrowingParticles;

    NavMeshAgent agent;

    public float walkSpeed, fleeSpeed;
    public float sightRange = 0; //0 means it ignores the player
    public float reactionTimeMin, reactionTimeMax; //How fast it does its action when approached by the player, IE time before fleeing after approached by player
    public float despawnChance = 100; //Chance of the state turning to Leave when approached by the player
    public float minTravelTime = 1.5f;
    public float maxTravelTime = 3; //how long until it finds a new point

    protected bool isMoving = false;
    protected bool coroutineRunning = false;
    protected bool isLeaving;
    protected bool playerInSightRange;

    protected Coroutine approachedRoutine, walkRoutine; 
    protected Transform target;
    protected Transform player;
    protected Collider hitBox;

    public BugObject bugData;
    public DespawnMethod despawnMethod;
    protected int hoursAlive = 0;
    protected int maxLifetime = 6;

    private Sequence flutter;

    public enum DespawnMethod
    {
        Poof,
        Burrow,
        Fly
    }

    public BugState currentState;

    public enum BugState
    {
        Wander,
        MovingToTarget,
        Panic,
        Leave,
        Stun,
        UniqueBehavior1,
        UniqueBehavior2
    }
    

    public void CheckState(BugState currentState)
    {
        switch (currentState)
        {
            case BugState.Wander:
                Wander();
                break;

            case BugState.MovingToTarget:
                MovingToTarget();
                break;

            case BugState.Panic:
                Panic();
                break;

            case BugState.Leave:
                if(!isLeaving)
                {
                    isLeaving = true;
                    StartCoroutine(Leaving());
                }
                break;

            case BugState.Stun:
                break;

            case BugState.UniqueBehavior1:
                break;

            case BugState.UniqueBehavior2:
                break;

            default:
                Debug.LogError("Unknown state: " + currentState);
                break;
        }
    }

    protected void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        hitBox = GetComponentInChildren<Collider>();
        target = null;
        if(!colliderObject) colliderObject = transform;
        startPos = colliderObject.position;
    }

    protected void Start()
    {
        player = PlayerInteraction.Instance.transform;
        StartCoroutine(AnimateBug());

        TimeManager.OnHourlyUpdate -= HourlyUpdate;

        //if(despawnMethod == DespawnMethod.Fly) Flutter();
    }

    protected void OnDisable()
    {
        TimeManager.OnHourlyUpdate -= HourlyUpdate;
        if(BugSpawningManager.Instance.allBugs.Contains(this.gameObject))
        {
            BugSpawningManager.Instance.allBugs.Remove(this.gameObject);
        }
    }

    protected virtual void HourlyUpdate()
    {
        hoursAlive++;
        if(hoursAlive >= maxLifetime || !bugData.activeHours.Contains(TimeManager.Instance.timeOfDay)) currentState = BugState.Leave;
    }

    protected virtual void Update()
    {
        if(currentState == BugState.Panic) agent.speed = fleeSpeed;
        else agent.speed = walkSpeed;

        float distance = Vector3.Distance(player.position, transform.position);
        playerInSightRange = distance <= sightRange;

        if(playerInSightRange && approachedRoutine == null) approachedRoutine = StartCoroutine(ApproachedByPlayer());

        CheckState(currentState);

        CheckOrientation();

        if(despawnMethod == DespawnMethod.Fly && currentState != BugState.Leave) Flutter();
        
    }

    protected virtual void Wander()
    {
        if (!isMoving && currentState == BugState.Wander)
        {
            Vector3 randomPoint;
            randomPoint = GetRandomPointAround(transform.position, 5f);
            walkRoutine = StartCoroutine(MoveToPoint(randomPoint));
        }
    }

    protected Vector3 GetRandomPointAround(Vector3 origin, float radius)
    {
        Vector2 randomDirection = Random.insideUnitCircle * radius;
        Vector3 randomPoint = new Vector3(randomDirection.x, origin.y, randomDirection.y) + origin;
        return randomPoint;
    }

    protected IEnumerator MoveToPoint(Vector3 destination)
    {
        isMoving = true;
        coroutineRunning = true;

        agent.destination = destination;

        float timeSpent = 0; //to make sure it doesnt get stuck
        float maxTime = Random.Range(minTravelTime, maxTravelTime);
        bool stopEarly = false;
        if(currentState == BugState.Panic) maxTime = maxTime/3;

        while (timeSpent < maxTime)
        {
            timeSpent += Time.deltaTime;
            if(target != null && currentState == BugState.Wander) stopEarly = true;

            if(currentState == BugState.Leave) stopEarly = true;

            //if(((agent.pathPending || agent.remainingDistance > agent.stoppingDistance) && currentState == BugState.Panic)) stopEarly = true;

            if(stopEarly)
            {
                isMoving = false;
                coroutineRunning = false;
                walkRoutine = null;
                yield break;
            }
            yield return null;
        }

        isMoving = false;
        coroutineRunning = false;
        walkRoutine = null;
    }

    protected void MovingToTarget()
    {
        if (!isMoving && currentState == BugState.MovingToTarget)
        {
            walkRoutine = StartCoroutine(MoveToPoint(target.position));
        }
    }

    private void Panic() 
    {
        if (!isMoving && currentState == BugState.Panic)
        {
            Vector3 randomPoint;
            randomPoint = GetRandomPointAround(transform.position, 5f);
            walkRoutine = StartCoroutine(MoveToPoint(randomPoint));
        }
    }

    private void Flee() //currently not implemented
    {
        if(!target) target = player;
        Vector3 runTo = transform.position + ((transform.position - target.position + new Vector3(Random.Range(-3, 3), 0, Random.Range(-3, 3)) * 1));
        agent.destination = runTo;
    }

    protected IEnumerator Leaving()
    {
        if(flutter != null) flutter.Kill();
        hitBox.enabled = false;
        agent.enabled = false;
        switch(despawnMethod)
        {
            case DespawnMethod.Poof:
            ParticlePoolManager.Instance.MoveAndPlayParticle(colliderObject.position, ParticlePoolManager.Instance.dirtParticle);
            Destroy(gameObject);
            break;
            case DespawnMethod.Burrow:
            colliderObject.DOShakePosition(3f, 0.9f, 0, 0.2f, false);
            colliderObject.DOMoveY(colliderObject.position.y - .5f, 4);
            if(colliderObject == transform) burrowingParticles.transform.DOMoveY(burrowingParticles.transform.position.y + .5f, 4); //to offset the dig
            burrowingParticles.Play();
            for(int i = 0; i < 5; i++)
            {
                yield return new WaitForSeconds(0.5f);
                ParticlePoolManager.Instance.MoveAndPlayParticle(colliderObject.position, ParticlePoolManager.Instance.dirtParticle);
            }
            ParticlePoolManager.Instance.GrabDirtPixelParticle().transform.position = transform.position;
            Destroy(gameObject);
            break;
            case DespawnMethod.Fly:
            for(int i = 0; i < 100; i++)
            {
                yield return new WaitForSeconds(0.1f);
                colliderObject.Translate(Vector3.up * Time.deltaTime * 20, Space.World);
            }
            Destroy(gameObject);
            break;
            default:
            Destroy(gameObject);
            break;
        }
    }

    protected IEnumerator ApproachedByPlayer()
    {
        if(currentState == BugState.Wander) currentState = BugState.Panic;
        yield return new WaitForSeconds(Random.Range(reactionTimeMin, reactionTimeMax));
        //if no specific bug behavior
        if(Random.Range(0, 100) < despawnChance) currentState = BugState.Leave;
        else
        {
            currentState = BugState.Wander;
            approachedRoutine = null;
        }
    }

    protected void Flutter()
    {
        /*flutter = colliderObject.DOLocalJump(colliderObject.position, 1, 1, 1.5f)
                 .SetLoops(-1, LoopType.Restart)
                 .SetEase(Ease.InOutQuad);*/

        float newY = Mathf.Sin(Time.time * 3) * 0.5f; //Last number is the height
        colliderObject.position = new Vector3(transform.position.x, startPos.y + newY, transform.position.z);
    }


    protected IEnumerator AnimateBug()
    {
        int currentSprite = 0;
        while(gameObject.activeSelf)
        {
            currentSprite++;
            if(currentSprite >= movingSprites.Length) currentSprite = 0;
            yield return new WaitForSeconds(animSpeed);
            r.sprite = movingSprites[currentSprite];
        }
    }

    void CheckOrientation()
    {
        Vector3 direction = player.position - transform.position;
        float dotProduct = Vector3.Dot(direction, transform.right);

        if (dotProduct > 0)
        {
            r.flipX = true;
            //Debug.Log("player is to the right");
        }
        else if (dotProduct < 0)
        {
            r.flipX = false;
            //Debug.Log("player is to the left");
        }
        else
        {
            //Debug.Log("InFront");
        }
    }

    public void Captured()
    {
        ParticlePoolManager.Instance.MoveAndPlayParticle(colliderObject.position, ParticlePoolManager.Instance.dirtParticle);
        Destroy(gameObject);
    }

    public void Struck() //By player shovel most likely
    {
        ParticlePoolManager.Instance.GrabBugSplatParticle().transform.position = colliderObject.position;
        Destroy(gameObject);
    }



}
