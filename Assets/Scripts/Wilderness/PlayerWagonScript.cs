using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWagonScript : MonoBehaviour
{
    public WagonType type;
    public List<Transform> weakPoints; // spots for enemies to choose from to attack. Have at least 12 so enemies do not clump, slightly in the wagon so they stare at it

    void Start()
    {
        if(type == WagonType.Farm) WagonManager.Instance.farmWagon = this;
        else if(type == WagonType.Wilderness) WagonManager.Instance.wildernessWagon = this;
    }

    public void TakeWagonDamage(float amount)
    {
        WagonManager.Instance.WagonHealthChange(-amount);
    }

    public Transform GetWeakPoint()
    {
        return weakPoints[Random.Range(0, weakPoints.Count)];
    }
}

public enum WagonType
{
    Farm,
    Wilderness,
    Transition,
    Extra
}
