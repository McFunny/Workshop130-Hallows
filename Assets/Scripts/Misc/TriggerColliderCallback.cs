using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerColliderCallback : MonoBehaviour
{
    public bool enteringTown;
    void OnTriggerEnter()
    {
        TownGate.Instance.Transition(enteringTown);
    }
}
