using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class PyreGrub : PetBehaviorScript, IInteractable
{
    public Material ignitedMat, extinguishedMat;
    public MeshRenderer ballMeshRenderer;
    public List<SkinnedMeshRenderer> skinnedMeshRenderers;
    float textureOffset = 0;
    public float offsetRate = .005f;
    public GameObject pyreFire;

    bool inBall = false;

    public PetState currentState;

    [Header("Debug tool to test out states")]
    public PetState forceState;

    public enum PetState
    {
        Decide, //Make a choice on the next action
        AwaitPlayer, //When the player is gone in the crypt/wilderness
        Idle,
        Follow, //Follow the player
        Ball, //Curl into a ball that can be kicked by the player in OnTriggerEnter, popped up and uses player rb velocity. If fast enough, burns/damages target
        Flee,
        Pet,
        Eat //Pet goes to bowl to eat
    }

    public void CheckState(PetState currentState)
    {
        switch (currentState)
        {
            case PetState.Decide:
                Decide();
                break;

            case PetState.AwaitPlayer:
                AwaitPlayer();
                break;

            case PetState.Idle:
                Idle();
                break;

            case PetState.Follow:
                Follow();
                break;

            case PetState.Ball:
                Ball();
                break;

            case PetState.Flee:
                Flee();
                break;

            case PetState.Pet:
                Pet();
                break;

            case PetState.Eat:
                Eat();
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
        }
        else
        {
            anim.SetBool("IsWalking", true);
        }

        CheckState(currentState);
    }

    void StateSwitch(PetState newState)
    {
        if(currentState == newState) return;

        //Leaving Old State Effects
        if(currentState == PetState.Idle)
        {
            anim.Play("CatIdle"); //Reset the anim
            anim.SetBool("IsSitting", false);
            if(currentRoutine != null) StopCoroutine(currentRoutine);
            StopCoroutine(IdleRoutine());
            currentRoutine = null;
            isMoving = false;
        }

        if(currentState == PetState.Follow)
        {
            if(currentRoutine != null) StopCoroutine(currentRoutine);
            StopCoroutine(FollowRoutine());
            currentRoutine = null;
            isMoving = false;

            if(inBall) currentRoutine = StartCoroutine(LeaveBall());
        }

        //Change the State
        agent.velocity = Vector3.zero;
        targetStructure = null;
        currentState = newState;

        //Entering New State Effects
    }

    void Decide()
    {
        if(isMoving || currentRoutine != null || TimeManager.Instance.stopTime) return; //Wait until all coroutines are done to avoid overlap

        if(forceState != PetState.Decide) //For debugging and testing states
        {
            StateSwitch(forceState);
            return;
        }

        if(TownGate.Instance.location != PlayerLocation.InFarm) //Make sure pet is following when not in farm
        {
            if(TownGate.Instance.location != PlayerLocation.InTown || friendshipLevel < 2) //Player is not within reach, so stay still
            {
                //currentState = PetState.AwaitPlayer;
                StateSwitch(PetState.AwaitPlayer);
                return;
            }
            //currentState = PetState.Follow;
            StateSwitch(PetState.Follow);
            forceFollows = 5;
            return;
        }

        if((hunger <= 25 && EatCheck(false)) || (thirst <= 25 && EatCheck(true)))
        {
            StateSwitch(PetState.Eat);
            return;
        }

        StateSwitch(PetState.Idle);
    }

    void AwaitPlayer()
    {
        if(TownGate.Instance.location == PlayerLocation.InFarm || TownGate.Instance.location == PlayerLocation.InTown)
        {
            if(friendshipLevel >= 2) StateSwitch(PetState.Follow);
            else StateSwitch(PetState.Idle);
        } 
    }

    void Idle()
    {
        if(!isMoving)
        {
            float distance = Vector3.Distance(player.position, spawnOrigin);
            if(distance > followDistance && friendshipLevel >= 2)
            {
                StateSwitch(PetState.Follow);
                currentRoutine = null;
                forceFollows = 5;
                return;
            }
        }

        if(currentRoutine == null)
        {
            target = StructureManager.Instance.GetRandomTile();
            target = GetRandomPointAround(target, 3);
            currentRoutine = StartCoroutine(MoveToPoint(target, 5));
        }
    }

    void Follow()
    {
        if(!isMoving && currentRoutine == null)
        {
            float distance = Vector3.Distance(player.position, spawnOrigin);
            if(distance < followDistance && TownGate.Instance.location == PlayerLocation.InFarm && forceFollows <= 0)
            {
                int r = Random.Range(0,100);
                if(r > 80)
                {
                    StateSwitch(PetState.Decide);
                    return;
                }
            }
            float playerDistance = Vector3.Distance(player.position, transform.position);
            float pointRange = 5;
            if(playerDistance > 15) //Pop out of ball if not already
            {
                pointRange = 1.5f;
                agent.speed = runSpeed;
            } 
            else agent.speed = walkSpeed; //pop into ball if not already

            target = GetRandomPointAround(player.position, pointRange);
            currentRoutine = StartCoroutine(MoveToPoint(target, 3));
            forceFollows--;
        }
    }

    void Ball()
    {
        //
    }

    void Flee()
    {
        if(!isMoving && currentRoutine == null)
        {
            //anim.Play("CatAttack1");

            // 1. Calculate direction to target
            Vector3 directionToTarget = transform.position - target;

            // 2. Normalize the direction
            Vector3 normalizedDirection = directionToTarget.normalized;

            // 3. Calculate the flee position
            Vector3 fleePosition = transform.position + normalizedDirection * 40;

            // 4. Set the agent's destination
            currentRoutine = StartCoroutine(MoveToPoint(fleePosition, 8));
        }
    }

    void Pet()
    {
        if(!isMoving && currentRoutine == null)
        {
            alreadyPet = true;
            FriendPointsChange(15, true);
            effectsHandler.PlaySound(effectsHandler.petSound);
            agent.ResetPath();
            currentRoutine = StartCoroutine(TimerRoutine(4));
            //anim.Play("CatPet");
        }
    }

    void Eat()
    {
        if(!targetStructure && currentRoutine == null) //If there is no bowl, then they should not be in this state
        {
            StateSwitch(PetState.Decide);
            return;
        }
        if(!interruptAction && targetStructure && Vector3.Distance(targetStructure.transform.position, transform.position) < 1.5f) //Are they close enough? If so, begin eating
        {
            if(currentRoutine == null) FinishedMoving();
            else interruptAction = true;
            return;
        }
        if(!isMoving && currentRoutine == null) //Move to the dish
        {
            currentRoutine = StartCoroutine(MoveToPoint(targetStructure.transform.position, 8));
        }
    }

    protected override void FinishedMoving()
    {
        if(currentState == PetState.Idle)
        {
            currentRoutine = StartCoroutine(IdleRoutine());
            isMoving = false;
            return;
        }

        if(currentState == PetState.Follow)
        {
            if(Vector3.Distance(player.position, transform.position) < 10) currentRoutine = StartCoroutine(FollowRoutine());
            else currentRoutine = null;
            isMoving = false;
            return;
        }

        if(currentState == PetState.Flee)
        {
            StateSwitch(PetState.Decide);
        }

        if(currentState == PetState.Eat && targetStructure) //Cat Reached the Bowl
        {
            PetBowl bowl = targetStructure as PetBowl;
            if(!bowl) //Bowl is gone
            {
                targetStructure = null;
                isMoving = false;
                currentRoutine = null;
                return;
            }
            bool isEating = false, isDrinking = false;
            if(hunger <= 25 && bowl.ContainsEdibleItem(foodDiet)) isEating = true;
            if(thirst <= 25 && bowl.containsWater) isDrinking = true;

            if(Vector3.Distance(targetStructure.transform.position, transform.position) < 1.5f && (isEating || isDrinking))
            {
                agent.velocity = Vector3.zero;
                agent.ResetPath();
                anim.Play("CatEat");
                if(isEating)
                {
                    bowl.RemoveItem(out InventoryItemData itemEaten);
                    EatFood(itemEaten);
                }
                else
                {
                    bowl.WaterChange(false);
                    thirst = maxThirst;
                    FriendPointsChange(5, true);
                }
                currentRoutine = StartCoroutine(TimerRoutine(3));
                isMoving = false;
                targetStructure = null;
                return;
            }
            else if(!isEating && !isDrinking) targetStructure = null;
        }
        
        isMoving = false;
        currentRoutine = null;
    }

    protected void FinishedCoroutine()
    {
        if(currentState == PetState.Pet || currentState == PetState.Eat)
        {
            StateSwitch(PetState.Decide);
        }
        currentRoutine = null;
    }

    IEnumerator EnterBall()
    {
        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator LeaveBall()
    {
        yield return new WaitForSeconds(0.5f);
    }
    

    IEnumerator IdleRoutine()
    {
        agent.ResetPath();
        float t = 0;
        float time = Random.Range(5f, 15f);
        while(t < time)
        {
            yield return new WaitForSeconds(1);
            t++;
        }
        StateSwitch(PetState.Decide);
        currentRoutine = null;
    }

    IEnumerator FollowRoutine()
    {
        yield return new WaitForSeconds(Random.Range(1f, 4f));
        currentRoutine = null;
    }

    IEnumerator TimerRoutine(float time)
    {
        yield return new WaitForSeconds(time);
        FinishedCoroutine();
    }

    /////IInteractable nonsense/////

    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        if(!alreadyPet)
        {
            //StateSwitch(PetState.Pet);
        }
        interactSuccessful = true;
    }

    public void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        if(item.ID == 2 && PlayerInteraction.Instance.waterHeld > 0 && (currentState == PetState.Idle || currentState == PetState.Follow))
        {
            PlayerInteraction.Instance.waterHeld--;
            interactSuccessful = true;
            target = player.position;
            //StateSwitch(PetState.Flee);
            effectsHandler.MiscSound();
            StopCoroutine(DripEffects());
            StartCoroutine(DripEffects());
            return;
        }
        if(hunger < 100 && (foodDiet.Contains(item)))
        {
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            PlayerInventoryHolder.Instance.UpdateInventory();
            EatFood(item);
            interactSuccessful = true;
            return;
        }
        interactSuccessful = false;
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
