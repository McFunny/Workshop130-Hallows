using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WagonManager : MonoBehaviour
{
    public static WagonManager Instance;

    [HideInInspector] public PlayerWagonScript farmWagon, wildernessWagon;

    public float wagonHealth = 500;
    public float maxWagonHealth = 500;

    public bool wagonDestroyed = false;

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else Instance = this;
    }

    void Start()
    {
        WildernessManager.OnWildernessLeave += LeaveWilderness;
    }

    void OnDestroy()
    {
        WildernessManager.OnWildernessLeave -= LeaveWilderness;
    }

    public void WagonHealthChange(float amount)
    {
        if(amount <= 0 && wagonHealth <= 0) return;
        
        wagonHealth += amount;

        if(wagonHealth < 0) wagonHealth = 0;

        if(wagonHealth > maxWagonHealth) wagonHealth = maxWagonHealth;

        print("Wagon health changed. Health is " + wagonHealth);

        if(!wagonDestroyed && wagonHealth == 0 && TownGate.Instance.location == PlayerLocation.InWilderness)
        {
            WildernessManager.Instance.ExitWilderness();
        }
    }

    void LeaveWilderness()
    {
        if(!wagonDestroyed) wagonHealth = maxWagonHealth;
    }
}
