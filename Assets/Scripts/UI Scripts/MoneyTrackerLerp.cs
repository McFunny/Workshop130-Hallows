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
    private CropStatsRework cropStatsRework;
    
    // Start is called before the first frame update
    void Start()
    {
        cropStatsRework = FindFirstObjectByType<CropStatsRework>();
        moneyTrackerTransform.position = lerpEnd.position;
    }

    // Update is called once per frame
    void Update()
    { 
        if(PlayerMovement.accessingInventory || TownGate.Instance.location == PlayerLocation.InTown || MainMenuScript.currentFileMode == FileMode.Survival){ forceActive = true; }
        else { forceActive = false; }

        if((cropStatsRework.isDetailed && moveProgress > 0) || (forceActive && moveProgress >= 0))
        {
            moveProgress -= Time.deltaTime;
            moneyTrackerTransform.position = Vector3.Lerp(lerpStart.position, lerpEnd.position, moveProgress/maxMoveProgress);
        }
        else if(!cropStatsRework.isDetailed && moveProgress < maxMoveProgress)
        {
            if(forceActive) return;
            moveProgress += Time.deltaTime;
            moneyTrackerTransform.position = Vector3.Lerp(lerpStart.position, lerpEnd.position, moveProgress/maxMoveProgress);
        }
    }
}
