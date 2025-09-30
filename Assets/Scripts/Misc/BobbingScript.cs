using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BobbingScript : MonoBehaviour
{
    float displacement = 0.2f;
    float speed = 0.0008f;
    bool movingUp = true;
    float currentDisplacement = 0;
    void Start()
    {
        displacement += Random.Range(-0.15f, 0.05f);
        speed += Random.Range(-0.0005f, 0.0005f);
    }

    void Update()
    {
        if(movingUp)
        {
            if(currentDisplacement > displacement) movingUp = false;
            else 
            {
                transform.position = new Vector3(transform.position.x, transform.position.y + speed, transform.position.z);
                currentDisplacement += speed;
            }
        }
        else
        {
            if(currentDisplacement < -displacement) movingUp = true;
            else
            {
                transform.position = new Vector3(transform.position.x, transform.position.y - speed, transform.position.z);
                currentDisplacement -= speed;
            }
        }
    }
}
