using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;

public class PetBehaviorScript : MonoBehaviour
{
    public PetType petType;

    public int friendshipLevel = 0;
    int maxFriendshipLevel = 10; //Increases frequency of actions
    public float friendPoints = 0;
    float maxFriendPoints = 100; //Increases level when maxed
    public List<InventoryItemData> foodDiet = new List<InventoryItemData>();
    public float hunger = 100; //Animals will eat once their hunger is below half
    public float maxHunger = 100;
    public float hungerDecayRate = 5;

    public Transform focalPoint;

    protected bool alreadyPet = false;

    public CreatureEffectsHandler effectsHandler;
    public Rigidbody rb;
    public Animator anim;
    public NavMeshAgent agent;

    public float walkSpeed, runSpeed;
    public float followDistance = 75; //Follow player once they leave this range

    protected bool isMoving = false;
    protected bool interruptAction = false;
    protected Coroutine currentRoutine;
    protected Transform player;
    protected Vector3 target, origin;
    
    void Start()
    {
        TimeManager.OnHourlyUpdate += OnHour;
        player = PlayerInteraction.Instance.transform;

        origin = new Vector3(transform.position.x, transform.position.y, transform.position.z);

        StartCoroutine(IdleSoundTimer());
    }

    protected virtual void OnHour()
    {
        hunger -= hungerDecayRate;
        if(hunger < 0) FriendPointsChange(-5);

        if(TimeManager.Instance.currentHour == 8) alreadyPet = false;
    }

    public void FriendPointsChange(float amount)
    {
        friendPoints += amount;
        if(friendPoints < 0) friendPoints = 0;
        if(friendPoints >= 100)
        {
            friendPoints = 0;
            friendshipLevel++;
        }
    }

    protected void EatFood(InventoryItemData item)
    {
        //hunger += item.staminaValue * 3;
        //if(item.staminaValue == 0) hunger += value * sellValueModifier * 2;
        hunger = 100;
        if(hunger > maxHunger) hunger = maxHunger;
        FriendPointsChange(10);
        effectsHandler.PlaySound(effectsHandler.eatSound);
    }

    protected IEnumerator MoveToPoint(Vector3 destination)
    {
        isMoving = true;
        interruptAction = false;

        agent.destination = destination;

        float timeSpent = 0; //to make sure it doesnt get stuck

        while ((agent.pathPending || agent.remainingDistance > agent.stoppingDistance) && timeSpent < 25)
        {
            if(StopMovingEarlyCheck()) timeSpent += 100;

            timeSpent += Time.deltaTime;
            yield return null;
        }

        FinishedMoving();
    }

    protected Vector3 GetRandomPointAround(Vector3 origin, float radius)
    {
        Vector2 randomDirection = Random.insideUnitCircle * radius;
        Vector3 randomPoint = new Vector3(randomDirection.x, origin.y, randomDirection.y) + origin;
        return randomPoint;
    }

    protected virtual void FinishedMoving()
    {
        isMoving = false;
        currentRoutine = null;
    }

    protected virtual bool StopMovingEarlyCheck()
    {
        if(currentRoutine == null || interruptAction) 
        {
            return true;
            interruptAction = false;
        }
        return false;
    }

    IEnumerator IdleSoundTimer()
    {
        while(true)
        {
            yield return new WaitForSeconds(Random.Range(5, 9));
            effectsHandler.RandomIdle();
        }

    }

    void OnDestroy()
    {
        TimeManager.OnHourlyUpdate -= OnHour;
    }
}

public enum PetType
{
    Cat,
    Shoebill,
    Grub,
    Crab,
    Dog
}
