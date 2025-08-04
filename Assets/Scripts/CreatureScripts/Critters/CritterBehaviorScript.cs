using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;

public class CritterBehaviorScript : CreatureBehaviorScript, ICritter
{
    [Header("Pet Variables")]
    public string name = "Dave";
    public int friendshipLevel = 0;
    protected int maxFriendshipLevel = 5; //Increases frequency of actions
    public float friendPoints = 0;
    protected float maxFriendPoints = 100; //Increases level when maxed
    public List<InventoryItemData> foodDiet = new List<InventoryItemData>();
    public float hunger = 100; //Animals will eat once their hunger is below half
    public float maxHunger = 100;
    public float hungerDecayRate = 5;
    public float thirst = 100; //Animals will drink once their thirst is below a fourth
    public float maxThirst = 100;
    public float thirstDecayRate = 4;
    protected bool alreadyPet = false;
    protected Coroutine currentRoutine;
    protected bool isMoving, interruptAction;
    protected Vector3 target;
    protected Transform targetObject;
    public NavMeshAgent agent;

    protected void Start() //Have all critters call these 2 functions in their Start method
    {
        base.Start();
        CritterStart();
    }
    
    protected void CritterStart()
    {
        TimeManager.OnHourlyUpdate += OnHour;
        BarnManager.Instance.allCritters.Add(this);
        OnHour();
    }

    protected virtual void OnHour()
    {
        if(isDead)
        {
            TakeDamage(999);
            return;
        }

        bool tookDamage = false;
        hunger -= hungerDecayRate;
        if(hunger < 0)
        {
            TakeDamage(5);
            tookDamage = true;
        }
        thirst -= thirstDecayRate;
        if(thirst < 0)
        {
            TakeDamage(5);
            tookDamage = true;
        }

        if(!tookDamage) health += 5;
        if(health > maxHealth) health = maxHealth;
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

    protected bool EatCheck(bool checkForThirst)
    {
        var foundSources = FindObjectsByType<Trough>(FindObjectsSortMode.None);
        if(foundSources.Length == 0) return false;
        for(int i = 0; i < foundSources.Length; i++)
        {
            if((!checkForThirst && foundSources[i].HasEdibleItem(foodDiet)) || (checkForThirst && foundSources[i].waterLevel > 0))
            {
                targetObject = foundSources[i].transform;
                return true;
            }
        }
        return false;
    }

    protected void FriendPointsChange(float amount, bool showHearts)
    {
        if(showHearts) ParticlePoolManager.Instance.GrabHeartParticle().transform.position = 
            new Vector3(corpseParticleTransform.position.x, corpseParticleTransform.position.y + 1, corpseParticleTransform.position.z);

        friendPoints += amount;
        if(friendPoints < 0) friendPoints = 0;
        if(friendPoints >= 100 && friendshipLevel < maxFriendshipLevel)
        {
            friendPoints = 0;
            friendshipLevel++;
        }
    }

    protected IEnumerator IdleSoundTimer()
    {
        while(health > 0)
        {
            int i = Random.Range(8,18);
            effectsHandler.RandomIdle();
            yield return new WaitForSeconds(i);
        }
    }

    protected Vector3 GetRandomPointAround(Vector3 origin, float radius)
    {
        Vector2 randomDirection = Random.insideUnitCircle * radius;
        Vector3 randomPoint = new Vector3(randomDirection.x, origin.y, randomDirection.y) + origin;
        return randomPoint;
    }

    protected IEnumerator MoveToPoint(Vector3 destination, float duration) //Used just for wandering it seems
    {
        isMoving = true;
        interruptAction = false;

        agent.destination = destination;

        float timeSpent = 0; //to make sure it doesnt get stuck

        while ((agent.pathPending || agent.remainingDistance > agent.stoppingDistance) && timeSpent < duration && !interruptAction)
        {
            timeSpent += Time.deltaTime;

            yield return null;
        }

        FinishedMoving();

    }

    protected virtual void FinishedMoving()
    {
        isMoving = false;
        currentRoutine = null;
    }

    protected void OnDestroy() //Have all critters call these 2 functions in their Destroy method
    {
        base.OnDestroy();
        OnCritterDestroy();
    }

    protected void OnCritterDestroy()
    {
        TimeManager.OnHourlyUpdate -= OnHour;
        BarnManager.Instance.allCritters.Remove(this);
    }

    //////////////ICritter Stuff\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\
    public float GetCritterHealth(){ return health;}
    public float GetCritterHunger(){ return hunger;}
    public float GetCritterThirst(){ return thirst;}
    public string GetCritterName(){ return name;}
    public int GetCritterID(){ return creatureData.id;}
    public CritterData GetCritterData(){ return new CritterData(creatureData.id, friendshipLevel, friendPoints, health, hunger, thirst, name);} //For saving purposes

    public virtual void Interact(PlayerInteraction interactor, out bool interactSuccessful)
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

    public virtual void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        interactSuccessful = true;
    }
}
