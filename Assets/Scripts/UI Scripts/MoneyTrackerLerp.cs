using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoneyTrackerLerp : MonoBehaviour
{
     // LEEEERP!!!!
    public Transform lerpStart, lerpEnd, moneyTrackerTransform;
    float moveProgress = 0;
    float maxMoveProgress = 0.5f;
    public bool forceActive = false;
    
    // Start is called before the first frame update
    void Start()
    {
        moneyTrackerTransform.position = lerpEnd.position;
    }

    // Update is called once per frame
    void Update()
    { 
        if(PlayerMovement.accessingInventory){ forceActive = true; }
        else { forceActive = false; }

        if((UICropStats.isDetailed && moveProgress > 0) || (forceActive && moveProgress >= 0))
        {
            moveProgress -= Time.deltaTime;
            moneyTrackerTransform.position = Vector3.Lerp(lerpStart.position, lerpEnd.position, moveProgress/maxMoveProgress);
        }
        else if(!UICropStats.isDetailed && moveProgress < maxMoveProgress)
        {
            if(forceActive) return;
            moveProgress += Time.deltaTime;
            moneyTrackerTransform.position = Vector3.Lerp(lerpStart.position, lerpEnd.position, moveProgress/maxMoveProgress);
        }
    }
}
