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

    public void ManualFill(out bool success)
    {
        success = false;
    }

    public void GivenWater();

    public void EmptyWater();
}
