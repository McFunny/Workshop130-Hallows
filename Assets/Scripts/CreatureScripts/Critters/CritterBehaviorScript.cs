using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CritterBehaviorScript : CreatureBehaviorScript, IInteractable
{
    public int friendshipLevel = 0;
    int maxFriendshipLevel = 5; //Increases frequency of actions
    public float friendPoints = 0;
    float maxFriendPoints = 100; //Increases level when maxed
    public List<InventoryItemData> foodDiet = new List<InventoryItemData>();
    public float hunger = 100; //Animals will eat once their hunger is below half
    public float maxHunger = 100;
    public float hungerDecayRate = 5;
    /////IInteractable nonsense/////
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        interactSuccessful = true;
    }

    public void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        interactSuccessful = true;
    }
    

    public void EndInteraction(){}

    public void ToggleHighlight(bool enabled){}

    public void ReturnFocalPoint(out Transform point)
    {
        if(corpseParticleTransform) point = corpseParticleTransform;
        else point = transform;
    }
    ///////////////////////////////////
    /// 
    void Start() //Have all critters call these 2 functions in their Start method
    {
        base.Start();
        CritterStart();
    }
    
    protected void CritterStart()
    {
        TimeManager.OnHourlyUpdate += OnHour;
    }

    protected virtual void OnHour()
    {
        hunger -= hungerDecayRate;
        if(hunger < 0) TakeDamage(5);
    }

    void OnDestroy() //Have all critters call these 2 functions in their Destroy method
    {
        base.OnDestroy();
        OnCritterDestroy();
    }

    void OnCritterDestroy()
    {
        TimeManager.OnHourlyUpdate -= OnHour;
    }
}
