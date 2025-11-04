using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WagonManager : MonoBehaviour
{
    public static WagonManager Instance;

    [HideInInspector] public PlayerWagonScript farmWagon, wildernessWagon;

    public float wagonHealth = 500;
    public float maxWagonHealth = 500;

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else Instance = this;
    }

    public void WagonHealthChange(float amount)
    {
        wagonHealth += amount;

        if(wagonHealth < 0) wagonHealth = 0;

        if(wagonHealth > maxWagonHealth) wagonHealth = maxWagonHealth;

        print("Wagon health changed. Health is " + wagonHealth);
    }
}
