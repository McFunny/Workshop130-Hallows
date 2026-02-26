using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class PyreGrub : PetBehaviorScript, IInteractable
{
    public Rigidbody rb;

    public Material ignitedMat, extinguishedMat;
    public GameObject bugObject, ballObject;
    public Transform ballPivot;
    public MeshRenderer ballMeshRenderer;
    public List<SkinnedMeshRenderer> skinnedMeshRenderers;
    float textureOffset = 0;
    public float offsetRate = .005f;
    public GameObject pyreFire;

    bool ignited = false;

    bool inBall = false;
    bool ballTransitioning; //Dont allow state switches like petting when transitioning ball
    public ParticleSystem enterBallParticles;

    //Homing Stats

    float detectionRadius = 10f;   // how far the ball looks for targets
    float homingStrength = 5f;     // how strongly it curves toward the target
    public float minSpeedForHoming = 10f;

    Vector3 origin;

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

    void Awake()
    {
        origin = new Vector3(transform.position.x, transform.position.y, transform.position.z);

        agent.speed = walkSpeed;
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

        AnimateTexture();

        CheckState(currentState);

        //print(rb.velocity.magnitude);
    }

    void LateUpdate()
    {
        RollBall();
    }

    void FixedUpdate()
    {
        HomingBall();
    }

    void StateSwitch(PetState newState)
    {
        if(currentState == newState) return;

        //Leaving Old State Effects
        if(currentState == PetState.Idle)
        {
            anim.SetBool("IsSitting", false);
            anim.Play("Idle"); //Reset the anim
            if(currentRoutine != null && !ballTransitioning) StopCoroutine(currentRoutine);
            StopCoroutine(IdleRoutine());
            currentRoutine = null;
            isMoving = false;
        }

        if(currentState == PetState.Follow)
        {
            if(currentRoutine != null && !ballTransitioning) StopCoroutine(currentRoutine);
            StopCoroutine(FollowRoutine());
            currentRoutine = null;
            isMoving = false;

            if(inBall) currentRoutine = StartCoroutine(ExitBall());
        }

        if(currentState == PetState.Ball)
        {
            rb.isKinematic = true;
            agent.enabled = true;
            if(inBall) currentRoutine = StartCoroutine(ExitBall());
        }

        //Change the State
        agent.velocity = Vector3.zero;
        if(newState != PetState.Eat) targetStructure = null;
        currentState = newState;

        //Entering New State Effects
        if(currentState == PetState.Ball)
        {
            rb.isKinematic = false;
            agent.ResetPath();
            agent.enabled = false;
            if(!inBall) currentRoutine = StartCoroutine(EnterBall());
            StartCoroutine(BallTimer());
        }

        if(currentState == PetState.Eat) //To force them to move there
        {
            currentRoutine = StartCoroutine(MoveToPoint(targetStructure.transform.position, 8));
        }
    }

    void Decide()
    {
        if(isMoving || currentRoutine != null || TimeManager.Instance.stopTime) return; //Wait until all coroutines are done to avoid overlap

        if(forceState != PetState.Decide) //For debugging and testing states
        {
            StateSwitch(forceState);
            return;
        }

        if((hunger <= 25 && EatCheck(false)) || (thirst <= 25 && EatCheck(true)))
        {
            StateSwitch(PetState.Eat);
            return;
        }

        if(TownGate.Instance.location != PlayerLocation.InFarm) //Make sure pet is following when not in farm
        {
            if(TownGate.Instance.location != PlayerLocation.InTown || friendshipLevel < 1) //Player is not within reach, so stay still
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

        StateSwitch(PetState.Idle);
    }

    void AwaitPlayer()
    {
        if(TownGate.Instance.location == PlayerLocation.InFarm || TownGate.Instance.location == PlayerLocation.InTown)
        {
            if(friendshipLevel >= 1) StateSwitch(PetState.Follow);
            else StateSwitch(PetState.Idle);
        } 
    }

    void Idle()
    {
        if(!isMoving)
        {
            float distance = Vector3.Distance(player.position, spawnOrigin);
            if(distance > followDistance && friendshipLevel >= 1 && TimeManager.Instance.isDay)
            {
                StateSwitch(PetState.Follow);
                currentRoutine = null;
                forceFollows = 5;
                return;
            }
        }

        if(currentRoutine == null)
        {
            if(hunger == 0)
            {
                target = FindPetBowl();
                if(target == Vector3.zero) target = StructureManager.Instance.GetRandomTile();
            }
            else
            {
                target = StructureManager.Instance.GetRandomTile();
                target = GetRandomPointAround(target, 3);
            }
            currentRoutine = StartCoroutine(MoveToPoint(target, 5));

            agent.speed = walkSpeed;
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
            if(playerDistance > 15) //Pop into of ball if not already
            {
                if(playerDistance > 200) transform.position = origin; //To make sure it doesnt get lost
                pointRange = 1.5f;
                agent.speed = runSpeed;
                if(!inBall)
                {
                    currentRoutine = StartCoroutine(EnterBall());
                    return;
                }
            } 
            else 
            {
                agent.speed = walkSpeed; //pop out of ball if not already
                if(inBall)
                {
                    currentRoutine = StartCoroutine(ExitBall());
                    return;
                }
            }

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
            anim.Play("Pet");
        }
    }

    void Eat()
    {
        if(!targetStructure && currentRoutine == null) //If there is no bowl, then they should not be in this state
        {
            print("No Bowl");
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
            if(hunger <= 25 && bowl.ContainsEdibleItem(petType)) isEating = true;
            if(thirst <= 25 && bowl.containsWater) isDrinking = true;

            if(Vector3.Distance(player.position, transform.position) > 70f) transform.position = targetStructure.transform.position; // To get pet unstuck if they get stuck

            if(Vector3.Distance(targetStructure.transform.position, transform.position) < 1.5f && (isEating || isDrinking))
            {
                agent.velocity = Vector3.zero;
                agent.ResetPath();
                anim.Play("Eat");
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
        ballTransitioning = true;
        inBall = true;
        anim.Play("Jump");
        yield return new WaitForSeconds(0.7f);
        effectsHandler.PlaySound(effectsHandler.miscSound2);
        enterBallParticles.Play();
        ballObject.SetActive(true);
        bugObject.SetActive(false);
        //if(currentState == PetState.Ball) ballObject.transform.position = new Vector3(ballObject.transform.position.x, ballObject.transform.position.y + 0.5f, ballObject.transform.position.z);
        rb.useGravity = true;
        currentRoutine = null;
        ballTransitioning = false;
    }

    IEnumerator ExitBall()
    {
        rb.useGravity = false;
        ballTransitioning = true;
        inBall = false;
        yield return new WaitForSeconds(0.1f);
        enterBallParticles.Play();
        ballObject.SetActive(false);
        bugObject.SetActive(true);
        anim.Play("JumpReverse");
        effectsHandler.loopingSource.volume = 0;
        effectsHandler.PlaySound(effectsHandler.miscSound3);
        yield return new WaitForSeconds(1);
        currentRoutine = null;
        ballTransitioning = false;

        effectsHandler.loopingSource.volume = 0;
    }

    IEnumerator BallTimer()
    {
        int checksPassed = 0;
        yield return new WaitForSeconds(3);
        while(currentState == PetState.Ball)
        {
            yield return new WaitForSeconds(3);
            if(rb.velocity.magnitude < 0.1f && currentState == PetState.Ball)
            {
                if(checksPassed < 4)
                {
                    checksPassed++;
                    continue;
                }
                rb.isKinematic = true;
                StateSwitch(PetState.Idle);
            }
            else checksPassed = 0;
        }
    }
    

    IEnumerator IdleRoutine()
    {
        agent.ResetPath();
        float t = 0;
        float time = Random.Range(2f, 10f);
        if(time > 8)
        {
            anim.SetBool("IsSitting", true);
            time += 10;
        }
        while(t < time)
        {
            yield return new WaitForSeconds(1);
            t++;
        }
        
        if(time > 10)
        {
            anim.SetBool("IsSitting", false);
            yield return new WaitForSeconds(1);

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

    void OnTriggerEnter(Collider other)
    {
        if(currentState == PetState.Ball && !ballTransitioning)
        {
            if(other.gameObject.layer == 10)
            {
                Vector3 dir = Vector3.Normalize(other.gameObject.transform.position - transform.position);
                rb.AddForce(110 * -dir, ForceMode.Impulse);

                effectsHandler.PlaySound(effectsHandler.hitSounds[0]);
                return;
            }

            if(other.gameObject.layer == 6 && rb.velocity.magnitude > 10f)
            {
                StructureBehaviorScript structure = other.GetComponentInParent<StructureBehaviorScript>();
                if(structure)
                {
                    if(ignited && structure.IsFlammable()) structure.LitOnFire();

                    if(structure.isObstacle || !structure.destructable)
                    {
                        Vector3 dir = Vector3.Normalize(other.gameObject.transform.position - transform.position);
                        rb.AddForce(25 * -dir, ForceMode.Impulse);
                    }
                    effectsHandler.PlaySound(effectsHandler.hitSounds[0]);
                    return;
                }
            }

            if(other.gameObject.layer == 9)
            {
                CreatureBehaviorScript creature = other.GetComponentInParent<CreatureBehaviorScript>();
                if(creature && creature.shovelVulnerable)
                {
                    bool applyRecoil = true;
                    if(rb.velocity.magnitude > 10f)
                    {
                        if(creature.health <= 10 && creature.canCorpseBreak) applyRecoil = false;

                        creature.TakeDamage(20);
                        if(creature.fireVulnerable && ignited) creature.ApplyStatusEffect(StatusDatabase.Instance.GetStatus(StatusEffectName.Fire), Random.Range(7, 12));
                        creature.PlayHitParticle(creature.transform.position);

                        if(ignited && Random.Range(0, 100) > (20 + friendshipLevel * 5)) IgnitionToggle(false);
                    }

                    if(applyRecoil)
                    {
                        Vector3 dir = Vector3.Normalize(other.gameObject.transform.position - transform.position);
                        rb.AddForce(25 * -dir, ForceMode.Impulse);
                        effectsHandler.PlaySound(effectsHandler.hitSounds[0]);
                    }
                }
            }
        }
    }

    public void IgnitionToggle(bool IsIgnited)
    {
        if(ignited == IsIgnited) return;
        ignited = IsIgnited;

        if(ignited)
        {
            pyreFire.SetActive(true);
            ballMeshRenderer.material = ignitedMat;
            foreach(SkinnedMeshRenderer r in skinnedMeshRenderers)
            {
                //r.materials[0] = ignitedMat;

                Material[] currentMaterials = r.materials; 
                currentMaterials[0] = ignitedMat; 
                r.materials = currentMaterials; 
            }
        }
        else
        {
            pyreFire.SetActive(false);
            ballMeshRenderer.material = extinguishedMat;
            foreach(SkinnedMeshRenderer r in skinnedMeshRenderers)
            {
                Material[] currentMaterials = r.materials; 
                currentMaterials[0] = extinguishedMat; 
                r.materials = currentMaterials; 
            }

            if(effectsHandler)
            {
                ParticlePoolManager.Instance.GrabExtinguishParticle().transform.position = transform.position;
                effectsHandler.MiscSound();
            }
        }

    }

    void AnimateTexture()
    {
        if(!ignited) return;

        textureOffset = textureOffset + offsetRate;
        ballMeshRenderer.material.mainTextureOffset = new Vector2(0, textureOffset);
        if(textureOffset > 500) textureOffset = 0;
        
        if(inBall) return;

        int i = 0;

        foreach(SkinnedMeshRenderer r in skinnedMeshRenderers)
        {
            r.materials[0].mainTextureOffset = new Vector2(0, textureOffset); 
            i++;
            if(i == 2) break; //To not animate the eyes
        }
    }

    void RollBall()
    {
        if(!inBall) return;

        Vector3 velocity = rb.velocity;

        if(agent.enabled) velocity = agent.velocity;

        if (rb.velocity.magnitude < minSpeedForHoming) 
        {
            effectsHandler.loopingSource.volume = 0;
        }
        else effectsHandler.loopingSource.volume = 0.5f;

        // Ignore very small movement (to prevent jitter)
        if (velocity.magnitude > 0.01f)
        {
            // Movement direction (projected onto XZ plane)
            Vector3 moveDir = velocity.normalized;
            moveDir.y = 0;

            // Rotation axis: perpendicular to movement direction
            Vector3 rotationAxis = Vector3.Cross(moveDir, Vector3.up);

            // Distance moved this frame = speed * deltaTime
            float distance = velocity.magnitude * Time.deltaTime;

            // Degrees to rotate the ball based on the distance and radius
            float rotationDegrees = (distance / (2 * Mathf.PI * 1)) * 360f;

            // Apply rotation
            ballPivot.Rotate(rotationAxis, -rotationDegrees, Space.World);
        }
    }

    void HomingBall()
    {
        if(!inBall) return;

        if (rb.velocity.magnitude < minSpeedForHoming) return;

        // Find targets within range
        Collider[] targets = Physics.OverlapSphere(transform.position, detectionRadius, 1 << 9);
        if (targets.Length == 0) return;

        // Pick closest target in front of the ball
        Transform bestTarget = null;
        float bestDot = 0.5f; // ensures it's at least somewhat in front
        float closestDist = Mathf.Infinity;

        foreach (var t in targets)
        {
            Vector3 dirToTarget = (t.transform.position - transform.position);

            // Flatten to XZ plane (ignore height difference)
            dirToTarget.y = 0;
            dirToTarget.Normalize();

            float dot = Vector3.Dot(rb.velocity.normalized, dirToTarget);
            float dist = Vector3.Distance(transform.position, t.transform.position);

            if (dot > bestDot && dist < closestDist)
            {
                bestDot = dot;
                closestDist = dist;
                bestTarget = t.transform;
            }
        }

        float currentHomingStrength = homingStrength + ((friendshipLevel * 0.5f) - 4.5f);

        if (bestTarget != null)
        {
            // Desired direction toward the target, but only in XZ
            Vector3 toTarget = (bestTarget.position - transform.position);
            toTarget.y = 0; // lock vertical adjustment
            Vector3 desiredDir = toTarget.normalized;

            // Preserve current speed
            Vector3 desiredVelocity = desiredDir * rb.velocity.magnitude;

            // Steering force (horizontal only)
            Vector3 steer = desiredVelocity - rb.velocity;
            steer.y = 0; // make absolutely sure no vertical force is applied

            rb.AddForce(steer * currentHomingStrength * Time.fixedDeltaTime, ForceMode.VelocityChange);
        }

        //Limit Velocity

        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // Limit velocity if needed
        if (flatVel.magnitude > 500)
        {
            Vector3 limitedVel = flatVel.normalized * 500;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    /////IInteractable nonsense/////

    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        if(!inBall && !ballTransitioning)
        {
            if(!alreadyPet) StateSwitch(PetState.Pet);
            else if(hunger > 0) StateSwitch(PetState.Ball);
        }
        interactSuccessful = true;
    }

    public void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        interactSuccessful = false;
        if((item.ID == 2 || item.ID == 270) && PlayerInteraction.Instance.waterHeld > 0 && ignited) //Water
        {
            PlayerInteraction.Instance.waterHeld--;
            interactSuccessful = true;
            effectsHandler.MiscSound();
            StopCoroutine(DripEffects());
            StartCoroutine(DripEffects());
            IgnitionToggle(false);
            
            thirst += 50;
            if(thirst > maxThirst) thirst = maxThirst;
            return;
        }
        else if(item.ID == 92 || item.ID == 274) //Torch
        {
            if(!PlayerInteraction.Instance.torchLit && ignited)
            {
                HandItemManager.Instance.TorchFlameToggle(true);
                interactSuccessful = true;
            }
            else if(PlayerInteraction.Instance.torchLit && !ignited)
            {
                IgnitionToggle(true);
                interactSuccessful = true;
            }
            else interactSuccessful = false;
        }
        else if(item.ID == 142) //Pyrefly
        {
            if(!PlayerInteraction.Instance.pyreflyLit && ignited)
            {
                HandItemManager.Instance.PyreflyFlameToggle(true);
                interactSuccessful = true;
            }
            else interactSuccessful = false;
        }
        else if(hunger < 100 && !inBall && !ballTransitioning)
        {
            if(item.foodForPets.Count == 0 || !item.foodForPets.Contains(petType))
            {
                thoughtBubbleScript.PlayEmotion(2);
                return;
            }
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            PlayerInventoryHolder.Instance.UpdateInventory();
            EatFood(item);
            interactSuccessful = true;
            return;
        }
        else interactSuccessful = false;
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
