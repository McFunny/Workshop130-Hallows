using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;

public class TownCat : MonoBehaviour, IInteractable
{
    public Transform headPivot, focalPoint;
    Vector3 starePoint;
    Quaternion defaultHeadRotation;

    public LayerMask PlayerStructureMask;

    public Animator anim;
    public NavMeshAgent agent;

    Vector3 origin;

    Coroutine currentRoutine;
    bool isMoving, interruptAction;

    public CreatureEffectsHandler effectsHandler;

    bool alreadyPet;


    void Awake()
    {
        defaultHeadRotation = headPivot.rotation;
        origin = new Vector3(transform.position.x, transform.position.y, transform.position.z);
    }

    void Start()
    {
        StartCoroutine(CheckSurroundings());
        StartCoroutine(IdleSoundTimer());
    }

    void Update()
    {

        Idle();
        LookAtObject();

        if(agent.velocity.magnitude < 0.2f)
        {
            anim.SetBool("IsWalking", false);
        }
        else
        {
            anim.SetBool("IsWalking", true);
        }
    }

    void Idle()
    {
        if(currentRoutine == null)
        {
            Vector3 target = StructureManager.Instance.GetRandomTile();
            target = GetRandomPointAround(origin, 30);
            currentRoutine = StartCoroutine(MoveToPoint(target, 7));
        }
    }

    void FinishedMoving()
    {
        currentRoutine = StartCoroutine(IdleRoutine());
        isMoving = false;
        interruptAction = false;
    }

    IEnumerator IdleRoutine()
    {
        agent.ResetPath();
        float time = Random.Range(2f, 15);
        if(time > 10)
        {
            anim.SetBool("IsSitting", true);
            if(Random.Range(0, 10) > 2) anim.Play("CatSit");
            else anim.Play("CatClean");
            time += 10;
        }
        yield return new WaitForSeconds(time);
        
        if(time > 10)
        {
            anim.SetBool("IsSitting", false);
            yield return new WaitForSeconds(1);

        }
        currentRoutine = null;
    }

    IEnumerator PetRoutine()
    {
        yield return new WaitForSeconds(4);
        currentRoutine = null;
    }

    IEnumerator CheckSurroundings()
    {
        Collider[] hitTargets = new Collider[10];
        int numColliders;
        while(true)
        {
            yield return new WaitForSeconds(1f);

            Vector3 closestTarget = Vector3.zero;
            float dist = 0;
            float minDist = 100;
            numColliders = Physics.OverlapSphereNonAlloc(transform.position, 8, hitTargets, PlayerStructureMask);
            for (int i = 0; i < numColliders; i++)
            {
                NPC npc = hitTargets[i].gameObject.GetComponentInParent<NPC>();
                if(npc || hitTargets[i].gameObject.layer == 10) 
                {
                    dist = Vector3.Distance(transform.position, hitTargets[i].gameObject.transform.position);
                    if(dist < minDist)
                    {
                        minDist = dist;
                        closestTarget = hitTargets[i].gameObject.transform.position;
                    }
                }
            }

            if(closestTarget != Vector3.zero)
            {
                starePoint = closestTarget;
            }
        }
    }

    void LookAtObject()
    {
        //Check the Dot
        if(starePoint == Vector3.zero) return;

        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 toTarget = Vector3.Normalize(starePoint - transform.position);

        if (Vector3.Dot(forward, toTarget) > .1f)
        {
            Vector3 direction = starePoint - headPivot.position;
            //direction.y = 0;
            Quaternion toRotation = Quaternion.LookRotation(direction);
            toRotation *= defaultHeadRotation;

            headPivot.rotation = Quaternion.Slerp(headPivot.rotation, toRotation, 2 * Time.deltaTime);
        }
        else
        {
            headPivot.rotation = Quaternion.Slerp(headPivot.rotation, transform.rotation, 2 * Time.deltaTime);
        }
    }

    protected IEnumerator MoveToPoint(Vector3 destination, float maxTime)
    {
        isMoving = true;
        interruptAction = false;

        agent.destination = destination;

        float timeSpent = 0; //to make sure it doesnt get stuck

        while ((agent.pathPending || agent.remainingDistance > agent.stoppingDistance) && timeSpent < maxTime)
        {
            if(interruptAction) timeSpent += maxTime;

            timeSpent += Time.deltaTime;
            yield return null;
        }

        FinishedMoving();
    }

    protected Vector3 GetRandomPointAround(Vector3 origin, float radius)
    {
        int x = 0;
        Vector3 randomPoint = origin;
        while(x < 20)
        {
            Vector2 randomDirection = Random.insideUnitCircle * radius;
            randomPoint = new Vector3(randomDirection.x, origin.y, randomDirection.y) + origin;
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
            {
                x += 20;
                randomPoint = hit.position;
            }

            x++;
        }

        return randomPoint;
    }

    IEnumerator IdleSoundTimer()
    {
        while(true)
        {
            yield return new WaitForSeconds(Random.Range(9, 16));
            effectsHandler.RandomIdle();
            alreadyPet = false;
        }

    }


    /////IInteractable nonsense/////
    /// 

    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        if(!alreadyPet)
        {
            alreadyPet = true;
            effectsHandler.PlaySound(effectsHandler.petSound);
            anim.Play("CatPet");
            anim.SetBool("IsSitting", false);
            ParticlePoolManager.Instance.GrabHeartParticle().transform.position = focalPoint.position;
            if(!isMoving) 
            {
                currentRoutine = StartCoroutine(PetRoutine());
                StopCoroutine(IdleRoutine());
            }
            else interruptAction = true;
        }
        interactSuccessful = true;
    }

    public void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        
        interactSuccessful = true;
    }
    
    public void EndInteraction(){}

    public void ToggleHighlight(bool enabled)
    {
        //showStats = enabled;
    }

    public void ReturnFocalPoint(out Transform point)
    {
        if(focalPoint) point = focalPoint;
        else point = transform;
    }
    ///////////////////////////////
}
