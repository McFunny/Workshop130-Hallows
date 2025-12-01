using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IWaterHolder
{
    Transform ObjectTransform { get; }

    public bool CanBeWatered()
    {
        return true;
    }

    public void GivenWater();

    public bool CanBeFrozen()
    {
        return false;
    }

    public void Freeze();
}
