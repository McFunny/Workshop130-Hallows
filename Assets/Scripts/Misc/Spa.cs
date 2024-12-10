using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spa : MonoBehaviour
{
    bool playerInSpa;

    void Start()
    {
        StartCoroutine(Heal());
    }
    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == 10)
        {
            playerInSpa = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.gameObject.layer == 10)
        {
            playerInSpa = false;
        }
    }

    IEnumerator Heal()
    {
        do
        {
            yield return new WaitForSeconds(0.5f);
            if(playerInSpa) PlayerInteraction.Instance.stamina += 5;
        }
        while(gameObject.activeSelf);
    }
}
