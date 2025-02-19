using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportToScript : MonoBehaviour
{
    public Transform teleportPoint;
    public bool goingToWilderness = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 10)
            {
                if(goingToWilderness) WildernessManager.Instance.EnterWilderness();
                else other.gameObject.transform.position = teleportPoint.position;
            }
    }
}
