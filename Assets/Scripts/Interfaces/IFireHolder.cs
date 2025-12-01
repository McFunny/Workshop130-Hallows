using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFireHolder
{
    public bool CanBeExtinguished()
    {
        return false;
    }

    public void ExternalExtinguish();
}
