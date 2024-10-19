using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolBehavior : ScriptableObject
{
    [HideInInspector] public bool usingPrimary, usingSecondary = false;
    [HideInInspector] public Transform player;
    [HideInInspector] public ToolType tool;
    public virtual void PrimaryUse(Transform player, ToolType tool)
    {

    }

    public virtual void SecondaryUse(Transform player, ToolType tool)
    {

    }

    public virtual void ItemUsed() { }


}
