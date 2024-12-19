using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MutatedCrow : CreatureBehaviorScript
{
    public enum CreatureState
    {
        Idle,
        Flee,
        CirclePlayer,
        CirclePoint,
        AttackPlayer,
        Stun,
        Land,
        Die,
        Trapped,
        Wait,
        GoAway
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
