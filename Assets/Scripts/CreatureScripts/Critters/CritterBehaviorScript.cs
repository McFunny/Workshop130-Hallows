using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;

public class CritterBehaviorScript : CreatureBehaviorScript, ICritter
{
    [Header("Pet Variables")]
    public CritterType critterType;
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
    public PenType penType;
    [HideInInspector] public CritterPen homePen;
    protected bool alreadyPet = false;
    protected Coroutine currentRoutine;
    protected bool isMoving, interruptAction;
    protected Vector3 target;
    protected Transform targetObject;
    public NavMeshAgent agent;

    protected int playerFollowTokens = 0; //How many paces they will spend following the player

    bool justSpawned = true;
    protected bool behaviorDelay = true;

    protected void Start() //Have all critters call these 2 functions in their Start method (Nvm?)
    {
        base.Start();
        CritterStart();
    }
    
    protected void CritterStart()
    {
        TimeManager.OnHourlyUpdate += OnHour;
        BarnManager.Instance.allCritters.Add(this);
        OnHour();
        StartCoroutine(BehaviorDelay());
    }

    IEnumerator BehaviorDelay()
    {
        yield return new WaitForSeconds(1);
        behaviorDelay = false;
    }

    protected virtual void OnHour()
    {
        if(justSpawned)
        {
            justSpawned = false;
            return;
        }
        else if(TimeManager.Instance.currentHour == 8 && !homePen)
        {
            PopupHandler.Instance.names.Enqueue(name);
            PopupHandler.Instance.AddToQueue(PopupHandler.Instance.critterLeftPopup);
            Destroy(gameObject);
        }
    
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
            FriendPointsChange(-2, false);
            tookDamage = true;
        }
        thirst -= thirstDecayRate;
        if(thirst < 0)
        {
            TakeDamage(5);
            FriendPointsChange(-2, false);
            tookDamage = true;
        }

        if(!tookDamage) health += 2;
        if(health > maxHealth) health = maxHealth;
        
        if(!homePen) FindHomePen();
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
        Collider[] hitStructures = Physics.OverlapSphere(transform.position, 80f, 1 << 6);
        foreach(Collider collider in hitStructures)
        {
            Trough t = collider.gameObject.GetComponent<Trough>();
            if(t && ((!checkForThirst && t.HasEdibleItem(foodDiet)) || (checkForThirst && t.waterLevel > 0)))
            {
                targetObject = t.transform;
                return true;
            }
        }
        //var foundSources = FindObjectsByType<Trough>(FindObjectsSortMode.None);
        /*if(foundSources.Length == 0) return false;
        for(int i = 0; i < foundSources.Length; i++)
        {
            if((!checkForThirst && foundSources[i].HasEdibleItem(foodDiet)) || (checkForThirst && foundSources[i].waterLevel > 0))
            {
                targetObject = foundSources[i].transform;
                return true;
            }
        }*/
        return false;
    }

    public void FriendPointsChange(float amount, bool showHearts)
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
            yield return new WaitForSeconds(i);
            effectsHandler.RandomIdle();
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
        if(playerFollowTokens > 0) playerFollowTokens--;

    }

    protected virtual void FinishedMoving()
    {
        isMoving = false;
        currentRoutine = null;
    }

    void FindHomePen()
    {
        Collider[] hitStructures = Physics.OverlapSphere(transform.position, 80f, 1 << 6);
        foreach(Collider collider in hitStructures)
        {
            CritterPen pen = collider.gameObject.GetComponent<CritterPen>();
            if(pen && pen.type == penType && pen.housedCritters.Count < pen.maxOccupency)
            {
                if(pen.type == PenType.Hive && pen.durability <= 0) continue;
                homePen = pen;
                pen.housedCritters.Add(this);
                return;
            }
        }
    }

    public override void OnDamage()
    {
        if(TutorialMiller.Instance) health = maxHealth;

        if(health > 0 && effectsHandler.hitSounds.Length > 0) effectsHandler.OnHit();
    }

    protected void OnDestroy() //Have all critters call these 2 functions in their Destroy method (Nvm?)
    {
        base.OnDestroy();
        OnCritterDestroy();
    }

    protected void OnCritterDestroy()
    {
        TimeManager.OnHourlyUpdate -= OnHour;
        BarnManager.Instance.allCritters.Remove(this);
        if(homePen) homePen.housedCritters.Remove(this);

        /*if(health <= 0)
        {
            PopupHandler.Instance.names.Enqueue(name);
            PopupHandler.Instance.AddToQueue(PopupHandler.Instance.critterDiedPopup);
        }*/
    }

    public int MaxLevel
    {
        get
        {
            return maxFriendshipLevel;
        }
    }

    //////////////ICritter Stuff\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\
    public float GetCritterHealth(){ return health;}
    public float GetCritterHunger(){ return hunger;}
    public float GetCritterThirst(){ return thirst;}
    public string GetCritterName(){ return name;}
    public int GetCritterID(){ return creatureData.id;}
    public bool IsCritterHomeless()
    {
        if(homePen) return false;
        else return true;
    }
    public CritterData GetCritterData(){ return new CritterData(creatureData.id, friendshipLevel, friendPoints, health, hunger, thirst, name, 0);} //For saving purposes
    public CritterBehaviorScript GetCritterScript(){ return this;}

    public virtual void LoadData(CritterData c)
    {
        friendshipLevel = c.friendshipLevel;
        friendPoints = c.friendPoints;
        health = c.health;
        hunger = c.hunger;
        thirst = c.thirst;
        name = c.name;
    }

    public virtual void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        print(GetCritterHunger());
        if(!alreadyPet)
        {
            alreadyPet = true;
            FriendPointsChange(25, true);
            effectsHandler.PlaySound(effectsHandler.petSound);
            interactSuccessful = true;

            if(TutorialMiller.Instance) 
            {
                TutorialMiller.Instance.HogPetted();
            }
            PopupEvents.current.PetCritter();
            return;
        }
        interactSuccessful = false;
    }

    public virtual void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        interactSuccessful = true;
    }
}
[System.Serializable]
public enum CritterType
{
    Hog,
    Mimic,
    Fly,
    Hen,
    Hare
}
