using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;

public class PetHen : CritterBehaviorScript
{
    float walkSpeed = 4;
    float runSpeed = 11;

    private bool hasTarget = false; //For fleeing

    public float eggProgress = 0; //max is 2

    HenNest targetNest;

    public LayerMask obstacleMask;

    public enum CritterState
    {
        Decide,
        Idle, //Standing still
        Wander, //Moving to random spot
        Eat, //Eating at the trough
        Flee,
        Pet,
        Dead
    }

    public CritterState currentState;

    void Start()
    {
        base.Start();
        //CritterStart();
        StartCoroutine(IdleSoundTimer());
    }

    void OnDestroy()
    {
        base.OnDestroy();
        //OnCritterDestroy();
    }
    //////////////ICritter Stuff\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\
    public CritterData GetCritterData(){ return new CritterData(creatureData.id, friendshipLevel, friendPoints, health, hunger, thirst, name, eggProgress);} //For saving purposes

    public void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        if(!alreadyPet)
        {
            alreadyPet = true;
            FriendPointsChange(25, true);
            effectsHandler.PlaySound(effectsHandler.petSound);
            interactSuccessful = true;
            if(currentState == CritterState.Idle || currentState == CritterState.Wander) 
            {
                anim.SetBool("IsSitting", false);
                currentState = CritterState.Pet;
                if(currentRoutine != null) StopCoroutine(currentRoutine);
                currentRoutine = null;
                isMoving = false;
            }
            return;
        }
        interactSuccessful = false;
    }

    public void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        if(hunger < 100 && (foodDiet.Contains(item)))
        {
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            PlayerInventoryHolder.Instance.UpdateInventory();
            EatFood(item);
            interactSuccessful = true;
            return;
        }
        interactSuccessful = true;
    }

    public override void LoadData(CritterData c)
    {
        base.LoadData(c);
        eggProgress = c.extraVar;
    }

    ////////Critter Specific Stuff///////////
    /// 
    /// 
    
    public void CheckState(CritterState currentState)
    {
        if(behaviorDelay) return;
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

            case CritterState.Eat:
                Eat();
                break;
            
            case CritterState.Flee:
                Flee();
                break;
            
            case CritterState.Pet:
                Pet();
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

    protected override void OnHour()
    {
        base.OnHour();
        if(TimeManager.Instance.currentHour == 8)
        {
            eggProgress++;
            if(Random.Range(0,6) <= friendshipLevel) eggProgress++;
            //if(eggProgress >= 2) StartCoroutine(MakeEggs());
        }
    }

    void Decide()
    {
        int r = 0;

        if((hunger <= 25 && EatCheck(false)) || (thirst <= 25 && EatCheck(true)))
        {
            currentState = CritterState.Eat;
            return;
        }

        if(playerInSightRange)
        {
            currentState = CritterState.Flee;
            return;
        }

        if(Random.Range(0, 10f) > 9.4f || eggProgress >= 2) //sit in nest
        {
            FindNearbyNest();
            if(targetObject)
            {
                currentState = CritterState.Wander;
                return;
            }
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
        if(targetNest && Vector3.Distance(transform.position, targetObject.position) < 3)
        {
            targetObject = null;
            time = 30;
            FriendPointsChange(10, true);
        }
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

        if(targetNest && eggProgress >= 2)
        {
            targetNest.EggChange(true);
            targetNest.containsHen = false;
            eggProgress = 0;
            targetNest = null;
        }
        
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
            if(targetObject) target = targetObject.position;
            else
            {
                target = StructureManager.Instance.GetRandomTile(GridType.Barn);
                target = GetRandomPointAround(target, 7f);
            }

            if(/*Random.Range(0, 10) == 9 && */CanFlyToPoint(target)) currentRoutine = StartCoroutine(FlyToPoint(target));
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


    void Eat()
    {
        if (!isMoving && currentRoutine == null)
        {
            agent.speed = runSpeed;
            if(!targetObject)
            {
                currentState = CritterState.Idle;
                return;
            }
            target = targetObject.position;
            currentRoutine = StartCoroutine(MoveToPoint(target, 10));
        }
        else if (Vector3.Distance(transform.position, target) < 1.7f)
        {
            interruptAction = true;
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

    void Pet()
    {
        if(!isMoving && currentRoutine == null)
        {
            currentRoutine = StartCoroutine(PetRoutine());
            anim.Play("HenPet");
        }
    }

    protected override void FinishedMoving()
    {
        //
        if(currentState == CritterState.Wander)
        {
            currentState = CritterState.Idle;
        }

        if(currentState == CritterState.Eat && targetObject) 
        {
            Trough trough = targetObject.GetComponent<Trough>();
            if(!trough) //Trough is gone
            {
                targetObject = null;
                isMoving = false;
                currentRoutine = null;
                return;
            }
            bool isEating = false, isDrinking = false;
            if(hunger <= 25 && trough.HasEdibleItem(foodDiet)) isEating = true;
            if(thirst <= 25 && trough.waterLevel > 0) isDrinking = true;

            if(Vector3.Distance(targetObject.transform.position, transform.position) < 1.5f && (isEating || isDrinking))
            {
                agent.velocity = Vector3.zero;
                agent.ResetPath();
                anim.Play("HenEat");
                if(isEating)
                {
                    trough.EatItem(foodDiet, out InventoryItemData itemEaten);
                    EatFood(itemEaten);
                }
                else
                {
                    trough.WaterLevelChange(-1);
                    thirst = maxThirst;
                    FriendPointsChange(5, true);
                }
                currentRoutine = StartCoroutine(EatRoutine());
                isMoving = false;
                targetObject = null;
                target = Vector3.zero;
                return;
            }
            else if(!isEating && !isDrinking) targetObject = null;
        }
        
        isMoving = false;
        currentRoutine = null;
    }

    void FindNearbyNest()
    {
        Collider[] hitStructures = Physics.OverlapSphere(transform.position, 80f, 1 << 6);
        foreach(Collider collider in hitStructures)
        {
            HenNest nest = collider.GetComponent<HenNest>();
            targetNest = nest;
            if(nest && !nest.containsEgg && !nest.containsHen && Random.Range(0,10) != 1)
            {
                nest.containsHen = true;
                targetObject = nest.transform;
                return;
            }
        }
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

    IEnumerator EatRoutine()
    {
        yield return new WaitForSeconds(5);
        currentState = CritterState.Wander;
        currentRoutine = null;
    }

    IEnumerator PetRoutine()
    {
        yield return new WaitForSeconds(3);
        currentState = CritterState.Wander;
        currentRoutine = null;
    }


    public override void OnDeath()
    {
        if (!isDead)
        {
            isDead = true;
            anim.Play("Death");
            base.OnDeath();
            agent.speed = 0;
            rb.isKinematic = true;
            rb.freezeRotation = true;
            StopAllCoroutines();
            canCorpseBreak = true;

            if(targetNest) targetNest.containsHen = false;

            PopupHandler.Instance.names.Enqueue(name);
            PopupHandler.Instance.AddToQueue(PopupHandler.Instance.critterDiedPopup);
        }
    }
}
