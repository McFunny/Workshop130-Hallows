using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;
using TMPro;

public class PetBehaviorScript : MonoBehaviour
{
    public PetType petType;
    public string name = "Kevin";

    public int friendshipLevel = 0;
    int maxFriendshipLevel = 10; //Increases frequency of actions
    public float friendPoints = 0;
    float maxFriendPoints = 100; //Increases level when maxed
    public List<InventoryItemData> foodDiet = new List<InventoryItemData>();
    public float hunger = 100; //Animals will eat once their hunger is below a fourth
    public float maxHunger = 100;
    public float hungerDecayRate = 5;
    public float thirst = 100; //Animals will drink once their thirst is below a fourth
    public float maxThirst = 100;
    public float thirstDecayRate = 4;

    public Transform focalPoint;
    protected CreatureBehaviorScript targetCreature;
    protected StructureBehaviorScript targetStructure;

    protected bool alreadyPet = false;

    public CreatureEffectsHandler effectsHandler;
    //public Rigidbody rb;
    public Animator anim;
    public NavMeshAgent agent;

    public float walkSpeed, runSpeed;
    public float followDistance = 80; //Follow player once they leave this range

    protected bool isMoving = false;
    protected bool interruptAction = false;
    protected Coroutine currentRoutine;
    protected Transform player;
    protected Vector3 target, spawnOrigin;
    protected int forceFollows = 0;

    protected bool showStats = false;
    public GameObject statsUI;
    public TextMeshProUGUI hungerText, friendshipText;

    public ParticleSystem dripParticles;
    
    protected void Start()
    {
        TimeManager.OnHourlyUpdate += OnHour;
        player = PlayerInteraction.Instance.transform;

        StartCoroutine(IdleSoundTimer());

        spawnOrigin = new Vector3(transform.position.x, transform.position.y, transform.position.z);

        GameSaveData.Instance.currentPet = this;
    }

    void OnDisable()
    {
        GameSaveData.Instance.currentPet = null;
    }

    protected void Update()
    {
        if(showStats)
        {
            if(!statsUI.activeSelf) statsUI.SetActive(true);
            hungerText.text = "Hunger: " + hunger + "/" + maxHunger;
            friendshipText.text = "Level: " + friendshipLevel;
        }
        else if(statsUI.activeSelf) statsUI.SetActive(false);
    }

    protected virtual void OnHour()
    {
        hunger -= hungerDecayRate;
        if(hunger <= 0)
        {
            FriendPointsChange(-2.5f, false);
            hunger = 0;
        }

        thirst -= thirstDecayRate;
        if(thirst <= 0)
        {
            FriendPointsChange(-2.5f, false);
            thirst = 0;
        }

        if(TimeManager.Instance.currentHour == 8) alreadyPet = false;
    }

    public void FriendPointsChange(float amount, bool showHearts)
    {
        if(showHearts) ParticlePoolManager.Instance.GrabHeartParticle().transform.position = focalPoint.position;

        friendPoints += amount;
        if(friendPoints < 0) friendPoints = 0;
        if(friendPoints >= 100 && friendshipLevel < maxFriendshipLevel)
        {
            friendPoints = 0;
            friendshipLevel++;
        }
    }

    protected void EatFood(InventoryItemData item)
    {
        float hungerRestored = item.animalHungerValue;
        if(hunger + hungerRestored > 100) hungerRestored -= hunger + hungerRestored - 100;
        hunger += hungerRestored;

        //hunger = 100;
        if(hunger > maxHunger) hunger = maxHunger;
        if(foodDiet.Contains(item)) FriendPointsChange(hungerRestored/4, true);
        else FriendPointsChange(hungerRestored/6, true);
        effectsHandler.PlaySound(effectsHandler.eatSound);
    }

    protected IEnumerator MoveToPoint(Vector3 destination, float maxTime)
    {
        if(agent.enabled == false) 
        {
            FinishedMoving();
            print("Agent is not enabled");
            yield break;
        }
        isMoving = true;
        interruptAction = false;

        agent.destination = destination;

        float timeSpent = 0; //to make sure it doesnt get stuck

        while ((agent.pathPending || agent.remainingDistance > agent.stoppingDistance) && timeSpent < maxTime)
        {
            if(StopMovingEarlyCheck()) timeSpent += maxTime;

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

    protected virtual void FinishedMoving()
    {
        isMoving = false;
        currentRoutine = null;
    }

    protected virtual bool StopMovingEarlyCheck()
    {
        if(interruptAction) 
        {
            interruptAction = false;
            print("Pet stopped moving early");
            return true;
        }
        return false;
    }

    protected bool EatCheck(bool checkForThirst)
    {
        var foundBowls = FindObjectsByType<PetBowl>(FindObjectsSortMode.None);
        if(foundBowls.Length == 0) return false;
        for(int i = 0; i < foundBowls.Length; i++)
        {
            if((!checkForThirst && foundBowls[i].ContainsEdibleItem(foodDiet)) || (checkForThirst && foundBowls[i].containsWater))
            {
                targetStructure = foundBowls[i];
                return true;
            }
        }
        return false;
    }


    IEnumerator IdleSoundTimer()
    {
        while(true)
        {
            yield return new WaitForSeconds(Random.Range(9, 16));
            effectsHandler.RandomIdle();
        }

    }

    protected IEnumerator DripEffects()
    {
        dripParticles.Play();
        yield return new WaitForSeconds(8);
        dripParticles.Stop();
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
