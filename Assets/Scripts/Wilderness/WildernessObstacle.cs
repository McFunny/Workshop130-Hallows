using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WildernessObstacle : MonoBehaviour
{
    public ObjectWithProbability[] obstacles;

    void Awake()
    {
        for(int i = 0; i < obstacles.Length; i++)
        {
            obstacles[i]._object.SetActive(false);
        }
    }
    
    void OnEnable()
    {
        bool success = false;
        int attempts = 0;
        int r; //random number
        int i; //index
        while(!success)
        {
            if(attempts > 20) r = 0;
            else r = Random.Range(0,100);
            i = Random.Range(0, obstacles.Length);
            if(obstacles[i]._probability >= r)
            {
                obstacles[i]._object.SetActive(true);
                success = true;
            }
            attempts++;
        }
    }

    void OnDisable()
    {
        for(int i = 0; i < obstacles.Length; i++)
        {
            obstacles[i]._object.SetActive(false);
        }
    }

}
