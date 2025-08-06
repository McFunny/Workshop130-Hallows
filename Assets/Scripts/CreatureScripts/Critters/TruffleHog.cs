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

    int burrowsToDig = 0; //How many burrows it plans to dig

    public GameObject burrowPrefab;
    public InventoryItemData truffleItem;
    

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
    public float GetCritterHealth(){ return health;}
    public float GetCritterHunger(){ return hunger;}
    public float GetCritterThirst(){ return thirst;}
    public string GetCritterName(){ return name;}
    public int GetCritterID(){ return creatureData.id;}
    public CritterData GetCritterData(){ return new CritterData(creatureData.id, friendshipLevel, friendPoints, health, hunger, thirst, name);} //For saving purposes

    public void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        print(GetCritterHunger());
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
        base.Update();

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
        base.OnHour();
        if(TimeManager.Instance.currentHour == 8)
        {
            burrowsToDig = Random.Range(3, 8);
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
        else if(burrowsToDig > 0)
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
        yield return new WaitForSeconds(r);
        currentState = CritterState.Decide;
        currentRoutine = null;
    }

    void Wander()
    {
        if (!isMoving && currentRoutine == null)
        {
            agent.speed = walkSpeed;
            if(BarnManager.Instance.WithinBarn(transform.position)) target = StructureManager.Instance.GetRandomTile(GridType.Barn);
            else target = StructureManager.Instance.GetRandomTile(GridType.Farm);
            target = GetRandomPointAround(transform.position, 10f);
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
            target = targetObject.position;
            currentRoutine = StartCoroutine(MoveToPoint(target, 10));
        }
        else if (Vector3.Distance(transform.position, target) < 1.5f)
        {
            interruptAction = true;
        }
    }

    protected override void FinishedMoving()
    {
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

        if(currentState == CritterState.Eat && targetObject) //Cat Reached the Bowl
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
        float truffleChance = (friendshipLevel + 1) * 4;
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
        }
    }
}
