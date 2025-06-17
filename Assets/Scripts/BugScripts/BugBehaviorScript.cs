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

    public Sprite[] movingSprites;

    public ParticleSystem burrowingParticles;

    NavMeshAgent agent;

    public float walkSpeed, fleeSpeed;
    public float sightRange = 0; //0 means it ignores the player
    public float reactionTimeMin, reactionTimeMax; //How fast it does its action when approached by the player, IE time before fleeing after approached by player

    protected bool isMoving = false;
    protected bool coroutineRunning = false;
    protected bool isLeaving;

    private Coroutine approachedRoutine, walkRoutine; 
    protected Transform target;
    protected Transform player;
    protected Collider hitBox;

    public BugObject bugData;
    public DespawnMethod despawnMethod;
    protected int hoursAlive = 0; //Leave at 10

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
        Flee,
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

            case BugState.Flee:
                Flee();
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

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        hitBox = GetComponent<Collider>();
        target = null;
    }

    void Start()
    {
        player = PlayerInteraction.Instance.transform;
        StartCoroutine(AnimateBug());

        TimeManager.OnHourlyUpdate -= HourlyUpdate;
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
        if(hoursAlive >= 10 || !bugData.activeHours.Contains(TimeManager.Instance.timeOfDay)) currentState = BugState.Leave;
    }

    protected virtual void Update()
    {
        if(currentState == BugState.Flee) agent.speed = fleeSpeed;
        else agent.speed = walkSpeed;

        float distance = Vector3.Distance(player.position, transform.position);
        bool playerInSightRange = distance <= sightRange;

        if(playerInSightRange && approachedRoutine == null) approachedRoutine = StartCoroutine(ApproachedByPlayer());

        CheckState(currentState);

        CheckOrientation();
        
    }

    protected void Wander()
    {
        if (!isMoving && currentState == BugState.Wander)
        {
            Vector3 randomPoint;
            randomPoint = GetRandomPointAround(transform.position, 5f);
            walkRoutine = StartCoroutine(MoveToPoint(randomPoint));
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
        coroutineRunning = true;

        agent.destination = destination;

        float timeSpent = 0; //to make sure it doesnt get stuck
        float maxTime = Random.Range(1.5f, 5f);

        while (timeSpent < maxTime)
        {
            timeSpent += Time.deltaTime;
            if((target != null && currentState == BugState.Wander) || currentState == BugState.Leave)
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

    private void Flee() //currently no timer is implemented
    {
        if(!target) target = player;
        Vector3 runTo = transform.position + ((transform.position - target.position + new Vector3(Random.Range(-3, 3), 0, Random.Range(-3, 3)) * 1));
        agent.destination = runTo;
    }

    protected IEnumerator Leaving()
    {
        hitBox.enabled = false;
        agent.enabled = false;
        switch(despawnMethod)
        {
            case DespawnMethod.Poof:
            ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);
            Destroy(gameObject);
            break;
            case DespawnMethod.Burrow:
            transform.DOShakePosition(3f, 0.9f, 0, 0.2f, false);
            transform.DOMoveY(transform.position.y - .5f, 4);
            burrowingParticles.transform.DOMoveY(burrowingParticles.transform.position.y + .5f, 4); //to offset the dig
            burrowingParticles.Play();
            for(int i = 0; i < 5; i++)
            {
                yield return new WaitForSeconds(0.5f);
                ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);
            }
            ParticlePoolManager.Instance.GrabDirtPixelParticle().transform.position = transform.position;
            Destroy(gameObject);
            break;
            case DespawnMethod.Fly:
            for(int i = 0; i < 100; i++)
            {
                yield return new WaitForSeconds(0.1f);
                transform.Translate(Vector3.up * Time.deltaTime, Space.World);
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
        yield return new WaitForSeconds(Random.Range(reactionTimeMin, reactionTimeMax));
        //if no specific bug behavior
        currentState = BugState.Leave;
    }


    protected IEnumerator AnimateBug()
    {
        int currentSprite = 0;
        while(gameObject.activeSelf)
        {
            currentSprite++;
            if(currentSprite >= movingSprites.Length) currentSprite = 0;
            yield return new WaitForSeconds(0.3f);
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
        ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);
        Destroy(gameObject);
    }

    public void Struck() //By player shovel most likely
    {
        ParticlePoolManager.Instance.GrabBugSplatParticle().transform.position = transform.position;
        Destroy(gameObject);
    }



}
