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
        StartCoroutine(DelayedStart());
    }

    void OnDestroy()
    {
        TimeManager.OnHourlyUpdate -= HourPassed;
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(5);
        HourPassed();
    }

    void HourPassed()
    {
        if(!GameSaveData.Instance.playerHasBox && !currentBox)
        {
            currentBox = Instantiate(box, transform.position, Quaternion.identity);
        }
    }
}
