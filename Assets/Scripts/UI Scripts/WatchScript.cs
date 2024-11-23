using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class WatchScript : MonoBehaviour
{
    public GameObject watchHand;

    // Update is called once per frame
    void Update()
    {
        //watchHand.transform.rotation *= Quaternion.Euler(0,0,-30);
        UpdateWatch();
        
    }

    void UpdateWatch() //Cam don't look at this again it's been a long week
    {
       watchHand.transform.rotation = Quaternion.Euler(0,0,TimeManager.Instance.currentHour * 30 * -1);
    }
}
