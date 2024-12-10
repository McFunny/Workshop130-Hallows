using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogBarricade : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if(!TownGate.Instance.inTown && GameSaveData.Instance.lumber_choppedTree)
        {
            if(!GameSaveData.Instance.bridgeCleared) GameSaveData.Instance.bridgeCleared = true;
            Destroy(this.gameObject);
        }
    }
}
