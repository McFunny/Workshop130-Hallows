using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWagonScript : MonoBehaviour
{
    public WagonType type;

    void Start()
    {
        if(type == WagonType.Farm) WagonManager.Instance.farmWagon = this;
        else if(type == WagonType.Wilderness) WagonManager.Instance.wildernessWagon = this;
    }
}

public enum WagonType
{
    Farm,
    Wilderness,
    Transition,
    Extra
}
