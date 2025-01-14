using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TownGate : MonoBehaviour
{
    public GameObject townMist, farmMist;
    //public Transform townPos, farmPos;
    public bool inTown = false;
    //bool transitioning = false;

    public static TownGate Instance;

    public Collider townEnter, townExit;


    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }
    }

    public void Transition(bool enteringTown)
    {
        //print(enteringTown);
        if((inTown && enteringTown) || (!inTown && !enteringTown)) return;
        if(enteringTown)
        {
            //PlayerInteraction.Instance.transform.position = farmPos.position;
            townMist.gameObject.SetActive(true);
            farmMist.gameObject.SetActive(false);
            inTown = true;
        }
        else
        {
            //PlayerInteraction.Instance.transform.position = townPos.position;
            townMist.gameObject.SetActive(false);
            farmMist.gameObject.SetActive(true);
            inTown = false;
        }
    }

    public void GameOver()
    {
        inTown = false;
        townMist.gameObject.SetActive(false);
        farmMist.gameObject.SetActive(true);
    }

}
