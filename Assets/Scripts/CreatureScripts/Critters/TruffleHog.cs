using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TruffleHog : CritterBehaviorScript
{
    public Collider attackHitbox;
    public Transform chargePosition;
    public ParticleSystem chargeParticles, dashParticles;

    float walkSpeed = 4;
    float runSpeed = 9;

    [HideInInspector] public int burrowsToDig = 0; //How many burrows it plans to dig

    public GameObject burrowPrefab;
    public InventoryItemData truffleItem;

    //Sometimes a hog will call nearby hogs to chase him, forcing him to be their chase target
    //hogs will chase the target until the target's chase tokens are 0
    [HideInInspector] public TruffleHog chaseTarget;
    [HideInInspector] public int hogChaseTokens = 0;

    [HideInInspector] public bool usedForWagon = false;

    

    public enum CritterState
    {
        Decide,
        Idle, //Standing still
        Wander, //Moving to random spot
        WalkTowards, //Moving to a position, likely to and from the barn
        Dig, //Moving to a burrow spot and making the burrow
        Eat, //Eating at the trough
        Charge, //Attacking a creature, Unimplemented
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
    public CritterData GetCritterData(){ return new CritterData(creatureData.id, friendshipLevel, friendPoints, health, hunger, thirst, name, 0);} //For saving purposes

    public void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        if(!alreadyPet)
        {
            alreadyPet = true;
            FriendPointsChange(25, true);
            effectsHandler.PlaySound(effectsHandler.petSound);
            interactSuccessful = true;
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
                
            case CritterState.WalkTowards:
                WalkTowards();
                break;

            case CritterState.Dig:
                Dig();
                break;

            case CritterState.Eat:
                Eat();
                break;

            case CritterState.Charge:
                //Charge();
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

        //
        if(!isDead) CheckState(currentState);
    }

    protected override void OnHour()
    {
        if(!usedForWagon) base.OnHour();
        if(TimeManager.Instance.currentHour == 8 && !TutorialMiller.Instance)
        {
            burrowsToDig = Random.Range(2, 4);
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

        if(burrowsToDig == 0 && !BarnManager.Instance.WithinBarn(transform.position))
        {
            target = GetRandomPointAround(BarnManager.Instance.barnSource.position, 20);
            currentState = CritterState.WalkTowards;
            return;
        }
        else if(burrowsToDig > 0 && !TimeManager.Instance.stopTime)
        {
            if(BarnManager.Instance.WithinBarn(transform.position))
            {
                target = StructureManager.Instance.GetRandomTile();
                currentState = CritterState.WalkTowards;
                return;
            }

            r = Random.Range(0, 10);
            if(r > 6)
            {
                target = StructureManager.Instance.GetRandomClearTile();
                currentState = CritterState.Dig;
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
        float r = Random.Range(1f, 5f);
        if(chaseTarget || hogChaseTokens > 0) r = 0.01f;
        yield return new WaitForSeconds(r);
        if(hogChaseTokens > 0 || chaseTarget || playerFollowTokens > 0) currentState = CritterState.Wander;
        else currentState = CritterState.Decide;
        currentRoutine = null;

        if(playerFollowTokens <= 0 && Vector3.Distance(player.position, transform.position) < 30 && Random.Range(0, 100) < friendshipLevel * 7) //Follow the player
        {
            playerFollowTokens = Random.Range(2, 6);
        }
        else if(hogChaseTokens <= 0 && Random.Range(0, 100) > 95) //Have hogs chase this hog
        {
            StartHogChase();
        }
    }

    void Wander()
    {
        if (!isMoving && currentRoutine == null)
        {
            if(chaseTarget)
            {
                if(chaseTarget.hogChaseTokens <= 0) chaseTarget = null;
                else
                {
                    agent.speed = runSpeed;
                    target = GetRandomPointAround(chaseTarget.transform.position, 2f);
                    currentRoutine = StartCoroutine(MoveToPoint(target, 5));
                    //print("Chasing hog");
                    return;
                }
            }

            if(hogChaseTokens > 0) agent.speed = runSpeed;
            else agent.speed = walkSpeed;
            if(BarnManager.Instance.WithinBarn(transform.position))
            {
                target = StructureManager.Instance.GetRandomTile(GridType.Barn);
                //print("Wandering to tile in barn");
            }
            else if(playerFollowTokens > 0)
            {
                target = player.position;
                //print("Wandering to player");
            }
            else
            {
                target = StructureManager.Instance.GetRandomTile(GridType.Farm);
                //print("Wandering to tile in farm");
            }
            target = GetRandomPointAround(target, 7f);
            currentRoutine = StartCoroutine(MoveToPoint(target, 5));
        }
    }

    void WalkTowards()
    {
        if(target == Vector3.zero)
        {
            currentState = CritterState.Decide;
            return;
        }
        if (!isMoving && currentRoutine == null)
        {
            currentRoutine = StartCoroutine(MoveToPoint(target, 30));
            agent.speed = runSpeed;
        }

        if (Vector3.Distance(transform.position, target) < 2f)
        {
            interruptAction = true;
        }
    }

    void Dig()
    {
        if (!isMoving && currentRoutine == null)
        {
            agent.speed = runSpeed;
            target = StructureManager.Instance.GetRandomClearTile();
            currentRoutine = StartCoroutine(MoveToPoint(target, 10));
        }
        else if (Vector3.Distance(transform.position, target) < 1.5f)
        {
            interruptAction = true;
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
        else if (Vector3.Distance(transform.position, target) < 2f)
        {
            interruptAction = true;
        }
    }

    protected override void FinishedMoving()
    {
        if(hogChaseTokens > 0) hogChaseTokens--;

        if(currentState == CritterState.Wander)
        {
            currentState = CritterState.Idle;
        }

        if(currentState == CritterState.WalkTowards)
        {
            currentState = CritterState.Decide;
            target = Vector3.zero;
        }
        if(currentState == CritterState.Dig)
        {
            if (Vector3.Distance(transform.position, target) < 1.5f)
            {
                anim.Play("Dig");
                ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);
                //agent.Stop();
                //agent.velocity = Vector3.zero;
                currentRoutine = StartCoroutine(DiggingRoutine());
                isMoving = false;
                return;
            }
        }

        if(currentState == CritterState.Eat && targetObject) //Critter Reached the Bowl
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
                anim.Play("Chew");
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

    IEnumerator DiggingRoutine()
    {
        yield return new WaitForSeconds(2);
        burrowsToDig--;
        ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);
        Burrow burrow = Instantiate(burrowPrefab, target, Quaternion.identity).GetComponent<Burrow>();
            //Code to add the item
        float truffleChance = (friendshipLevel + 1) * 8;
        if(Random.Range(0,100) < truffleChance) burrow.InsertItem(truffleItem);

        FriendPointsChange(2, true);

        yield return new WaitForSeconds(2);

        currentState = CritterState.Wander;
        target = Vector3.zero;
        currentRoutine = null;
        isMoving = false;
    }

    IEnumerator EatRoutine()
    {
        yield return new WaitForSeconds(5);
        currentState = CritterState.Wander;
        currentRoutine = null;
    }

    void StartHogChase()
    {
        Collider[] nearbyHogs = Physics.OverlapSphere(transform.position, 30f, 1 << 9);
        bool foundHog = false;
        hogChaseTokens = Random.Range(2, 7);
        foreach(Collider collider in nearbyHogs)
        {
            TruffleHog hog = collider.GetComponentInParent<TruffleHog>();
            if(hog && hog != this && hog.hogChaseTokens <= 0 && Random.Range(0,10) > 3)
            {
                foundHog = true;
                hog.chaseTarget = this;
            }
        }

        if(!foundHog) hogChaseTokens = 0;
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
            //dashParticles.Stop();
            //chargeParticles.Stop();
            StopAllCoroutines();
            canCorpseBreak = true;

            PopupHandler.Instance.names.Enqueue(name);
            PopupHandler.Instance.AddToQueue(PopupHandler.Instance.critterDiedPopup);
        }
    }
}
