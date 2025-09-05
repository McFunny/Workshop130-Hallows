using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HydroFly : CritterBehaviorScript
{
    public InventoryItemData bugItem;

    private bool coroutineRunning = false;
    //public List<StructureObject> targettableStructures; //Call the HitWithWater Function

    float waterDistance = 3.5f; //distance to water structures

    [HideInInspector]public bool hydrated = true;
    public GameObject bubbleObject, splashObject;
    public ParticleSystem waterSpray;
    //public Material ignitedMat, extinguishedMat;
    public MeshRenderer meshRenderer;
    float textureOffset = 0;
    public float offsetRate = .005f;

    private IWaterHolder targetStructure; //Struct to water

    bool idleTurn = false;

    public GameObject fearObject; //The particle system
    Vector3 fearedObjectPosition; //Where the lavent leaf is
    Vector3 fleeToPos; //Where its fleeing to

    public enum CritterState
    {
        Decide,
        Wander,
        WalkTowardsStructure,
        CollectWater,
        Dead,
        Eat,
        Flee
    }

    public CritterState currentState;

    void Start()
    {
        base.Start();

        StartCoroutine(PlayerTurn());
        HydrationToggle(false);
        anim.SetBool("HighBob", true);
    }

    void OnDestroy()
    {
        base.OnDestroy();
        if(!gameObject.scene.isLoaded) return;

        if(hydrated && health <= 0)
        {
            ParticlePoolManager.Instance.GrabSplashParticle().transform.position = corpseParticleTransform.position;
            if(PlayerInteraction.Instance.stamina > 0) effectsHandler.ThrowSound(effectsHandler.deathSound);

            Collider[] hitStructures = Physics.OverlapSphere(transform.position, 5f, 1 << 6);
            foreach(Collider collider in hitStructures)
            {
                StructureBehaviorScript structure = collider.gameObject.GetComponentInParent<StructureBehaviorScript>();
                if(structure)
                {
                    structure.HitWithWater();
                }
            }

            Collider[] hitEnemies = Physics.OverlapSphere(transform.position, 5f, 1 << 9);
            foreach(Collider collider in hitEnemies)
            {
                var creature = collider.GetComponentInParent<CreatureBehaviorScript>();
                if (creature != null && creature.shovelVulnerable)
                {
                    creature.HitWithWater();
                }
            }
        }
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

            case CritterState.Wander:
                Wander();
                break;

            case CritterState.WalkTowardsStructure:
                WalkTowardsStructure();
                break;
                
            case CritterState.CollectWater:
                CollectWater();
                break;

            case CritterState.Flee:
                Flee();
                break;

            case CritterState.Eat:
                //Eat();
                break;

            default:
                Debug.LogError("Unknown state: " + currentState);
                break;
        }
    }

    void Update()
    {
        if (health <= 0) isDead = true;

        if (!isDead && currentState != CritterState.Dead)
        {
            CheckState(currentState);
        }

        textureOffset = textureOffset + offsetRate;
        meshRenderer.material.mainTextureOffset = new Vector2(0, textureOffset);
        if(textureOffset > 500) textureOffset = 0;

        float distance = Vector3.Distance(player.position, transform.position);
        playerInSightRange = distance <= sightRange;
        if(playerInAttackRange) playerInAttackRange = distance <= attackRange + 5;
        else playerInAttackRange = distance <= attackRange;

        if(idleTurn)
        {
            Vector3 targetPosition = player.position;

            Vector3 direction = targetPosition - transform.position;
            direction.y = 0;
            Quaternion toRotation = Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, 3.5f * Time.deltaTime);
        }
    }

    void Decide()
    {
        /*if((hunger <= 25 && EatCheck(false)) || (thirst <= 25 && EatCheck(true)))
        {
             currentState = CritterState.Eat;
            return;
        }*/

        if(fearedObjectPosition != Vector3.zero)
        {
            currentState = CritterState.Flee;
            return;
        }

        if(hydrated)
        {
            if(targetStructure != null && targetStructure.CanBeWatered()) currentState = CritterState.WalkTowardsStructure;
            else
            {
                FindStructure();
                if(targetStructure != null && targetStructure.CanBeWatered()) currentState = CritterState.WalkTowardsStructure;
                else currentState = CritterState.Wander;
            }
        }
        else currentState = CritterState.CollectWater;
    }

    void Wander()
    {
        if (!isMoving && currentState == CritterState.Wander)
        {
            Vector3 newDestination = GetRandomPointAround(transform.position, 10);
            if(Vector3.Distance(BarnManager.Instance.barnSource.position, transform.position) > 30) newDestination = GetRandomPointAround(BarnManager.Instance.barnSource.position, 20);
            StartCoroutine(MoveToPoint(newDestination, 8));
        }
    }

    void WalkTowardsStructure()
    {
        if(coroutineRunning) return;

        if(!hydrated) //Not watered, so go back and get some water
        {
            currentState = CritterState.Decide;
            return;
        }

        if (targetStructure == null) //Old structure gone? Find a new one
        {
            FindStructure();
            if (targetStructure != null)
            {
                target = targetStructure.ObjectTransform.position;
                agent.destination = target;
            }
            else
            {
                currentState = CritterState.Decide;
            }
        }
        else if (Vector3.Distance(transform.position, targetStructure.ObjectTransform.position) < waterDistance)//(!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 1f)
        {
            agent.ResetPath();
            interruptAction = true;
            coroutineRunning = true;
            StartCoroutine(WaterStructure());
        }
        else if(agent.destination != target)
        {
            target = targetStructure.ObjectTransform.position;
            agent.destination = target;
        }
    }

    void CollectWater() //Refresh at the barn well
    {
        if(hydrated) //Its watered, so we r done here
        {
            currentState = CritterState.Decide;
            return;
        }

        if (!isMoving)
        {
            target = BarnManager.Instance.barnWell.position;
            StartCoroutine(MoveToPoint(target, 25));
        }
        else if(Vector3.Distance(transform.position, target) < 3)
        {
            interruptAction = true;
        }
    }

    void FindStructure() //Find thing to burn, regardless of distance
    {
        List<IWaterHolder> availableHolders = new List<IWaterHolder>();
        foreach (StructureBehaviorScript structure in structManager.allStructs)
        {
            IWaterHolder potentialHolder = structure as IWaterHolder;
            if(potentialHolder != null && potentialHolder.CanBeWatered()) availableHolders.Add(potentialHolder);
        }

        if (availableHolders.Count > 0)
        {
            int r = Random.Range(0, availableHolders.Count);
            targetStructure = availableHolders[r];
        }
    }

    void Flee()
    {
        if(Vector3.Distance(transform.position, fearedObjectPosition) > 8)
        {
            fearedObjectPosition = Vector3.zero;
            targetStructure = null;
            currentState = CritterState.Decide;
            fearObject.SetActive(false);
            return;
        }
    }

    protected override void FinishedMoving()
    {
        if(currentState == CritterState.Wander)
        {
            StartCoroutine(WaitAround());
            return;
        }
        if(currentState == CritterState.CollectWater)
        {
            if(Vector3.Distance(transform.position, target) < 5) HydrationToggle(true);
        }
        base.FinishedMoving();
    }

    IEnumerator WaitAround()
    {
        yield return new WaitForSeconds(Random.Range(1f, 5f));
        currentState = CritterState.Decide;
        isMoving = false;
        coroutineRunning = false;
    }

    IEnumerator WaterStructure()
    {
        if(hydrated)
        {
            if(targetStructure != null && targetStructure.CanBeWatered())
            {
                waterSpray.Play();
                yield return new WaitForSeconds(0.5f);
                HydrationToggle(false);
                targetStructure.GivenWater();
                if(!targetStructure.CanBeWatered()) targetStructure = null;
                yield return new WaitForSeconds(0.9f);
                currentState = CritterState.Decide;
            }
            else
            {
                FindStructure();
            }
        }
        coroutineRunning = false;
        currentState = CritterState.Decide;
    }

    public void HydrationToggle(bool IsHydrated)
    {
        if(hydrated == IsHydrated) return;
        hydrated = IsHydrated;

        if(hydrated)
        {
            bubbleObject.SetActive(true);
            //meshRenderer.material = ignitedMat;
        }
        else
        {
            bubbleObject.SetActive(false);
            //meshRenderer.material = extinguishedMat;

            currentState = CritterState.CollectWater;
        }
        splashObject.SetActive(true);

    }

    IEnumerator PlayerTurn()
    {
        while(health > 0)
        {
            float r = Random.Range(2,6);
            yield return new WaitForSeconds(r);
            r = Random.Range(0,10);
            if(r > 2 && Vector3.Distance(transform.position, player.position) < 8 && currentState != CritterState.WalkTowardsStructure)
            {
                agent.updateRotation = false;
                idleTurn = true;
                yield return new WaitForSeconds(1.5f);
                idleTurn = false;
                agent.updateRotation = true;
            }
        }
    }

    public override void NearLaventLeaf(Vector3 pos)
    {
        if(currentState == CritterState.Flee) return;
        fearedObjectPosition = pos;
        fleeToPos = transform.position + ((transform.position - fearedObjectPosition + new Vector3(Random.Range(-3, 3), 0, Random.Range(-3, 3)) * 8));
        agent.destination = fleeToPos;
        fearObject.SetActive(true);
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        if(type == ToolType.WateringCan && PlayerInteraction.Instance.waterHeld > 0 && !hydrated && currentState == CritterState.CollectWater)
        {
            PlayerInteraction.Instance.waterHeld--;
            splashObject.SetActive(true);
            HydrationToggle(true);
            success = true;
            currentState = CritterState.Decide;
        }
        else success = false;
    }

    public override bool CaughtByBugNet(out InventoryItemData item)
    {
        item = bugItem;
        
        return true;
    }

    public override void OnDeath()
    {
        if (!isDead)
        {
            isDead = true;

            PopupHandler.Instance.names.Enqueue(name);
            PopupHandler.Instance.AddToQueue(PopupHandler.Instance.critterDiedPopup);
        }
    }

}
