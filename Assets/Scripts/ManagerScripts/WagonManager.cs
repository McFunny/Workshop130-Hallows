using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WagonManager : MonoBehaviour
{
    public static WagonManager Instance;

    [HideInInspector] public PlayerWagonScript farmWagon, wildernessWagon;

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else Instance = this;
    }
}
