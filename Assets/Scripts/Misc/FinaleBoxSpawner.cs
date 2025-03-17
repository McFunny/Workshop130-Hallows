using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinaleBoxSpawner : MonoBehaviour
{
    public GameObject box;
    GameObject currentBox;

    void Start()
    {
        TimeManager.OnHourlyUpdate += HourPassed;
        HourPassed();
    }

    void OnDestroy()
    {
        TimeManager.OnHourlyUpdate -= HourPassed;
    }

    void HourPassed()
    {
        if(!GameSaveData.Instance.playerHasBox && !currentBox)
        {
            currentBox = Instantiate(box, transform.position, Quaternion.identity);
        }
    }
}
