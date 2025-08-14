using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TownClock : MonoBehaviour
{
    [SerializeField] private GameObject hourHand;
    [SerializeField] private GameObject minuteHand;
    private Quaternion targetHourRotation;
    private void OnEnable()
    {
        TimeManager.OnHourlyUpdate += UpdateHour;
    }
    private void OnDisable()
    {
        TimeManager.OnHourlyUpdate -= UpdateHour;
    }
    void Start()
    {
        UpdateHour();
    }
    void Update()
    {
        float zRot;
        if (TimeManager.Instance.isDay)
        {
            zRot = TimeManager.Instance.currentMinute * 5.142f;
        }
        else
        {
            zRot = TimeManager.Instance.currentMinute * 9;
        }
        minuteHand.transform.localRotation = Quaternion.Euler(0f, 0f, zRot);
        hourHand.transform.localRotation = Quaternion.Lerp(hourHand.transform.localRotation, targetHourRotation, Time.deltaTime * 10f);
    }
    private void UpdateHour()
    {
        float zRot = TimeManager.Instance.currentHour * 30;
        if (zRot == 720) { zRot = 0; }
        targetHourRotation = Quaternion.Euler(0f, 0f, zRot);
    }
}
