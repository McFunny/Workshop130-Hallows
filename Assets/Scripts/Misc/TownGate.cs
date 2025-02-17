using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TownGate : MonoBehaviour
{
    public GameObject townMist, farmMist;
    //public Transform townPos, farmPos;
    //public bool inTown = false;

    public PlayerLocation location;
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
        if((location == PlayerLocation.InTown && enteringTown) || (location != PlayerLocation.InTown && !enteringTown)) return;
        if(enteringTown)
        {
            //PlayerInteraction.Instance.transform.position = farmPos.position;
            townMist.gameObject.SetActive(true);
            farmMist.gameObject.SetActive(false);
            //inTown = true;
            location = PlayerLocation.InTown;
        }
        else
        {
            //PlayerInteraction.Instance.transform.position = townPos.position;
            townMist.gameObject.SetActive(false);
            farmMist.gameObject.SetActive(true);
            location = PlayerLocation.InFarm;
        }
    }

    public void Transition(PlayerLocation newLocation)
    {
        switch(newLocation)
        {
            case PlayerLocation.InFarm:
            townMist.gameObject.SetActive(false);
            farmMist.gameObject.SetActive(true);
            break;
            case PlayerLocation.InTown:
            townMist.gameObject.SetActive(true);
            farmMist.gameObject.SetActive(false);
            break;
            case PlayerLocation.InCrypt:
            townMist.gameObject.SetActive(false);
            farmMist.gameObject.SetActive(false);
            break;
            case PlayerLocation.InWilderness:
            townMist.gameObject.SetActive(false);
            farmMist.gameObject.SetActive(false);
            break;
        }

        location = newLocation;
    }

    public void GameOver()
    {
        location = PlayerLocation.InFarm;
        townMist.gameObject.SetActive(false);
        farmMist.gameObject.SetActive(true);
    }

}

public enum PlayerLocation
{
    InFarm,
    InTown,
    InCrypt,
    InWilderness
}
