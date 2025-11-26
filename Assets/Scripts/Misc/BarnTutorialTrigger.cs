using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarnTutorialTrigger : MonoBehaviour
{
    public GameObject tutorial;
    void OnTriggerEnter(Collider other)
    {
        if(!GameSaveData.Instance.mil_gavePen && TimeManager.Instance.isDay && TimeManager.Instance.currentHour != 19 && Vector3.Distance(PlayerInteraction.Instance.transform.position, transform.position) < 7)
        {
            GameSaveData.Instance.mil_gavePen = true;
            PlayerInteraction.Instance.invincible = true;
            tutorial.SetActive(true);
        }
    }
}
