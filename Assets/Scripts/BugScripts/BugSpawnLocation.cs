using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BugSpawnLocation : MonoBehaviour
{
    public BugSpawnArea areaName;

    void OnEnable()
    {
        if(BugSpawningManager.Instance) BugSpawningManager.Instance.bugSpawns.Add(this);
    }

    void Start()
    {
        BugSpawningManager.Instance.bugSpawns.Add(this);
    }

    void OnDisable()
    {
        BugSpawningManager.Instance.bugSpawns.Remove(this);
    }
}
public enum BugSpawnArea
{
    Farm,
    Barn,
    Town,
    Wilderness,
    Catacombs,
    Indoors
}
