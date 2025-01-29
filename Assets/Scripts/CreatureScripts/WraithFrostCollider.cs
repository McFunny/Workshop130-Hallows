using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WraithFrostCollider : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        var farmTile = other.GetComponentInParent<FarmLand>();
        if(farmTile)
        {
            farmTile.RecieveFrost();
        }
    }
}
