using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TruffleHog : CreatureBehaviorScript, ICritter
{
    public int friendshipLevel = 0;
    int maxFriendshipLevel = 5; //Increases frequency of actions
    public float friendPoints = 0;
    float maxFriendPoints = 100; //Increases level when maxed
    public List<InventoryItemData> foodDiet = new List<InventoryItemData>();
    public float hunger = 100; //Animals will eat once their hunger is below half
    public float maxHunger = 100;
    public float hungerDecayRate = 5;
    public float thirst = 100; //Animals will drink once their thirst is below a fourth
    public float maxThirst = 100;
    public float thirstDecayRate = 4;

    void Start()
    {
        base.Start();
        TimeManager.OnHourlyUpdate += OnHour;
    }

    void OnHour()
    {
        hunger -= hungerDecayRate;
        if(hunger < 0) TakeDamage(5);
    }

    void OnDestroy()
    {
        base.OnDestroy();
        TimeManager.OnHourlyUpdate -= OnHour;
    }

    public float GetCritterHealth(){ return health;}


    public float GetCritterHunger(){ return hunger;}
    public float GetCritterThirst(){ return thirst;}

    public string GetCritterName(){ return name;}

    public int GetCritterID(){ return creatureData.id;}

    public void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        interactSuccessful = true;
    }

    public void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        interactSuccessful = true;
    }
}
