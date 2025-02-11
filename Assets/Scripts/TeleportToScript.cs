using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportToScript : MonoBehaviour
{
    public Transform teleportPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 10)
            {
                other.gameObject.transform.position = teleportPoint.position;
            }
    }
}
