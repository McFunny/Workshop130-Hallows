using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogTeleporter : MonoBehaviour
{
    public Transform otherEnd;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == 10)
        {
            if(otherEnd) other.transform.position = otherEnd.position;
        }
        else if(other.gameObject.layer == 9)
        {
            if(TimeManager.isDay) Destroy(other.gameObject);
            else if(otherEnd) other.transform.position = otherEnd.position;
        }


    }
}
