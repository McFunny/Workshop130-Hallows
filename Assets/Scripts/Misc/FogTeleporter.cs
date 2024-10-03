using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogTeleporter : MonoBehaviour
{
    public Transform otherEnd;

    private void OnTriggerEnter(Collider other)
    {
        print("triggered");
        if(other.gameObject.layer == 10)
        {
            print("Teleported");
            if(otherEnd) other.transform.position = otherEnd.position;
        }


    }
}
